using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏结算UI - 显示最终得分和统计信息
/// </summary>
public class GameResultUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("结算面板GameObject")]
    [SerializeField] private GameObject resultPanel;
    
    [Tooltip("最终得分文本")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    
    [Tooltip("完成任务数文本")]
    [SerializeField] private TextMeshProUGUI completedTasksText;
    
    [Tooltip("时间奖励文本")]
    [SerializeField] private TextMeshProUGUI timeBonusText;
    
    [Tooltip("难度倍率文本")]
    [SerializeField] private TextMeshProUGUI difficultyText;
    
    [Tooltip("评价文本")]
    [SerializeField] private TextMeshProUGUI rankText;
    
    [Header("引用")]
    [Tooltip("ScoreManager引用")]
    [SerializeField] private ScoreManager scoreManager;
    
    [SerializeField]DifficultyManager difficultyManager;
    //DifficultyConfig currentDifficulty;

    [Header("評價設定")]
    [Tooltip("S級評價分數線")]
    [SerializeField] private int sRankThreshold = 1000;
    
    [Tooltip("A級評價分數線")]
    [SerializeField] private int aRankThreshold = 750;
    
    [Tooltip("B級評價分數線")]
    [SerializeField] private int bRankThreshold = 500;
    
    [Tooltip("C級評價分數線")]
    [SerializeField] private int cRankThreshold = 250;
    
    [Header("遊戲結束時隱藏的物件")]
    [Tooltip("遊戲結束時需要隱藏的 GameObject 列表（水桶、魚、按鈕等）")]
    [SerializeField] private GameObject[] objectsToHideOnGameEnd;
    
    [Tooltip("是否自動找到並隱藏所有魚")]
    [SerializeField] private bool autoHideAllFish = true;
    
    [Tooltip("是否隱藏水桶")]
    [SerializeField] private bool hideBuckets = true;
    
    [Tooltip("是否隱藏UI按鈕（確認、重試等）")]
    [SerializeField] private bool hideGameplayButtons = true;

    
    void Awake()
    {
        // 初始隐藏结算面板
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
    
    void Start()
    {
        // 獲取依賴
        TryGetDependencies();
        
        // 订阅游戏结束事件
        SubscribeToEvents();
    }
    
    /// <summary>
    /// 嘗試獲取依賴
    /// </summary>
    private void TryGetDependencies()
    {
        // 獲取 ScoreManager - 使用 TryGet 避免錯誤日誌
        if (scoreManager == null)
        {
            if (!ServiceLocator.Instance.TryGet(out scoreManager))
            {
                scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    Debug.Log("[GameResultUI] 從場景中找到 ScoreManager");
                }
            }
        }
        
        // 獲取 DifficultyManager - 使用 TryGet 避免錯誤日誌
        if (difficultyManager == null)
        {
            if (!ServiceLocator.Instance.TryGet(out difficultyManager))
            {
                // 嘗試使用單例
                difficultyManager = DifficultyManager.Instance;
                
                // 如果單例也為空，嘗試在場景中查找
                if (difficultyManager == null)
                {
                    difficultyManager = FindFirstObjectByType<DifficultyManager>();
                }
                

            }
        }
    }
    
    /// <summary>
    /// 訂閱事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (scoreManager != null)
        {
            scoreManager.OnGameEnd.AddListener(ShowGameResult);
            Debug.Log("[GameResultUI] 已订阅游戏结束事件");
        }
        else
        {
            Debug.LogWarning("[GameResultUI] ScoreManager 未找到！請確保場景中有 ScoreManager");
        }
    }
    
    void OnDestroy()
    {
        // 取消订阅
        if (scoreManager != null)
        {
            scoreManager.OnGameEnd.RemoveListener(ShowGameResult);
        }
    }
    
    /// <summary>
    /// 显示游戏结算
    /// </summary>
    public void ShowGameResult(GameResult result)
    {
        Debug.Log($"[GameResultUI] 显示游戏结算 - 最终得分: {result.finalScore}");
        
        // ⭐ 先顯示結束面板，避免提前關閉物件導致無法顯示
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        
        // 更新各项统计
        UpdateFinalScore(result.finalScore);
        UpdateCompletedTasks(result.completedTasks);
        UpdateTimeBonus(result.totalTimeSpent);
        UpdateDifficulty(result.difficultyMultiplier);
        UpdateRank(result.finalScore);
        
        // 最後才隱藏遊戲物件（確保結束面板已完全顯示）
        HideGameplayObjects();
    }
    
    /// <summary>
    /// 更新最终得分
    /// </summary>
    private void UpdateFinalScore(int score)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"最終得分: {score}";
        }
    }
    
    /// <summary>
    /// 更新完成任务数
    /// </summary>
    private void UpdateCompletedTasks(int tasks)
    {
        if (completedTasksText != null)
        {
            completedTasksText.text = $"完成任務: {tasks}";
        }
    }
    
    /// <summary>
    /// 更新时间奖励
    /// </summary>
        private void UpdateTimeBonus(float totalTime)
    {
        if (timeBonusText != null)
        {
                int minutes = Mathf.FloorToInt(totalTime / 60f);
                int seconds = Mathf.FloorToInt(totalTime % 60f);
                timeBonusText.text = $"本次遊玩時間: {minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// 更新难度信息
    /// </summary>
    private void UpdateDifficulty(float multiplier)
    {
        if (difficultyText != null)
        {
            if (difficultyManager != null)
            {
                var currentDifficulty = difficultyManager.GetCurrentDifficulty();
                if (currentDifficulty != null)
                {
                    string difficultyName = currentDifficulty.GetDifficultyName();
                    difficultyText.text = $"難度: {difficultyName}";
                }
                else
                {
                    // 如果難度配置為空，使用倍率顯示
                    difficultyText.text = $"難度倍率: x{multiplier:F1}";
                    Debug.LogWarning("[GameResultUI] GetCurrentDifficulty() 返回 null，使用倍率顯示");
                }
            }
            else
            {
                // 如果沒有 DifficultyManager，使用倍率顯示
                difficultyText.text = $"難度倍率: x{multiplier:F1}";
                Debug.LogWarning("[GameResultUI] DifficultyManager 為空，使用倍率顯示");
            }
        }
    }
    
    /// <summary>
    /// 更新评价等级
    /// </summary>
    private void UpdateRank(int score)
    {
        if (rankText != null)
        {
            string rank = GetRank(score);
            string color = GetRankColor(rank);
            rankText.text = $"<color={color}>評價: {rank}</color>";
        }
    }
    
    /// <summary>
    /// 获取评价等级
    /// </summary>
    private string GetRank(int score)
    {
        if (score >= sRankThreshold) return "S";
        if (score >= aRankThreshold) return "A";
        if (score >= bRankThreshold) return "B";
        if (score >= cRankThreshold) return "C";
        return "D";
    }
    
    /// <summary>
    /// 获取评价等级颜色
    /// </summary>
    private string GetRankColor(string rank)
    {
        switch (rank)
        {
            case "S": return "#FFD700"; // 金色
            case "A": return "#00FF00"; // 绿色
            case "B": return "#00BFFF"; // 蓝色
            case "C": return "#FFA500"; // 橙色
            case "D": return "#808080"; // 灰色
            default: return "#FFFFFF";  // 白色
        }
    }
    
    /// <summary>
    /// 重新开始游戏按钮
    /// </summary>
    public void OnRestartButtonPressed()
    {
        Debug.Log("[GameResultUI] 重新开始游戏");
        
        // 恢复时间流速
        Time.timeScale = 1f;
        
        // 重新加载场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// 返回主菜单按钮
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        Debug.Log("[GameResultUI] 返回主菜单");
        
        // 恢复时间流速
        Time.timeScale = 1f;
        
        // 隐藏结算面板
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
        
        // 这里可以添加返回主菜单的逻辑
        // 例如：SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// 退出游戏按钮
    /// </summary>
    public void OnQuitButtonPressed()
    {
        Debug.Log("[GameResultUI] 退出游戏");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 隱藏遊戲進行時的物件（水桶、魚、按鈕等）
    /// </summary>
    private void HideGameplayObjects()
    {
        Debug.Log("[GameResultUI] 🎮 開始隱藏遊戲物件...");
        int hiddenCount = 0;
        
        // 1. 隱藏手動指定的物件列表
        if (objectsToHideOnGameEnd != null && objectsToHideOnGameEnd.Length > 0)
        {
            foreach (GameObject obj in objectsToHideOnGameEnd)
            {
                if (obj != null && obj.activeSelf)
                {
                    obj.SetActive(false);
                    hiddenCount++;
                    Debug.Log($"[GameResultUI] 隱藏物件: {obj.name}");
                }
            }
        }
        
        // 2. 自動隱藏所有魚
        if (autoHideAllFish)
        {
            // 使用統一的 FishTags 配置（更易擴展）
            foreach (string fishTag in FishTags.GetAllFishTags())
            {
                try
                {
                    GameObject[] fishes = GameObject.FindGameObjectsWithTag(fishTag);
                    foreach (GameObject fish in fishes)
                    {
                        if (fish != null && fish.activeSelf)
                        {
                            fish.SetActive(false);
                            hiddenCount++;
                        }
                    }
                }
                catch (UnityException ex)
                {
                    // Tag 不存在時忽略錯誤
                    Debug.LogWarning($"[GameResultUI] Tag '{fishTag}' 不存在或未定義，跳過: {ex.Message}");
                }
            }
            Debug.Log($"[GameResultUI] 隱藏所有魚完成");
        }
        
        // 3. 隱藏水桶
        if (hideBuckets)
        {
            // 【修正】不使用 FindObjectsByType 避免誤觸發 normalBucket
            // 而是透過 MultiBucketManager 統一管理隱藏
            if (MultiBucketManager.Instance != null)
            {
                MultiBucketManager.Instance.HideAllBuckets();
                Debug.Log($"[GameResultUI] 隱藏 MultiBucketManager 的所有水桶");
                hiddenCount++;
            }
            else
            {
                // 備用：如果沒有 MultiBucketManager，才使用 FindObjectsByType
                BucketEvent[] buckets = FindObjectsByType<BucketEvent>(FindObjectsSortMode.None);
                foreach (BucketEvent bucket in buckets)
                {
                    if (bucket != null && bucket.gameObject.activeSelf)
                    {
                        bucket.gameObject.SetActive(false);
                        hiddenCount++;
                        Debug.Log($"[GameResultUI] 隱藏水桶: {bucket.gameObject.name}");
                    }
                }
            }
        }
        
        // 4. 隱藏遊戲操作按鈕
        if (hideGameplayButtons)
        {
            // 隱藏確認按鈕
            ConfirmButtonHandler confirmButton = FindFirstObjectByType<ConfirmButtonHandler>();
            if (confirmButton != null && confirmButton.gameObject.activeSelf)
            {
                confirmButton.gameObject.SetActive(false);
                hiddenCount++;
                Debug.Log($"[GameResultUI] 隱藏確認按鈕");
            }
            
            // 隱藏重試按鈕
            RetryButtonHandler retryButton = FindFirstObjectByType<RetryButtonHandler>();
            if (retryButton != null && retryButton.gameObject.activeSelf)
            {
                retryButton.gameObject.SetActive(false);
                hiddenCount++;
                Debug.Log($"[GameResultUI] 隱藏重試按鈕");
            }
            
            // 隱藏所有水桶重試按鈕
            BucketRetryButton[] bucketRetryButtons = FindObjectsByType<BucketRetryButton>(FindObjectsSortMode.None);
            foreach (BucketRetryButton btn in bucketRetryButtons)
            {
                if (btn != null && btn.gameObject.activeSelf)
                {
                    btn.gameObject.SetActive(false);
                    hiddenCount++;
                }
            }
            Debug.Log($"[GameResultUI] 隱藏所有水桶重試按鈕");
        }
        
        Debug.Log($"[GameResultUI] ✅ 完成！共隱藏 {hiddenCount} 個物件");
    }
}
