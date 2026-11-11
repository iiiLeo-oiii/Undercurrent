
using UnityEngine;
using System.Collections.Generic;

public class TableFoodInteract : InteractableBase
{
    [Header("桌面放置设置")]
    public Transform dropPoint;

    [Header("玩家携带管理器")]
    public PlayerCarryController carryController;

    [Header("需要放置的盘子")]
    public PickupObjectInteract foodPlate;

    [Header("食物消失顺序")]
    public List<GameObject> foodItems = new List<GameObject>();

    private bool foodPlaced = false;
    private int currentFoodIndex = 0;

    protected override void OnInteract()
    {
        // 第一步：将盘子放到桌面
        if (!foodPlaced)
        {
            if (carryController == null || foodPlate == null || dropPoint == null)
                return;

            if (!carryController.HasObject())
                return;

            if (carryController.CurrentCarriedObject != foodPlate)
                return;

            carryController.DropObject(dropPoint);

            foodPlaced = true;

            return;
        }

        // 第二步：逐个隐藏食物
        while (currentFoodIndex < foodItems.Count)
        {
            GameObject currentFood = foodItems[currentFoodIndex];

            currentFoodIndex++;

            if (currentFood != null)
            {
                currentFood.SetActive(false);
                return;
            }
        }
    }
}
