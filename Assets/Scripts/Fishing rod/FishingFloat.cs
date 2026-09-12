using UnityEngine;

public class FishingFloat : MonoBehaviour
{
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 撞到地面
        if (collision.gameObject.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }

        // 撞到水面
        if (collision.gameObject.CompareTag("Water"))
        {
            // 停止运动
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 关闭重力
            rb.useGravity = false;

            // 让鱼漂停止旋转
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // 获取碰撞点
            ContactPoint contact = collision.contacts[0];

            // 把鱼漂放到碰撞位置
            transform.position = new Vector3(
                transform.position.x,
                contact.point.y,
                transform.position.z
            );
        }
    }
}