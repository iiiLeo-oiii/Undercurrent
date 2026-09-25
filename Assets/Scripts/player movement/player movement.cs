using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 5f;

    [Header("疾跑")]
    public float sprintSpeed = 9f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("体力")]
    public float maxStamina = 100f;
    public float staminaDrain = 20f;
    public float staminaRecovery = 15f;

    [Header("体力条 UI")]
    public GameObject staminaUI;
    public Slider staminaSlider;

    [Header("跳跃")]
    public float jumpForce = 7f;

    [Header("视角")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    [Header("下蹲")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchCameraHeight = 0.6f;
    public float crouchColliderHeight = 1.2f;
    public float crouchSpeed = 2.5f;

    [Header("下蹲视角")]
    public float cameraMoveSpeed = 8f;

    [Header("钓鱼距离")]
    public GameObject fishingFloat;
    public FishingMinigame fishingMinigame;
    public float maxFishingDistance = 30f;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    private Vector3 normalCameraPosition;
    private float normalColliderHeight;
    private Vector3 normalColliderCenter;

    private float currentStamina;
    private float xRotation;

    private bool isSprinting;
    private bool isCrouching;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        normalCameraPosition = cameraTransform.localPosition;

        normalColliderHeight = capsuleCollider.height;
        normalColliderCenter = capsuleCollider.center;

        currentStamina = maxStamina;

        if (staminaUI != null)
            staminaUI.SetActive(false);

        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
            staminaSlider.gameObject.SetActive(false);
        }

        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleCrouch();
        HandleSprint();
        HandleStamina();
        HandleCamera();
        CheckFishingFloatDistance();

        // 跳跃
        if (Input.GetKeyDown(KeyCode.Space) &&
            isGrounded &&
            !isCrouching &&
            !FishingMinigame.IsPlaying)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // =========================================================
    // 移动
    // =========================================================

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move =
            transform.right * horizontal +
            transform.forward * vertical;

        move.Normalize();

        float speed = moveSpeed;

        if (isCrouching)
            speed = crouchSpeed;
        else if (isSprinting)
            speed = sprintSpeed;

        Vector3 desiredVelocity =
            move * speed;

        // 防止撞墙后黏住
        if (desiredVelocity.sqrMagnitude > 0.001f)
        {
            Vector3 direction =
                desiredVelocity.normalized;

            if (Physics.CapsuleCast(
                GetCapsuleBottom(),
                GetCapsuleTop(),
                capsuleCollider.radius * 0.95f,
                direction,
                out RaycastHit hit,
                0.15f,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject != gameObject)
                {
                    // 沿着墙面滑动
                    desiredVelocity =
                        Vector3.ProjectOnPlane(
                            desiredVelocity,
                            hit.normal
                        );
                }
            }
        }

        rb.velocity = new Vector3(
            desiredVelocity.x,
            rb.velocity.y,
            desiredVelocity.z
        );
    }

    // =========================================================
    // 地面检测
    // =========================================================

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == gameObject)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact =
                collision.GetContact(i);

            // 只有表面朝上的碰撞才算地面
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    // =========================================================
    // 跳跃
    // =========================================================

    void Jump()
    {
        isGrounded = false;

        Vector3 velocity = rb.velocity;

        // 清除向下速度
        if (velocity.y < 0f)
            velocity.y = 0f;

        rb.velocity = velocity;

        rb.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );
    }

    // =========================================================
    // Capsule
    // =========================================================

    Vector3 GetCapsuleBottom()
    {
        Vector3 center =
            transform.TransformPoint(
                capsuleCollider.center
            );

        float halfHeight =
            Mathf.Max(
                0f,
                capsuleCollider.height / 2f -
                capsuleCollider.radius
            );

        return center -
               transform.up * halfHeight;
    }

    Vector3 GetCapsuleTop()
    {
        Vector3 center =
            transform.TransformPoint(
                capsuleCollider.center
            );

        float halfHeight =
            Mathf.Max(
                0f,
                capsuleCollider.height / 2f -
                capsuleCollider.radius
            );

        return center +
               transform.up * halfHeight;
    }

    // =========================================================
    // 鼠标
    // =========================================================

    void HandleMouseLook()
    {
        if (cameraTransform == null)
            return;

        float mouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity;

        xRotation -= mouseY;

        xRotation =
            Mathf.Clamp(
                xRotation,
                -90f,
                90f
            );

        cameraTransform.localRotation =
            Quaternion.Euler(
                xRotation,
                0f,
                0f
            );

        transform.Rotate(
            Vector3.up * mouseX
        );
    }

    // =========================================================
    // 下蹲
    // =========================================================

    void HandleCrouch()
    {
        if (FishingMinigame.IsPlaying)
            return;

        bool crouching =
            Input.GetKey(crouchKey);

        if (crouching)
        {
            isCrouching = true;
            isSprinting = false;

            capsuleCollider.height =
                crouchColliderHeight;

            float centerY =
                normalColliderCenter.y -
                (normalColliderHeight -
                crouchColliderHeight) / 2f;

            capsuleCollider.center =
                new Vector3(
                    normalColliderCenter.x,
                    centerY,
                    normalColliderCenter.z
                );
        }
        else
        {
            isCrouching = false;

            capsuleCollider.height =
                normalColliderHeight;

            capsuleCollider.center =
                normalColliderCenter;
        }
    }

    // =========================================================
    // 疾跑
    // =========================================================

    void HandleSprint()
    {
        bool holdingSprint =
            Input.GetKey(sprintKey);

        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float vertical =
            Input.GetAxisRaw("Vertical");

        bool moving =
            Mathf.Abs(horizontal) > 0.01f ||
            Mathf.Abs(vertical) > 0.01f;

        isSprinting =
            holdingSprint &&
            moving &&
            !isCrouching &&
            !FishingMinigame.IsPlaying &&
            currentStamina > 0f;

        if (currentStamina <= 0f)
            isSprinting = false;
    }

    // =========================================================
    // 体力
    // =========================================================

    void HandleStamina()
    {
        if (isSprinting)
        {
            currentStamina -=
                staminaDrain *
                Time.deltaTime;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isSprinting = false;
            }
        }
        else
        {
            currentStamina +=
                staminaRecovery *
                Time.deltaTime;
        }

        currentStamina =
            Mathf.Clamp(
                currentStamina,
                0f,
                maxStamina
            );

        if (staminaSlider != null)
        {
            staminaSlider.maxValue =
                maxStamina;

            staminaSlider.value =
                currentStamina;

            staminaSlider.gameObject.SetActive(
                isSprinting
            );
        }

        if (staminaUI != null)
        {
            staminaUI.SetActive(
                isSprinting
            );
        }
    }

    // =========================================================
    // 摄像机
    // =========================================================

    void HandleCamera()
    {
        if (cameraTransform == null)
            return;

        Vector3 target =
            normalCameraPosition;

        if (isCrouching)
        {
            target =
                normalCameraPosition +
                Vector3.down *
                crouchCameraHeight;
        }

        cameraTransform.localPosition =
            Vector3.Lerp(
                cameraTransform.localPosition,
                target,
                cameraMoveSpeed *
                Time.deltaTime
            );
    }

    // =========================================================
    // 钓鱼距离
    // =========================================================

    void CheckFishingFloatDistance()
    {
        if (fishingFloat == null)
            return;

        if (!fishingFloat.activeSelf)
            return;

        Vector3 playerPosition =
            transform.position;

        Vector3 floatPosition =
            fishingFloat.transform.position;

        playerPosition.y = 0f;
        floatPosition.y = 0f;

        float distance =
            Vector3.Distance(
                playerPosition,
                floatPosition
            );

        if (distance >= maxFishingDistance)
        {
            if (FishingMinigame.IsPlaying &&
                fishingMinigame != null)
            {
                fishingMinigame.CancelGame();
            }

            fishingFloat.SetActive(false);
        }
    }
}