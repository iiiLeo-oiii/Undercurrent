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

    private void Start()
    {
        dialogueText.gameObject.SetActive(false);
    }

    // 对话期间允许在范围外继续交互
    protected override bool CanInteractOutsideRange()
    {
        return isDialogueShowing;
    }

    protected override void OnInteract()
    {
        if (!isDialogueShowing)
        {
            if (dialogueContent.Count == 0)
                return;

            dialogueText.gameObject.SetActive(true);
            dialogueText.text = dialogueContent[0];

            isDialogueShowing = true;
        }
        else
        {
            dialogueText.gameObject.SetActive(false);
            isDialogueShowing = false;
        }
    }
}