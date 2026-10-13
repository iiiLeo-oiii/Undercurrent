
using System;
using System.Collections.Generic;
using UnityEngine;

public class AfterFishHoldingManager : MonoBehaviour
{
    [Serializable]
    public class FishEntry
    {
        [Header("鱼模型（场景中的模板）")]
        public GameObject fishModel;

        [Header("相对于持鱼位置的坐标")]
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localRotation = Vector3.zero;

        [Header("是否随机大小")]
        public bool randomizeScale = true;

        [Header("关闭随机大小时使用的缩放")]
        public Vector3 localScale = Vector3.one;
    }

    [Header("所有可钓到的鱼")]
    public List<FishEntry> fishList = new List<FishEntry>();

    [Header("玩家拿鱼的位置")]
    public Transform fishHoldPoint;

    [Header("玩家摄像机")]
    public Transform playerCamera;

    [Header("鱼的随机大小（整数）")]
    [Min(1)] public int minRandomScale = 1;
    [Min(1)] public int maxRandomScale = 8;

    [Header("地上鱼的交互")]
    public float pickupDistance = 2f;
    public float dropForwardDistance = 1.1f;

    [Header("碰撞箱缩放")]
    [Range(0.3f, 1f)]
    public float colliderSizeMultiplier = 0.3f;

    private GameObject currentFish;
    private int currentFishIndex = -1;
    private int lastFishIndex = -1;

    private System.Random fishRandom;

    private GameObject returningFish;
    private int returningFishIndex = -1;
    private Vector3 returningFishScale;

    void Awake()
    {
        fishRandom = new System.Random(Guid.NewGuid().GetHashCode());
    }

    void Start()
    {
        if (maxRandomScale < minRandomScale)
            maxRandomScale = minRandomScale;

        foreach (FishEntry entry in fishList)
        {
            if (entry != null && entry.fishModel != null)
                entry.fishModel.SetActive(false);
        }
    }

    void Update()
    {
        if (FishingMinigame.IsPlaying || returningFish != null)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            HandleQKey();
    }

    // Q键：手里有鱼时只丢鱼；手里没鱼时才捡鱼
    void HandleQKey()
    {
        // 情况一：手里有鱼
        // 丢下后立刻结束本次操作，不能顺便捡起其他鱼
        if (IsHoldingFish())
        {
            DropCurrentFish();
            return;
        }

        // 情况二：手里没鱼，寻找附近的鱼
        FishWorldPickup nearbyFish = FindNearestDroppedFish();

        if (nearbyFish != null)
        {
            PickUpDroppedFish(nearbyFish);
            return;
        }

        // 情况三：手里没鱼，附近也没有鱼
        Debug.Log("附近没有可以捡起的鱼。");
    }

    bool IsHoldingFish()
    {
        return currentFish != null
            && currentFish.activeInHierarchy
            && fishHoldPoint != null
            && currentFish.transform.parent == fishHoldPoint;
    }

    // 保留原来的获得随机鱼功能
    public void CatchRandomFish()
    {
        if (fishHoldPoint == null)
        {
            Debug.LogError("AfterFishHoldingManager：没有设置 Fish Hold Point！");
            return;
        }

        List<int> validIndices = GetValidFishIndices();

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("鱼类列表中没有有效的鱼模型！");
            return;
        }

        int selectedIndex = SelectFishIndex(validIndices);
        FishEntry entry = fishList[selectedIndex];

        // 获得新鱼前，丢掉手里的旧鱼
        DropCurrentFish();

        GameObject newFish = Instantiate(entry.fishModel);
        newFish.name = entry.fishModel.name;

        RemoveWorldPhysics(newFish);

        newFish.transform.SetParent(fishHoldPoint, false);
        newFish.transform.localPosition = entry.localPosition;
        newFish.transform.localEulerAngles = entry.localRotation;
        newFish.SetActive(true);

        Vector3 finalScale = GetFishScale(entry);
        newFish.transform.localScale = finalScale;

        currentFish = newFish;
        currentFishIndex = selectedIndex;
        lastFishIndex = selectedIndex;

        Debug.Log("获得鱼：" + currentFish.name);
    }

    // 创建飞回途中的鱼：这个实例最后也会成为手持的鱼
    public GameObject CreateFishForReturn(Vector3 startPosition)
    {
        if (fishHoldPoint == null)
        {
            Debug.LogError("AfterFishHoldingManager：没有设置 Fish Hold Point！");
            return null;
        }

        List<int> validIndices = GetValidFishIndices();

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("鱼类列表中没有有效的鱼模型！");
            return null;
        }

        int selectedIndex = SelectFishIndex(validIndices);
        FishEntry entry = fishList[selectedIndex];

        // 钓到新鱼时，先丢掉手里的旧鱼
        DropCurrentFish();

        GameObject fish = Instantiate(entry.fishModel);
        fish.name = entry.fishModel.name;

        RemoveWorldPhysics(fish);

        fish.transform.SetParent(null);
        fish.transform.position = startPosition;
        fish.transform.rotation = entry.fishModel.transform.rotation;

        returningFishScale = GetFishScale(entry);

        // 计算不挂在 FishHoldPoint 下时对应的世界缩放
        Vector3 holdScale = fishHoldPoint.lossyScale;

        fish.transform.localScale = new Vector3(
            returningFishScale.x * holdScale.x,
            returningFishScale.y * holdScale.y,
            returningFishScale.z * holdScale.z
        );

        fish.SetActive(true);

        returningFish = fish;
        returningFishIndex = selectedIndex;

        Debug.Log("开始收回鱼：" + fish.name);

        return fish;
    }

    // 飞回结束后，将同一个鱼实例放到手上
    public void FinishFishReturn(GameObject fish)
    {
        if (fish == null || fish != returningFish)
        {
            Debug.LogWarning("收鱼失败：飞回的鱼模型不存在或不匹配。");
            return;
        }

        if (fishHoldPoint == null ||
            returningFishIndex < 0 ||
            returningFishIndex >= fishList.Count)
        {
            Debug.LogError("AfterFishHoldingManager：鱼的位置或索引无效！");
            return;
        }

        FishEntry entry = fishList[returningFishIndex];

        fish.transform.SetParent(fishHoldPoint, false);
        fish.transform.localPosition = entry.localPosition;
        fish.transform.localEulerAngles = entry.localRotation;
        fish.transform.localScale = returningFishScale;
        fish.SetActive(true);

        currentFish = fish;
        currentFishIndex = returningFishIndex;
        lastFishIndex = returningFishIndex;

        returningFish = null;
        returningFishIndex = -1;

        Debug.Log("鱼已飞回并拿在手上：" + currentFish.name);
    }

    List<int> GetValidFishIndices()
    {
        List<int> validIndices = new List<int>();

        if (fishList == null)
            return validIndices;

        for (int i = 0; i < fishList.Count; i++)
        {
            if (fishList[i] != null && fishList[i].fishModel != null)
                validIndices.Add(i);
        }

        return validIndices;
    }

    int SelectFishIndex(List<int> validIndices)
    {
        if (validIndices.Count > 1 && validIndices.Contains(lastFishIndex))
            validIndices.Remove(lastFishIndex);

        return validIndices[fishRandom.Next(0, validIndices.Count)];
    }

    Vector3 GetFishScale(FishEntry entry)
    {
        if (!entry.randomizeScale)
            return entry.localScale;

        int minScale = Mathf.Max(1, minRandomScale);
        int maxScale = Mathf.Max(minScale, maxRandomScale);
        int randomScale = fishRandom.Next(minScale, maxScale + 1);

        Debug.Log("本次随机缩放值：" + randomScale);

        return Vector3.one * randomScale;
    }

    public void DropCurrentFish()
    {
        if (!IsHoldingFish())
        {
            currentFish = null;
            currentFishIndex = -1;
            return;
        }

        GameObject fish = currentFish;
        int fishIndex = currentFishIndex;

        if (fishIndex < 0 || fishList == null
            || fishIndex >= fishList.Count
            || fishList[fishIndex] == null)
        {
            Debug.LogError("无法放鱼：鱼列表索引无效。");
            return;
        }

        Vector3 heldLocalPosition = fishList[fishIndex].localPosition;
        Vector3 heldLocalRotation = fishList[fishIndex].localRotation;
        float heldScale = fish.transform.localScale.x;

        currentFish = null;
        currentFishIndex = -1;

        fish.transform.SetParent(null, true);

        if (playerCamera != null)
        {
            fish.transform.position =
                playerCamera.position
                + playerCamera.forward * dropForwardDistance
                - playerCamera.up * 0.35f;
        }
        else
        {
            Debug.LogWarning("没有设置 Player Camera，鱼将在原位置放下。");
        }

        fish.SetActive(true);

        FishWorldPickup pickup = fish.GetComponent<FishWorldPickup>();

        if (pickup == null)
            pickup = fish.AddComponent<FishWorldPickup>();

        pickup.owner = this;
        pickup.fishEntryIndex = fishIndex;
        pickup.heldLocalPosition = heldLocalPosition;
        pickup.heldLocalRotation = heldLocalRotation;
        pickup.heldScale = heldScale;

        BoxCollider box = fish.GetComponent<BoxCollider>();

        if (box == null)
            box = fish.AddComponent<BoxCollider>();

        SetFishColliderBounds(fish, box);

        Rigidbody rb = fish.GetComponent<Rigidbody>();

        if (rb == null)
            rb = fish.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (playerCamera != null)
        {
            rb.velocity =
                playerCamera.forward * 1.2f
                + Vector3.up * 0.5f;
        }

        Debug.Log("放下鱼：" + fish.name);
    }

    FishWorldPickup FindNearestDroppedFish()
    {
        if (playerCamera == null)
        {
            Debug.LogError("AfterFishHoldingManager：没有设置 Player Camera！");
            return null;
        }

        FishWorldPickup[] droppedFish =
            FindObjectsOfType<FishWorldPickup>();

        FishWorldPickup nearestFish = null;
        float nearestDistance = Mathf.Max(0f, pickupDistance);

        foreach (FishWorldPickup fish in droppedFish)
        {
            if (fish == null || !fish.gameObject.activeInHierarchy)
                continue;

            if (fish.owner != this)
                continue;

            Collider fishCollider = fish.GetComponent<Collider>();
            float distance;

            if (fishCollider != null && fishCollider.enabled)
            {
                Vector3 closestPoint =
                    fishCollider.ClosestPoint(playerCamera.position);

                distance = Vector3.Distance(
                    playerCamera.position,
                    closestPoint
                );
            }
            else
            {
                distance = Vector3.Distance(
                    playerCamera.position,
                    fish.transform.position
                );
            }

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearestFish = fish;
            }
        }

        return nearestFish;
    }

    void PickUpDroppedFish(FishWorldPickup pickup)
    {
        if (pickup == null || fishHoldPoint == null)
            return;

        GameObject fish = pickup.gameObject;

        int fishIndex = pickup.fishEntryIndex;
        Vector3 heldPosition = pickup.heldLocalPosition;
        Vector3 heldRotation = pickup.heldLocalRotation;
        float heldScale = pickup.heldScale;

        if (fishIndex < 0 || fishList == null
            || fishIndex >= fishList.Count
            || fishList[fishIndex] == null)
        {
            Debug.LogError("无法捡鱼：鱼列表索引无效。");
            return;
        }

        // 正常情况下，进入这里时手里没有鱼。
        // 保留这层保护，避免其他脚本调用时出现两条手持鱼。
        if (IsHoldingFish() && currentFish != fish)
            DropCurrentFish();

        RemoveWorldPhysics(fish);

        FishWorldPickup marker = fish.GetComponent<FishWorldPickup>();

        if (marker != null)
            Destroy(marker);

        fish.transform.SetParent(fishHoldPoint, false);
        fish.transform.localPosition = heldPosition;
        fish.transform.localEulerAngles = heldRotation;
        fish.transform.localScale = Vector3.one * heldScale;

        fish.SetActive(true);

        currentFish = fish;
        currentFishIndex = fishIndex;
        lastFishIndex = fishIndex;

        Debug.Log("捡起鱼：" + fish.name);
    }

    void RemoveWorldPhysics(GameObject fish)
    {
        if (fish == null)
            return;

        FishWorldPickup marker = fish.GetComponent<FishWorldPickup>();

        if (marker != null)
            Destroy(marker);

        Rigidbody rb = fish.GetComponent<Rigidbody>();

        if (rb != null)
            Destroy(rb);

        Collider[] colliders = fish.GetComponents<Collider>();

        foreach (Collider col in colliders)
        {
            if (col != null)
                Destroy(col);
        }
    }

    void SetFishColliderBounds(GameObject fish, BoxCollider box)
    {
        Renderer[] renderers = fish.GetComponentsInChildren<Renderer>();

        bool initialized = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            Bounds rendererBounds = renderer.localBounds;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 rendererLocalCorner = new Vector3(
                            x == 0 ? rendererBounds.min.x : rendererBounds.max.x,
                            y == 0 ? rendererBounds.min.y : rendererBounds.max.y,
                            z == 0 ? rendererBounds.min.z : rendererBounds.max.z
                        );

                        Vector3 worldCorner =
                            renderer.transform.TransformPoint(rendererLocalCorner);

                        Vector3 fishLocalCorner =
                            fish.transform.InverseTransformPoint(worldCorner);

                        if (!initialized)
                        {
                            localBounds = new Bounds(
                                fishLocalCorner,
                                Vector3.zero
                            );

                            initialized = true;
                        }
                        else
                        {
                            localBounds.Encapsulate(fishLocalCorner);
                        }
                    }
                }
            }
        }

        if (initialized)
        {
            box.center = localBounds.center;

            float multiplier = Mathf.Clamp(
                colliderSizeMultiplier,
                0.3f,
                1f
            );

            Vector3 adjustedSize = localBounds.size * multiplier;

            box.size = new Vector3(
                Mathf.Max(0.05f, adjustedSize.x),
                Mathf.Max(0.05f, adjustedSize.y),
                Mathf.Max(0.05f, adjustedSize.z)
            );
        }
        else
        {
            box.center = Vector3.zero;
            box.size = Vector3.one * 0.25f;
        }
    }
}
