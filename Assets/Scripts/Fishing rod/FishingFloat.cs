using System.Collections;
using UnityEngine;

public class FishingFloat : MonoBehaviour
{
    [Header("Fishing Minigame")]
    public FishingMinigame fishingMinigame;

    [Header("Fishing Rod Animator")]
    public Animator fishingRodAnimator;

    [Header("Fishing Rod Manager")]
    public Fishingrodmanager fishingRodManager;

    [Header("Fish Bite Time")]
    public float minBiteTime = 2f;
    public float maxBiteTime = 6f;

    private Rigidbody rb;
    private bool isOnWater = false;
    private Coroutine biteCoroutine;
    private float waterHeight;

    // 保存场景中鱼漂原本正确的朝向
    private Quaternion uprightRotation;
    private bool uprightRotationSaved = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        uprightRotation = transform.rotation;
        uprightRotationSaved = true;
    }

    // 恢复鱼漂原本的正确朝向
    public void RestoreUprightRotation()
    {
        if (!uprightRotationSaved)
        {
            uprightRotation = transform.rotation;
            uprightRotationSaved = true;
        }

        transform.rotation = uprightRotation;

        if (rb != null)
            rb.angularVelocity = Vector3.zero;
    }

    void OnEnable()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (fishingRodManager != null &&
            !fishingRodManager.hasRod)
        {
            StopFishingFloat();
            return;
        }

        isOnWater = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 防止碰撞造成鱼漂翻转
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    void OnDisable()
    {
        isOnWater = false;

        if (biteCoroutine != null)
        {
            StopCoroutine(biteCoroutine);
            biteCoroutine = null;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    void StopFishingFloat()
    {
        isOnWater = false;

        if (biteCoroutine != null)
        {
            StopCoroutine(biteCoroutine);
            biteCoroutine = null;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    // ==================================================
    // 普通碰撞
    // ==================================================

    private void OnCollisionEnter(Collision collision)
    {
        if (fishingRodManager != null &&
            !fishingRodManager.hasRod)
            return;

        if (IsGround(collision.collider))
        {
            ReturnToRod();
            return;
        }

        if (IsWater(collision.collider))
        {
            waterHeight = collision.collider.bounds.max.y;
            StartFishing();
        }
    }

    // ==================================================
    // Trigger
    // ==================================================

    private void OnTriggerEnter(Collider other)
    {
        if (fishingRodManager != null &&
            !fishingRodManager.hasRod)
            return;

        if (IsGround(other))
        {
            ReturnToRod();
            return;
        }

        if (IsWater(other))
        {
            waterHeight = other.bounds.max.y;
            StartFishing();
        }
    }

    bool IsGround(Collider collider)
    {
        return collider.CompareTag("Ground") ||
               collider.transform.root.CompareTag("Ground");
    }

    bool IsWater(Collider collider)
    {
        return collider.CompareTag("Water") ||
               collider.transform.root.CompareTag("Water");
    }

    // ==================================================
    // 开始钓鱼
    // ==================================================

    private void StartFishing()
    {
        if (fishingRodManager != null &&
            !fishingRodManager.hasRod)
            return;

        if (isOnWater)
            return;

        isOnWater = true;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            Vector3 position = rb.position;
            position.y = waterHeight;
            rb.position = position;
        }

        StartWaitingForFish();
    }

    void FixedUpdate()
    {
        if (!isOnWater || rb == null)
            return;

        Vector3 position = rb.position;
        position.y = waterHeight;

        rb.MovePosition(position);
    }

    // ==================================================
    // 等待鱼咬钩
    // ==================================================

    void StartWaitingForFish()
    {
        if (biteCoroutine != null)
            StopCoroutine(biteCoroutine);

        biteCoroutine = StartCoroutine(WaitForFish());
    }

    IEnumerator WaitForFish()
    {
        float waitTime = Random.Range(minBiteTime, maxBiteTime);

        Debug.Log("鱼漂已经落水，等待鱼咬钩……");

        yield return new WaitForSeconds(waitTime);

        if (!gameObject.activeSelf || !isOnWater)
            yield break;

        if (fishingRodManager != null &&
            !fishingRodManager.hasRod)
            yield break;

        Debug.Log("有鱼咬钩！");

        if (fishingRodAnimator != null)
            fishingRodAnimator.Play("鱼咬竿", 0, 0f);

        if (fishingMinigame != null)
            fishingMinigame.StartGame();
        else
            Debug.LogError("FishingFloat：没有设置 Fishing Minigame！");

        biteCoroutine = null;
    }

    // ==================================================
    // 小游戏失败后继续钓鱼
    // ==================================================

    public void ContinueFishing()
    {
        if (!gameObject.activeSelf)
            return;

        if (fishingRodManager != null &&
            !fishingRodManager.hasRod)
        {
            ReturnToRod();
            return;
        }

        isOnWater = true;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        StartWaitingForFish();
    }

    // ==================================================
    // 收回实际鱼漂
    // ==================================================

    void ReturnToRod()
    {
        if (fishingRodManager != null)
            fishingRodManager.FinishFishingCast();
        else
            gameObject.SetActive(false);
    }

    // ==================================================
    // 下一次抛竿前重置
    // ==================================================

    public void ResetForNextCast()
    {
        isOnWater = false;

        if (biteCoroutine != null)
        {
            StopCoroutine(biteCoroutine);
            biteCoroutine = null;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        RestoreUprightRotation();
    }
}