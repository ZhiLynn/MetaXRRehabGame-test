using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 任务显示UI - 负责显示当前任务文本、进度
/// </summary>
public class TaskDisplayUI : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private TextMeshProUGUI taskDescriptionText;   // 任务描述文本
    [SerializeField] private TextMeshProUGUI errorMessageText;      // 错误信息文本
    [SerializeField] private Image taskFishImage;                   // 任务魚圖片（簡單/普通模式）
    
    [Header("困難模式 UI")]
    [SerializeField] private TextMeshProUGUI stageProgressText;     // 階段進度文本 (例: "階段 2/5")
    [SerializeField] private TextMeshProUGUI currentStageText;      // 當前階段描述
    
    [Header("魚顏色圖片 Sprites")]
    [Tooltip("紅色魚的圖片")]
    [SerializeField] private Sprite redFishSprite;
    [Tooltip("灰色魚的圖片")]
    [SerializeField] private Sprite grayFishSprite;
    [Tooltip("綠色魚的圖片")]
    [SerializeField] private Sprite greenFishSprite;
    [Tooltip("黃色魚的圖片")]
    [SerializeField] private Sprite yellowFishSprite;
    [Tooltip("藍色魚的圖片")]
    [SerializeField] private Sprite blueFishSprite;
    
    [Header("错误信息配置")]
    [SerializeField] private float errorMessageDuration = 2f;       // 错误信息显示时长
    
    private TaskManager taskManager;
    private HardModeManager hardModeManager;
    
    // 用于跟踪当前运行的协程
    private Coroutine errorMessageCoroutine = null;
    
    private void Awake()
    {   
        taskManager = ServiceLocator.Instance.Get<TaskManager>();
        
        if (taskManager == null)
        {
            Debug.LogError("[TaskDisplayUI] 找不到TaskManager!");
        }
        
       
    }
    
    private void OnEnable()
    {
         // 獲取 HardModeManager 引用
        hardModeManager = HardModeManager.Instance;

        if (taskManager != null)
        {
            // 订阅事件
            taskManager.OnTaskGenerated.AddListener(OnTaskGenerated);
            taskManager.OnTaskValidated.AddListener(OnTaskValidated);
            taskManager.OnSubTaskComplete.AddListener(OnSubTaskComplete);
            taskManager.OnTaskFailed.AddListener(OnTaskFailed);
            
            Debug.Log("[TaskDisplayUI] 已订阅 TaskManager 事件");
        }
        else
        {
            Debug.LogError("[TaskDisplayUI] OnEnable: taskManager 为空，无法订阅事件！");
        }
        
        // 訂閱 HardModeManager 事件
        SubscribeToHardModeEvents();
    }
    
    private void OnDisable()
    {
        if (taskManager != null)
        {
            // 取消订阅
            taskManager.OnTaskGenerated.RemoveListener(OnTaskGenerated);
            taskManager.OnTaskValidated.RemoveListener(OnTaskValidated);
            taskManager.OnSubTaskComplete.RemoveListener(OnSubTaskComplete);
            taskManager.OnTaskFailed.RemoveListener(OnTaskFailed);
        }
        
        // 取消訂閱 HardModeManager 事件
        UnsubscribeFromHardModeEvents();
        
        // 停止所有错误信息协程
        StopErrorMessageCoroutine();
    }
    
    /// <summary>
    /// 訂閱 HardModeManager 事件
    /// </summary>
    private void SubscribeToHardModeEvents()
    {
        if (hardModeManager == null)
        {
            hardModeManager = HardModeManager.Instance;
        }
        

        
        
        if (hardModeManager != null)
        {
            hardModeManager.OnTaskGenerated.AddListener(hideTaskDescription);
            hardModeManager.OnStageAdvanced.AddListener(OnHardModeStageAdvanced);
            hardModeManager.OnTaskCompleted.AddListener(OnHardModeTaskCompleted);
            hardModeManager.OnSequenceError.AddListener(OnHardModeSequenceError);
            Debug.Log("[TaskDisplayUI] 已訂閱 HardModeManager 事件");
        }
    }
    
    /// <summary>
    /// 取消訂閱 HardModeManager 事件
    /// </summary>
    private void UnsubscribeFromHardModeEvents()
    {
        if (hardModeManager != null)
        {
            hardModeManager.OnStageAdvanced.RemoveListener(OnHardModeStageAdvanced);
            hardModeManager.OnTaskCompleted.RemoveListener(OnHardModeTaskCompleted);
            hardModeManager.OnSequenceError.RemoveListener(OnHardModeSequenceError);
        }
    }
    
    /// <summary>
    /// 困難模式任務生成時隱藏任務描述和魚圖片
    /// </summary>
    private void hideTaskDescription(HardModeTask hardTask)
    {
        // 困難模式：顯示特殊提示文字，不顯示圖片
        if (taskDescriptionText != null)
        {
            taskDescriptionText.text = "請根據右邊的提示完成任務";
        }
        
        // 關閉魚圖片
        if (taskFishImage != null)
        {
            taskFishImage.enabled = false;
        }
    }
    
    /// <summary>
    /// 困難模式階段進階
    /// </summary>
    private void OnHardModeStageAdvanced(int currentStage, int totalStages)
    {
        UpdateHardModeStageDisplay(currentStage, totalStages);
    }
    
    /// <summary>
    /// 困難模式任務完成
    /// </summary>
    private void OnHardModeTaskCompleted()
    {
        if (stageProgressText != null)
        {
            stageProgressText.text = "任務完成！";
        }
        if (currentStageText != null)
        {
            currentStageText.text = "";
        }
    }
    
    /// <summary>
    /// 困難模式順序錯誤
    /// </summary>
    private void OnHardModeSequenceError(string errorMessage)
    {
        ShowErrorMessage(errorMessage);
    }
    
    /// <summary>
    /// 更新困難模式階段顯示
    /// </summary>
    private void UpdateHardModeStageDisplay(int currentStage, int totalStages)
    {
        if (stageProgressText != null)
        {
            stageProgressText.text = $"階段 {currentStage}/{totalStages}";
        }
        
        if (currentStageText != null && hardModeManager != null)
        {
            currentStageText.text = hardModeManager.GetCurrentStageInstruction();
        }

       
    }
    
    /// <summary>
    /// 任务生成时更新UI
    /// </summary>
    private void OnTaskGenerated(TaskData task)
    {
        Debug.Log($"[TaskDisplayUI] OnTaskGenerated 被调用，任务类型: {task?.taskType}");
        UpdateTaskDescription(task);
        HideErrorMessage();
        
        // 如果是 MultiStage 任務，顯示困難模式 UI
        if (task != null && task.taskType == TaskType.MultiStage)
        {
            ShowHardModeUI(true);
        }
        else
        {
            ShowHardModeUI(false);
        }
    }
    
    /// <summary>
    /// 顯示/隱藏困難模式 UI
    /// </summary>
    private void ShowHardModeUI(bool show)
    {
        if (stageProgressText != null)
        {
            stageProgressText.gameObject.SetActive(show);
        }
        if (currentStageText != null)
        {
            currentStageText.gameObject.SetActive(show);
        }
    }
    
    /// <summary>
    /// 任务验证时更新UI
    /// </summary>
    private void OnTaskValidated(TaskValidationResult result)
    {
        switch (result)
        {
            case TaskValidationResult.Success:
                // 任务完成，可以显示完成信息或清空UI
                Debug.Log("[TaskDisplayUI] 任务完成！");
                break;
                
            case TaskValidationResult.Failed:
                // 任务失败（OnTaskFailed 會處理錯誤訊息）
                Debug.Log("[TaskDisplayUI] 任務失敗，OnTaskFailed 將顯示錯誤訊息");
                ShowErrorMessage("任務失敗，請重新嘗試");
                break;
                
            case TaskValidationResult.Incomplete:
                // 任务未完成，顯示提示信息
                ShowErrorMessage("請撈金魚來完成任務");
                break;
                
            case TaskValidationResult.SubTaskComplete:
                // 子任务完成，显示下一个子任务
                if (taskManager != null)
                {
                    TaskData currentTask = taskManager.GetCurrentTask();
                    UpdateTaskDescription(currentTask);
                }
                break;
        }
    }
    
    /// <summary>
    /// 子任务完成时
    /// </summary>
    private void OnSubTaskComplete(SubTask subTask)
    {
        Debug.Log($"[TaskDisplayUI] 子任务完成：{subTask.color} x {subTask.count}");
        // 直接显示下一个任务，不需要额外信息
    }
    
    /// <summary>
    /// 任务失败时顯示錯誤訊息
    /// </summary>
    private void OnTaskFailed(TaskValidationResult result)
    {
        Debug.Log($"[TaskDisplayUI] OnTaskFailed 被調用，結果：{result}");
        ShowErrorMessage("撈錯了！請重新開始");
    }
    
    /// <summary>
    /// 更新任务描述
    /// </summary>
    private void UpdateTaskDescription(TaskData task)
    {
        if (taskDescriptionText == null)
        {
            Debug.LogError("[TaskDisplayUI] taskDescriptionText 引用为空！请在 Inspector 中设置");
            return;
        }
        
        if (taskManager == null)
        {
            Debug.LogError("[TaskDisplayUI] taskManager 引用为空！");
            return;
        }
        
        if (task == null)
        {
            Debug.LogError("[TaskDisplayUI] task 数据为空！");
            return;
        }
        
        if(task.taskType != TaskType.MultiStage)
        {
            // 簡單/普通模式：獲取任务描述和顏色信息
            string description = taskManager.GetTaskDescription(task);
            string colorKey = taskManager.GetTaskColorKey(task);
            
            taskDescriptionText.text = description;
            
            // 更新魚圖片（只有在有顏色要求時顯示）
            if (!string.IsNullOrEmpty(colorKey) && taskFishImage != null)
            {
                Sprite fishSprite = GetFishSpriteByColorKey(colorKey);
                if (fishSprite != null)
                {
                    taskFishImage.sprite = fishSprite;
                    taskFishImage.enabled = true;
                }
                else
                {
                    taskFishImage.enabled = false;
                }
            }
            else if (taskFishImage != null)
            {
                // CountOnly 模式不需要顯示魚圖片
                taskFishImage.enabled = false;
            }
        }
        
        Debug.Log($"[TaskDisplayUI] 更新任务描述: {taskDescriptionText.text}");
        Debug.Log($"[TaskDisplayUI] TextMeshPro 组件状态: enabled={taskDescriptionText.enabled}, gameObject.activeSelf={taskDescriptionText.gameObject.activeSelf}");
    }
    
    /// <summary>
    /// 显示错误信息
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        // 安全检查：确保组件存在
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("[TaskDisplayUI] 尝试显示空的错误信息");
            return;
        }
        
        // 设置错误文本
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
        }
        else
        {
            Debug.LogWarning("[TaskDisplayUI] errorMessageText 未设置，无法显示错误文本");
        }
        
        // 显示错误面板
        // 启动新的协程来自动隐藏错误信息
        errorMessageCoroutine = StartCoroutine(ErrorMessageCoroutine());
        
        Debug.Log($"[TaskDisplayUI] 显示错误信息: {message}");
    }
    
    /// <summary>
    /// 错误信息协程 - 负责在指定时间后自动隐藏错误信息
    /// </summary>
    private IEnumerator ErrorMessageCoroutine()
    {
        // 安全检查：确保持续时间有效
        float duration = Mathf.Max(0.1f, errorMessageDuration);
        
        Debug.Log($"[TaskDisplayUI] 错误信息将在 {duration} 秒后隐藏");
        
        // 等待指定时间
        yield return new WaitForSeconds(duration);
        
        // 隐藏错误信息
        HideErrorMessage();
        
        // 清空协程引用
        errorMessageCoroutine = null;
    }
    
    /// <summary>
    /// 停止错误信息协程
    /// </summary>
    private void StopErrorMessageCoroutine()
    {
        if (errorMessageCoroutine != null)
        {
            StopCoroutine(errorMessageCoroutine);
            errorMessageCoroutine = null;
            Debug.Log("[TaskDisplayUI] 已停止错误信息协程");
        }
    }
    
    /// <summary>
    /// 隐藏错误信息
    /// </summary>
    private void HideErrorMessage()
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = "";
            Debug.Log("[TaskDisplayUI] 已隐藏错误信息");
        }
    }
    
    /// <summary>
    /// 清空任务显示
    /// </summary>
    public void ClearTaskDisplay()
    {
        if (taskDescriptionText != null)
        {
            taskDescriptionText.text = "";
        }
        if (taskFishImage != null)
        {
            taskFishImage.enabled = false;
        }
        HideErrorMessage();
    }
    
    /// <summary>
    /// 根據魚顏色 key 獲取對應的 Sprite
    /// </summary>
    private Sprite GetFishSpriteByColorKey(string colorKey)
    {
        switch (colorKey.ToLower())
        {
            case "redfish":
                return redFishSprite;
            case "grayfish":
                return grayFishSprite;
            case "greenfish":
                return greenFishSprite;
            case "yellowfish":
                return yellowFishSprite;
            case "bluefish":
                return blueFishSprite;
            default:
                Debug.LogWarning($"[TaskDisplayUI] 未知的魚顏色 key: {colorKey}");
                return null;
        }
    }
}
