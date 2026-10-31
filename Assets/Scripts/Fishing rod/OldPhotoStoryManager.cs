using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OldPhotoStoryManager : MonoBehaviour
{
    [Header("剧情对话 UI")]
    public TMP_Text text4;

    [Header("对话内容")]
    public List<string> dialogueList = new List<string>();

    [Header("目标场景名称")]
    public string targetSceneName = "第一张，家，旧物";

    private int currentDialogueIndex = 0;
    private bool isPlaying = false;

    void Start()
    {
        if (text4 != null)
        {
            text4.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("OldPhotoStoryManager：没有设置 Text4！");
        }
    }

    void Update()
    {
        if (!isPlaying)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            ShowNextDialogue();
        }
    }

    // 由 AfterFishHoldingManager 调用
    public void StartOldPhotoStory()
    {
        if (isPlaying)
            return;

        if (text4 == null)
        {
            Debug.LogError("OldPhotoStoryManager：Text4 未设置！");
            return;
        }

        if (dialogueList == null || dialogueList.Count == 0)
        {
            Debug.LogError("OldPhotoStoryManager：对话列表为空！");
            return;
        }

        currentDialogueIndex = 0;
        isPlaying = true;

        text4.gameObject.SetActive(true);
        text4.text = dialogueList[currentDialogueIndex];

        Debug.Log("老照片剧情开始。");
    }

    void ShowNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < dialogueList.Count)
        {
            text4.text = dialogueList[currentDialogueIndex];
        }
        else
        {
            EndStory();
        }
    }

    void EndStory()
    {
        isPlaying = false;

        text4.gameObject.SetActive(false);

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("OldPhotoStoryManager：目标场景名称为空！");
            return;
        }

        Debug.Log("正在切换场景：" + targetSceneName);

        SceneManager.LoadScene(targetSceneName);
    }
}