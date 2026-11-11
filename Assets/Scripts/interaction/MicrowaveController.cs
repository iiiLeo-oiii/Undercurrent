
using UnityEngine;
using System.Collections;

public class MicrowaveController : MonoBehaviour
{
    private enum MicrowaveState
    {
        Closed,
        OpeningForPlacement,
        OpenForPlacement,
        ClosingAfterPlacement,
        Cooking,
        OpeningForPickup,
        OpenForPickup,
        ClosingAfterPickup
    }

    [Header("微波炉门")]
    public Transform doorPivot;

    [Header("微波炉灯光")]
    public GameObject microwaveLight;

    [Header("工作设置")]
    public float cookingDuration = 5f;
    public float doorAnimationDuration = 0.5f;

    [Header("门的旋转角度")]
    public float closedAngle = -55f;
    public float openAngle = 75f;

    private MicrowaveState currentState = MicrowaveState.Closed;

    private PickupObjectInteract storedFood;

    private void Start()
    {
        if (doorPivot == null)
        {
            Debug.LogError("MicrowaveController：没有设置 Door Pivot！");
            return;
        }

        SetDoorAngle(closedAngle);

        if (microwaveLight != null)
            microwaveLight.SetActive(false);
    }

    public void HandleInteraction(
        PlayerCarryController carryController,
        Transform dropPoint)
    {
        if (carryController == null || doorPivot == null)
            return;

        switch (currentState)
        {
            case MicrowaveState.Closed:
                StartCoroutine(OpenForPlacement());
                break;

            case MicrowaveState.OpenForPlacement:
                if (!carryController.HasObject())
                    return;

                if (dropPoint == null)
                    return;

                storedFood = carryController.CurrentCarriedObject;

                if (storedFood == null)
                    return;

                storedFood.SetPickupEnabled(false);

                carryController.DropObject(dropPoint);

                StartCoroutine(CloseAndCook());
                break;

            case MicrowaveState.OpenForPickup:
                if (storedFood == null)
                    return;

                if (storedFood.TryPickupFromMicrowave())
                {
                    storedFood = null;
                    StartCoroutine(CloseAfterPickup());
                }
                break;
        }
    }

    private IEnumerator OpenForPlacement()
    {
        currentState = MicrowaveState.OpeningForPlacement;

        yield return AnimateDoor(openAngle);

        currentState = MicrowaveState.OpenForPlacement;
    }

    private IEnumerator CloseAndCook()
    {
        currentState = MicrowaveState.ClosingAfterPlacement;

        yield return AnimateDoor(closedAngle);

        currentState = MicrowaveState.Cooking;

        if (microwaveLight != null)
            microwaveLight.SetActive(true);

        yield return new WaitForSeconds(cookingDuration);

        if (microwaveLight != null)
            microwaveLight.SetActive(false);

        currentState = MicrowaveState.OpeningForPickup;

        yield return AnimateDoor(openAngle);

        currentState = MicrowaveState.OpenForPickup;
    }

    private IEnumerator CloseAfterPickup()
    {
        currentState = MicrowaveState.ClosingAfterPickup;

        yield return AnimateDoor(closedAngle);

        currentState = MicrowaveState.Closed;
    }

    private IEnumerator AnimateDoor(float targetY)
    {
        Vector3 startEuler = doorPivot.localEulerAngles;

        Quaternion startRotation = doorPivot.localRotation;

        Quaternion targetRotation = Quaternion.Euler(
            startEuler.x,
            targetY,
            startEuler.z
        );

        float elapsed = 0f;

        while (elapsed < doorAnimationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / doorAnimationDuration);

            doorPivot.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        doorPivot.localRotation = targetRotation;
    }

    private void SetDoorAngle(float angle)
    {
        Vector3 euler = doorPivot.localEulerAngles;

        euler.y = angle;

        doorPivot.localEulerAngles = euler;
    }
}
