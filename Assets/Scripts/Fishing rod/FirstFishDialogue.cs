using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FirstFishDialogue : MonoBehaviour
{
    [Header("对话文本：拖入你创建的 Text3")]
    public TMP_Text text3;

    [Header("对话面板（可选）")]
    public GameObject dialoguePanel;

    [Header("玩家移动脚本（可选）")]
    public PlayerController playerController;

    [Header("第一次钓鱼的对话内容")]
    public List<string> dialogueList = new List<string>()
    {
        "这里填写第一句对话。",
        "这里填写第二句对话。",
        "这里填写第三句对话。"
    };

    private int dialogueIndex = 0;
    private bool dialogueStarted = false;
    private bool dialogueFinished = false;
    private bool hasTriggered = false;

    private Transform fishHoldPoint;
    private bool originalMovementEnabled;

    void Start()
    {
        // 获取鱼的持有位置
        AfterFishHoldingManager fishManager =
            FindObjectOfType<AfterFishHoldingManager>();

        if (fishManager != null)
        {
            fishHoldPoint = fishManager.fishHoldPoint;
        }
        else
        {
            Debug.LogError(
                "FirstFishDialogue：找不到 AfterFishHoldingManager！"
            );
        }

        // 游戏开始时隐藏对话
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else if (text3 != null)
        {
            text3.gameObject.SetActive(false);
        }

        if (text3 == null)
        {
            Debug.LogError(
                "FirstFishDialogue：请在 Inspector 中设置 Text3！"
            );
        }

        if (playerController != null)
        {
            originalMovementEnabled = playerController.enabled;
        }
    }

    void Update()
    {
        // 第一次钓到鱼，鱼回到手上后触发
        if (!hasTriggered &&
            fishHoldPoint != null &&
            fishHoldPoint.childCount > 0)
        {
            hasTriggered = true;
            StartDialogue();
        }

        // 对话期间，点击鼠标左键继续
        if (dialogueStarted &&
            !dialogueFinished &&
            Input.GetMouseButtonDown(0))
        {
            ShowNextDialogue();
        }
    }

    void StartDialogue()
    {
        if (text3 == null || dialogueList == null ||
            dialogueList.Count == 0)
        {
            Debug.LogWarning(
                "FirstFishDialogue：对话文本为空或没有设置 Text3。"
            );

            dialogueFinished = true;
            return;
        }

        dialogueStarted = true;
        dialogueIndex = 0;

        // 显示对话
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        else
        {
            text3.gameObject.SetActive(true);
        }

        // 暂停玩家移动
        if (playerController != null)
        {
            originalMovementEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        text3.text = dialogueList[dialogueIndex];

        Debug.Log("第一次钓鱼对话开始！");
    }

    void ShowNextDialogue()
    {
        dialogueIndex++;

        // 所有对话已经播放完毕
        if (dialogueIndex >= dialogueList.Count)
        {
            EndDialogue();
            return;
        }

        text3.text = dialogueList[dialogueIndex];
    }

    void EndDialogue()
    {
        dialogueStarted = false;
        dialogueFinished = true;

        // 隐藏对话
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else if (text3 != null)
        {
            text3.gameObject.SetActive(false);
        }

        // 恢复玩家移动
        if (playerController != null)
        {
            playerController.enabled = originalMovementEnabled;
        }

        Debug.Log("第一次钓鱼对话结束！");
    }
}