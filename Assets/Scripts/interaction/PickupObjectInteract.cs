
using UnityEngine;

public class PickupObjectInteract : InteractableBase
{
    private PlayerCarryController carryController;
    private bool isPickedUp = false;
    private bool pickupEnabled = true;

    private void Start()
    {
        carryController = FindObjectOfType<PlayerCarryController>();
    }

    protected override void OnInteract()
    {
        TryPickupInternal();
    }

    private bool TryPickupInternal(bool fromMicrowave = false)
    {
        if (!FridgeNoteInteract.IsFoodUnlocked)
            return false;

        if ((!pickupEnabled && !fromMicrowave) ||
            isPickedUp || carryController == null)
            return false;

        if (carryController.TryPickup(this))
        {
            isPickedUp = true;
            playerInRange = false;
            pickupEnabled = true;
            return true;
        }

        return false;
    }

    public bool TryPickupFromMicrowave()
    {
        return TryPickupInternal(true);
    }
    public void SetPickupEnabled(bool enabled)
    {
        pickupEnabled = enabled;

        if (!enabled)
            playerInRange = false;
    }

    public void OnDropped()
    {
        isPickedUp = false;
        playerInRange = false;
    }
}
