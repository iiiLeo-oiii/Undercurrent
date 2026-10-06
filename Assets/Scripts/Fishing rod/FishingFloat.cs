using System.Collections;
using UnityEngine;

public class FishingFloat : MonoBehaviour
{
    [Header("Fishing Minigame")]
    public FishingMinigame fishingMinigame;

    [Header("Fishing Rod Animator")]
    public Animator fishingRodAnimator;

    [Header("Fishing Rod Manager")]
    public Fishingrodmanager fishingRodManager;

    [Header("Fish Bite Time")]
    public float minBiteTime = 2f;
    public float maxBiteTime = 6f;

    private Rigidbody rb;

    private bool isOnWater = false;

    private Coroutine biteCoroutine;

    private float waterHeight;


    // ==================================================
    // 初始化
    // ==================================================

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    // ==================================================
    // 鱼漂被打开
    // ==================================================

    void OnEnable()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // 没拿到鱼竿，不允许鱼漂工作
        if (
            fishingRodManager != null &&
            !fishingRodManager.hasRod
        )
        {
            StopFishingFloat();

            return;
        }

        // 拿到鱼竿后正常初始化
        isOnWater = false;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.constraints =
            RigidbodyConstraints.None;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }


    // ==================================================
    // 停止鱼漂
    // ==================================================

    void StopFishingFloat()
    {
        isOnWater = false;

        if (biteCoroutine != null)
        {
            StopCoroutine(biteCoroutine);
            biteCoroutine = null;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
            rb.useGravity = false;

            rb.constraints =
                RigidbodyConstraints.FreezeAll;
        }
    }


    // ==================================================
    // 普通碰撞
    // ==================================================

    private void OnCollisionEnter(Collision collision)
    {
        // 没拿鱼竿，不处理任何事情
        if (
            fishingRodManager != null &&
            !fishingRodManager.hasRod
        )
        {
            return;
        }


        // ------------------------------
        // Ground
        // ------------------------------

        if (IsGround(collision.collider))
        {
            gameObject.SetActive(false);
            return;
        }


        // ------------------------------
        // Water
        // ------------------------------

        if (IsWater(collision.collider))
        {
            waterHeight =
                collision.collider.bounds.max.y;

            StartFishing();
        }
    }


    // ==================================================
    // Trigger
    // ==================================================

    private void OnTriggerEnter(Collider other)
    {
        // 没拿鱼竿，不处理任何事情
        if (
            fishingRodManager != null &&
            !fishingRodManager.hasRod
        )
        {
            return;
        }


        // ------------------------------
        // Ground
        // ------------------------------

        if (IsGround(other))
        {
            gameObject.SetActive(false);
            return;
        }


        // ------------------------------
        // Water
        // ------------------------------

        if (IsWater(other))
        {
            waterHeight =
                other.bounds.max.y;

            StartFishing();
        }
    }


    // ==================================================
    // 判断是不是 Ground
    // ==================================================

    bool IsGround(Collider collider)
    {
        if (collider.CompareTag("Ground"))
        {
            return true;
        }

        if (collider.transform.root.CompareTag("Ground"))
        {
            return true;
        }

        return false;
    }


    // ==================================================
    // 判断是不是 Water
    // ==================================================

    bool IsWater(Collider collider)
    {
        if (collider.CompareTag("Water"))
        {
            return true;
        }

        if (collider.transform.root.CompareTag("Water"))
        {
            return true;
        }

        return false;
    }


    // ==================================================
    // 开始钓鱼
    // ==================================================

    private void StartFishing()
    {
        if (
            fishingRodManager != null &&
            !fishingRodManager.hasRod
        )
        {
            return;
        }

        if (isOnWater)
        {
            return;
        }

        isOnWater = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;

        rb.constraints =
            RigidbodyConstraints.FreezeRotation;

        Vector3 position =
            transform.position;

        position.y =
            waterHeight;

        transform.position =
            position;

        StartWaitingForFish();
    }


    // ==================================================
    // 保持鱼漂在水面
    // ==================================================

    void FixedUpdate()
    {
        if (!isOnWater)
        {
            return;
        }

        Vector3 position =
            rb.position;

        position.y =
            waterHeight;

        rb.MovePosition(position);
    }


    // ==================================================
    // 开始等待鱼
    // ==================================================

    void StartWaitingForFish()
    {
        if (biteCoroutine != null)
        {
            StopCoroutine(biteCoroutine);
        }

        biteCoroutine =
            StartCoroutine(
                WaitForFish()
            );
    }


    // ==================================================
    // 随机等待鱼咬钩
    // ==================================================

    IEnumerator WaitForFish()
    {
        float waitTime =
            Random.Range(
                minBiteTime,
                maxBiteTime
            );

        Debug.Log(
            "鱼漂已经落水，等待鱼咬钩……"
        );

        yield return
            new WaitForSeconds(
                waitTime
            );


        if (!gameObject.activeSelf)
        {
            yield break;
        }

        if (!isOnWater)
        {
            yield break;
        }

        if (
            fishingRodManager != null &&
            !fishingRodManager.hasRod
        )
        {
            yield break;
        }


        Debug.Log(
            "有鱼咬钩！"
        );


        if (fishingRodAnimator != null)
        {
            fishingRodAnimator.Play(
                "鱼咬竿",
                0,
                0
            );
        }


        if (fishingMinigame != null)
        {
            fishingMinigame.StartGame();
        }
        else
        {
            Debug.LogError(
                "FishingFloat：没有设置 Fishing Minigame！"
            );
        }
    }


    // ==================================================
    // 小游戏失败
    // ==================================================

    public void ContinueFishing()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (
            fishingRodManager != null &&
            !fishingRodManager.hasRod
        )
        {
            gameObject.SetActive(false);
            return;
        }

        isOnWater = true;

        StartWaitingForFish();
    }
}