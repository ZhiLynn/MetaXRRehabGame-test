using System.Collections.Generic;

public struct FishSpawnConfig
{
    public int MinFishPerColor;
    public string[] EnabledColors;
}

public struct TaskConfig
{
    public TaskType TaskType;
    public int MinFishPerColor;
}

/// <summary>
/// 困難模式專用配置 - 用於 HardModeManager
/// </summary>
public struct HardModeConfig
{
    public int MinStages;           // 最少階段數
    public int MaxStages;           // 最多階段數
    public int MinFishPerStage;     // 每階段最少魚數
    public int MaxFishPerStage;     // 每階段最多魚數
    public FishColor[] AvailableColors;  // 可用顏色
}