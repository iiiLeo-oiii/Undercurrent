
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FridgeNoteInteract : InteractableBase
{
    [Header("纸条内容")]
    [TextArea(3, 10)]
    public List<string> dialogueContent = new List<string>();

    [Header("对话文本")]
    public TMP_Text dialogueText;

    private bool isDialogueShowing = false;
    private int currentDialogueIndex = 0;

    // 食物是否已经解锁
    public static bool IsFoodUnlocked { get; private set; } = false;

    private void Awake()
    {
        IsFoodUnlocked = false;
    }

    private void Start()
    {
        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);
    }

    protected override void Update()
    {
        // 保留父类 E 键检测
        base.Update();

        // 只有对话开始后才检测左键
        if (!isDialogueShowing)
            return;

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

    // 对话期间允许在范围外继续交互
    protected override bool CanInteractOutsideRange()
    {
        return isDialogueShowing;
    }

    protected override void OnInteract()
    {
        // 对话期间按 E 无效
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

        // 阅读完成后解锁食物
        IsFoodUnlocked = true;

        Debug.Log("纸条阅读完成，食物已解锁！");
    }
}
