using System.Collections;
using UnityEngine;

public class Fishingrodmanager : MonoBehaviour
{
    public GameObject fishingFloat;

    public Transform castPoint;

    public float forwardForce = 8f;
    public float upwardForce = 3f;

    // 抛竿动画
    public AnimationClip castAnimation;

    // 钓鱼小游戏
    public FishingMinigame fishingMinigame;

    private Animator animator;

    // 防止抛竿动画期间再次按 E
    private bool isCasting = false;


    void Start()
    {
        animator = GetComponent<Animator>();

        // 游戏开始时隐藏鱼漂
        fishingFloat.SetActive(false);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isCasting)
        {
            // ==================================================
            // 如果正在玩钓鱼小游戏
            // 按 E = 取消当前小游戏 + 重新抛竿
            // ==================================================

            if (FishingMinigame.IsPlaying)
            {
                if (fishingMinigame != null)
                {
                    fishingMinigame.CancelGame();
                }
            }

            // ==================================================
            // 重新抛竿
            // ==================================================

            StartCoroutine(CastFishingRod());
        }
    }


    IEnumerator CastFishingRod()
    {
        // ==================================================
        // 一按 E 立刻锁住
        // 防止动画播放期间再次按 E
        // ==================================================

        isCasting = true;


        // ==================================================
        // 关闭旧鱼漂
        // ==================================================

        fishingFloat.SetActive(false);


        // ==================================================
        // 1. 播放抛竿动画
        // ==================================================

        animator.enabled = true;

        animator.Play(
            "抛竿",
            0,
            0
        );


        // ==================================================
        // 2. 等待抛竿动画结束
        // ==================================================

        yield return new WaitForSeconds(
            castAnimation.length
        );


        // ==================================================
        // 3. 鱼漂出现
        // ==================================================

        fishingFloat.SetActive(true);


        // ==================================================
        // 4. 鱼漂放到鱼竿尖端
        // ==================================================

        fishingFloat.transform.position =
            castPoint.position;


        // ==================================================
        // 5. 获取 Rigidbody
        // ==================================================

        Rigidbody floatRb =
            fishingFloat.GetComponent<Rigidbody>();


        // ==================================================
        // 6. 重置物理状态
        // ==================================================

        floatRb.isKinematic = false;

        floatRb.useGravity = true;

        floatRb.constraints =
            RigidbodyConstraints.None;

        floatRb.velocity =
            Vector3.zero;

        floatRb.angularVelocity =
            Vector3.zero;


        // ==================================================
        // 7. 抛出去
        // ==================================================

        Vector3 castDirection =
            castPoint.forward * forwardForce
            +
            Vector3.up * upwardForce;


        floatRb.AddForce(
            castDirection,
            ForceMode.VelocityChange
        );


        // ==================================================
        // 8. 抛竿完成
        // ==================================================

        isCasting = false;
    }


    // ==================================================
    // 鱼漂碰到地面
    // ==================================================

    private void OnCollisionEnter(
        Collision collision
    )
    {
        if (
            collision.gameObject.CompareTag("Ground")
        )
        {
            fishingFloat.SetActive(false);
        }
    }
}