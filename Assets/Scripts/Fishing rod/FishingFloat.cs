using System.Collections;
using UnityEngine;

public class FishingFloat : MonoBehaviour
{
    [Header("Fishing Minigame")]
    public FishingMinigame fishingMinigame;

    [Header("Fishing Rod Animator")]
    public Animator fishingRodAnimator;

    [Header("Fish Bite Time")]
    public float minBiteTime = 2f;
    public float maxBiteTime = 6f;

    private Rigidbody rb;

    private bool isOnWater = false;

    private Coroutine biteCoroutine;


    // ==================================================
    // 鱼漂每次重新出现时自动执行
    // ==================================================

    void OnEnable()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        isOnWater = false;

        rb.isKinematic = false;

        rb.useGravity = true;

        rb.constraints =
            RigidbodyConstraints.None;

        rb.velocity = Vector3.zero;

        rb.angularVelocity = Vector3.zero;
    }


    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }


    // ==================================================
    // 碰到水
    // ==================================================

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            StartFishing();
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }
    }


    // ==================================================
    // 如果 Water 是 Trigger
    // ==================================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            StartFishing();
        }

        if (other.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }
    }


    // ==================================================
    // 开始钓鱼
    // ==================================================

    private void StartFishing()
    {
        // 已经在水里，不重复触发
        if (isOnWater)
        {
            return;
        }

        isOnWater = true;


        // =========================
        // 停止鱼漂运动
        // =========================

        rb.velocity = Vector3.zero;

        rb.angularVelocity = Vector3.zero;


        // =========================
        // 关闭重力
        // =========================

        rb.useGravity = false;


        // =========================
        // 防止鱼漂旋转
        // =========================

        rb.constraints =
            RigidbodyConstraints.FreezeRotation;


        // =========================
        // 开始等待鱼咬钩
        // =========================

        StartWaitingForFish();
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
            StartCoroutine(WaitForFish());
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
            new WaitForSeconds(waitTime);


        Debug.Log("有鱼咬钩！");


        // =========================
        // 鱼竿弯曲动画
        // =========================

        if (fishingRodAnimator != null)
        {
            fishingRodAnimator.Play(
                "鱼咬竿",
                0,
                0
            );
        }


        // =========================
        // 开始小游戏
        // =========================

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
    // 小游戏失败后调用
    // ==================================================

    public void ContinueFishing()
    {
        // 鱼漂还在水里
        if (!gameObject.activeSelf)
        {
            return;
        }

        // 保持在水里
        isOnWater = true;

        // 重新等待下一条鱼
        StartWaitingForFish();
    }
}