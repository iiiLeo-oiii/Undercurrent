using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 5f;

    [Header("跳跃")]
    public float jumpForce = 7f;

    [Header("视角")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    private Rigidbody rb;

    private bool isGrounded;

    private float cameraVerticalRotation;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        // =====================
        // 鼠标视角
        // =====================

        float mouseX =
            Input.GetAxis("Mouse X")
            * mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y")
            * mouseSensitivity;


        // 左右旋转玩家
        transform.Rotate(
            Vector3.up * mouseX
        );


        // 上下旋转相机
        cameraVerticalRotation -= mouseY;

        cameraVerticalRotation =
            Mathf.Clamp(
                cameraVerticalRotation,
                -80f,
                80f
            );

        cameraTransform.localRotation =
            Quaternion.Euler(
                cameraVerticalRotation,
                0f,
                0f
            );


        // =====================
        // 跳跃
        // =====================

        if (
            Input.GetKeyDown(KeyCode.Space)
            && isGrounded
        )
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );
        }
    }


    void FixedUpdate()
    {
        // =====================
        // WASD移动
        // =====================

        float x =
            Input.GetAxisRaw("Horizontal");

        float z =
            Input.GetAxisRaw("Vertical");


        Vector3 movement =
            transform.right * x +
            transform.forward * z;

        movement.Normalize();


        Vector3 velocity =
            rb.velocity;

        velocity.x =
            movement.x * moveSpeed;

        velocity.z =
            movement.z * moveSpeed;

        rb.velocity =
            velocity;
    }


    void OnCollisionStay(Collision collision)
    {
        foreach (
            ContactPoint contact
            in collision.contacts
        )
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;

                return;
            }
        }

        isGrounded = false;
    }


    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}