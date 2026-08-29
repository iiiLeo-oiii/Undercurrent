using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public Animator rodAnimator;

    public GameObject fishingFloat;

    public Transform castPoint;

    public float castForce = 10f;

    private bool isFishing = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isFishing)
            {
                CastRod();
            }
        }
    }

    void CastRod()
    {
        isFishing = true;

        rodAnimator.SetTrigger("Cast");

        Rigidbody rb = fishingFloat.GetComponent<Rigidbody>();

        fishingFloat.SetActive(true);

        fishingFloat.transform.position = castPoint.position;

        rb.velocity = Vector3.zero;

        rb.AddForce(
            transform.forward * castForce +
            Vector3.up * 3f,
            ForceMode.Impulse
        );
    }
}