using System.Collections;
using UnityEngine;

public class Fishingrodmanager : MonoBehaviour
{
    [Header("Fishing Float")]
    public GameObject fishingFloat;

    [Header("Cast Point")]
    public Transform castPoint;

    [Header("Cast Force")]
    public float forwardForce = 8f;
    public float upwardForce = 3f;

    [Header("Cast Animation")]
    public AnimationClip castAnimation;

    [Header("Fishing Minigame")]
    public FishingMinigame fishingMinigame;

    private Animator animator;

    // 防止抛竿动画期间再次按 E
    private bool isCasting = false;

    // 是否已经拿到鱼竿
    public bool hasRod = false;


    // ==================================================
    // 初始化
    // ==================================================

    void Start()
    {
        animator = GetComponent<Animator>();

        // 游戏开始时隐藏鱼漂
        if (fishingFloat != null)
        {
            fishingFloat.SetActive(false);
        }

        // 注意：
        // 这里不要再写 hasRod = false;
        //
        // 因为 HandRod 一开始是关闭的，
        // 玩家捡起它时才会启动这个脚本。
    }


    // ==================================================
    // 玩家输入
    // ==================================================

    void Update()
    {
        // 没拿到鱼竿
        if (!hasRod)
        {
            return;
        }

        // 正在抛竿
        if (isCasting)
        {
            return;
        }

        // 按 E
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 如果正在玩小游戏
            if (FishingMinigame.IsPlaying)
            {
                if (fishingMinigame != null)
                {
                    fishingMinigame.CancelGame();
                }

                return;
            }

            StartCoroutine(CastFishingRod());
        }
    }


    // ==================================================
    // 抛竿
    // ==================================================

    IEnumerator CastFishingRod()
    {
        isCasting = true;


        // --------------------------------------------------
        // 检查必要组件
        // --------------------------------------------------

        if (fishingFloat == null)
        {
            Debug.LogError(
                "Fishingrodmanager：没有设置 Fishing Float！"
            );

            isCasting = false;
            yield break;
        }

        if (castPoint == null)
        {
            Debug.LogError(
                "Fishingrodmanager：没有设置 Cast Point！"
            );

            isCasting = false;
            yield break;
        }

        if (animator == null)
        {
            Debug.LogError(
                "Fishingrodmanager：HandRod 上没有 Animator！"
            );

            isCasting = false;
            yield break;
        }

        if (castAnimation == null)
        {
            Debug.LogError(
                "Fishingrodmanager：没有设置 Cast Animation！"
            );

            isCasting = false;
            yield break;
        }


        // --------------------------------------------------
        // 关闭旧鱼漂
        // --------------------------------------------------

        fishingFloat.SetActive(false);


        // --------------------------------------------------
        // 播放抛竿动画
        // --------------------------------------------------

        animator.enabled = true;

        animator.Play(
            "抛竿",
            0,
            0f
        );


        // --------------------------------------------------
        // 等待动画结束
        // --------------------------------------------------

        yield return new WaitForSeconds(
            castAnimation.length
        );


        // --------------------------------------------------
        // 鱼漂出现
        // --------------------------------------------------

        fishingFloat.SetActive(true);


        // --------------------------------------------------
        // 鱼漂放到鱼竿尖端
        // --------------------------------------------------

        fishingFloat.transform.position =
            castPoint.position;


        // --------------------------------------------------
        // 获取 Rigidbody
        // --------------------------------------------------

        Rigidbody floatRb =
            fishingFloat.GetComponent<Rigidbody>();


        if (floatRb == null)
        {
            Debug.LogError(
                "Fishingrodmanager：FishingFloat 没有 Rigidbody！"
            );

            isCasting = false;
            yield break;
        }


        // --------------------------------------------------
        // 重置物理
        // --------------------------------------------------

        floatRb.isKinematic = false;

        floatRb.useGravity = true;

        floatRb.constraints =
            RigidbodyConstraints.None;

        floatRb.velocity =
            Vector3.zero;

        floatRb.angularVelocity =
            Vector3.zero;


        // --------------------------------------------------
        // 计算抛竿方向
        // --------------------------------------------------

        Vector3 castDirection =
            castPoint.forward * forwardForce
            +
            Vector3.up * upwardForce;


        // --------------------------------------------------
        // 抛出去
        // --------------------------------------------------

        floatRb.AddForce(
            castDirection,
            ForceMode.VelocityChange
        );


        // --------------------------------------------------
        // 抛竿完成
        // --------------------------------------------------

        isCasting = false;
    }


    // ==================================================
    // 鱼漂 / 鱼竿碰到 Ground
    // ==================================================

    private void OnCollisionEnter(
        Collision collision
    )
    {
        if (
            collision.gameObject.CompareTag("Ground")
        )
        {
            if (fishingFloat != null)
            {
                fishingFloat.SetActive(false);
            }
        }
    }
}