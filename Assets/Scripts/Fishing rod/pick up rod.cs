using UnityEngine;

public class RodPickup : MonoBehaviour
{
    [Header("Rod")]
    public GameObject handRod;
    public GameObject groundRod;

    [Header("Pickup Settings")]
    public float pickupDistance = 2.5f;

    private bool pickedUp = false;


    // ==================================================
    // 游戏开始
    // ==================================================

    void Awake()
    {
        // 地上的鱼竿显示
        if (groundRod != null)
        {
            groundRod.SetActive(true);
        }

        // 手里的鱼竿隐藏
        if (handRod != null)
        {
            handRod.SetActive(false);
        }

        pickedUp = false;
    }


    // ==================================================
    // 玩家靠近鱼竿
    // ==================================================

    void Update()
    {
        // 已经拿到了
        if (pickedUp)
        {
            return;
        }

        // 没有设置 GroundRod
        if (groundRod == null)
        {
            return;
        }

        // 计算玩家和鱼竿之间的距离
        float distance = Vector3.Distance(
            transform.position,
            groundRod.transform.position
        );

        // 靠近鱼竿 + 按 E
        if (
            distance <= pickupDistance &&
            Input.GetKeyDown(KeyCode.E)
        )
        {
            PickUpRod();
        }
    }


    // ==================================================
    // 捡起鱼竿
    // ==================================================

    void PickUpRod()
    {
        pickedUp = true;


        // --------------------------------------------------
        // 1. 地上的鱼竿消失
        // --------------------------------------------------

        if (groundRod != null)
        {
            groundRod.SetActive(false);
        }


        // --------------------------------------------------
        // 2. 手里的鱼竿出现
        // --------------------------------------------------

        if (handRod != null)
        {
            handRod.SetActive(true);
        }


        // --------------------------------------------------
        // 3. 告诉 Fishingrodmanager：
        //    玩家已经拿到鱼竿
        // --------------------------------------------------

        if (handRod != null)
        {
            Fishingrodmanager rodManager =
                handRod.GetComponent<Fishingrodmanager>();

            if (rodManager != null)
            {
                rodManager.hasRod = true;

                Debug.Log(
                    "RodPickup：已经拿到鱼竿，可以钓鱼！"
                );
            }
            else
            {
                Debug.LogError(
                    "RodPickup：HandRod 上没有 Fishingrodmanager！"
                );
            }
        }


        // --------------------------------------------------
        // 4. 告诉 FishingMinigame：
        //    现在允许小游戏启动
        // --------------------------------------------------

        FishingMinigame minigame =
            FindObjectOfType<FishingMinigame>();

        if (minigame != null)
        {
            minigame.canFish = true;
        }
        else
        {
            Debug.LogError(
                "RodPickup：找不到 FishingMinigame！"
            );
        }
    }
}