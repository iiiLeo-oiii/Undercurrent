using UnityEngine;
using TMPro;

public class FishingMinigame : MonoBehaviour
{
    [Header("UI")]
    public GameObject fishingUI;
    public RectTransform fishingBar;
    public RectTransform greenZone;
    public RectTransform playerMarker;
    public TextMeshProUGUI timerText;

    [Header("Fishing Float")]
    public FishingFloat fishingFloat;

    [Header("Game Settings")]
    public float gameTime = 10f;

    public float fallSpeed = 80f;

    // 空格每次上升距离
    public float jumpForce = 12f;

    private float currentTime;
    private float playerY;

    private static bool isPlaying = false;


    public static bool IsPlaying
    {
        get
        {
            return isPlaying;
        }
    }


    void Start()
    {
        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }

        isPlaying = false;
    }


    void Update()
    {
        if (!isPlaying)
        {
            return;
        }


        currentTime -= Time.deltaTime;


        if (timerText != null)
        {
            timerText.text =
                Mathf.Ceil(currentTime).ToString();
        }


        // =========================
        // 自动下降
        // =========================

        playerY -=
            fallSpeed * Time.deltaTime;


        // =========================
        // 空格上升
        // =========================

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerY += jumpForce;
        }


        // =========================
        // 限制范围
        // =========================

        float top =
            fishingBar.rect.height / 2f;

        float bottom =
            -fishingBar.rect.height / 2f;


        playerY =
            Mathf.Clamp(
                playerY,
                bottom,
                top
            );


        playerMarker.anchoredPosition =
            new Vector2(
                playerMarker.anchoredPosition.x,
                playerY
            );


        // =========================
        // 时间结束
        // =========================

        if (currentTime <= 0f)
        {
            EndGame();
        }
    }


    public void StartGame()
    {
        if (fishingUI == null)
        {
            Debug.LogError(
                "FishingMinigame：没有设置 Fishing UI！"
            );

            return;
        }


        fishingUI.SetActive(true);

        isPlaying = true;

        currentTime = gameTime;

        playerY = 0f;


        if (playerMarker != null)
        {
            playerMarker.anchoredPosition =
                new Vector2(
                    playerMarker.anchoredPosition.x,
                    playerY
                );
        }


        if (timerText != null)
        {
            timerText.text =
                Mathf.Ceil(currentTime).ToString();
        }


        Debug.Log("钓鱼小游戏开始！");
    }


    void EndGame()
    {
        isPlaying = false;


        float playerPosition =
            playerMarker.anchoredPosition.y;

        float greenPosition =
            greenZone.anchoredPosition.y;

        float greenHeight =
            greenZone.rect.height;


        bool fishingSuccess =
            playerPosition >=
            greenPosition - greenHeight / 2f
            &&
            playerPosition <=
            greenPosition + greenHeight / 2f;


        // ==================================================
        // 成功
        // ==================================================

        if (fishingSuccess)
        {
            Debug.Log("钓鱼成功！");

            // 把鱼漂拉起来
            if (fishingFloat != null)
            {
                fishingFloat.gameObject.SetActive(false);
            }
        }


        // ==================================================
        // 失败
        // ==================================================

        else
        {
            Debug.Log("钓鱼失败！");

            // 鱼漂继续留在水里
            if (fishingFloat != null)
            {
                fishingFloat.ContinueFishing();
            }
        }


        // 隐藏UI
        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }
    }
}