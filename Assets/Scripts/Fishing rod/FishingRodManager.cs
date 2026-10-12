using System.Collections;
using UnityEngine;

public class Fishingrodmanager : MonoBehaviour
{
    [Header("实际钓鱼鱼漂")]
    public GameObject fishingFloat;

    [Header("装饰鱼漂：整个装饰鱼漂的根物体")]
    public Transform decorativeFloat;

    [Header("视角")]
    public Transform viewTransform;

    [Header("Cast Point")]
    public Transform castPoint;

    [Header("鱼线")]
    public LineRenderer fishingLine;
    public Transform decorativeLinePoint;
    public Transform fishingFloatLinePoint;

    [Header("装饰鱼漂惯性")]
    public float rotationInertia = 0.015f;
    public float movementInertia = 1.5f;
    public float inertiaReturnSpeed = 8f;
    public float maxInertiaOffset = 0.12f;

    [Header("Cast Force")]
    public float forwardForce = 8f;
    public float upwardForce = 3f;

    [Header("Cast Animation")]
    public AnimationClip castAnimation;

    [Header("Fishing Minigame")]
    public FishingMinigame fishingMinigame;

    [Header("成功钓鱼后的收回动画")]
    public float returnDuration = 1.2f;
    public float returnArcHeight = 1.5f;

    private Animator animator;
    private bool isCasting = false;
    private bool isFishingCastActive = false;
    private bool isReturning = false;

    public bool hasRod = false;

    private Vector3 decorativeStartPosition;
    private Quaternion decorativeStartRotation;
    private bool decorativePositionSaved = false;

    private bool hideDecorativeDuringCast = false;

    private Vector3 currentInertiaOffset;
    private Vector3 inertiaVelocity;

    private Vector3 previousViewPosition;
    private Quaternion previousViewRotation;
    private bool previousViewSaved = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (fishingFloat != null)
        {
            fishingFloat.SetActive(false);
        }
        else
        {
            Debug.LogError("Fishingrodmanager：没有设置实际 Fishing Float！");
        }

        if (decorativeFloat != null)
        {
            decorativeStartPosition = decorativeFloat.localPosition;
            decorativeStartRotation = decorativeFloat.localRotation;
            decorativePositionSaved = true;

            decorativeFloat.gameObject.SetActive(
                hasRod && !hideDecorativeDuringCast
            );
        }
        else
        {
            Debug.LogError("Fishingrodmanager：没有设置 Decorative Float！");
        }

        if (viewTransform == null && Camera.main != null)
        {
            viewTransform = Camera.main.transform;
        }

        SaveViewTransform();

        if (fishingLine != null)
        {
            fishingLine.useWorldSpace = true;
            fishingLine.positionCount = 2;
            fishingLine.enabled = hasRod;
        }
    }

    void SaveViewTransform()
    {
        if (viewTransform == null)
        {
            previousViewSaved = false;
            return;
        }

        previousViewPosition = viewTransform.position;
        previousViewRotation = viewTransform.rotation;
        previousViewSaved = true;
    }

    void LateUpdate()
    {
        UpdateDecorativeFloat();
        UpdateFishingLine();

        if (isFishingCastActive &&
            !isCasting &&
            !isReturning &&
            fishingFloat != null &&
            !fishingFloat.activeSelf)
        {
            FinishFishingCast();
        }
    }

    void UpdateDecorativeFloat()
    {
        if (decorativeFloat == null)
            return;

        bool shouldShow = hasRod && !hideDecorativeDuringCast;

        if (decorativeFloat.gameObject.activeSelf != shouldShow)
            decorativeFloat.gameObject.SetActive(shouldShow);

        if (!shouldShow || !decorativePositionSaved)
        {
            SaveViewTransform();
            return;
        }

        Vector3 targetOffset = Vector3.zero;

        if (viewTransform != null)
        {
            if (!previousViewSaved)
            {
                SaveViewTransform();
            }
            else
            {
                float yawDelta = Mathf.DeltaAngle(
                    previousViewRotation.eulerAngles.y,
                    viewTransform.rotation.eulerAngles.y
                );

                float pitchDelta = Mathf.DeltaAngle(
                    previousViewRotation.eulerAngles.x,
                    viewTransform.rotation.eulerAngles.x
                );

                targetOffset.x -= yawDelta * rotationInertia;
                targetOffset.y += pitchDelta * rotationInertia;

                Vector3 worldMovement =
                    viewTransform.position - previousViewPosition;

                Vector3 localMovement =
                    viewTransform.InverseTransformVector(worldMovement);

                targetOffset.x -= localMovement.x * movementInertia;
                targetOffset.y -= localMovement.y * movementInertia;

                previousViewPosition = viewTransform.position;
                previousViewRotation = viewTransform.rotation;
            }
        }

        targetOffset = Vector3.ClampMagnitude(
            targetOffset,
            maxInertiaOffset
        );

        currentInertiaOffset = Vector3.SmoothDamp(
            currentInertiaOffset,
            targetOffset,
            ref inertiaVelocity,
            1f / Mathf.Max(0.01f, inertiaReturnSpeed)
        );

        decorativeFloat.localPosition =
            decorativeStartPosition + currentInertiaOffset;

        decorativeFloat.localRotation = decorativeStartRotation;
    }

    void Update()
    {
        if (!hasRod || isCasting || isReturning)
            return;

        // E 键：优先判断是否正在钓鱼，确保等待鱼咬钩时也能退出
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 情况一：正在进行钓鱼小游戏
            if (FishingMinigame.IsPlaying)
            {
                if (fishingMinigame != null)
                {
                    fishingMinigame.CancelGame();
                }

                FinishFishingCast();

                Debug.Log("已退出钓鱼小游戏。");
                return;
            }

            // 情况二：鱼漂已经抛出，正在等待鱼咬钩
            if (isFishingCastActive)
            {
                FinishFishingCast();

                Debug.Log("已取消钓鱼等待。");
                return;
            }

            // 情况三：当前没有进行钓鱼，开始抛竿
            StartCoroutine(CastFishingRod());
        }
    }

    void UpdateFishingLine()
    {
        if (fishingLine == null)
            return;

        bool shouldShowLine = hasRod;

        if (fishingLine.enabled != shouldShowLine)
            fishingLine.enabled = shouldShowLine;

        if (!shouldShowLine || castPoint == null)
            return;

        Transform endPoint = null;

        if (isFishingCastActive &&
            fishingFloat != null &&
            fishingFloat.activeSelf)
        {
            endPoint = fishingFloatLinePoint != null
                ? fishingFloatLinePoint
                : fishingFloat.transform;
        }
        else if (!hideDecorativeDuringCast)
        {
            endPoint = decorativeLinePoint != null
                ? decorativeLinePoint
                : decorativeFloat;
        }

        if (isCasting || endPoint == null)
        {
            fishingLine.enabled = false;
            return;
        }

        fishingLine.enabled = true;
        fishingLine.positionCount = 2;
        fishingLine.SetPosition(0, castPoint.position);
        fishingLine.SetPosition(1, endPoint.position);
    }

    IEnumerator CastFishingRod()
    {
        isCasting = true;

        if (fishingFloat == null ||
            castPoint == null ||
            animator == null ||
            castAnimation == null)
        {
            Debug.LogError(
                "Fishingrodmanager：请检查 Fishing Float、Cast Point、Animator 和 Cast Animation。"
            );

            isCasting = false;
            yield break;
        }

        hideDecorativeDuringCast = false;

        fishingFloat.SetActive(false);
        isFishingCastActive = false;

        animator.enabled = true;
        animator.Play("抛竿", 0, 0f);

        yield return new WaitForSeconds(castAnimation.length);

        hideDecorativeDuringCast = true;

        if (decorativeFloat != null)
            decorativeFloat.gameObject.SetActive(false);

        if (!hasRod)
        {
            isCasting = false;
            FinishFishingCast();
            yield break;
        }

        fishingFloat.transform.position = castPoint.position;

        FishingFloat floatScript =
            fishingFloat.GetComponent<FishingFloat>();

        if (floatScript != null)
            floatScript.RestoreUprightRotation();

        fishingFloat.SetActive(true);

        Rigidbody floatRb = fishingFloat.GetComponent<Rigidbody>();

        if (floatRb == null)
        {
            Debug.LogError("Fishingrodmanager：实际鱼漂没有 Rigidbody！");

            fishingFloat.SetActive(false);
            isCasting = false;
            FinishFishingCast();
            yield break;
        }

        floatRb.velocity = Vector3.zero;
        floatRb.angularVelocity = Vector3.zero;
        floatRb.constraints = RigidbodyConstraints.FreezeRotation;
        floatRb.isKinematic = false;
        floatRb.useGravity = true;

        Vector3 castDirection =
            castPoint.forward * forwardForce +
            Vector3.up * upwardForce;

        floatRb.AddForce(castDirection, ForceMode.VelocityChange);

        isFishingCastActive = true;
        isCasting = false;

        UpdateFishingLine();
    }

    // 钓鱼成功时，由 FishingMinigame 调用
    public void StartSuccessfulReturn(
        AfterFishHoldingManager fishManager)
    {
        if (isReturning || fishingFloat == null)
            return;

        StartCoroutine(
            SuccessfulReturnCoroutine(fishManager)
        );
    }

    IEnumerator SuccessfulReturnCoroutine(
        AfterFishHoldingManager fishManager)
    {
        isReturning = true;

        FishingFloat floatScript =
            fishingFloat.GetComponent<FishingFloat>();

        if (floatScript != null)
            floatScript.ResetForNextCast();

        Rigidbody rb = fishingFloat.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        Vector3 floatStart = fishingFloat.transform.position;

        Vector3 floatEnd = castPoint != null
            ? castPoint.position
            : transform.position;

        GameObject fish = null;

        if (fishManager != null)
        {
            Vector3 fishStart =
                floatStart + Vector3.up * 0.15f;

            fish = fishManager.CreateFishForReturn(fishStart);
        }
        else
        {
            Debug.LogError(
                "Fishingrodmanager：没有设置 AfterFishHoldingManager！"
            );
        }

        Vector3 fishStartPosition = fish != null
            ? fish.transform.position
            : Vector3.zero;

        Vector3 fishEndPosition = floatEnd;

        if (castPoint != null)
            fishEndPosition += castPoint.right * 0.2f;

        float duration = Mathf.Max(0.05f, returnDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float arc = Mathf.Sin(t * Mathf.PI) * returnArcHeight;

            fishingFloat.transform.position =
                Vector3.Lerp(floatStart, floatEnd, t)
                + Vector3.up * arc;

            if (fish != null)
            {
                fish.transform.position =
                    Vector3.Lerp(fishStartPosition, fishEndPosition, t)
                    + Vector3.up * arc;
            }

            yield return null;
        }

        fishingFloat.transform.position = floatEnd;

        if (fish != null && fishManager != null)
            fishManager.FinishFishReturn(fish);

        FinishFishingCast();
    }

    // 结束本次钓鱼
    public void FinishFishingCast()
    {
        isFishingCastActive = false;
        isReturning = false;
        hideDecorativeDuringCast = false;

        if (fishingFloat != null)
        {
            FishingFloat floatScript =
                fishingFloat.GetComponent<FishingFloat>();

            if (floatScript != null)
                floatScript.ResetForNextCast();

            fishingFloat.SetActive(false);
        }

        currentInertiaOffset = Vector3.zero;
        inertiaVelocity = Vector3.zero;

        if (decorativeFloat != null)
        {
            decorativeFloat.localPosition = decorativeStartPosition;
            decorativeFloat.localRotation = decorativeStartRotation;
            decorativeFloat.gameObject.SetActive(hasRod);
        }

        SaveViewTransform();
        UpdateFishingLine();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (isFishingCastActive && !isReturning)
                FinishFishingCast();
            else if (!isReturning && fishingFloat != null)
                fishingFloat.SetActive(false);
        }
    }
}