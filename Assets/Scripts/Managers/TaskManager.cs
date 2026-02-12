using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 任务管理器 - 负责生成任务、验证任务、管理多阶段任务
/// </summary>
public class TaskManager : MonoBehaviour
{
    [Header("颜色配置")]
    [SerializeField] private string[] availableColors => FishTags.GetAllFishTags();
    
    [Header("颜色显示名称映射")]
    [SerializeField] private ColorNameMapping[] colorNameMappings;
    
    [Header("任务数量范围")]
    [SerializeField] private int minFishCount = 1;
    [SerializeField] private int maxFishCount = 5;
    
    [Header("高级模式子任务数量")]
    [SerializeField] private int minSubTasks = 2;
    [SerializeField] private int maxSubTasks = 3;
    
    [Header("事件")]
    public UnityEvent<TaskData> OnTaskGenerated;        // 任务生成
    public UnityEvent<TaskValidationResult> OnTaskValidated; // 任务验证
    public UnityEvent<SubTask> OnSubTaskComplete;       // 子任务完成
    public UnityEvent <TaskValidationResult>  OnTaskFailed;                     // 任务失败
    
    // 当前任务
    private TaskData currentTask;
    
    /// <summary>
    /// 颜色名称映射（用于UI显示）
    /// </summary>
    [System.Serializable]
    public class ColorNameMapping
    {
        public string colorKey;         // "redFish"
        public string displayName;      // "紅色"
        
        public ColorNameMapping(string key, string name)
        {
            colorKey = key;
            displayName = name;
        }
    }
    
    private void Awake()
    {
        // 初始化颜色映射（如果未设置）
        if (colorNameMappings == null || colorNameMappings.Length == 0)
        {
            colorNameMappings = new ColorNameMapping[]
            {
                new ColorNameMapping("redFish", "紅色"),
                new ColorNameMapping("blueFish", "藍色"),
                new ColorNameMapping("yellowFish", "黃色"),
                new ColorNameMapping("greenFish", "綠色")
            };
        }
    }
    
    /// <summary>
    /// 获取当前任务
    /// </summary>
    public TaskData GetCurrentTask()
    {
        return currentTask;
    }
    
    /// <summary>
    /// 生成随机任务
    /// </summary>
    public void GenerateRandomTask(TaskType taskType)
    {
        currentTask = new TaskData();
        currentTask.taskType = taskType;
        
        switch (taskType)
        {
            case TaskType.CountOnly:
                GenerateCountOnlyTask();
                break;
            case TaskType.ColorCount:
                GenerateColorCountTask();
                break;
            case TaskType.MultiStage:
                GenerateMultiStageTask();
                break;
        }
        
        string description = GetTaskDescription(currentTask);
        Debug.Log($"[TaskManager] 生成任务: {description}");
        Debug.Log($"[TaskManager] 当前任务类型: {currentTask.taskType}, 目标数量: {currentTask.targetCount}");
        
        if (OnTaskGenerated != null)
        {
            Debug.Log($"[TaskManager] 触发 OnTaskGenerated 事件");
            OnTaskGenerated.Invoke(currentTask);
        }
        else
        {
            Debug.LogWarning($"[TaskManager] OnTaskGenerated 事件没有订阅者！TaskDisplayUI 可能未正确订阅");
        }
    }
    
    /// <summary>
    /// 生成初级任务：数量认知
    /// </summary>
    private void GenerateCountOnlyTask()
    {
        currentTask.targetCount = Random.Range(minFishCount, maxFishCount + 1);
        currentTask.targetColor = null; // 不限颜色
    }
    
    /// <summary>
    /// 生成中级任务：颜色+数量
    /// </summary>
    private void GenerateColorCountTask()
    {
        currentTask.targetCount = Random.Range(minFishCount, maxFishCount + 1);
        currentTask.targetColor = availableColors[Random.Range(0, availableColors.Length)];
    }
    
    /// <summary>
    /// 生成高级任务：多阶段
    /// </summary>
    private void GenerateMultiStageTask()
    {
        int subTaskCount = Random.Range(minSubTasks, maxSubTasks + 1);
        
        for (int i = 0; i < subTaskCount; i++)
        {
            string color = availableColors[Random.Range(0, availableColors.Length)];
            int count = Random.Range(minFishCount, maxFishCount + 1);
            currentTask.subTasks.Add(new SubTask(color, count));
        }
    }
    
    /// <summary>
    /// 验证任务（在确认按钮按下时调用）
    /// </summary>
    public TaskValidationResult ValidateTask(List<GameObject> fishInBucket)
    {
        if (currentTask == null)
        {
            Debug.LogWarning("[TaskManager] 没有当前任务");
            return TaskValidationResult.Failed;
        }
        
        TaskValidationResult result;
        
        switch (currentTask.taskType)
        {
            case TaskType.CountOnly:
                result = ValidateCountOnlyTask(fishInBucket);
                break;
            case TaskType.ColorCount:
                result = ValidateColorCountTask(fishInBucket);
                break;
            case TaskType.MultiStage:
                result = ValidateMultiStageTask(fishInBucket);
                break;
            default:
                result = TaskValidationResult.Failed;
                break;
        }
        
        OnTaskValidated?.Invoke(result);
        
        return result;
    }
    
    /// <summary>
    /// 验证初级任务
    /// </summary>
    private TaskValidationResult ValidateCountOnlyTask(List<GameObject> fishInBucket)
    {
        int fishCount = fishInBucket.Count;
        
        if (fishCount == currentTask.targetCount)
        {
            Debug.Log($"[TaskManager] 初级任务完成！撈了 {fishCount} 隻魚");
            return TaskValidationResult.Success;
        }
        else if (fishCount > 0)
        {
            // 如果有抓鱼但数量不对，算作失败
            Debug.Log($"[TaskManager] 初级任务失败！目标 {currentTask.targetCount} 隻，但撈了 {fishCount} 隻");
            return TaskValidationResult.Failed;
        }
        else
        {
            // 如果桶里没有鱼，算作未完成
            Debug.Log($"[TaskManager] 初级任务未完成：目标 {currentTask.targetCount}，当前 {fishCount}");
            return TaskValidationResult.Incomplete;
        }
    }
    
    /// <summary>
    /// 验证中级任务
    /// </summary>
    private TaskValidationResult ValidateColorCountTask(List<GameObject> fishInBucket)
    {
        int correctColorCount = 0;
        
        foreach (GameObject fish in fishInBucket)
        {
            FishData fishData = fish.GetComponent<FishData>();
            if (fishData != null && fishData.prefabName == currentTask.targetColor)
            {
                correctColorCount++;
            }
        }
        
        // 检查是否有错误的鱼
        bool hasWrongFish = fishInBucket.Count > correctColorCount;
        
        if (hasWrongFish)
        {
            Debug.Log($"[TaskManager] 中级任务失败！撈错了");
            return TaskValidationResult.Failed;
        }
        
        if (correctColorCount == currentTask.targetCount)
        {
            Debug.Log($"[TaskManager] 中级任务完成！撈了 {correctColorCount} 隻{GetColorDisplayName(currentTask.targetColor)}的魚");
            return TaskValidationResult.Success;
        }
        else
        {
            Debug.Log($"[TaskManager] 中级任务未完成：目标 {currentTask.targetCount}，当前 {correctColorCount}");
            return TaskValidationResult.Incomplete;
        }
    }
    
    /// <summary>
    /// 验证高级任务
    /// </summary>
    private TaskValidationResult ValidateMultiStageTask(List<GameObject> fishInBucket)
    {
        SubTask currentSubTask = currentTask.GetCurrentSubTask();
        if (currentSubTask == null)
        {
            Debug.LogWarning("[TaskManager] 没有当前子任务");
            return TaskValidationResult.Failed;
        }
        
        int correctColorCount = 0;
        List<GameObject> wrongFish = new List<GameObject>();
        
        foreach (GameObject fish in fishInBucket)
        {
            FishData fishData = fish.GetComponent<FishData>();
            if (fishData != null)
            {
                if (fishData.prefabName == currentSubTask.color)
                {
                    correctColorCount++;
                }
                else
                {
                    wrongFish.Add(fish);
                }
            }
        }
        
        // 有错误的鱼 -> 销毁并失败
        if (wrongFish.Count > 0)
        {
            Debug.Log($"[TaskManager] 高级任务失败！撈错了，销毁 {wrongFish.Count} 条错误的鱼");
            DestroyWrongFish(wrongFish);
            return TaskValidationResult.Failed;
        }
        
        // 检查子任务是否完成
        if (correctColorCount == currentSubTask.count)
        {
            currentSubTask.currentCount = correctColorCount;
            Debug.Log($"[TaskManager] 子任务完成！撈了 {correctColorCount} 隻{GetColorDisplayName(currentSubTask.color)}的魚");
            
            OnSubTaskComplete?.Invoke(currentSubTask);
            
            // 检查是否还有下一个子任务
            if (currentTask.MoveToNextSubTask())
            {
                Debug.Log($"[TaskManager] 移动到下一个子任务");
                return TaskValidationResult.SubTaskComplete;
            }
            else
            {
                Debug.Log($"[TaskManager] 所有子任务完成！");
                return TaskValidationResult.Success;
            }
        }
        else
        {
            Debug.Log($"[TaskManager] 子任务未完成：目标 {currentSubTask.count}，当前 {correctColorCount}");
            return TaskValidationResult.Incomplete;
        }
    }
    
    /// <summary>
    /// 销毁错误的鱼（高级模式）
    /// </summary>
    private void DestroyWrongFish(List<GameObject> wrongFish)
    {
        foreach (GameObject fish in wrongFish)
        {
            Destroy(fish);
        }
    }
    
    /// <summary>
    /// 重试当前子任务（高级模式，用于重新开始按钮）
    /// </summary>
    public void RetryCurrentSubTask()
    {
        if (currentTask != null && currentTask.taskType == TaskType.MultiStage)
        {
            SubTask currentSubTask = currentTask.GetCurrentSubTask();
            if (currentSubTask != null)
            {
                currentSubTask.Reset();
                Debug.Log($"[TaskManager] 重试当前子任务");
            }
        }
    }
    
    /// <summary>
    /// 获取任务描述（用于UI显示）
    /// </summary>
    public string GetTaskDescription(TaskData task)
    {
        if (task == null)
            return "";
        
        switch (task.taskType)
        {
            case TaskType.CountOnly:
                return $"撈 {task.targetCount} 隻";
                
            case TaskType.ColorCount:
                return $"撈 {task.targetCount} 隻";
                
            case TaskType.MultiStage:
                SubTask currentSubTask = task.GetCurrentSubTask();
                if (currentSubTask != null)
                {
                    return $"撈 {currentSubTask.count} 隻";
                }
                return "多阶段任务";
                
            default:
                return "";
        }
    }
    
    /// <summary>
    /// 獲取任务的顏色 key（用於 UI 圖片顯示）
    /// </summary>
    public string GetTaskColorKey(TaskData task)
    {
        if (task == null)
            return "";
        
        switch (task.taskType)
        {
            case TaskType.CountOnly:
                return ""; // 沒有顏色要求
                
            case TaskType.ColorCount:
                return task.targetColor;
                
            case TaskType.MultiStage:
                SubTask currentSubTask = task.GetCurrentSubTask();
                if (currentSubTask != null)
                {
                    return currentSubTask.color;
                }
                return "";
                
            default:
                return "";
        }
    }
    
    /// <summary>
    /// 获取颜色显示名称
    /// </summary>
    public string GetColorDisplayName(string colorKey)
    {
        foreach (ColorNameMapping mapping in colorNameMappings)
        {
            if (mapping.colorKey == colorKey)
            {
                return mapping.displayName;
            }
        }
        return colorKey; // 如果没有映射，返回原始key
    }
    
    /// <summary>
    /// 应用任务配置 - 使用数据驱动方式
    /// </summary>
    public void ApplyTaskConfig(TaskConfig config)
    {
        if (config.MinFishPerColor <= 0)
        {
            Debug.LogError("MinFishPerColor 必須大於 0");
            return;
        }
       
        // 设置最小鱼数量（可用于任务生成逻辑）
        if (config.MinFishPerColor > 0)
        {
            minFishCount = Mathf.Max(1, config.MinFishPerColor / 2);
            maxFishCount = config.MinFishPerColor;
        }
        
        Debug.Log($"[TaskManager] 应用配置：任务类型 {config.TaskType}，鱼数量范围 {minFishCount}-{maxFishCount}");
    }
    
    /// <summary>
    /// 重置任务
    /// </summary>
    public void ResetTask()
    {
        if (currentTask != null)
        {
            currentTask.Reset();
        }
    }
}
