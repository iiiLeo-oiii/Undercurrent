using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("第一段 Text")]
    public TMP_Text dialogueText;

    [Header("黑幕")]
    public GameObject black;

    [Header("第二段 Text")]
    public TMP_Text text2;

    [Header("玩家")]
    public PlayerController playerController;

    [Header("摄像机起身效果")]
    [Tooltip("摄像机降低的距离，数值越大越贴近地面")]
    public float cameraLowerDistance = 1.0f;

    [Header("第一段对话")]
    public List<string> dialogueList = new List<string>()
    {
        "1",
        "2",
        "3",
        "4",
        "5",
        "6"
    };

    [Header("第二段对话")]
    public List<string> dialogueList2 = new List<string>()
    {
        "第二段1",
        "第二段2",
        "第二段3",
        "第二段4"
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

    void Start()
    {
        // 获取 CanvasGroup
        if (black != null)
            blackGroup = black.GetComponent<CanvasGroup>();

        if (text2 != null)
            text2Group = text2.GetComponent<CanvasGroup>();

        // 获取摄像机，并记录正常高度
        if (playerController != null)
        {
            cameraTransform = playerController.cameraTransform;

            if (cameraTransform != null)
            {
                normalCameraPosition = cameraTransform.localPosition;

                lowCameraPosition = normalCameraPosition
                    + Vector3.down * cameraLowerDistance;

                // 关键修改：游戏开始时直接放到最低位置
                cameraTransform.localPosition = lowCameraPosition;
            }
            else
            {
                Debug.LogError(
                    "DialogueManager：请在 PlayerController 中设置 Camera Transform！"
                );
            }
        }
        else
        {
            Debug.LogError(
                "DialogueManager：请设置 Player Controller！"
            );
        }

        // 显示第一段文字
        if (dialogueText != null)
            dialogueText.gameObject.SetActive(true);

        // 开启黑幕
        if (black != null)
        {
            black.SetActive(true);

            if (blackGroup != null)
                blackGroup.alpha = 1f;
            else
                Debug.LogError("黑幕物体缺少 CanvasGroup 组件！");
        }

        // 关闭第二段文字
        if (text2 != null)
        {
            text2.gameObject.SetActive(false);

            if (text2Group != null)
                text2Group.alpha = 0f;
        }

        // 显示第一句话
        if (dialogueList.Count > 0)
        {
            currentIndex = 0;

            if (dialogueText != null)
                dialogueText.text = dialogueList[0];
        }
        else
        {
            firstDialogueActive = false;
            firstDialogueFinishing = true;
            StartCoroutine(FinishFirstDialogue());
        }

        // 开场期间禁止玩家移动
        if (playerController != null)
            playerController.dialogueLocked = true;
    }

    void Update()
    {
        if (dialogueFinished)
            return;

        // 第一段对话：鼠标左键继续
        if (firstDialogueActive && Input.GetMouseButtonDown(0))
            NextFirstDialogue();

        // 第二段对话：鼠标左键继续
        if (secondDialogueActive && Input.GetMouseButtonDown(0))
            NextSecondDialogue();
    }

    // =========================================================
    // 第一段对话
    // =========================================================

    void NextFirstDialogue()
    {
        if (firstDialogueFinishing)
            return;

        currentIndex++;

        if (currentIndex < dialogueList.Count)
        {
            if (dialogueText != null)
                dialogueText.text = dialogueList[currentIndex];
        }
        else
        {
            firstDialogueActive = false;
            firstDialogueFinishing = true;

            StartCoroutine(FinishFirstDialogue());
        }
    }

    // =========================================================
    // 第一段结束：黑幕淡出、低位停留、摄像机起身
    // =========================================================

    IEnumerator FinishFirstDialogue()
    {
        firstDialogueActive = false;
        firstDialogueFinishing = true;

        // 隐藏第一段文字
        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);

        // 黑幕淡出
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

        // 关闭黑幕
        if (black != null)
            black.SetActive(false);

        // 此时摄像机已经处于最低位置，不再执行下降动画

        if (cameraTransform != null)
        {
            // 低视角停留2秒
            yield return new WaitForSeconds(2f);

            // 用2秒平滑升回原来的高度
            float timer = 0f;
            float riseDuration = 2f;

            while (timer < riseDuration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / riseDuration);

                cameraTransform.localPosition = Vector3.Lerp(
                    lowCameraPosition,
                    normalCameraPosition,
                    Mathf.SmoothStep(0f, 1f, t)
                );

                yield return null;
            }

            // 确保最终高度准确
            cameraTransform.localPosition = normalCameraPosition;
        }

        // 开始第二段对话
        StartSecondDialogue();
    }

    // =========================================================
    // 第二段开始
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
            text2.text = dialogueList2[0];

        if (text2Group != null)
            text2Group.alpha = 1f;

        if (text2 != null)
            text2.gameObject.SetActive(true);

        secondDialogueActive = true;
    }

    // =========================================================
    // 第二段对话
    // =========================================================

    void NextSecondDialogue()
    {
        currentIndex2++;

        if (currentIndex2 < dialogueList2.Count)
        {
            if (text2 != null)
                text2.text = dialogueList2[currentIndex2];
        }
        else
        {
            secondDialogueActive = false;
            dialogueFinished = true;

            StartCoroutine(FadeOutText2());
        }
    }

    // =========================================================
    // 第二段结束淡出
    // =========================================================

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
            text2.gameObject.SetActive(false);

        // 对话全部结束，恢复玩家控制
        if (playerController != null)
            playerController.dialogueLocked = false;
    }

    // =========================================================
    // 没有第二段对话时直接结束
    // =========================================================

    void EndDialogue()
    {
        firstDialogueActive = false;
        secondDialogueActive = false;
        dialogueFinished = true;

        if (playerController != null)
            playerController.dialogueLocked = false;
    }
}