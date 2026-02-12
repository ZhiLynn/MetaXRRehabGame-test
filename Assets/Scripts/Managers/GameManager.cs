using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject scoopNet;
    [SerializeField] TMP_Text timerText;
    [SerializeField] private ScoreManager scoreManager;
    

    private float timer = 0f;
    private float timerSpent;
    private bool isEnd = false;
    private bool isGameStarted = false; // 新增：遊戲是否已開始
    public int score = 0;
    
    void Start()
    {
        scoopNet.SetActive(true);
        
        // 使用 ServiceLocator 獲取 ScoreManager
        if (scoreManager == null)
        {
            scoreManager = ServiceLocator.Instance.Get<ScoreManager>();
        }
    }
    
    void Update()
    {
        // 只有在遊戲開始後才計時
        if (!isGameStarted) return;
        
        if (timer > 0f && !isEnd)
        {
            timer -= Time.deltaTime;
            float minutes = Mathf.Floor(timer / 60f);
            float seconds = Mathf.Floor(timer % 60f);
            timerText.text = "剩餘時間: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        else if (timer <= 0f && !isEnd && isGameStarted)
        {
            isEnd = true;
            Debug.LogWarning("Time's up!");

            // 記錄遊戲時間用完
            if (CSVLogger.Instance != null)
            {
                CSVLogger.Instance.OnGameTimeExpired();
            }

            // 觸發遊戲結束並計算分數
            if (scoreManager != null)
            {
                scoreManager.EndGame(timerSpent); // 時間用完，傳入本次遊玩時間
            }
            timer = 0f; // 確保時間不會變成負數
            timerText.text = "Time: " + "00" + ":" + "00";
        }
    }

    public void SetTime(int index, float timeLimit)
    {
        timer = timeLimit;
        timerSpent = timeLimit;
        isEnd = false; // 重置結束標記
        isGameStarted = true; // 標記遊戲已開始
        Debug.Log($"[GameManager] 遊戲開始！時間限制：{timeLimit} 秒");
    }
    
    /// <summary>
    /// 獲取剩餘時間
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timer);
    }
    
    /// <summary>
    /// 手動結束遊戲（例如提前完成所有任務）
    /// </summary>
    public void EndGameEarly()
    {
        if (!isEnd && scoreManager != null)
        {
            isEnd = true;
            float remainingTime = GetRemainingTime();
            Debug.Log($"[GameManager] 提前結束遊戲！剩餘時間：{remainingTime:F1} 秒");
            scoreManager.EndGame(remainingTime);
        }
    }
}
