using UnityEngine;

/// <summary>
/// 普通難度配置 - 要求顏色+數量認知
/// </summary>
[System.Serializable]
public class NormalDifficultyConfig : DifficultyConfig
{
    [Header("普通模式特殊設定")]
    [Tooltip("產生的顏色數量")]
    public int colorCount = 3;
    
    [Tooltip("每個顏色的任務魚數量範圍")]
    public int minFishPerColorTask = 1;
    public int maxFishPerColorTask = 3;
    
    public NormalDifficultyConfig()
    {
        SetDifficultyName("普通");
        difficultyIndex = 1;
        taskType = TaskType.ColorCount;
        timeLimit = 300f;  // 5分鐘
        scoreMultiplier = 1.5f;
        minFishPerColor = 5;
    }
    
    public override string GetDescription()
    {
        return $"普通模式 - {timeLimit}秒 - 顏色+數量認知 - {scoreMultiplier}x分數";
    }
    
    /// <summary>
    /// 取得普通模式啟用的顏色 - 根據colorCount配置
    /// </summary>
    protected override string[] GetEnabledColors()
    {
        // 普通模式使用 FishTags 的所有可用顏色
        string[] allColors = FishTags.GetAllFishTags();

        // 限制在指定數量內
        int count = Mathf.Min(colorCount, allColors.Length);
        string[] enabledColors = new string[count];
        System.Array.Copy(allColors, enabledColors, count);
        
        return enabledColors;
    }
}
