using TMPro;
using UnityEngine;

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

    // ==================================================
    // 是否已经拿到鱼竿
    // ==================================================

    public bool canFish = false;


    // ==================================================
    // 告诉其他脚本：
    // 现在是不是正在玩小游戏
    // ==================================================

    public static bool IsPlaying
    {
        get
        {
            return isPlaying;
        }
    }


    // ==================================================
    // 初始化
    // ==================================================

    void Start()
    {
        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }

        isPlaying = false;

        // 游戏开始时禁止钓鱼
        canFish = false;
    }


    // ==================================================
    // 游戏运行
    // ==================================================

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

        playerY -=
            fallSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerY += jumpForce;
        }

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

        if (currentTime <= 0f)
        {
            EndGame();
        }
    }


    // ==================================================
    // 开始小游戏
    // ==================================================

    public void StartGame()
    {
        // 没有鱼竿，绝对不能开始
        if (!canFish)
        {
            return;
        }

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


    // ==================================================
    // 按 E 取消小游戏
    // ==================================================

    public void CancelGame()
    {
        isPlaying = false;

        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }

        Debug.Log(
            "钓鱼小游戏已取消。"
        );
    }


    // ==================================================
    // 小游戏正常结束
    // ==================================================

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


        // =========================
        // 成功
        // =========================

        if (fishingSuccess)
        {
            Debug.Log("钓鱼成功！");

            if (fishingFloat != null)
            {
                fishingFloat.gameObject.SetActive(false);
            }
        }


        // =========================
        // 失败
        // =========================

        else
        {
            Debug.Log("钓鱼失败！");

            if (fishingFloat != null)
            {
                fishingFloat.ContinueFishing();
            }
        }


        // 隐藏 UI
        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }
    }
}