using UnityEngine;

public class FishWorldPickup : MonoBehaviour
{
    [HideInInspector]
    public AfterFishHoldingManager owner;

    [HideInInspector]
    public int fishEntryIndex;

    [HideInInspector]
    public Vector3 heldLocalPosition;

    [HideInInspector]
    public Vector3 heldLocalRotation;

    [HideInInspector]
    public float heldScale = 1f;

    // 是否为特殊物品：老照片
    [HideInInspector]
    public bool isPhoto = false;

    // 保存物品原本的三轴缩放
    [HideInInspector]
    public Vector3 heldLocalScale = Vector3.one;
}