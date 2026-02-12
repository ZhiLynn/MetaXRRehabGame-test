using UnityEngine;

/// <summary>
/// 困難難度配置 - 多階段任務，要求執行順序
/// </summary>
[System.Serializable]
public class HardDifficultyConfig : DifficultyConfig
{
    [Header("困難模式特殊設定")]
    [Tooltip("生成的顏色數量")]
    public int colorCount = 4;
    
    [Tooltip("多階段任務數量")]
    public int subTaskCount = 3;
    
    [Tooltip("每個子任務的魚數量範圍")]
    public int minFishPerSubTask = 1;
    public int maxFishPerSubTask = 3;
    
    public HardDifficultyConfig()
    {
        SetDifficultyName("困難");
        difficultyIndex = 2;
        taskType = TaskType.MultiStage;
        timeLimit = 600f;  // 10分鐘
        scoreMultiplier = 2.0f;
        minFishPerColor = 5;
    }
    
    public override string GetDescription()
    {
        return $"困難模式 - {timeLimit}秒 - 多階段任務 - {scoreMultiplier}x分數";
    }
    
    /// <summary>
    /// 取得困難模式啟用的顏色 - 根據colorCount配置
    /// </summary>
    protected override string[] GetEnabledColors()
    {
        // 困難模式使用 FishTags 的所有可用顏色
        string[] allColors = FishTags.GetAllFishTags();

        // 限制在指定數量內
        int count = Mathf.Min(colorCount, allColors.Length);
        string[] enabledColors = new string[count];
        System.Array.Copy(allColors, enabledColors, count);
        
        return enabledColors;
    }
    
    /// <summary>
    /// 取得困難模式專用配置 - 用於 HardModeManager
    /// </summary>
    public HardModeConfig GetHardModeConfig()
    {
        // 將 Tag 格式轉換為 FishColor 枚舉
        string[] colorTags = GetEnabledColors();
        FishColor[] fishColors = new FishColor[colorTags.Length];
        
        for (int i = 0; i < colorTags.Length; i++)
        {
            fishColors[i] = FishColorHelper.GetColorFromTag(colorTags[i]);
        }
        
        return new HardModeConfig
        {
            MinStages = 2,                      // 固定最少 2 階段
            MaxStages = subTaskCount,           // 使用 subTaskCount 作為最大階段數
            MinFishPerStage = minFishPerSubTask,
            MaxFishPerStage = maxFishPerSubTask,
            AvailableColors = fishColors
        };
    }
}
