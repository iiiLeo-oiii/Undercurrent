using UnityEngine;

public class BoxFishTrigger : MonoBehaviour
{
    [Header("鱼管理器")]
    public AfterFishHoldingManager fishManager;

    private void OnTriggerEnter(Collider other)
    {
        // 优先从进入 Trigger 的物体或其父物体寻找鱼标记。
        FishWorldPickup pickup =
            other.GetComponent<FishWorldPickup>();

        if (pickup == null)
            pickup = other.GetComponentInParent<FishWorldPickup>();

        // 只有玩家丢下、由这个管理器管理的物品才有效。
        if (pickup == null)
            return;

        if (pickup.isPhoto)
            return;

        AfterFishHoldingManager owner =
            fishManager != null ? fishManager : pickup.owner;

        if (owner == null)
            return;

        if (pickup.owner != owner)
            return;

        owner.MarkPhotoCatchReady();
    }
}