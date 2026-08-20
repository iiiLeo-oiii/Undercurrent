using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    private Rigidbody rb;
    private bool isGrounded = false;

    private float cameraVerticalRotation = 0f;

    // 当前是否碰到墙
    private bool touchingWall = false;
    private Vector3 wallNormal;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        // =========================
        // 鼠标控制视角
        // =========================

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 左右旋转玩家
        transform.Rotate(Vector3.up * mouseX);

        // 上下旋转Camera
        cameraVerticalRotation -= mouseY;

        cameraVerticalRotation =
            Mathf.Clamp(cameraVerticalRotation, -90f, 90f);

        cameraTransform.localRotation =
            Quaternion.Euler(
                cameraVerticalRotation,
                0f,
                0f
            );


        // =========================
        // 跳跃
        // =========================

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );

            isGrounded = false;
        }
    }


    void FixedUpdate()
    {
        // =========================
        // WASD
        // =========================

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 movement =
            transform.right * x +
            transform.forward * z;

        movement.Normalize();


        // =========================
        // 如果撞到墙
        // =========================

        if (touchingWall)
        {
            // 如果玩家正在往墙里面走，
            // 就把这部分速度取消
            float directionIntoWall =
                Vector3.Dot(movement, -wallNormal);

            if (directionIntoWall > 0)
            {
                movement =
                    Vector3.ProjectOnPlane(
                        movement,
                        wallNormal
                    );
            }
        }


        // =========================
        // 设置速度
        // =========================

        Vector3 velocity = rb.velocity;

        velocity.x = movement.x * moveSpeed;
        velocity.z = movement.z * moveSpeed;

        rb.velocity = velocity;
    }


    void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision);
    }


    void OnCollisionStay(Collision collision)
    {
        CheckCollision(collision);
    }


    void OnCollisionExit(Collision collision)
    {
        touchingWall = false;
    }


    void CheckCollision(Collision collision)
    {
        isGrounded = false;
        touchingWall = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 normal = contact.normal;

            // 地面
            if (normal.y > 0.5f)
            {
                isGrounded = true;
            }

            // 墙
            if (Mathf.Abs(normal.y) < 0.5f)
            {
                touchingWall = true;
                wallNormal = normal;
            }
        }
    }
}
