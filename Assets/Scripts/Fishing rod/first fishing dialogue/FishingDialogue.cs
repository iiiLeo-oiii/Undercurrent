using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FishingDialogue : MonoBehaviour
{
    [Header("对话文本")]
    public TMP_Text dialogueText;

    [Header("对话内容")]
    [TextArea(2, 5)]
    public List<string> dialogueList = new List<string>();

    [Header("淡入淡出")]
    public CanvasGroup canvasGroup;

    [Header("文字出现速度")]
    public float textFadeSpeed = 2f;

    private int currentIndex = 0;
    private bool isTalking = false;
    private bool isFading = false;

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
    }

    void Update()
    {
        if (!isTalking)
            return;

        if (isFading)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            NextDialogue();
        }
    }

    public void StartDialogue()
    {
        if (isTalking)
            return;

        if (dialogueList == null || dialogueList.Count == 0)
        {
            Debug.LogWarning("FishingDialogue：List 里面没有对话内容！");
            return;
        }

        if (dialogueText == null)
        {
            Debug.LogWarning("FishingDialogue：没有设置 Dialogue Text！");
            return;
        }

        currentIndex = 0;
        isTalking = true;

        dialogueText.text = dialogueList[currentIndex];

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex >= dialogueList.Count)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = dialogueList[currentIndex];
    }

    void EndDialogue()
    {
        isTalking = false;

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        isFading = true;

        if (canvasGroup != null)
        {
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime * textFadeSpeed;
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        isFading = false;
    }
}