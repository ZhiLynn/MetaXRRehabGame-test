using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 分数管理器 - 负责记录和管理游戏分数
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("分数设置")]
    [Tooltip("完成任务的基础分数")]
    [SerializeField] private int baseTaskScore = 5;
    
    [Tooltip("简单模式分数倍率")]
    [SerializeField] private float easyModeMultiplier = 1.0f;
    
    [Tooltip("普通模式分数倍率")]
    [SerializeField] private float normalModeMultiplier = 1.5f;
    
    [Tooltip("困难模式分数倍率")]
    [SerializeField] private float hardModeMultiplier = 2.0f;
    
    [Tooltip("完成子任务的分数（高级模式）")]
    [SerializeField] private int subTaskScore = 50;
    
    [Header("事件")]
    [Tooltip("分数变化时触发")]
    public UnityEvent<int> OnScoreChanged;
    
    [Tooltip("游戏结束时触发")]
    public UnityEvent<GameResult> OnGameEnd;
    
    // 当前分数
    private int currentScore = 0;
    
    // 当前难度倍率
    private float currentMultiplier = 1.0f;
    
    // 完成的任务数量
    private int completedTasks = 0;
    
    // 游戏是否结束
    private bool isGameEnded = false;
    
    void Awake()
    {
        ResetScore();
    }
    
    /// <summary>
    /// 设置难度倍率
    /// </summary>
    public void SetDifficulty(TaskType taskType)
    {
        switch (taskType)
        {
            case TaskType.CountOnly:
                currentMultiplier = easyModeMultiplier;
                Debug.Log($"[ScoreManager] 设置简单模式，倍率: {currentMultiplier}x");
                break;
            case TaskType.ColorCount:
                currentMultiplier = normalModeMultiplier;
                Debug.Log($"[ScoreManager] 设置普通模式，倍率: {currentMultiplier}x");
                break;
            case TaskType.MultiStage:
                currentMultiplier = hardModeMultiplier;
                Debug.Log($"[ScoreManager] 设置困难模式，倍率: {currentMultiplier}x");
                break;
        }
    }
    
    /// <summary>
    /// 任务完成时加分
    /// </summary>
    public void AddTaskScore()
    {
        if (isGameEnded) return;
        
        int score = Mathf.RoundToInt(baseTaskScore);
        currentScore += score;
        completedTasks++;
        
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// 子任务完成时加分（高级模式）
    /// </summary>
    public void AddSubTaskScore()
    {
        if (isGameEnded) return;
        
        int score = Mathf.RoundToInt(subTaskScore * currentMultiplier);
        currentScore += score;
        
        Debug.Log($"[ScoreManager] 子任务完成！获得 {score} 分");
        Debug.Log($"[ScoreManager] 当前总分: {currentScore}");
        
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// 添加自定义分数
    /// </summary>
    public void AddCustomScore(int score)
    {
        if (isGameEnded) return;
        
        currentScore += score;
        Debug.Log($"[ScoreManager] 添加自定义分数: {score}，当前总分: {currentScore}");
        
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// 计算时间奖励并结束游戏
    /// </summary>
    public void EndGame(float totalTimeSpent)
    {
        if (isGameEnded) return;
        
        isGameEnded = true;
        
        // 计算时间奖励
        //int timeBonusScore = Mathf.RoundToInt(remainingTime * timeBonus);
        //currentScore += timeBonusScore;
        
        Debug.Log($"[ScoreManager] 游戏结束！");
        Debug.Log($"[ScoreManager] 總花費 {totalTimeSpent:F1} 秒");
        Debug.Log($"[ScoreManager] 最终得分: {currentScore}");
        
        // 创建游戏结果
        GameResult result = new GameResult
        {
            finalScore = currentScore,
            completedTasks = completedTasks,
            totalTimeSpent = totalTimeSpent,
            difficultyMultiplier = currentMultiplier
        };
        
        OnScoreChanged?.Invoke(currentScore);
        OnGameEnd?.Invoke(result);
    }
    
    /// <summary>
    /// 重置分数
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        completedTasks = 0;
        currentMultiplier = 1.0f;
        isGameEnded = false;
        
        Debug.Log("[ScoreManager] 分数已重置");
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// 获取当前分数
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// 获取完成的任务数量
    /// </summary>
    public int GetCompletedTasks()
    {
        return completedTasks;
    }
    
    /// <summary>
    /// 获取当前倍率
    /// </summary>
    public float GetCurrentMultiplier()
    {
        return currentMultiplier;
    }
}

/// <summary>
/// 游戏结果数据
/// </summary>
[System.Serializable]
public class GameResult
{
    public int finalScore;              // 最终得分
    public int completedTasks;          // 完成的任务数量
    public float totalTimeSpent;         // 總花費时间
    public float difficultyMultiplier;  // 难度倍率
}
