using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 水桶數據模型 - Data Layer
/// 只負責數據存儲和變更通知，不包含業務邏輯
/// 
/// 職責：
/// 1. 追蹤桶內的魚數據
/// 2. 發布數據變更事件
/// 3. 提供數據訪問接口
/// 4. 不依賴任何 Business Layer (Managers)
/// </summary>
public class BucketEvent : MonoBehaviour
{
    [SerializeField] private TMP_Text bucketText;
    [SerializeField] private TMP_Text statisticsText;
    private FishSpawnManager fishSpawnManager;
    
    private int fishCount = 0; 
    private List<Fish> fishes;
    private Dictionary<string, int> fishInBucket = new Dictionary<string, int>(); 
    private bool isInitialized = false;
    
    // 任务系统：追踪桶中的鱼GameObject列表
    private List<GameObject> fishGameObjectsInBucket = new List<GameObject>();
    
    // 困難模式：追蹤魚的進入順序
    private List<GameObject> fishEntryOrder = new List<GameObject>();
    
    // 困難模式：鎖定的魚（不可取出）
    private HashSet<GameObject> lockedFish = new HashSet<GameObject>();
    
    // 當前是否為困難模式
    private bool isHardMode = false;
    
    // 是否被 MultiBucketManager 管理（進行嚴格顏色檢查）
    private bool isMultiBucketManaged = false;
    
    // 水桶狀態（平行任務模式）
    public enum BucketStatus
    {
        Normal,      // 正常狀態
        Error,       // 錯誤狀態（等待重試）
        Completed    // 完成狀態
    }
    private BucketStatus currentStatus = BucketStatus.Normal;
    
    // ========== 多水桶支援 ==========
    [Header("多水桶設定")]
    [Tooltip("此水桶對應的階段索引（0-based）")]
    [SerializeField] private int stageIndex = 0;
    
    [Tooltip("此水桶的魚容量限制（0 = 無限制）")]
    [SerializeField] private int capacity = 0;
    
    [Tooltip("目標魚顏色")]
    [SerializeField] private FishColor targetColor = FishColor.Red;
    
    [Tooltip("魚被彈出時的力道")]
    [SerializeField] private float ejectForce = 3f;
    
    [Tooltip("魚進入此桶後的返回點位置（可設為桶子的子物件）")]
    [SerializeField] private Transform fishReturnPoint;
    
    // ========== Data Layer 事件：向 Business Layer 發布數據變更 ==========
    [Header("數據事件（向 Business Layer 發布）")]
    public UnityEvent<BucketDataChangedEvent> OnDataChanged;
    
    // 用於通知 BucketManager 的引用（依賴注入）
    private BucketManager bucketManager;

    private void Awake()
    {
        // 初始化事件
        if (OnDataChanged == null)
        {
            OnDataChanged = new UnityEvent<BucketDataChangedEvent>();
        }
        
        // 使用 FishTags 動態初始化字典（確保與 FishTags.cs 同步）
        foreach (string fishTag in FishTags.GetAllFishTags())
        {
            if (!fishInBucket.ContainsKey(fishTag))
            {
                fishInBucket[fishTag] = 0;
            }
        }
    }

    private void Start()
    {
        // 通过 ServiceLocator 获取 FishSpawnManager
        fishSpawnManager = ServiceLocator.Instance.Get<FishSpawnManager>();
        
        // ✅ 獲取 BucketManager（Business Layer）用於發布事件
        if (!ServiceLocator.Instance.TryGet(out bucketManager))
        {
            Debug.LogWarning($"[BucketEvent] BucketManager 未找到，將無法發布業務事件");
        }
        
        // initialize Fish data in Start to ensure Generator is ready
        fishes = fishSpawnManager != null ? fishSpawnManager.GetFish() : new List<Fish>();
        isInitialized = true;
        
        // 初始化 UI 顯示
        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        // make sure initialized.
        if (!isInitialized) return;
        
        // 【修正】確保此水桶的 GameObject 是啟用的
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[BucketEvent] {gameObject.name} 未啟用，忽略魚的進入事件");
            return;
        }
        
        string fishTag = GetFishTag(other.gameObject);
        if (string.IsNullOrEmpty(fishTag)) return;
        
        Debug.Log($"[BucketEvent] {gameObject.name}: {fishTag} 進入桶子");
        
        // ========== 只更新數據，業務邏輯判斷移至 BucketManager ==========
        FishColor fishColor = FishColorHelper.GetColorFromTag(fishTag);
        
        // 創建驗證狀態（純數據，不做業務判斷）
        BucketValidationState validationState = new BucketValidationState
        {
            IsFull = capacity > 0 && fishGameObjectsInBucket.Count >= capacity,
            IsColorCorrect = fishColor == targetColor,
            CurrentCount = fishGameObjectsInBucket.Count,
            Capacity = capacity,
            ActualColor = fishColor,
            TargetColor = targetColor
        };
        
        // ✅ 先將魚添加到列表（所有進入水桶的魚都要記錄，包括顏色錯誤的）
        // 这样 ClearBucket() 才能清空所有鱼
        if (!fishGameObjectsInBucket.Contains(other.gameObject))
        {
            fishGameObjectsInBucket.Add(other.gameObject);
            
            // 設置魚的 isInBucket 狀態
            FishForwardMovement fishMovement = other.GetComponent<FishForwardMovement>();
            if (fishMovement != null)
            {
                fishMovement.isInBucket = true;
            }
        }
        
        // 多水桶模式：檢查錯誤情況
        if (isMultiBucketManaged && capacity > 0)
        {
            if (currentStatus == BucketStatus.Error)
            {
                Debug.LogWarning($"[BucketEvent] 水桶處於錯誤狀態，請按重試按鈕！");
                // ❌ 不要在這裡移除魚，因為玩家可能需要重試後清空
                return;
            }
            
            if (validationState.IsFull)
            {
                Debug.LogWarning($"[BucketEvent] 水桶已滿！容量: {capacity}");
                currentStatus = BucketStatus.Error;
                PublishErrorEvent(BucketEventType.Full, validationState, other.gameObject);
                return;  // 發布錯誤後返回，但魚已在列表中
            }
            
            if (!validationState.IsColorCorrect)
            {
                Debug.LogWarning($"[BucketEvent] 顏色錯誤！");
                currentStatus = BucketStatus.Error;
                PublishErrorEvent(BucketEventType.ColorMismatch, validationState, other.gameObject);
                return;  // 發布錯誤後返回，但魚已在列表中
            }
        }
        
        // 困難模式：記錄進入順序並鎖定（只在顏色正確且不在錯誤狀態時）
        if (isHardMode && currentStatus != BucketStatus.Error)
        {
            if (!fishEntryOrder.Contains(other.gameObject))
            {
                fishEntryOrder.Add(other.gameObject);
                lockedFish.Add(other.gameObject);
                
                Rigidbody fishRb = other.GetComponent<Rigidbody>();
                if (fishRb != null)
                {
                    fishRb.isKinematic = true;
                }
            }
        }
        
        // ✅ 只在非錯誤狀態下發布正常的數據變更事件和更新計數
        if (currentStatus != BucketStatus.Error)
        {
            fishCount += 1;
            fishInBucket[fishTag] += 1;
            
            // update corresponding Fish object's caught amount
            Fish fishData = fishes.Find(f => f.color == fishTag);
            if (fishData != null)
            {
                fishData.IncrementCaught();
            }
            
            // 發布數據變更事件到 Business Layer
            PublishDataChangedEvent(BucketEventType.FishAdded, validationState, other.gameObject);
        }
        
        UpdateUI();
        PrintStatistics();
    }
    
    /// <summary>
    /// 發布數據變更事件到 Business Layer（正常事件）
    /// </summary>
    private void PublishDataChangedEvent(BucketEventType eventType, BucketValidationState state, GameObject fish)
    {
        var eventData = new BucketDataChangedEvent
        {
            EventType = eventType,
            BucketIndex = stageIndex,
            Fish = fish,
            FishColor = state.ActualColor,
            CurrentCount = fishGameObjectsInBucket.Count,
            Capacity = capacity,
            TargetColor = targetColor,
            ValidationState = state
        };
        
        // 發布 UnityEvent
        OnDataChanged?.Invoke(eventData);
        
        // ✅ 通知 BucketManager (Business Layer) 而不是直接調用 MultiBucketManager.Instance
        bucketManager?.OnFishEntered(stageIndex, fish, state);
    }
    
    /// <summary>
    /// 發布錯誤事件到 Business Layer
    /// </summary>
    private void PublishErrorEvent(BucketEventType eventType, BucketValidationState state, GameObject fish)
    {
        var eventData = new BucketDataChangedEvent
        {
            EventType = eventType,
            BucketIndex = stageIndex,
            Fish = fish,
            FishColor = state.ActualColor,
            CurrentCount = fishGameObjectsInBucket.Count,
            Capacity = capacity,
            TargetColor = targetColor,
            ValidationState = state
        };
        
        OnDataChanged?.Invoke(eventData);
        bucketManager?.OnFishEntered(stageIndex, fish, state);
    }

    private void OnTriggerStay(Collider other)
    {
        // can add logic for fish staying in the bucket here
    }

    private void OnTriggerExit(Collider other)
    {
        // make sure initialized.
        if (!isInitialized) return;
        
        string fishTag = GetFishTag(other.gameObject);
        
        if (!string.IsNullOrEmpty(fishTag))
        {
            // 困難模式：如果魚已鎖定，阻止離開（強制放回）
            if (isHardMode && lockedFish.Contains(other.gameObject))
            {
                Debug.LogWarning($"[BucketEvent] 困難模式：{fishTag} 已鎖定，無法取出！強制放回桶內");
                
                // 強制將魚推回桶內中心
                Vector3 bucketCenter = transform.position;
                other.transform.position = bucketCenter;
                
                // 重置速度
                Rigidbody fishRb = other.GetComponent<Rigidbody>();
                if (fishRb != null)
                {
                    fishRb.linearVelocity = Vector3.zero;
                    fishRb.angularVelocity = Vector3.zero;
                }
                
                // ✅ 發布鎖定事件（Data Layer 只通知，不決策）
                FishColor lockedFishColor = FishColorHelper.GetColorFromTag(fishTag);
                var lockedState = new BucketValidationState
                {
                    IsColorCorrect = lockedFishColor == targetColor,
                    CurrentCount = fishGameObjectsInBucket.Count,
                    Capacity = capacity,
                    ActualColor = lockedFishColor,
                    TargetColor = targetColor
                };
                PublishDataChangedEvent(BucketEventType.FishLocked, lockedState, other.gameObject);
                return;
            }
            
            Debug.Log($"[BucketEvent] {fishTag} 離開桶子");
            
            // 重置鱼的 isInBucket 状态
            FishForwardMovement fishMovement = other.GetComponent<FishForwardMovement>();
            if (fishMovement != null)
            {
                fishMovement.isInBucket = false;
                Debug.Log($"[BucketEvent] 设置 {fishTag} isInBucket = false");
            }
            
            // 从鱼GameObject列表中移除（任务系统需要）
            fishGameObjectsInBucket.Remove(other.gameObject);
            
            // 非困難模式：也從順序列表中移除
            if (!isHardMode)
            {
                fishEntryOrder.Remove(other.gameObject);
            }
            
            fishCount -= 1;
            fishInBucket[fishTag] -= 1;
            
            // update corresponding Fish object's caught amount
            Fish fishData = fishes.Find(f => f.color == fishTag);
            if (fishData != null)
            {
                fishData.DecrementCaught();
            }
            
            // ✅ 發布魚移除事件到 Business Layer
            FishColor fishColor = FishColorHelper.GetColorFromTag(fishTag);
            var state = new BucketValidationState
            {
                IsColorCorrect = fishColor == targetColor,
                CurrentCount = fishGameObjectsInBucket.Count,
                Capacity = capacity,
                ActualColor = fishColor,
                TargetColor = targetColor
            };
            PublishDataChangedEvent(BucketEventType.FishRemoved, state, other.gameObject);
            
            UpdateUI();
            PrintStatistics();
        }
    }

    /// <summary>
    /// get fish tag from GameObject
    /// </summary>
    private string GetFishTag(GameObject obj)
    {
        // 使用 FishTags 來檢查所有可用的魚類（更易維護和擴展）
        if (FishTags.IsFish(obj))
        {
            return obj.tag;
        }
        return null;
    }

    /// <summary>
    /// UPdate UI display
    /// </summary>
    private void UpdateUI()
    {
        // make sure initialized
        if (!isInitialized || fishes == null) return;
        
        //update bucket fish count display
        if (bucketText != null)
        {
            bucketText.text = $"Fish in bucket: {fishCount}";
        }

        // update detailed statistics display
        if (statisticsText != null)
        {
            string stats = "=== Caught Information ===\n";
            
            foreach (Fish f in fishes)
            {
                stats += $"{GetFishDisplayName(f.color)}: {f.caughtAmount}/{f.spawnedAmount} ";
                stats += $"({f.GetProgress():P0})\n";
            }
            
            int totalCaught = fishSpawnManager != null ? fishSpawnManager.GetTotalCaughtCount() : 0;
            int totalSpawned = fishSpawnManager != null ? fishSpawnManager.GetTotalSpawnedCount() : 0;
            stats += $"\nTotal: {totalCaught}/{totalSpawned}";
            
            statisticsText.text = stats;
        }
    }

    /// <summary>
    /// Print detailed fish statistics to Console
    /// </summary>
    private void PrintStatistics()
    {
        // make sure initialized
        if (!isInitialized || fishes == null) return;
        
        Debug.Log("==================== 魚類統計 ====================");
        foreach (Fish f in fishes)
        {
            Debug.Log(f.ToString());
        }
        
        Debug.Log($"桶內魚數: RedFish {fishInBucket["redFish"]} | BlueFish {fishInBucket["blueFish"]} | GreenFish {fishInBucket["greenFish"]}");
        
        if (fishSpawnManager != null)
        {
            Debug.Log($"總捕獲進度: {fishSpawnManager.GetTotalCaughtCount()}/{fishSpawnManager.GetTotalSpawnedCount()}");
        }
        Debug.Log("=================================================");
    }

    /// <summary>
    /// get fish display name
    /// </summary>
    private string GetFishDisplayName(string tag)
    {
        switch (tag)
        {
            case "redFish": return "redFish";
            case "blueFish": return "blueFish";
            case "greenFish": return "greenFish";
            default: return tag;
        }
    }

    /// <summary>
    /// check if all fish are caught
    /// </summary>
    public bool IsAllFishCaught()
    {
        if (!isInitialized || fishes == null) return false;
        
        foreach (Fish f in fishes)
        {
            if (!f.IsAllCaught())
                return false;
        }
        return true;
    }

    /// <summary>
    ///get overall progress
    /// </summary>
    public float GetOverallProgress()
    {
        if (!isInitialized || fishSpawnManager == null) return 0f;
        
        int totalSpawned = fishSpawnManager.GetTotalSpawnedCount();
        if (totalSpawned == 0) return 0f;
        
        int totalCaught = fishSpawnManager.GetTotalCaughtCount();
        return (float)totalCaught / totalSpawned;
    }
    
    // ========== 任务系统接口 ==========
    
    /// <summary>
    /// 获取桶中的鱼GameObject列表（任务系统使用）
    /// </summary>
    public List<GameObject> GetFishInBucket()
    {
        return new List<GameObject>(fishGameObjectsInBucket);
    }
    
    /// <summary>
    /// 清空桶中的所有鱼（任务系统使用）
    /// Data Layer 只執行清空動作，由 Business Layer 決定何時調用
    /// </summary>
    public void ClearBucket()
    {
        Debug.Log($"[BucketEvent] 清空桶，销毁 {fishGameObjectsInBucket.Count} 条鱼");
        
        int clearedCount = fishGameObjectsInBucket.Count;
        
        // 记录需要重新生成的鱼（按颜色统计）
        Dictionary<string, int> fishToRegenerate = new Dictionary<string, int>();
        
        // 销毁所有桶中的鱼
        foreach (GameObject fish in fishGameObjectsInBucket)
        {
            if (fish != null)
            {
                // 记录鱼的颜色
                string fishTag = GetFishTag(fish);
                if (!string.IsNullOrEmpty(fishTag))
                {
                    if (!fishToRegenerate.ContainsKey(fishTag))
                    {
                        fishToRegenerate[fishTag] = 0;
                    }
                    fishToRegenerate[fishTag]++;
                }
                
                Destroy(fish);
            }
        }
        
        // 调用 FishSpawnManager 重新生成被销毁的鱼
        if (fishToRegenerate.Count > 0)
        {
            FishSpawnManager spawnManager = FindFirstObjectByType<FishSpawnManager>();
            if (spawnManager != null)
            {
                spawnManager.RegenerateFish(fishToRegenerate);
                Debug.Log($"[BucketEvent] 已请求重新生成 {clearedCount} 条鱼");
            }
            else
            {
                Debug.LogWarning("[BucketEvent] 无法找到 FishSpawnManager，无法重新生成鱼！");
            }
        }
        
        // 清空列表
        fishGameObjectsInBucket.Clear();
        
        // 清空困難模式數據
        ClearHardModeData();
        
        // 重置计数
        fishCount = 0;
        foreach (string key in new List<string>(fishInBucket.Keys))
        {
            fishInBucket[key] = 0;
        }
        
        // ✅ 發布清空事件（Data Layer 通知 Business Layer）
        var state = new BucketValidationState
        {
            IsColorCorrect = true,
            CurrentCount = 0,
            Capacity = capacity,
            ActualColor = FishColor.Red,
            TargetColor = targetColor
        };
        
        var eventData = new BucketDataChangedEvent
        {
            EventType = BucketEventType.BucketCleared,
            BucketIndex = stageIndex,
            Fish = null,
            FishColor = FishColor.Red,
            CurrentCount = 0,
            Capacity = capacity,
            TargetColor = targetColor,
            ValidationState = state
        };
        
        OnDataChanged?.Invoke(eventData);
        
        // 更新UI
        UpdateUI();
        
        Debug.Log($"[BucketEvent] 已清空 {clearedCount} 條魚，並通知 Business Layer");
    }
    
    // ========== 困難模式接口 ==========
    
    /// <summary>
    /// 設置困難模式狀態
    /// </summary>
    public void SetHardMode(bool enabled)
    {
        isHardMode = enabled;
        Debug.Log($"[BucketEvent] {gameObject.name} 困難模式: {(enabled ? "啟用" : "停用")}");
        
        if (!enabled)
        {
            ClearHardModeData();
            isMultiBucketManaged = false;
        }
    }
    
    /// <summary>
    /// 設置是否被 MultiBucketManager 管理（進行嚴格顏色檢查）
    /// </summary>
    public void SetMultiBucketManaged(bool managed)
    {
        isMultiBucketManaged = managed;
        Debug.Log($"[BucketEvent] {gameObject.name} 多水桶管理: {(managed ? "啟用" : "停用")}");
    }
    
    /// <summary>
    /// 檢查是否為困難模式
    /// </summary>
    public bool IsHardMode()
    {
        return isHardMode;
    }
    
    /// <summary>
    /// 檢查魚是否已被鎖定（不可取出）
    /// </summary>
    public bool IsFishLocked(GameObject fish)
    {
        return isHardMode && lockedFish.Contains(fish);
    }
    
    /// <summary>
    /// 獲取魚的進入順序列表
    /// </summary>
    public List<GameObject> GetFishEntryOrder()
    {
        return new List<GameObject>(fishEntryOrder);
    }
    
    /// <summary>
    /// 獲取鎖定魚的數量
    /// </summary>
    public int GetLockedFishCount()
    {
        return lockedFish.Count;
    }
    
    /// <summary>
    /// 清空困難模式數據（用於重試）
    /// </summary>
    public void ClearHardModeData()
    {
        fishEntryOrder.Clear();
        lockedFish.Clear();
        Debug.Log("[BucketEvent] 困難模式數據已清空");
    }
    
    /// <summary>
    /// 重試困難模式任務：清空桶並重置狀態
    /// </summary>
    public void RetryHardModeTask()
    {
        if (!isHardMode)
        {
            Debug.LogWarning("[BucketEvent] RetryHardModeTask 只能在困難模式下使用");
            return;
        }
        
        Debug.Log("[BucketEvent] 重試困難模式任務");
        
        // 釋放所有桶中的魚到場景中（不銷毀）
        foreach (GameObject fish in fishGameObjectsInBucket)
        {
            if (fish != null)
            {
                // 解除鎖定
                lockedFish.Remove(fish);
                
                // 解除 kinematic，允許抓取
                Rigidbody fishRb = fish.GetComponent<Rigidbody>();
                if (fishRb != null)
                {
                    fishRb.isKinematic = false;
                    Debug.Log($"[BucketEvent] 解除 kinematic，魚可以被抓取了");
                }
                
                // 重置魚的狀態
                FishForwardMovement fishMovement = fish.GetComponent<FishForwardMovement>();
                if (fishMovement != null)
                {
                    fishMovement.isInBucket = false;
                }
                
                // 將魚移到桶外的隨機位置
                Vector3 releasePosition = transform.position + 
                    new Vector3(
                        UnityEngine.Random.Range(-2f, 2f),
                        UnityEngine.Random.Range(0.5f, 1.5f),
                        UnityEngine.Random.Range(-2f, 2f)
                    );
                fish.transform.position = releasePosition;
                
                string fishTag = GetFishTag(fish);
                if (!string.IsNullOrEmpty(fishTag))
                {
                    fishInBucket[fishTag] -= 1;
                    
                    Fish fishData = fishes.Find(f => f.color == fishTag);
                    if (fishData != null)
                    {
                        fishData.DecrementCaught();
                    }
                }
            }
        }
        
        // 清空列表
        fishGameObjectsInBucket.Clear();
        fishEntryOrder.Clear();
        lockedFish.Clear();
        fishCount = 0;
        
        UpdateUI();
        
        Debug.Log("[BucketEvent] 困難模式任務已重置，魚已釋放");
    }
    
    // ========== 多水桶支援接口 ==========
    
    /// <summary>
    /// 設置此水桶對應的階段索引
    /// </summary>
    public void SetStageIndex(int index)
    {
        stageIndex = index;
        Debug.Log($"[BucketEvent] {gameObject.name} - 設置階段索引: {index}");
    }
    
    /// <summary>
    /// 獲取此水桶對應的階段索引
    /// </summary>
    public int GetStageIndex() => stageIndex;
    
    /// <summary>
    /// 獲取此水桶的魚返回點位置
    /// </summary>
    public Transform GetFishReturnPoint()
    {
        // 如果有設定專屬返回點，使用它
        if (fishReturnPoint != null)
        {
            return fishReturnPoint;
        }
        
        // 備用方案：嘗試找子物件 "FishReturnPoint"
        Transform childPoint = transform.Find("FishReturnPoint");
        if (childPoint != null)
        {
            return childPoint;
        }
        
        // 最後備用：返回水桶自身的 Transform
        return transform;
    }
    
    /// <summary>
    /// 設置此水桶的容量限制
    /// </summary>
    public void SetCapacity(int cap)
    {
        capacity = cap;
        Debug.Log($"[BucketEvent] {gameObject.name} - 設置容量: {cap}");
    }
    
    /// <summary>
    /// 獲取此水桶的容量限制
    /// </summary>
    public int GetCapacity() => capacity;
    
    /// <summary>
    /// 設置此水桶的目標魚顏色
    /// </summary>
    public void SetTargetColor(FishColor color)
    {
        targetColor = color;
        Debug.Log($"[BucketEvent] {gameObject.name} - 設置目標顏色: {color} ({FishColorHelper.GetDisplayName(color)})");
    }
    
    /// <summary>
    /// 獲取此水桶的目標魚顏色
    /// </summary>
    public FishColor GetTargetColor() => targetColor;
    
    /// <summary>
    /// 彈出指定的魚（用於容量超限或顏色錯誤時）
    /// </summary>
    public void EjectFish(GameObject fish)
    {
        if (fish == null) return;
        
        Debug.Log($"[BucketEvent] 彈出魚: {fish.name}");
        
        // 從列表中移除
        fishGameObjectsInBucket.Remove(fish);
        fishEntryOrder.Remove(fish);
        lockedFish.Remove(fish);
        
        // 重置魚的狀態
        FishForwardMovement fishMovement = fish.GetComponent<FishForwardMovement>();
        if (fishMovement != null)
        {
            fishMovement.isInBucket = false;
        }
        
        // 解除 kinematic，允許抓取
        Rigidbody rb = fish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        // 計算彈出方向（向上並向外）
        Vector3 ejectDirection = (fish.transform.position - transform.position).normalized;
        ejectDirection.y = 1f; // 確保向上彈
        ejectDirection = ejectDirection.normalized;
        
        // 應用彈出力
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(ejectDirection * ejectForce, ForceMode.Impulse);
        }
        else
        {
            // 如果沒有 Rigidbody，直接移動位置
            fish.transform.position = transform.position + ejectDirection * 1.5f;
        }
        
        // 更新計數
        string fishTag = GetFishTag(fish);
        if (!string.IsNullOrEmpty(fishTag) && fishInBucket.ContainsKey(fishTag))
        {
            fishCount = Mathf.Max(0, fishCount - 1);
            fishInBucket[fishTag] = Mathf.Max(0, fishInBucket[fishTag] - 1);
            
            Fish fishData = fishes?.Find(f => f.color == fishTag);
            if (fishData != null)
            {
                fishData.DecrementCaught();
            }
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// 檢查此水桶是否已滿
    /// </summary>
    public bool IsFull()
    {
        if (capacity <= 0) return false; // 無容量限制
        return fishGameObjectsInBucket.Count >= capacity;
    }
    
    /// <summary>
    /// 獲取當前魚數量
    /// </summary>
    public int GetCurrentFishCount() => fishGameObjectsInBucket.Count;
    
    /// <summary>
    /// 檢查此水桶是否已達到目標（數量和顏色都正確）
    /// </summary>
    public bool IsTargetReached()
    {
        if (capacity <= 0) return false;
        if (fishGameObjectsInBucket.Count != capacity) return false;
        
        string expectedTag = FishColorHelper.GetTagFromColor(targetColor);
        foreach (var fish in fishGameObjectsInBucket)
        {
            if (fish != null && !fish.CompareTag(expectedTag))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 獲取當前水桶狀態
    /// </summary>
    public BucketStatus GetStatus() => currentStatus;
    
    /// <summary>
    /// 設置水桶狀態
    /// </summary>
    public void SetStatus(BucketStatus status)
    {
        currentStatus = status;
        Debug.Log($"[BucketEvent] {gameObject.name} 狀態設置為: {status}");
    }
    
    /// <summary>
    /// 重置水桶狀態為正常
    /// </summary>
    public void ResetStatus()
    {
        currentStatus = BucketStatus.Normal;
        Debug.Log($"[BucketEvent] {gameObject.name} 狀態已重置為 Normal");
    }
}
