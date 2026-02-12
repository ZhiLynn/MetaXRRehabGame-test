using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务类型枚举
/// </summary>
public enum TaskType
{
    CountOnly,      // 初级：数量认知
    ColorCount,     // 中级：颜色+数量
    MultiStage      // 高级：多阶段
}

/// <summary>
/// 任务验证结果
/// </summary>
public enum TaskValidationResult
{
    Success,            // 任务完成
    Failed,             // 任务失败（撈错）
    Incomplete,         // 任务未完成
    SubTaskComplete     // 子任务完成（高级模式）
}

/// <summary>
/// 子任务数据（高级模式使用）
/// </summary>
[System.Serializable]
public class SubTask
{
    public string color;        // 目标颜色（例如："redFish"）
    public int count;           // 目标数量
    public int currentCount;    // 当前已撈数量
    
    public SubTask(string color, int count)
    {
        this.color = color;
        this.count = count;
        this.currentCount = 0;
    }
    
    public bool IsComplete()
    {
        return currentCount >= count;
    }
    
    public void Reset()
    {
        currentCount = 0;
    }
}

/// <summary>
/// 任务数据
/// </summary>
[System.Serializable]
public class TaskData
{
    public TaskType taskType;                   // 任务类型
    public int targetCount;                     // 目标数量
    public string targetColor;                  // 目标颜色（中级使用）
    public List<SubTask> subTasks;              // 子任务列表（高级使用）
    public int currentSubTaskIndex;             // 当前子任务索引（高级使用）
    
    public TaskData()
    {
        subTasks = new List<SubTask>();
        currentSubTaskIndex = 0;
    }
    
    /// <summary>
    /// 获取当前子任务（高级模式）
    /// </summary>
    public SubTask GetCurrentSubTask()
    {
        if (taskType == TaskType.MultiStage && subTasks.Count > 0)
        {
            return subTasks[currentSubTaskIndex];
        }
        return null;
    }
    
    /// <summary>
    /// 移动到下一个子任务
    /// </summary>
    public bool MoveToNextSubTask()
    {
        if (taskType == TaskType.MultiStage && currentSubTaskIndex < subTasks.Count - 1)
        {
            currentSubTaskIndex++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 检查所有子任务是否完成
    /// </summary>
    public bool IsAllSubTasksComplete()
    {
        if (taskType != TaskType.MultiStage)
            return false;
            
        foreach (SubTask subTask in subTasks)
        {
            if (!subTask.IsComplete())
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// 重置任务进度
    /// </summary>
    public void Reset()
    {
        currentSubTaskIndex = 0;
        if (subTasks != null)
        {
            foreach (SubTask subTask in subTasks)
            {
                subTask.Reset();
            }
        }
    }
}
