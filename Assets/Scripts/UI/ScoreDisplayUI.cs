using UnityEngine;
using TMPro;

/// <summary>
/// 分数显示UI - 实时显示当前分数
/// </summary>
public class ScoreDisplayUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("显示分数的文本组件")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Tooltip("显示任务完成数的文本组件（可选）")]
    [SerializeField] private TextMeshProUGUI tasksCompletedText;
    
    [Header("引用")]
    [Tooltip("ScoreManager引用")]
    [SerializeField] private ScoreManager scoreManager;
    
    [Header("显示设置")]
    [Tooltip("分数文本格式（{0}会被替换为分数）")]
    [SerializeField] private string scoreFormat = "分數: {0}";
    
    [Tooltip("任务完成文本格式")]
    [SerializeField] private string tasksFormat = "完成任務: {0}";
    
    [Tooltip("是否启用动画效果")]
    [SerializeField] private bool enableAnimation = true;
    
    [Tooltip("动画持续时间")]
    [SerializeField] private float animationDuration = 0.5f;
    
    // 动画相关
    private int displayedScore = 0;
    private int targetScore = 0;
    private float animationTimer = 0f;
    
    void Awake()
    {
        // 使用 ServiceLocator 獲取 ScoreManager
        TryGetScoreManager();
    }
    
    void Start()
    {
        // 再次嘗試獲取（確保在所有 Awake 完成後）
        TryGetScoreManager();
        
        // 订阅分数变化事件
        SubscribeToEvents();
        
        // 初始化显示
        UpdateScoreDisplay(0);
        UpdateTasksDisplay();
    }
    
    /// <summary>
    /// 嘗試獲取 ScoreManager
    /// </summary>
    private void TryGetScoreManager()
    {
        if (scoreManager == null)
        {
            if (!ServiceLocator.Instance.TryGet(out scoreManager))
            {
                scoreManager = FindFirstObjectByType<ScoreManager>();
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
            scoreManager.OnScoreChanged.AddListener(OnScoreChanged);
            UpdateScoreDisplay(scoreManager.GetCurrentScore());
        }
    }
    
    void OnDestroy()
    {
        // 取消订阅
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged.RemoveListener(OnScoreChanged);
        }
    }
    
    void Update()
    {
        // 分数动画
        if (enableAnimation && displayedScore != targetScore)
        {
            animationTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(animationTimer / animationDuration);
            
            // 使用缓动函数
            float easedProgress = EaseOutCubic(progress);
            displayedScore = Mathf.RoundToInt(Mathf.Lerp(displayedScore, targetScore, easedProgress));
            
            UpdateScoreDisplay(displayedScore);
            
            // 动画完成
            if (progress >= 1f)
            {
                displayedScore = targetScore;
                UpdateScoreDisplay(displayedScore);
            }
        }
    }
    
    /// <summary>
    /// 分数变化回调
    /// </summary>
    public void OnScoreChanged(int newScore)
    {
        if (enableAnimation)
        {
            targetScore = newScore;
            animationTimer = 0f;
        }
        else
        {
            displayedScore = newScore;
            targetScore = newScore;
            UpdateScoreDisplay(newScore);
        }
        
        UpdateTasksDisplay();
    }
    
    /// <summary>
    /// 更新分数显示
    /// </summary>
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, score);
        }
        else
        {
            Debug.LogWarning("[ScoreDisplayUI] scoreText 未设置！");
        }
    }
    
    /// <summary>
    /// 更新任务完成数显示
    /// </summary>
    private void UpdateTasksDisplay()
    {
        if (tasksCompletedText != null && scoreManager != null)
        {
            int completedTasks = scoreManager.GetCompletedTasks();
            tasksCompletedText.text = string.Format(tasksFormat, completedTasks);
        }
    }
    
    /// <summary>
    /// 缓动函数（EaseOutCubic）
    /// </summary>
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
    
    /// <summary>
    /// 手动更新显示（用于调试）
    /// </summary>
    [ContextMenu("Force Update Display")]
    public void ForceUpdateDisplay()
    {
        if (scoreManager != null)
        {
            int currentScore = scoreManager.GetCurrentScore();
            displayedScore = currentScore;
            targetScore = currentScore;
            UpdateScoreDisplay(currentScore);
            UpdateTasksDisplay();
        }
    }
}
