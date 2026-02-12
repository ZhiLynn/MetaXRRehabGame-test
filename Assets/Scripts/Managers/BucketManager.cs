using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 水桶業務邏輯管理器 - 作為 BucketEvent 和其他系統之間的中介者
/// Business Layer - 協調數據層和其他管理器之間的交互
/// 
/// 職責：
/// 1. 處理水桶驗證業務邏輯
/// 2. 協調 TaskManager、GameModeManager 等
/// 3. 向 UI/Event Layer 發布業務事件
/// 4. 不直接操作 UI，只發布事件
/// </summary>
public class BucketManager : MonoBehaviour
{
    [Header("依賴注入（通過 ServiceLocator）")]
    private TaskManager taskManager;
    private GameModeManager gameModeManager;
    private MultiBucketManager multiBucketManager;
    
    [Header("事件：向上層發布（UI/Event Layer 監聽）")]
    public UnityEvent<BucketValidationResult> OnBucketValidated;
    public UnityEvent<string> OnBucketError;
    public UnityEvent OnBucketCleared;
    public UnityEvent<int, GameObject> OnFishEnteredBucket;  // (bucketIndex, fish)
    
    [Header("音效事件（由 Event Layer 處理）")]
    public UnityEvent OnValidationSuccess;
    public UnityEvent OnValidationFailed;
    public UnityEvent OnValidationIncomplete;
    
    private void Awake()
    {
        // 初始化事件
        if (OnBucketValidated == null) OnBucketValidated = new UnityEvent<BucketValidationResult>();
        if (OnBucketError == null) OnBucketError = new UnityEvent<string>();
        if (OnBucketCleared == null) OnBucketCleared = new UnityEvent();
        if (OnFishEnteredBucket == null) OnFishEnteredBucket = new UnityEvent<int, GameObject>();
        if (OnValidationSuccess == null) OnValidationSuccess = new UnityEvent();
        if (OnValidationFailed == null) OnValidationFailed = new UnityEvent();
        if (OnValidationIncomplete == null) OnValidationIncomplete = new UnityEvent();
    }
    
    private void Start()
    {
        // 從 ServiceLocator 獲取依賴
        if (!ServiceLocator.Instance.TryGet(out taskManager))
        {
            Debug.LogError("[BucketManager] TaskManager 未找到！");
        }
        
        if (!ServiceLocator.Instance.TryGet(out gameModeManager))
        {
            Debug.LogError("[BucketManager] GameModeManager 未找到！");
        }
        
        if (!ServiceLocator.Instance.TryGet(out multiBucketManager))
        {
            Debug.LogError("[BucketManager] MultiBucketManager 未找到！");
        }
        
        Debug.Log("[BucketManager] ✅ Business Layer 初始化完成");
    }
    
    /// <summary>
    /// 驗證當前活躍的水桶（被 Event Layer 調用）
    /// 這是 Event Layer 到 Business Layer 的入口
    /// </summary>
    public void ValidateActiveBucket()
    {
        Debug.Log("[BucketManager] 開始驗證活躍水桶...");
        
        if (taskManager == null)
        {
            OnBucketError?.Invoke("TaskManager 未初始化");
            return;
        }
        
        // 困難模式平行任務：使用 MultiBucketManager 驗證
        if (multiBucketManager != null && multiBucketManager.IsHardMode)
        {
            ValidateHardModeBuckets();
            return;
        }
        
        // 普通模式：驗證單一水桶
        ValidateNormalModeBucket();
    }
    
    /// <summary>
    /// 驗證普通模式水桶
    /// </summary>
    private void ValidateNormalModeBucket()
    {
        // 1. 從 Data Layer 獲取數據
        BucketEvent activeBucket = GetActiveBucketFromDataLayer();
        if (activeBucket == null)
        {
            OnBucketError?.Invoke("無法獲取有效的 BucketEvent");
            return;
        }
        
        Debug.Log($"[BucketManager] 使用水桶: {activeBucket.gameObject.name}");
        
        // 2. 獲取水桶內的魚數據
        var fishInBucket = activeBucket.GetFishInBucket();
        
        // 3. 執行業務邏輯驗證
        TaskValidationResult result = taskManager.ValidateTask(fishInBucket);
        Debug.Log($"[BucketManager] 驗證結果: {result}");
        
        // 4. 處理驗證結果
        ProcessValidationResult(result, activeBucket);
        
        // 5. 發布事件給上層
        OnBucketValidated?.Invoke(new BucketValidationResult 
        { 
            IsValid = result == TaskValidationResult.Success || result == TaskValidationResult.SubTaskComplete,
            Result = result,
            Message = GetValidationMessage(result)
        });
    }
    
    /// <summary>
    /// 驗證困難模式所有水桶
    /// </summary>
    private void ValidateHardModeBuckets()
    {
        Debug.Log("[BucketManager] 困難模式平行任務驗證");
        
        bool allValid = multiBucketManager.ValidateAllBuckets();
        
        if (allValid)
        {
            Debug.Log("[BucketManager] 所有水桶任務完成！");
            OnValidationSuccess?.Invoke();
        }
        else
        {
            Debug.Log("[BucketManager] 尚有水桶未完成或有錯誤");
            OnValidationIncomplete?.Invoke();
        }
    }
    
    /// <summary>
    /// 處理驗證結果的業務邏輯
    /// </summary>
    private void ProcessValidationResult(TaskValidationResult result, BucketEvent bucket)
    {
        switch (result)
        {
            case TaskValidationResult.Success:
                // 任務完成，清空桶並生成新任務
                bucket.ClearBucket();
                OnValidationSuccess?.Invoke();
                Debug.Log("[BucketManager] 任務完成，生成新任務");
                break;
                
            case TaskValidationResult.Failed:
                // 任務失敗，清空桶並重置任務
                bucket.ClearBucket();
                OnValidationFailed?.Invoke();
                Debug.Log("[BucketManager] 任務失敗，重新生成任務和魚");
                break;
                
            case TaskValidationResult.Incomplete:
                // 任務不完整，提示玩家繼續完成任務
                OnValidationIncomplete?.Invoke();
                break;
                
            case TaskValidationResult.SubTaskComplete:
                // 子任務完成，清空桶並繼續下一個子任務
                bucket.ClearBucket();
                OnValidationSuccess?.Invoke();
                break;
        }
    }
    
    /// <summary>
    /// 清空當前活躍的水桶（被 Event Layer 調用）
    /// </summary>
    public void ClearActiveBucket()
    {
        Debug.Log("[BucketManager] 清空活躍水桶");
        
        if (multiBucketManager == null) return;
        
        if (multiBucketManager.IsHardMode)
        {
            multiBucketManager.ClearAllBuckets();
        }
        else
        {
            BucketEvent normalBucket = multiBucketManager.GetNormalModeBucketEvent();
            normalBucket?.ClearBucket();
        }
        
        OnBucketCleared?.Invoke();
    }
    
    /// <summary>
    /// 處理魚進入水桶事件（被 BucketEvent/Data Layer 調用）
    /// 這是 Data Layer 到 Business Layer 的回調
    /// </summary>
    public void OnFishEntered(int bucketIndex, GameObject fish, BucketValidationState state)
    {
        Debug.Log($"[BucketManager] 魚進入水桶 {bucketIndex}, 狀態: 滿={state.IsFull}, 顏色正確={state.IsColorCorrect}");
        
        // 業務邏輯：檢查是否需要發布錯誤事件
        if (multiBucketManager != null && multiBucketManager.IsHardMode)
        {
            // 困難模式邏輯
            if (state.IsFull)
            {
                OnBucketError?.Invoke($"水桶 {bucketIndex + 1} 已滿！只需要 {state.Capacity} 隻魚");
            }
            else if (!state.IsColorCorrect)
            {
                string targetColorName = FishColorHelper.GetDisplayName(state.TargetColor);
                string actualColorName = FishColorHelper.GetDisplayName(state.ActualColor);
                OnBucketError?.Invoke($"水桶 {bucketIndex + 1} 顏色錯誤！需要 {targetColorName}，但放入了 {actualColorName}");
            }
        }
        
        // 發布魚進入事件
        OnFishEnteredBucket?.Invoke(bucketIndex, fish);
    }
    
    /// <summary>
    /// 從 Data Layer 獲取當前活躍的水桶
    /// </summary>
    private BucketEvent GetActiveBucketFromDataLayer()
    {
        if (multiBucketManager == null)
        {
            // 備用方案：直接查找
            return Object.FindFirstObjectByType<BucketEvent>();
        }
        
        if (multiBucketManager.IsHardMode)
        {
            // 困難模式由 MultiBucketManager 統一處理
            return null;
        }
        else
        {
            return multiBucketManager.GetNormalModeBucketEvent();
        }
    }
    
    /// <summary>
    /// 獲取驗證結果的訊息
    /// </summary>
    private string GetValidationMessage(TaskValidationResult result)
    {
        switch (result)
        {
            case TaskValidationResult.Success:
                return "任務完成！";
            case TaskValidationResult.Failed:
                return "任務失敗";
            case TaskValidationResult.Incomplete:
                return "任務未完成，請繼續";
            case TaskValidationResult.SubTaskComplete:
                return "階段完成！";
            default:
                return "";
        }
    }
}
