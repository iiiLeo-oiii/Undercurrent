
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
    public float colliderSizeMultiplier = 0.8f;

    private GameObject currentFish;
    private int currentFishIndex = -1;
    private int lastFishIndex = -1;

    void Start()
    {
        if (maxRandomScale < minRandomScale)
        {
            maxRandomScale = minRandomScale;
        }

        // 隐藏模板，实际获得鱼时再复制
        foreach (FishEntry entry in fishList)
        {
            if (entry != null && entry.fishModel != null)
            {
                entry.fishModel.SetActive(false);
            }
        }
    }

    void Update()
    {
        // 钓鱼小游戏进行时，不处理 Q 键
        if (FishingMinigame.IsPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleQKey();
        }
    }

    // Q 键：优先捡附近的鱼，否则放下手中的鱼
    void HandleQKey()
    {
        FishWorldPickup nearbyFish = FindNearestDroppedFish();

        if (nearbyFish != null)
        {
            PickUpDroppedFish(nearbyFish);
            return;
        }

        if (IsHoldingFish())
        {
            DropCurrentFish();
        }
        else
        {
            Debug.Log("附近没有可以捡起的鱼。");
        }
    }

    bool IsHoldingFish()
    {
        return currentFish != null
            && currentFish.activeInHierarchy
            && fishHoldPoint != null
            && currentFish.transform.parent == fishHoldPoint;
    }

    // 钓鱼成功后获得随机鱼
    public void CatchRandomFish()
    {
        if (fishHoldPoint == null)
        {
            Debug.LogError(
                "AfterFishHoldingManager：没有设置 Fish Hold Point！"
            );
            return;
        }

        if (fishList == null || fishList.Count == 0)
        {
            Debug.LogWarning("鱼类列表为空！");
            return;
        }

        List<int> validIndices = new List<int>();

        for (int i = 0; i < fishList.Count; i++)
        {
            if (fishList[i] != null && fishList[i].fishModel != null)
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("鱼类列表中没有有效的鱼模型！");
            return;
        }

        // 尽量避免连续两次获得同一种鱼
        if (validIndices.Count > 1)
        {
            validIndices.Remove(lastFishIndex);
        }

        int selectedIndex = validIndices[
            UnityEngine.Random.Range(0, validIndices.Count)
        ];

        FishEntry selectedFish = fishList[selectedIndex];

        // 新鱼替换旧鱼：先将旧鱼放到地上
        DropCurrentFish();

        // 从场景模板复制独立的鱼模型
        GameObject newFish = Instantiate(selectedFish.fishModel);
        newFish.name = selectedFish.fishModel.name;

        RemoveWorldPhysics(newFish);

        newFish.transform.SetParent(fishHoldPoint, false);
        newFish.transform.localPosition = selectedFish.localPosition;
        newFish.transform.localEulerAngles = selectedFish.localRotation;

        // 三个轴使用相同的随机缩放
        if (selectedFish.randomizeScale)
        {
            int minScale = Mathf.Max(1, minRandomScale);
            int maxScale = Mathf.Max(minScale, maxRandomScale);

            int randomScale = UnityEngine.Random.Range(
                minScale,
                maxScale + 1
            );

            newFish.transform.localScale = Vector3.one * randomScale;
        }
        else
        {
            newFish.transform.localScale = selectedFish.localScale;
        }

        newFish.SetActive(true);

        currentFish = newFish;
        currentFishIndex = selectedIndex;
        lastFishIndex = selectedIndex;

        Debug.Log(
            "获得鱼：" + currentFish.name
            + "，大小：" + currentFish.transform.localScale.x
        );
    }

    // 放下手中的鱼
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

        // 防止鱼列表变化后索引越界
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

        // 保留鱼离开手中时的世界朝向和大小
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
            Debug.LogWarning(
                "没有设置 Player Camera，鱼将在原位置放下。"
            );
        }

        fish.SetActive(true);

        // 添加地上鱼的交互标记
        FishWorldPickup pickup = fish.GetComponent<FishWorldPickup>();

        if (pickup == null)
        {
            pickup = fish.AddComponent<FishWorldPickup>();
        }

        pickup.owner = this;
        pickup.fishEntryIndex = fishIndex;
        pickup.heldLocalPosition = heldLocalPosition;
        pickup.heldLocalRotation = heldLocalRotation;
        pickup.heldScale = heldScale;

        // 添加碰撞箱
        BoxCollider box = fish.GetComponent<BoxCollider>();

        if (box == null)
        {
            box = fish.AddComponent<BoxCollider>();
        }

        SetFishColliderBounds(fish, box);

        // 添加物理组件
        Rigidbody rb = fish.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = fish.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // 轻轻向前抛出
        if (playerCamera != null)
        {
            rb.velocity =
                playerCamera.forward * 1.2f
                + Vector3.up * 0.5f;
        }

        Debug.Log("放下鱼：" + fish.name);
    }

    // 查找拾取范围内最近的地上鱼
    FishWorldPickup FindNearestDroppedFish()
    {
        if (playerCamera == null)
        {
            Debug.LogError(
                "AfterFishHoldingManager：没有设置 Player Camera！"
            );
            return null;
        }

        FishWorldPickup[] droppedFish =
            FindObjectsOfType<FishWorldPickup>();

        FishWorldPickup nearestFish = null;
        float nearestDistance = Mathf.Max(0f, pickupDistance);

        foreach (FishWorldPickup fish in droppedFish)
        {
            if (fish == null || !fish.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (fish.owner != this)
            {
                continue;
            }

            // 优先根据碰撞箱判断距离，而不是鱼模型的中心点
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

    // 捡起地上的鱼
    void PickUpDroppedFish(FishWorldPickup pickup)
    {
        if (pickup == null || fishHoldPoint == null)
        {
            return;
        }

        GameObject fish = pickup.gameObject;

        int fishIndex = pickup.fishEntryIndex;
        Vector3 heldPosition = pickup.heldLocalPosition;
        Vector3 heldRotation = pickup.heldLocalRotation;
        float heldScale = pickup.heldScale;

        // 检查索引，避免无效数据导致报错
        if (fishIndex < 0 || fishList == null
            || fishIndex >= fishList.Count)
        {
            Debug.LogError("无法捡鱼：鱼列表索引无效。");
            return;
        }

        // 如果手里有另一条鱼，先放下它
        if (IsHoldingFish() && currentFish != fish)
        {
            DropCurrentFish();
        }

        // 移除地上鱼的物理组件
        RemoveWorldPhysics(fish);

        FishWorldPickup marker = fish.GetComponent<FishWorldPickup>();

        if (marker != null)
        {
            Destroy(marker);
        }

        fish.transform.SetParent(fishHoldPoint, false);
        fish.transform.localPosition = heldPosition;
        fish.transform.localEulerAngles = heldRotation;
        fish.transform.localScale = Vector3.one * heldScale;

        fish.SetActive(true);

        currentFish = fish;
        currentFishIndex = fishIndex;

        Debug.Log("捡起鱼：" + fish.name);
    }

    // 移除鱼身上的地面物理组件
    void RemoveWorldPhysics(GameObject fish)
    {
        FishWorldPickup marker = fish.GetComponent<FishWorldPickup>();

        if (marker != null)
        {
            Destroy(marker);
        }

        Rigidbody rb = fish.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Destroy(rb);
        }

        Collider[] colliders = fish.GetComponents<Collider>();

        foreach (Collider col in colliders)
        {
            Destroy(col);
        }
    }

    // 根据模型自身的局部包围盒计算碰撞箱
    void SetFishColliderBounds(GameObject fish, BoxCollider box)
    {
        Renderer[] renderers =
            fish.GetComponentsInChildren<Renderer>();

        bool initialized = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            // 使用 Renderer 的局部包围盒，避免直接使用世界轴对齐包围盒
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

                        // 转换到世界坐标，再转换到鱼根物体的局部坐标
                        Vector3 worldCorner =
                            renderer.transform.TransformPoint(
                                rendererLocalCorner
                            );

                        Vector3 fishLocalCorner =
                            fish.transform.InverseTransformPoint(
                                worldCorner
                            );

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

            // 缩小碰撞箱，减少超出鱼模型的部分
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