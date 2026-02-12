using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魚的顏色枚舉 - 用於困難模式的嚴格類型檢查
/// </summary>
public enum FishColor
{
    Red,
    Blue,
    Yellow,
    Green,
    Gray
}

/// <summary>
/// 定義單一階段的任務需求 (困難模式專用)
/// </summary>
[System.Serializable]
public struct TaskStage
{
    public FishColor targetColor;   // 目標顏色
    public int count;               // 該階段需要的數量
    public int currentCount;        // 當前已完成數量
    
    public TaskStage(FishColor color, int targetCount)
    {
        targetColor = color;
        count = targetCount;
        currentCount = 0;
    }
    
    /// <summary>
    /// 檢查該階段是否完成
    /// </summary>
    public bool IsComplete => currentCount >= count;
    
    /// <summary>
    /// 重置階段進度
    /// </summary>
    public void Reset()
    {
        currentCount = 0;
    }
    
    /// <summary>
    /// 獲取顏色的Tag名稱
    /// </summary>
    public string GetColorTag()
    {
        return FishColorHelper.GetTagFromColor(targetColor);
    }
    
    /// <summary>
    /// 獲取顏色的顯示名稱
    /// </summary>
    public string GetColorDisplayName()
    {
        return FishColorHelper.GetDisplayName(targetColor);
    }
}

/// <summary>
/// 定義一個完整的困難模式任務
/// </summary>
[System.Serializable]
public class HardModeTask
{
    public int taskID;
    public List<TaskStage> stages;          // 任務序列，例如: [ {Red, 2}, {Yellow, 1} ]
    public int currentStageIndex;           // 當前階段索引
    public string instructionText;          // 顯示給玩家的提示文字
    
    public HardModeTask()
    {
        stages = new List<TaskStage>();
        currentStageIndex = 0;
        taskID = 0;
    }
    
    public HardModeTask(int id)
    {
        taskID = id;
        stages = new List<TaskStage>();
        currentStageIndex = 0;
    }
    
    /// <summary>
    /// 獲取當前階段
    /// </summary>
    public TaskStage? GetCurrentStage()
    {
        if (stages.Count > 0 && currentStageIndex < stages.Count)
        {
            return stages[currentStageIndex];
        }
        return null;
    }
    
    /// <summary>
    /// 移動到下一階段
    /// </summary>
    /// <returns>是否還有下一階段</returns>
    public bool MoveToNextStage()
    {
        if (currentStageIndex < stages.Count - 1)
        {
            currentStageIndex++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 檢查是否所有階段都完成
    /// </summary>
    public bool IsAllStagesComplete()
    {
        foreach (TaskStage stage in stages)
        {
            if (!stage.IsComplete)
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// 是否為最後一個階段
    /// </summary>
    public bool IsLastStage => currentStageIndex >= stages.Count - 1;
    
    /// <summary>
    /// 獲取總階段數
    /// </summary>
    public int TotalStages => stages.Count;
    
    /// <summary>
    /// 獲取當前階段編號 (1-based for display)
    /// </summary>
    public int CurrentStageNumber => currentStageIndex + 1;
    
    /// <summary>
    /// 重置任務進度
    /// </summary>
    public void Reset()
    {
        currentStageIndex = 0;
        for (int i = 0; i < stages.Count; i++)
        {
            TaskStage stage = stages[i];
            stage.Reset();
            stages[i] = stage;
        }
    }
    
    /// <summary>
    /// 生成完整的任務指示文字
    /// </summary>
    public string GenerateInstructionText()
    {
        if (stages.Count == 0)
            return "無任務";
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        for (int i = 0; i < stages.Count; i++)
        {
            TaskStage stage = stages[i];
            string prefix = i == 0 ? "" : "再";
            sb.Append($"撈 {stage.count} 隻{stage.GetColorDisplayName()}的魚");
            
            if (i < stages.Count - 1)
            {
                sb.Append(" → ");
            }
        }
        
        instructionText = sb.ToString();
        return instructionText;
    }
    
    /// <summary>
    /// 獲取當前階段的顯示文字
    /// </summary>
    public string GetCurrentStageDisplayText()
    {
        TaskStage? currentStage = GetCurrentStage();
        if (currentStage.HasValue)
        {
            string prefix = currentStageIndex == 0 ? "" : "再";
            return $"階段 {CurrentStageNumber}/{TotalStages}: {prefix}撈 {currentStage.Value.count} 隻{currentStage.Value.GetColorDisplayName()}的魚";
        }
        return "";
    }
    
    /// <summary>
    /// 計算完成該任務所需的總魚數
    /// </summary>
    public int GetTotalFishRequired()
    {
        int total = 0;
        foreach (TaskStage stage in stages)
        {
            total += stage.count;
        }
        return total;
    }
}

/// <summary>
/// 魚顏色輔助類 - 處理顏色枚舉與Tag/顯示名稱之間的轉換
/// </summary>
public static class FishColorHelper
{
    /// <summary>
    /// 從顏色枚舉獲取Tag名稱
    /// </summary>
    public static string GetTagFromColor(FishColor color)
    {
        switch (color)
        {
            case FishColor.Red: return "redFish";
            case FishColor.Blue: return "blueFish";
            case FishColor.Yellow: return "yellowFish";
            case FishColor.Green: return "greenFish";
            case FishColor.Gray: return "grayFish";
            default: return "redFish";
        }
    }
    
    /// <summary>
    /// 從Tag名稱獲取顏色枚舉
    /// </summary>
    public static FishColor GetColorFromTag(string tag)
    {
        switch (tag.ToLower())
        {
            case "redfish": return FishColor.Red;
            case "bluefish": return FishColor.Blue;
            case "yellowfish": return FishColor.Yellow;
            case "greenfish": return FishColor.Green;
            case "grayfish": return FishColor.Gray;
            default: return FishColor.Red;
        }
    }
    
    /// <summary>
    /// 獲取顏色的顯示名稱
    /// </summary>
    public static string GetDisplayName(FishColor color)
    {
        switch (color)
        {
            case FishColor.Red: return "紅色";
            case FishColor.Blue: return "藍色";
            case FishColor.Yellow: return "黃色";
            case FishColor.Green: return "綠色";
            case FishColor.Gray: return "灰色";
            default: return "紅色";
        }
    }
    
    /// <summary>
    /// 獲取顏色的顯示名稱（別名）
    /// </summary>
    public static string GetColorName(FishColor color)
    {
        return GetDisplayName(color);
    }
    
    /// <summary>
    /// 獲取所有可用的顏色
    /// </summary>
    public static FishColor[] GetAllColors()
    {
        return new FishColor[] 
        { 
            FishColor.Red, 
            FishColor.Blue, 
            FishColor.Yellow, 
            FishColor.Green, 
            FishColor.Gray 
        };
    }
}

/// <summary>
/// 困難模式驗證結果
/// </summary>
public enum HardModeValidationResult
{
    Success,            // 任務完全完成
    StageComplete,      // 當前階段完成，進入下一階段
    Incomplete,         // 當前階段未完成
    WrongSequence,      // 順序錯誤
    WrongColor,         // 顏色錯誤
    ExcessFish          // 魚太多
}
