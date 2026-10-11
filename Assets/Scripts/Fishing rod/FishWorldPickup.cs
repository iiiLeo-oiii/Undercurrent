
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
}