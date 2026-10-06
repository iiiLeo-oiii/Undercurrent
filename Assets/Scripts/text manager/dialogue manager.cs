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

    // 第一段当前句子
    private int currentIndex = 0;

    // 第二段当前句子
    private int currentIndex2 = 0;

    // 第一段是否正在接受点击
    private bool firstDialogueActive = true;

    // 第二段是否正在接受点击
    private bool secondDialogueActive = false;

    // 整个开场对话是否已经彻底结束
    private bool dialogueFinished = false;

    private CanvasGroup blackGroup;
    private CanvasGroup text2Group;


    void Start()
    {
        // 获取 CanvasGroup
        blackGroup = black.GetComponent<CanvasGroup>();
        text2Group = text2.GetComponent<CanvasGroup>();

        // =========================
        // 第一段
        // =========================

        dialogueText.gameObject.SetActive(true);

        black.SetActive(true);
        blackGroup.alpha = 1f;

        // =========================
        // 第二段关闭
        // =========================

        text2.gameObject.SetActive(false);
        text2Group.alpha = 0f;

        // =========================
        // 显示第一句话
        // =========================

        if (dialogueList.Count > 0)
        {
            currentIndex = 0;
            dialogueText.text = dialogueList[0];
        }
        else
        {
            // 如果第一段没有内容，直接进入第二段
            firstDialogueActive = false;
            StartCoroutine(FinishFirstDialogue());
        }

        // =========================
        // 开场期间禁止玩家移动
        // =========================

        if (playerController != null)
        {
            playerController.dialogueLocked = true;
        }
    }


    void Update()
    {
        // 整个对话已经结束
        // 直接忽略所有鼠标输入
        if (dialogueFinished)
        {
            return;
        }

        // =========================
        // 第一段
        // =========================

        if (firstDialogueActive &&
            Input.GetMouseButtonDown(0))
        {
            NextFirstDialogue();
        }

        // =========================
        // 第二段
        // =========================

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
        currentIndex++;

        if (currentIndex < dialogueList.Count)
        {
            // 显示下一句
            dialogueText.text =
                dialogueList[currentIndex];
        }
        else
        {
            // 第一段已经结束
            // 立刻禁止继续点击第一段

            firstDialogueActive = false;

            StartCoroutine(FinishFirstDialogue());
        }
    }


    // =========================================================
    // 第一段结束
    // =========================================================

    IEnumerator FinishFirstDialogue()
    {
        // 再保险一次
        firstDialogueActive = false;

        // 隐藏第一段文字
        dialogueText.gameObject.SetActive(false);

        // =========================
        // 黑幕淡出
        // =========================

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            blackGroup.alpha = Mathf.Lerp(
                1f,
                0f,
                timer / fadeDuration
            );

            yield return null;
        }

        blackGroup.alpha = 0f;

        // 关闭黑幕
        black.SetActive(false);

        // =========================
        // 等待4秒
        // =========================

        // 此时玩家依然不能移动
        // firstDialogueActive = false
        // secondDialogueActive = false

        yield return new WaitForSeconds(4f);

        // =========================
        // 开始第二段
        // =========================

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

        // 从第一句开始
        currentIndex2 = 0;

        text2.text =
            dialogueList2[0];

        // 显示 Text2
        text2Group.alpha = 1f;
        text2.gameObject.SetActive(true);

        // 开始接受鼠标左键
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
            // 显示下一句
            text2.text =
                dialogueList2[currentIndex2];
        }
        else
        {
            // =========================
            // 第二段彻底结束
            // =========================

            // 立刻禁止继续点击
            secondDialogueActive = false;

            // 整个开场对话已经结束
            dialogueFinished = true;

            // 开始淡出
            StartCoroutine(FadeOutText2());
        }
    }


    // =========================================================
    // 第二段结束淡出
    // =========================================================

    IEnumerator FadeOutText2()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            text2Group.alpha = Mathf.Lerp(
                1f,
                0f,
                timer / fadeDuration
            );

            yield return null;
        }

        text2Group.alpha = 0f;

        // 关闭 Text2
        text2.gameObject.SetActive(false);

        // =========================
        // 所有开场对话结束
        // =========================

        if (playerController != null)
        {
            playerController.dialogueLocked = false;
        }
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
        {
            playerController.dialogueLocked = false;
        }
    }
}