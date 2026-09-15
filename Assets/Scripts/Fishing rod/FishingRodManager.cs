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

    private Animator animator;

    // 防止动画播放期间再次按E
    private bool isCasting = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 游戏开始时隐藏鱼漂
        fishingFloat.SetActive(false);
    }

    void Update()
    {
        // 只能在没有抛竿的时候按E
        if (Input.GetKeyDown(KeyCode.E) && !isCasting)
        {
            StartCoroutine(CastFishingRod());
        }
    }

    IEnumerator CastFishingRod()
    {
        // 一按E立刻锁住
        isCasting = true;

        // ==================================================
        // 重要：
        // 如果上一次的鱼漂还在场景里，先把它关闭
        // 这样下一次 SetActive(true) 时会重新触发 OnEnable()
        // ==================================================

        fishingFloat.SetActive(false);

        // =========================
        // 1. 播放抛竿动画
        // =========================

        animator.enabled = true;

        animator.Play("抛竿", 0, 0);

        // =========================
        // 2. 等待抛竿动画结束
        // =========================

        yield return new WaitForSeconds(castAnimation.length);

        // =========================
        // 3. 鱼漂出现
        // =========================

        fishingFloat.SetActive(true);

        // =========================
        // 4. 鱼漂放到鱼竿尖端
        // =========================

        fishingFloat.transform.position = castPoint.position;

        // =========================
        // 5. 获取鱼漂 Rigidbody
        // =========================

        Rigidbody floatRb = fishingFloat.GetComponent<Rigidbody>();

        // =========================
        // 6. 确保物理状态正常
        // =========================

        floatRb.isKinematic = false;
        floatRb.useGravity = true;
        floatRb.constraints = RigidbodyConstraints.None;

        floatRb.velocity = Vector3.zero;
        floatRb.angularVelocity = Vector3.zero;

        // =========================
        // 7. 抛出去
        // =========================

        Vector3 castDirection =
            castPoint.forward * forwardForce
            + Vector3.up * upwardForce;

        floatRb.AddForce(
            castDirection,
            ForceMode.VelocityChange
        );

        // =========================
        // 8. 抛竿完成
        // =========================

        isCasting = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            fishingFloat.SetActive(false);
        }
    }
}