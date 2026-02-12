using UnityEngine;

/// <summary>
/// 重试按钮处理器 - 重试当前子任务（高级模式专用）
/// </summary>
public class RetryButtonHandler : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BucketEvent bucketEvent;  // 備用：直接指定的水桶
    
    private void Awake()
    {
        // 使用 ServiceLocator 獲取服務
        if (taskManager == null)
        {
            taskManager = ServiceLocator.Instance.Get<TaskManager>();
        }
        
        // bucketEvent 會在 GetActiveBucketEvent() 中動態獲取，這裡只作為備用
        if (bucketEvent == null)
        {
            bucketEvent = ServiceLocator.Instance.Get<BucketEvent>();
        }
    }
    
    /// <summary>
    /// 獲取當前應該使用的 BucketEvent（根據難度模式）
    /// </summary>
    private BucketEvent GetActiveBucketEvent()
    {
        // 如果有 MultiBucketManager，根據當前模式獲取正確的水桶
        if (MultiBucketManager.Instance != null)
        {
            if (!MultiBucketManager.Instance.IsHardMode)
            {
                // 普通模式：使用普通水桶
                BucketEvent normalBucket = MultiBucketManager.Instance.GetNormalModeBucketEvent();
                if (normalBucket != null)
                {
                    return normalBucket;
                }
            }
        }
        
        // 備用：使用直接設置的 bucketEvent
        return bucketEvent;
    }
    
    /// <summary>
    /// 重试按钮按下（由ButtonEvent的UnityEvent调用）
    /// </summary>
    public void OnRetryButtonPressed()
    {
        BucketEvent activeBucket = GetActiveBucketEvent();
        
        if (taskManager == null || activeBucket == null)
        {
            Debug.LogWarning("[RetryButtonHandler] TaskManager或BucketEvent未设置");
            return;
        }
        
        Debug.Log($"[RetryButtonHandler] 重试当前子任务，使用水桶: {activeBucket.gameObject.name}");
        
        // 清空桶
        activeBucket.ClearBucket();
        
        // ✅ 重置水桶错误状态（防止 Error 状态阻止新鱼进入）
        activeBucket.ResetStatus();
        
        // 重置当前子任务进度
        taskManager.RetryCurrentSubTask();
    }
}
