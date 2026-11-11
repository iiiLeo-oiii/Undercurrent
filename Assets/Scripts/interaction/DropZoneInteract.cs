
using UnityEngine;

public class DropZoneInteract : InteractableBase
{
    [Header("放置位置")]
    public Transform dropPoint;

    [Header("玩家携带管理器")]
    public PlayerCarryController carryController;

    [Header("微波炉控制器")]
    public MicrowaveController microwaveController;

    protected override void OnInteract()
    {
        if (microwaveController == null || carryController == null)
            return;

        microwaveController.HandleInteraction(carryController, dropPoint);
    }
}
