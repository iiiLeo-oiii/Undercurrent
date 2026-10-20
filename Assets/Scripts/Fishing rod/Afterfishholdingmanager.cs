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

    [Header("地上物品的交互")]
    public float pickupDistance = 2f;
    public float dropForwardDistance = 1.1f;

    [Header("碰撞箱缩放")]
    [Range(0.3f, 1f)]
    public float colliderSizeMultiplier = 0.3f;

    [Header("特殊物品：老照片")]
    [Tooltip("直接拖入场景中的老照片 GameObject，不需要加入 fishList")]
    public GameObject oldPhoto;

    [Tooltip("老照片拿在手上的局部位置")]
    public Vector3 photoHeldLocalPosition = Vector3.zero;

    [Tooltip("老照片拿在手上的局部旋转")]
    public Vector3 photoHeldLocalRotation = Vector3.zero;

    [Tooltip("老照片拿在手上的局部缩放")]
    public Vector3 photoHeldLocalScale = Vector3.one;

    // 当前手持物品
    private GameObject currentFish;
    private int currentFishIndex = -1;
    private int lastFishIndex = -1;
    private bool currentIsPhoto = false;

    private System.Random fishRandom;

    // 正在飞回的物品
    private GameObject returningFish;
    private int returningFishIndex = -1;
    private Vector3 returningFishScale;
    private bool returningIsPhoto = false;

    // 箱子触发后，等待下一次成功钓鱼
    private bool photoCatchReady = false;

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

        // 老照片是场景中的独立物体。
        // 先隐藏，等触发特殊钓鱼后再显示。
        if (oldPhoto != null)
            oldPhoto.SetActive(false);
        else
            Debug.LogWarning(
                "AfterFishHoldingManager：还没有设置 Old Photo！"
            );
    }

    void Update()
    {
        if (FishingMinigame.IsPlaying || returningFish != null)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            HandleQKey();
    }

    // Q键逻辑：
    // 手上有物品时，只丢下。
    // 手上没有物品时，才允许捡起附近的物品。
    void HandleQKey()
    {
        if (IsHoldingFish())
        {
            DropCurrentFish();
            return;
        }

        FishWorldPickup nearbyFish = FindNearestDroppedFish();

        if (nearbyFish != null)
        {
            PickUpDroppedFish(nearbyFish);
            return;
        }

        Debug.Log("附近没有可以捡起的物品。");
    }

    bool IsHoldingFish()
    {
        return currentFish != null
            && currentFish.activeInHierarchy
            && fishHoldPoint != null
            && currentFish.transform.parent == fishHoldPoint;
    }

    // 箱子里的鱼触发后调用。
    // 只设置标记，不会立即给照片。
    public void MarkPhotoCatchReady()
    {
        if (photoCatchReady)
            return;

        photoCatchReady = true;

        Debug.Log("箱子已触发：下一次成功钓鱼将获得老照片。");
    }

    // 保留原来的获得随机鱼功能。
    // 如果照片标记已经触发，这里也会优先给照片。
    public void CatchRandomFish()
    {
        if (photoCatchReady)
        {
            GivePhotoDirectly();
            return;
        }

        if (fishHoldPoint == null)
        {
            Debug.LogError(
                "AfterFishHoldingManager：没有设置 Fish Hold Point！"
            );
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

        DropCurrentFish();

        GameObject newFish = Instantiate(entry.fishModel);
        newFish.name = entry.fishModel.name;

        RemoveWorldPhysics(newFish);

        newFish.transform.SetParent(fishHoldPoint, false);
        newFish.transform.localPosition = entry.localPosition;
        newFish.transform.localEulerAngles = entry.localRotation;
        newFish.SetActive(true);
        newFish.transform.localScale = GetFishScale(entry);

        currentFish = newFish;
        currentFishIndex = selectedIndex;
        currentIsPhoto = false;
        lastFishIndex = selectedIndex;

        Debug.Log("获得鱼：" + currentFish.name);
    }

    // 创建飞回途中的物品。
    // 普通鱼使用 Instantiate；老照片直接使用场景中的对象。
    public GameObject CreateFishForReturn(Vector3 startPosition)
    {
        if (fishHoldPoint == null)
        {
            Debug.LogError(
                "AfterFishHoldingManager：没有设置 Fish Hold Point！"
            );
            return null;
        }

        // 先检查是否需要给照片。
        if (photoCatchReady)
            return CreatePhotoForReturn(startPosition);

        List<int> validIndices = GetValidFishIndices();

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("鱼类列表中没有有效的鱼模型！");
            return null;
        }

        int selectedIndex = SelectFishIndex(validIndices);
        FishEntry entry = fishList[selectedIndex];

        // 新物品替换旧物品：旧物品先落到地上。
        DropCurrentFish();

        GameObject fish = Instantiate(entry.fishModel);
        fish.name = entry.fishModel.name;

        RemoveWorldPhysics(fish);

        fish.transform.SetParent(null);
        fish.transform.position = startPosition;
        fish.transform.rotation = entry.fishModel.transform.rotation;

        returningFishScale = GetFishScale(entry);

        Vector3 holdScale = fishHoldPoint.lossyScale;

        fish.transform.localScale = new Vector3(
            returningFishScale.x * holdScale.x,
            returningFishScale.y * holdScale.y,
            returningFishScale.z * holdScale.z
        );

        fish.SetActive(true);

        returningFish = fish;
        returningFishIndex = selectedIndex;
        returningIsPhoto = false;

        Debug.Log("开始收回鱼：" + fish.name);

        return fish;
    }

    GameObject CreatePhotoForReturn(Vector3 startPosition)
    {
        if (oldPhoto == null)
        {
            Debug.LogError(
                "AfterFishHoldingManager：触发了照片奖励，但没有设置 Old Photo！"
            );
            return null;
        }

        // 消耗标记：这次成功钓鱼已经使用照片奖励。
        photoCatchReady = false;

        // 如果手里有旧物品，先将其放下。
        DropCurrentFish();

        GameObject photo = oldPhoto;

        RemoveWorldPhysics(photo);

        photo.transform.SetParent(null);
        photo.transform.position = startPosition;
        photo.transform.rotation = Quaternion.Euler(
            photoHeldLocalRotation
        );

        // 飞行期间使用适应 FishHoldPoint 缩放的世界缩放。
        Vector3 holdScale = fishHoldPoint.lossyScale;

        photo.transform.localScale = new Vector3(
            photoHeldLocalScale.x * holdScale.x,
            photoHeldLocalScale.y * holdScale.y,
            photoHeldLocalScale.z * holdScale.z
        );

        photo.SetActive(true);

        returningFish = photo;
        returningFishIndex = -1;
        returningFishScale = photoHeldLocalScale;
        returningIsPhoto = true;

        Debug.Log("特殊奖励：开始收回老照片。");

        return photo;
    }

    // 照片奖励触发后，如果其他系统直接调用 CatchRandomFish，
    // 也能正确获得照片。
    void GivePhotoDirectly()
    {
        if (oldPhoto == null || fishHoldPoint == null)
        {
            Debug.LogError(
                "无法获得老照片：请检查 Old Photo 和 Fish Hold Point。"
            );
            return;
        }

        photoCatchReady = false;

        DropCurrentFish();
        RemoveWorldPhysics(oldPhoto);

        oldPhoto.transform.SetParent(fishHoldPoint, false);
        oldPhoto.transform.localPosition = photoHeldLocalPosition;
        oldPhoto.transform.localEulerAngles = photoHeldLocalRotation;
        oldPhoto.transform.localScale = photoHeldLocalScale;
        oldPhoto.SetActive(true);

        currentFish = oldPhoto;
        currentFishIndex = -1;
        currentIsPhoto = true;

        Debug.Log("获得特殊物品：老照片。");
    }

    // 飞回结束后，将同一个物品放到手上。
    public void FinishFishReturn(GameObject fish)
    {
        if (fish == null || fish != returningFish)
        {
            Debug.LogWarning("收回物品失败：飞回的物品不存在或不匹配。");
            return;
        }

        if (fishHoldPoint == null)
        {
            Debug.LogError(
                "AfterFishHoldingManager：没有设置 Fish Hold Point！"
            );
            return;
        }

        // 特殊情况：飞回的是老照片。
        if (returningIsPhoto)
        {
            fish.transform.SetParent(fishHoldPoint, false);
            fish.transform.localPosition = photoHeldLocalPosition;
            fish.transform.localEulerAngles = photoHeldLocalRotation;
            fish.transform.localScale = photoHeldLocalScale;
            fish.SetActive(true);

            currentFish = fish;
            currentFishIndex = -1;
            currentIsPhoto = true;

            returningFish = null;
            returningFishIndex = -1;
            returningIsPhoto = false;

            Debug.Log("老照片已飞回并拿在手上。");
            return;
        }

        // 普通鱼的原有逻辑。
        if (returningFishIndex < 0 ||
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
        currentIsPhoto = false;
        lastFishIndex = returningFishIndex;

        returningFish = null;
        returningFishIndex = -1;
        returningIsPhoto = false;

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
            currentIsPhoto = false;
            return;
        }

        GameObject fish = currentFish;
        int fishIndex = currentFishIndex;
        bool droppingPhoto = currentIsPhoto;

        Vector3 heldLocalPosition;
        Vector3 heldLocalRotation;
        Vector3 heldLocalScale;

        if (droppingPhoto)
        {
            heldLocalPosition = photoHeldLocalPosition;
            heldLocalRotation = photoHeldLocalRotation;
            heldLocalScale = photoHeldLocalScale;
        }
        else
        {
            if (fishIndex < 0 || fishList == null
                || fishIndex >= fishList.Count
                || fishList[fishIndex] == null)
            {
                Debug.LogError("无法放鱼：鱼列表索引无效。");
                return;
            }

            heldLocalPosition = fishList[fishIndex].localPosition;
            heldLocalRotation = fishList[fishIndex].localRotation;
            heldLocalScale = fish.transform.localScale;
        }

        currentFish = null;
        currentFishIndex = -1;
        currentIsPhoto = false;

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
            Debug.LogWarning("没有设置 Player Camera，物品将在原位置放下。");
        }

        fish.SetActive(true);

        FishWorldPickup pickup = fish.GetComponent<FishWorldPickup>();

        if (pickup == null)
            pickup = fish.AddComponent<FishWorldPickup>();

        pickup.owner = this;
        pickup.fishEntryIndex = droppingPhoto ? -1 : fishIndex;
        pickup.heldLocalPosition = heldLocalPosition;
        pickup.heldLocalRotation = heldLocalRotation;
        pickup.heldLocalScale = heldLocalScale;
        pickup.heldScale = heldLocalScale.x;
        pickup.isPhoto = droppingPhoto;

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

        Debug.Log("放下物品：" + fish.name);
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

        bool pickingUpPhoto = pickup.isPhoto;
        int fishIndex = pickup.fishEntryIndex;

        Vector3 heldPosition = pickup.heldLocalPosition;
        Vector3 heldRotation = pickup.heldLocalRotation;
        Vector3 heldScale = pickup.heldLocalScale;

        if (!pickingUpPhoto)
        {
            if (fishIndex < 0 || fishList == null
                || fishIndex >= fishList.Count
                || fishList[fishIndex] == null)
            {
                Debug.LogError("无法捡鱼：鱼列表索引无效。");
                return;
            }
        }

        // 捡起新物品前，先放下手中已有的物品。
        if (IsHoldingFish() && currentFish != fish)
            DropCurrentFish();

        RemoveWorldPhysics(fish);

        FishWorldPickup marker = fish.GetComponent<FishWorldPickup>();

        if (marker != null)
            Destroy(marker);

        fish.transform.SetParent(fishHoldPoint, false);
        fish.transform.localPosition = heldPosition;
        fish.transform.localEulerAngles = heldRotation;
        fish.transform.localScale = heldScale;
        fish.SetActive(true);

        currentFish = fish;
        currentFishIndex = pickingUpPhoto ? -1 : fishIndex;
        currentIsPhoto = pickingUpPhoto;

        if (pickingUpPhoto)
        {
            Debug.Log("捡起老照片。");
        }
        else
        {
            lastFishIndex = fishIndex;
            Debug.Log("捡起鱼：" + fish.name);
        }
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