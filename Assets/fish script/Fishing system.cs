using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public GameObject fishingUI;

    public RectTransform movingBar;
    public RectTransform greenArea;

    public float barSpeed = 300f;

    private bool fishing = false;
    private bool miniGame = false;

    private float direction = 1f;

    void Start()
    {
        fishingUI.SetActive(false);
    }

    void Update()
    {
        // 按E开始钓鱼
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!fishing)
            {
                StartFishing();
            }
            else if (miniGame)
            {
                CheckFishing();
            }
        }

        // 小条左右移动
        if (miniGame)
        {
            MoveBar();
        }
    }

    void StartFishing()
    {
        fishing = true;

        Debug.Log("抛竿！");

        Invoke("StartMiniGame", 3f);
    }

    void StartMiniGame()
    {
        miniGame = true;

        fishingUI.SetActive(true);

        Debug.Log("有鱼上钩！");
    }

    void MoveBar()
    {
        Vector3 position = movingBar.localPosition;

        position.x += direction * barSpeed * Time.deltaTime;

        // 左右边界
        if (position.x > 230)
        {
            direction = -1f;
        }

        if (position.x < -230)
        {
            direction = 1f;
        }

        movingBar.localPosition = position;
    }

    void CheckFishing()
    {
        float barX = movingBar.position.x;
        float greenX = greenArea.position.x;

        float distance = Mathf.Abs(barX - greenX);

        if (distance < 60f)
        {
            Debug.Log("钓鱼成功！");
        }
        else
        {
            Debug.Log("鱼跑了！");
        }

        EndFishing();
    }

    void EndFishing()
    {
        fishing = false;
        miniGame = false;

        fishingUI.SetActive(false);
    }
}