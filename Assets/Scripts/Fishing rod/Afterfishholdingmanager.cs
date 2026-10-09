
using System;
using System.Collections.Generic;
using UnityEngine;

public class AfterFishHoldingManager : MonoBehaviour
{
    [Serializable]
    public class FishEntry
    {
        [Header("鱼模型")]
        public GameObject fishModel;

        [Header("相对于持鱼位置的坐标")]
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localRotation = Vector3.zero;
        public Vector3 localScale = Vector3.one;
    }

    [Header("所有可钓到的鱼")]
    public List<FishEntry> fishList = new List<FishEntry>();

    [Header("玩家拿鱼的位置")]
    public Transform fishHoldPoint;

    private GameObject currentFish;
    private int lastFishIndex = -1;

    void Start()
    {
        foreach (FishEntry entry in fishList)
        {
            if (entry != null && entry.fishModel != null)
            {
                entry.fishModel.SetActive(false);
            }
        }
    }

    public void CatchRandomFish()
    {
        if (fishHoldPoint == null)
        {
            Debug.LogError(
                "AfterFishHoldingManager：没有设置 Fish Hold Point！"
            );
            return;
        }

        if (fishList == null || fishList.Count == 0)
        {
            Debug.LogWarning("鱼类列表为空！");
            return;
        }

        List<int> validIndices = new List<int>();

        for (int i = 0; i < fishList.Count; i++)
        {
            if (fishList[i] != null &&
                fishList[i].fishModel != null)
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("鱼类列表中没有有效的鱼模型！");
            return;
        }

        // 有多种鱼时，尽量避免连续两次获得同一种
        if (validIndices.Count > 1)
        {
            validIndices.Remove(lastFishIndex);
        }

        int selectedIndex = validIndices[
            UnityEngine.Random.Range(0, validIndices.Count)
        ];

        lastFishIndex = selectedIndex;

        FishEntry selectedFish = fishList[selectedIndex];

        // 隐藏上一条鱼
        if (currentFish != null)
        {
            currentFish.SetActive(false);
        }

        currentFish = selectedFish.fishModel;

        currentFish.transform.SetParent(fishHoldPoint, false);
        currentFish.transform.localPosition = selectedFish.localPosition;
        currentFish.transform.localEulerAngles = selectedFish.localRotation;
        currentFish.transform.localScale = selectedFish.localScale;

        currentFish.SetActive(true);

        Debug.Log("获得鱼：" + currentFish.name);
    }
}