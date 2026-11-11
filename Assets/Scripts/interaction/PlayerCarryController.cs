
using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    [Header("持有位置")]
    public Transform holdPoint;

    private PickupObjectInteract carriedObject;
    private Transform originalParent;

    public PickupObjectInteract CurrentCarriedObject => carriedObject;

    public bool HasObject()
    {
        return carriedObject != null;
    }

    public bool TryPickup(PickupObjectInteract item)
    {
        if (HasObject() || item == null || holdPoint == null)
            return false;

        carriedObject = item;
        originalParent = item.transform.parent;

        item.transform.SetParent(holdPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        return true;
    }

    public void DropObject(Transform dropPoint)
    {
        if (!HasObject() || dropPoint == null)
            return;

        carriedObject.transform.SetParent(originalParent);
        carriedObject.transform.position = dropPoint.position;
        carriedObject.transform.rotation = dropPoint.rotation;

        carriedObject.OnDropped();

        carriedObject = null;
        originalParent = null;
    }
}
