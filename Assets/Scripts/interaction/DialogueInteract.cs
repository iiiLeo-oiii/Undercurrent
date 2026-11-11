
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogueInteract : InteractableBase
{
    [Header("对话内容")]
    [TextArea(3, 10)]
    public List<string> dialogueContent = new List<string>();

    [Header("对话文本")]
    public TMP_Text dialogueText;

    private bool isDialogueShowing = false;
    private int currentDialogueIndex = 0;

    private void Start()
    {
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
        }
    }

    protected override void Update()
    {
        // 保留父类的 E 键检测
        base.Update();

        // 没有开始对话时，不检测左键
        if (!isDialogueShowing)
            return;

        // 左键推进对话
        if (Input.GetMouseButtonDown(0))
        {
            currentDialogueIndex++;

            if (currentDialogueIndex < dialogueContent.Count)
            {
                dialogueText.text = dialogueContent[currentDialogueIndex];
            }
            else
            {
                EndDialogue();
            }
        }
    }

    // 对话期间允许离开交互范围
    protected override bool CanInteractOutsideRange()
    {
        return isDialogueShowing;
    }

    protected override void OnInteract()
    {
        // 对话进行中，E 不执行任何操作
        if (isDialogueShowing)
            return;

        if (dialogueContent.Count == 0 || dialogueText == null)
            return;

        currentDialogueIndex = 0;

        dialogueText.gameObject.SetActive(true);
        dialogueText.text = dialogueContent[currentDialogueIndex];

        isDialogueShowing = true;
    }

    private void EndDialogue()
    {
        dialogueText.gameObject.SetActive(false);

        isDialogueShowing = false;
        currentDialogueIndex = 0;
    }
}
