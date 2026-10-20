using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("第一段 Text")]
    public TMP_Text dialogueText;

    [Header("原始黑幕")]
    public GameObject black;

    [Header("第二段 Text")]
    public TMP_Text text2;

    [Header("玩家")]
    public PlayerController playerController;

    [Header("摄像机起身效果")]
    public float cameraLowerDistance = 1f;
    public float riseDuration = 2f;

    [Header("上下眼皮 UI")]
    public RectTransform topEyelid;
    public RectTransform bottomEyelid;

    [Range(0.05f, 0.4f)]
    [Tooltip("半睁时屏幕中央露出的空隙比例")]
    public float partialOpenRatio = 0.16f;

    [Range(1, 8)]
    [Tooltip("眨眼次数")]
    public int blinkCount = 2;

    public float blinkCloseDuration = 0.10f;
    public float blinkOpenDuration = 0.12f;
    public float closedHoldDuration = 0.05f;
    public float blinkPauseDuration = 0.25f;

    [Header("起身时的轻微晃动")]
    public float leftTiltAngle = 1.8f;
    public float rightTiltAngle = 1.3f;
    public float downwardPitchAngle = 1f;

    [Header("第一段对话")]
    public List<string> dialogueList = new List<string>()
    {
        "1", "2", "3", "4", "5", "6"
    };

    [Header("第二段对话")]
    public List<string> dialogueList2 = new List<string>()
    {
        "第二段1", "第二段2", "第二段3", "第二段4"
    };

    [Header("淡出时间")]
    public float fadeDuration = 1f;

    private int currentIndex = 0;
    private int currentIndex2 = 0;

    private bool firstDialogueActive = true;
    private bool secondDialogueActive = false;
    private bool dialogueFinished = false;
    private bool firstDialogueFinishing = false;

    private CanvasGroup blackGroup;
    private CanvasGroup text2Group;

    private Transform cameraTransform;

    private Vector3 normalCameraPosition;
    private Vector3 lowCameraPosition;
    private Quaternion normalCameraRotation;

    private RectTransform eyelidCanvasRect;
    private float canvasWidth;
    private float canvasHeight;

    // =========================================================
    // 初始化
    // =========================================================

    void Start()
    {
        if (black != null)
        {
            blackGroup = black.GetComponent<CanvasGroup>();

            if (blackGroup == null)
            {
                Debug.LogError(
                    "DialogueManager：原始黑幕缺少 CanvasGroup 组件。"
                );
            }
        }

        if (text2 != null)
        {
            text2Group = text2.GetComponent<CanvasGroup>();
        }

        if (playerController != null)
        {
            cameraTransform = playerController.cameraTransform;

            if (cameraTransform != null)
            {
                normalCameraPosition = cameraTransform.localPosition;
                normalCameraRotation = cameraTransform.localRotation;

                lowCameraPosition = normalCameraPosition
                    + Vector3.down * cameraLowerDistance;

                StartCoroutine(InitializeCameraLowPosition());
            }
            else
            {
                Debug.LogError(
                    "DialogueManager：PlayerController 的 Camera Transform 未设置。"
                );
            }
        }
        else
        {
            Debug.LogError(
                "DialogueManager：请在 Inspector 设置 Player Controller。"
            );
        }

        if (black != null)
        {
            black.SetActive(true);

            if (blackGroup != null)
            {
                blackGroup.alpha = 1f;
            }
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
        }

        if (text2 != null)
        {
            text2.gameObject.SetActive(false);

            if (text2Group != null)
            {
                text2Group.alpha = 0f;
            }
        }

        InitializeEyelids();

        if (dialogueList.Count > 0)
        {
            currentIndex = 0;

            if (dialogueText != null)
            {
                dialogueText.text = dialogueList[0];
            }
        }
        else
        {
            firstDialogueActive = false;
            firstDialogueFinishing = true;
            StartCoroutine(FinishFirstDialogue());
        }

        if (playerController != null)
        {
            playerController.dialogueLocked = true;
        }
    }

    IEnumerator InitializeCameraLowPosition()
    {
        yield return new WaitForEndOfFrame();

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = lowCameraPosition;
            cameraTransform.localRotation = normalCameraRotation;
        }
    }

    // =========================================================
    // 寻找 Canvas
    // =========================================================

    RectTransform FindEyelidCanvas()
    {
        if (topEyelid != null)
        {
            Canvas canvas = topEyelid.GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                return canvas.rootCanvas.transform as RectTransform;
            }
        }

        if (bottomEyelid != null)
        {
            Canvas canvas = bottomEyelid.GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                return canvas.rootCanvas.transform as RectTransform;
            }
        }

        Canvas[] canvases = FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in canvases)
        {
            if (canvas.isRootCanvas &&
                canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return canvas.transform as RectTransform;
            }
        }

        return null;
    }

    // =========================================================
    // 自动创建眼皮
    // =========================================================

    RectTransform CreateEyelid(string objectName)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        obj.transform.SetParent(eyelidCanvasRect, false);

        Image image = obj.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        return obj.GetComponent<RectTransform>();
    }

    // =========================================================
    // 眼皮布局
    // =========================================================

    void InitializeEyelids()
    {
        Canvas.ForceUpdateCanvases();

        eyelidCanvasRect = FindEyelidCanvas();

        if (eyelidCanvasRect == null)
        {
            Debug.LogError(
                "眼皮初始化失败：没有找到根 Canvas。" +
                "请确认 Canvas 为 Screen Space - Overlay。"
            );

            return;
        }

        canvasWidth = eyelidCanvasRect.rect.width;
        canvasHeight = eyelidCanvasRect.rect.height;

        if (canvasWidth <= 0f || canvasHeight <= 0f)
        {
            Debug.LogError(
                "眼皮初始化失败：Canvas 宽高为 0。"
            );

            return;
        }

        if (topEyelid == null)
        {
            topEyelid = CreateEyelid("TopEyelid_Auto");
        }

        if (bottomEyelid == null)
        {
            bottomEyelid = CreateEyelid("BottomEyelid_Auto");
        }

        if (topEyelid == bottomEyelid)
        {
            Debug.LogError("上下眼皮不能引用同一个 UI 物体！");
            return;
        }

        topEyelid.SetParent(eyelidCanvasRect, false);
        bottomEyelid.SetParent(eyelidCanvasRect, false);

        ConfigureEyelid(topEyelid);
        ConfigureEyelid(bottomEyelid);

        bottomEyelid.SetAsLastSibling();
        topEyelid.SetAsLastSibling();

        topEyelid.gameObject.SetActive(true);
        bottomEyelid.gameObject.SetActive(true);

        // 关键修复：
        // 初始化时直接处于半睁状态，而不是屏幕外。
        SetPartialOpenPosition();

        Debug.Log(
            "眼皮初始化成功。Canvas 尺寸：" +
            canvasWidth + " × " + canvasHeight
        );
    }

    void ConfigureEyelid(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.sizeDelta = new Vector2(
            canvasWidth,
            canvasHeight * 0.5f
        );

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.localPosition = Vector3.zero;

        Image image = rect.GetComponent<Image>();

        if (image == null)
        {
            image = rect.gameObject.AddComponent<Image>();
        }

        image.enabled = true;
        image.color = Color.black;
        image.raycastTarget = false;

        CanvasRenderer renderer =
            rect.GetComponent<CanvasRenderer>();

        if (renderer != null)
        {
            renderer.cullTransparentMesh = false;
            renderer.SetAlpha(1f);
        }

        CanvasGroup group = rect.GetComponent<CanvasGroup>();

        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    // =========================================================
    // 半睁位置
    // =========================================================

    void SetPartialOpenPosition()
    {
        if (topEyelid == null || bottomEyelid == null)
        {
            return;
        }

        float quarterHeight = canvasHeight * 0.25f;
        float halfGap = canvasHeight * partialOpenRatio * 0.5f;

        float topY = quarterHeight + halfGap;
        float bottomY = -quarterHeight - halfGap;

        SetEyelidPositions(topY, bottomY);
    }

    // =========================================================
    // 设置上下眼皮位置
    // =========================================================

    void SetEyelidPositions(float topY, float bottomY)
    {
        if (topEyelid == null || bottomEyelid == null)
        {
            return;
        }

        topEyelid.anchoredPosition = new Vector2(0f, topY);
        bottomEyelid.anchoredPosition = new Vector2(0f, bottomY);
    }

    IEnumerator MoveEyelidsTo(
        float targetTopY,
        float targetBottomY,
        float duration)
    {
        if (topEyelid == null || bottomEyelid == null)
        {
            yield break;
        }

        Vector2 startTop = topEyelid.anchoredPosition;
        Vector2 startBottom = bottomEyelid.anchoredPosition;

        Vector2 endTop = new Vector2(0f, targetTopY);
        Vector2 endBottom = new Vector2(0f, targetBottomY);

        if (duration <= 0f)
        {
            topEyelid.anchoredPosition = endTop;
            bottomEyelid.anchoredPosition = endBottom;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            topEyelid.anchoredPosition = Vector2.Lerp(
                startTop,
                endTop,
                smoothT
            );

            bottomEyelid.anchoredPosition = Vector2.Lerp(
                startBottom,
                endBottom,
                smoothT
            );

            yield return null;
        }

        topEyelid.anchoredPosition = endTop;
        bottomEyelid.anchoredPosition = endBottom;
    }

    // =========================================================
    // 输入
    // =========================================================

    void Update()
    {
        if (dialogueFinished)
        {
            return;
        }

        if (firstDialogueActive &&
            Input.GetMouseButtonDown(0))
        {
            NextFirstDialogue();
        }

        if (secondDialogueActive &&
            Input.GetMouseButtonDown(0))
        {
            NextSecondDialogue();
        }
    }

    // =========================================================
    // 第一段对话
    // =========================================================

    void NextFirstDialogue()
    {
        if (firstDialogueFinishing)
        {
            return;
        }

        currentIndex++;

        if (currentIndex < dialogueList.Count)
        {
            if (dialogueText != null)
            {
                dialogueText.text = dialogueList[currentIndex];
            }
        }
        else
        {
            firstDialogueActive = false;
            firstDialogueFinishing = true;

            StartCoroutine(FinishFirstDialogue());
        }
    }

    // =========================================================
    // 第一段结束
    // =========================================================

    IEnumerator FinishFirstDialogue()
    {
        firstDialogueActive = false;
        firstDialogueFinishing = true;

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
        }

        // 原始黑幕淡出
        if (blackGroup != null)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                blackGroup.alpha = Mathf.Lerp(
                    1f,
                    0f,
                    Mathf.Clamp01(timer / fadeDuration)
                );

                yield return null;
            }

            blackGroup.alpha = 0f;
        }

        if (black != null)
        {
            black.SetActive(false);
        }

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = lowCameraPosition;
            cameraTransform.localRotation = normalCameraRotation;
        }

        if (eyelidCanvasRect != null &&
            topEyelid != null &&
            bottomEyelid != null)
        {
            yield return StartCoroutine(PlayEyelidSequence());
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        if (cameraTransform != null)
        {
            yield return StartCoroutine(PlayStandUpAnimation());
        }

        StartSecondDialogue();
    }

    // =========================================================
    // 眨眼动画：只眨两次
    // =========================================================

    IEnumerator PlayEyelidSequence()
    {
        Canvas.ForceUpdateCanvases();

        canvasWidth = eyelidCanvasRect.rect.width;
        canvasHeight = eyelidCanvasRect.rect.height;

        ConfigureEyelid(topEyelid);
        ConfigureEyelid(bottomEyelid);

        float quarterHeight = canvasHeight * 0.25f;
        float halfGap = canvasHeight * partialOpenRatio * 0.5f;

        float partialTopY = quarterHeight + halfGap;
        float partialBottomY = -quarterHeight - halfGap;

        float closedTopY = quarterHeight;
        float closedBottomY = -quarterHeight;

        // 不播放入场动画，直接显示半睁状态
        SetEyelidPositions(partialTopY, partialBottomY);

        for (int i = 0; i < 2; i++)
        {
            // 闭眼
            yield return StartCoroutine(
                MoveEyelidsTo(
                    closedTopY,
                    closedBottomY,
                    blinkCloseDuration
                )
            );

            yield return new WaitForSeconds(closedHoldDuration);

            // 睁回半睁状态
            yield return StartCoroutine(
                MoveEyelidsTo(
                    partialTopY,
                    partialBottomY,
                    blinkOpenDuration
                )
            );

            if (i == 0)
            {
                yield return new WaitForSeconds(blinkPauseDuration);
            }
        }

        // 最终完全睁眼：上下眼皮彻底离开画面
        float offscreenY = canvasHeight * 0.75f + 10f;

        yield return StartCoroutine(
            MoveEyelidsTo(
                offscreenY,
                -offscreenY,
                0.25f
            )
        );

        SetEyelidPositions(offscreenY, -offscreenY);
    }

    // =========================================================
    // 起身动画：保留原有动作
    // =========================================================

    IEnumerator PlayStandUpAnimation()
    {
        Vector3 startPosition = lowCameraPosition;
        Vector3 endPosition = normalCameraPosition;

        Quaternion startRotation = normalCameraRotation;

        float timer = 0f;

        while (timer < riseDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / riseDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.localPosition = Vector3.Lerp(
                startPosition,
                endPosition,
                smoothT
            );

            float roll = GetRollOffset(t);
            float pitch = GetPitchOffset(t);

            cameraTransform.localRotation =
                startRotation *
                Quaternion.Euler(pitch, 0f, roll);

            yield return null;
        }

        cameraTransform.localPosition = normalCameraPosition;
        cameraTransform.localRotation = normalCameraRotation;
    }

    float GetRollOffset(float t)
    {
        if (t < 0.25f)
        {
            float localT = t / 0.25f;

            return Mathf.Lerp(
                0f,
                -leftTiltAngle,
                Mathf.SmoothStep(0f, 1f, localT)
            );
        }

        if (t < 0.40f)
        {
            return -leftTiltAngle;
        }

        if (t < 0.65f)
        {
            float localT = (t - 0.40f) / 0.25f;

            return Mathf.Lerp(
                -leftTiltAngle,
                rightTiltAngle,
                Mathf.SmoothStep(0f, 1f, localT)
            );
        }

        if (t < 0.78f)
        {
            return rightTiltAngle;
        }

        float finalT = (t - 0.78f) / 0.22f;

        return Mathf.Lerp(
            rightTiltAngle,
            0f,
            Mathf.SmoothStep(0f, 1f, finalT)
        );
    }

    float GetPitchOffset(float t)
    {
        if (t < 0.20f)
        {
            float localT = t / 0.20f;

            return Mathf.Lerp(
                0f,
                -downwardPitchAngle,
                Mathf.SmoothStep(0f, 1f, localT)
            );
        }

        if (t < 0.65f)
        {
            return -downwardPitchAngle;
        }

        if (t < 0.90f)
        {
            float localT = (t - 0.65f) / 0.25f;

            return Mathf.Lerp(
                -downwardPitchAngle,
                0f,
                Mathf.SmoothStep(0f, 1f, localT)
            );
        }

        return 0f;
    }

    // =========================================================
    // 第二段对话
    // =========================================================

    void StartSecondDialogue()
    {
        if (dialogueList2.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentIndex2 = 0;

        if (text2 != null)
        {
            text2.text = dialogueList2[0];
            text2.gameObject.SetActive(true);
        }

        if (text2Group != null)
        {
            text2Group.alpha = 1f;
        }

        secondDialogueActive = true;
    }

    void NextSecondDialogue()
    {
        currentIndex2++;

        if (currentIndex2 < dialogueList2.Count)
        {
            if (text2 != null)
            {
                text2.text = dialogueList2[currentIndex2];
            }
        }
        else
        {
            secondDialogueActive = false;
            dialogueFinished = true;

            StartCoroutine(FadeOutText2());
        }
    }

    IEnumerator FadeOutText2()
    {
        float timer = 0f;

        if (text2Group != null)
        {
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                text2Group.alpha = Mathf.Lerp(
                    1f,
                    0f,
                    Mathf.Clamp01(timer / fadeDuration)
                );

                yield return null;
            }

            text2Group.alpha = 0f;
        }

        if (text2 != null)
        {
            text2.gameObject.SetActive(false);
        }

        if (playerController != null)
        {
            playerController.dialogueLocked = false;
        }
    }

    void EndDialogue()
    {
        firstDialogueActive = false;
        secondDialogueActive = false;
        dialogueFinished = true;

        if (playerController != null)
        {
            playerController.dialogueLocked = false;
        }
    }
}