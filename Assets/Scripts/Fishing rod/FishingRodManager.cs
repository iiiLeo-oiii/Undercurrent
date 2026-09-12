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
        isCasting = true;

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
        // 3. 动画结束后，鱼漂出现
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


        // 清除之前的速度
        floatRb.velocity = Vector3.zero;
        floatRb.angularVelocity = Vector3.zero;


        // =========================
        // 6. 把鱼漂抛出去
        // =========================

        Vector3 castDirection =
            castPoint.forward * forwardForce
            + Vector3.up * upwardForce;

        floatRb.AddForce(
            castDirection,
            ForceMode.VelocityChange
        );


        // 抛竿完成
        isCasting = false;
    }


    // =========================
    // 鱼漂碰到地面
    // =========================

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            fishingFloat.SetActive(false);
        }
    }
}