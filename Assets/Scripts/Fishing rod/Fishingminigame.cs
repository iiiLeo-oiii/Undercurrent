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

    [Header("Green Zone Random Position")]
    [Tooltip("绿色区域随机位置的上下边界留白，单位为 UI 像素")]
    public float greenZoneEdgePadding = 5f;

    [Header("Fish Holding Manager")]
    public AfterFishHoldingManager afterFishHoldingManager;

    private float currentTime;
    private float playerY;

    private static bool isPlaying = false;

    // 是否已经拿到鱼竿
    public bool canFish = false;

    // 告诉其他脚本现在是不是正在玩小游戏
    public static bool IsPlaying
    {
        get { return isPlaying; }
    }

    void Start()
    {
        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }

        isPlaying = false;
        canFish = false;
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
            timerText.text = Mathf.Ceil(currentTime).ToString();
        }

        playerY -= fallSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerY += jumpForce;
        }

        if (fishingBar == null || playerMarker == null)
        {
            Debug.LogError(
                "FishingMinigame：请设置 Fishing Bar 和 Player Marker！"
            );
            CancelGame();
            return;
        }

        float top = fishingBar.rect.height / 2f;
        float bottom = -fishingBar.rect.height / 2f;

        playerY = Mathf.Clamp(playerY, bottom, top);

        playerMarker.anchoredPosition = new Vector2(
            playerMarker.anchoredPosition.x,
            playerY
        );

        if (currentTime <= 0f)
        {
            EndGame();
        }
    }

    // 每次开始小游戏时随机移动绿色区域
    void RandomizeGreenZonePosition()
    {
        if (fishingBar == null || greenZone == null)
        {
            Debug.LogError(
                "FishingMinigame：请设置 Fishing Bar 和 Green Zone！"
            );
            return;
        }

        float barHeight = fishingBar.rect.height;
        float greenHeight = greenZone.rect.height;
        float padding = Mathf.Max(0f, greenZoneEdgePadding);

        float minY = -barHeight / 2f + greenHeight / 2f + padding;
        float maxY = barHeight / 2f - greenHeight / 2f - padding;

        if (minY > maxY)
        {
            minY = 0f;
            maxY = 0f;

            Debug.LogWarning(
                "绿色区域过高，无法随机移动，请检查 UI 尺寸。"
            );
        }

        float randomY = Random.Range(minY, maxY);

        greenZone.anchoredPosition = new Vector2(
            greenZone.anchoredPosition.x,
            randomY
        );

        Debug.Log("本次绿色目标区域位置：" + randomY);
    }

    // 开始小游戏
    public void StartGame()
    {
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

        RandomizeGreenZonePosition();

        fishingUI.SetActive(true);

        isPlaying = true;

        currentTime = gameTime;
        playerY = 0f;

        if (playerMarker != null)
        {
            playerMarker.anchoredPosition = new Vector2(
                playerMarker.anchoredPosition.x,
                playerY
            );
        }

        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(currentTime).ToString();
        }

        Debug.Log("钓鱼小游戏开始！");
    }

    // 按 E 取消小游戏
    public void CancelGame()
    {
        isPlaying = false;

        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }

        Debug.Log("钓鱼小游戏已取消。");
    }

    // 小游戏正常结束
    void EndGame()
    {
        isPlaying = false;

        if (playerMarker == null || greenZone == null)
        {
            Debug.LogError(
                "FishingMinigame：请设置 Player Marker 和 Green Zone！"
            );

            if (fishingUI != null)
            {
                fishingUI.SetActive(false);
            }

            return;
        }

        float playerPosition = playerMarker.anchoredPosition.y;
        float greenPosition = greenZone.anchoredPosition.y;
        float greenHeight = greenZone.rect.height;

        bool fishingSuccess =
            playerPosition >= greenPosition - greenHeight / 2f
            &&
            playerPosition <= greenPosition + greenHeight / 2f;

        // 钓鱼成功：鱼漂和鱼一起飞回
        if (fishingSuccess)
        {
            Debug.Log("钓鱼成功！");

            if (fishingFloat != null &&
                fishingFloat.fishingRodManager != null)
            {
                fishingFloat.fishingRodManager.StartSuccessfulReturn(
                    afterFishHoldingManager
                );
            }
            else
            {
                Debug.LogError(
                    "FishingMinigame：鱼漂没有设置 Fishingrodmanager！"
                );
            }
        }
        // 钓鱼失败：原有逻辑保持不变
        else
        {
            Debug.Log("钓鱼失败！");

            if (fishingFloat != null)
            {
                fishingFloat.ContinueFishing();
            }
        }

        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }
    }

    // 保留原有方法，供其他系统需要时调用
    public void GiveFishAfterSuccess()
    {
        Debug.Log("GiveFishAfterSuccess 已执行");

        if (afterFishHoldingManager != null)
        {
            afterFishHoldingManager.CatchRandomFish();
        }
        else
        {
            Debug.LogError(
                "FishingMinigame：没有设置 AfterFishHoldingManager！"
            );
        }
    }
}