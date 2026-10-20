using UnityEngine;

public class PlayerHeadBob : MonoBehaviour
{
    [Header("引用")]
    public PlayerController playerController;
    public Transform cameraBobPivot;

    [Header("普通走路")]
    public float walkFrequency = 1.8f;
    public float walkAmplitude = 0.025f;

    [Header("疾跑")]
    public float sprintFrequency = 2.8f;
    public float sprintAmplitude = 0.04f;

    [Header("下蹲")]
    public float crouchFrequency = 1.5f;
    public float crouchAmplitude = 0.01f;

    [Header("平滑")]
    public float bobSmoothSpeed = 8f;
    public float returnSmoothSpeed = 5f;

    private Vector3 originalPivotPosition;
    private float bobTimer;
    private float currentOffset;

    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (playerController == null || cameraBobPivot == null)
        {
            Debug.LogError(
                "PlayerHeadBob：请指定 PlayerController 和 CameraBobPivot！",
                this
            );

            enabled = false;
            return;
        }

        originalPivotPosition = cameraBobPivot.localPosition;
    }

    void LateUpdate()
    {
        if (playerController == null || cameraBobPivot == null)
            return;

        // 对话或钓鱼小游戏期间，平滑停止晃动
        if (playerController.dialogueLocked ||
            FishingMinigame.IsPlaying)
        {
            ReturnToCenter();
            ApplyOffset();
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool hasInput =
            Mathf.Abs(horizontal) > 0.01f ||
            Mathf.Abs(vertical) > 0.01f;

        if (!hasInput)
        {
            ReturnToCenter();
            ApplyOffset();
            return;
        }

        bool crouching =
            Input.GetKey(playerController.crouchKey);

        bool sprinting =
            Input.GetKey(playerController.sprintKey) &&
            !crouching;

        float frequency = walkFrequency;
        float amplitude = walkAmplitude;

        if (sprinting)
        {
            frequency = sprintFrequency;
            amplitude = sprintAmplitude;
        }
        else if (crouching)
        {
            frequency = crouchFrequency;
            amplitude = crouchAmplitude;
        }

        bobTimer += Time.deltaTime * frequency * Mathf.PI * 2f;

        float targetOffset =
            Mathf.Sin(bobTimer) * amplitude;

        currentOffset = Mathf.Lerp(
            currentOffset,
            targetOffset,
            bobSmoothSpeed * Time.deltaTime
        );

        ApplyOffset();
    }

    void ReturnToCenter()
    {
        currentOffset = Mathf.Lerp(
            currentOffset,
            0f,
            returnSmoothSpeed * Time.deltaTime
        );
    }

    void ApplyOffset()
    {
        // 每帧根据固定基准计算，不会不断累加位置
        cameraBobPivot.localPosition =
            originalPivotPosition +
            Vector3.up * currentOffset;
    }

    void OnDisable()
    {
        // 禁用脚本时恢复枢轴位置
        if (cameraBobPivot != null)
        {
            cameraBobPivot.localPosition = originalPivotPosition;
        }
    }
}