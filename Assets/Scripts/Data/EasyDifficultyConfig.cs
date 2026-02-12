using UnityEngine;

/// <summary>
/// 簡單難度配置 - 只要求數量認知，不限顏色
/// </summary>
[System.Serializable]
public class EasyDifficultyConfig : DifficultyConfig
{
    [Header("簡單模式特殊設定")]
    [Tooltip("是否只生成單一顏色的魚")]
    public bool useSingleColor = true;
    
    [Tooltip("任務魚數量範圍")]
    public int minFishCount = 1;
    public int maxFishCount = 3;
    
    public EasyDifficultyConfig()
    {
        SetDifficultyName("簡單");
        difficultyIndex = 0;
        taskType = TaskType.CountOnly;
        timeLimit = 180f;  // 3分鐘
        scoreMultiplier = 1.0f;
        minFishPerColor = 5;
    }
    
    public override string GetDescription()
    {
        return $"簡單模式 - {timeLimit}秒 - 單一顏色任務 - {scoreMultiplier}x分數";
    }
    
    /// <summary>
    /// 取得簡單模式啟用的顏色 - 只啟用紅色魚
    /// </summary>
    protected override string[] GetEnabledColors()
    {
        // 簡單模式只啟用紅色魚，避免玩家混亂
        if (useSingleColor)
        {
            return new string[] { "redFish" };
        }
        
        // 如果不使用單一顏色，則啟用所有顏色（使用 FishTags 保持同步）
        return FishTags.GetAllFishTags();
    }
}
