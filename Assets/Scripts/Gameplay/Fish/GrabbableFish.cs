using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// 讓魚可以被 Meta XR Grab Interaction 抓取
/// 需要配合 Grabbable 組件使用
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GrabbableFish : MonoBehaviour
{
    [Header("Fish Settings")]
    [SerializeField] private bool disableMovementWhenGrabbed = true;
    [SerializeField] private bool destroyOnRelease = false;
    [SerializeField] private float releaseVelocityMultiplier = 1.0f;
    
    [Header("References")]
    [SerializeField] private FishSpawnManager fishSpawnManager;
    
    private Rigidbody rb;
    private FishMovement fishMovement;
    private bool isGrabbed = false;
    private string fishColor;
    
    // 困難模式鎖定相關
    private BucketEvent bucketEvent;
    private bool isLockedInBucket = false;
   
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        fishMovement = GetComponent<FishMovement>();
        
        // 從 Tag 判斷魚的顏色
        fishColor = gameObject.tag;
        
        // 設置 Rigidbody
        if (rb != null)
        {
            rb.useGravity = false; // 魚在水中不受重力影響
            rb.linearDamping = 1.0f; // 增加阻力模擬水的阻力
            rb.angularDamping = 0.5f;
        }
    }

    private void Start()
    {
        // 使用 ServiceLocator 獲取 FishSpawnManager
        if (fishSpawnManager == null)
        {
            fishSpawnManager = ServiceLocator.Instance.Get<FishSpawnManager>();
        }
        
        // 【修正】不使用 FindFirstObjectByType，避免找到 normalBucket
        // bucketEvent 應該在困難模式下透過 IsLockedInAnyBucket() 動態尋找
        bucketEvent = null;
    }

    /// <summary>
    /// 當魚被抓取時呼叫
    /// 從 Grabbable 的 UnityEvent 中綁定
    /// </summary>
    public void OnFishGrabbed()
    {
        // 困難模式：檢查魚是否被任何水桶鎖定
        if (IsLockedInAnyBucket())
        {
            Debug.LogWarning($"[GrabbableFish] {fishColor} 已在困難模式下鎖定，無法抓取！");
            isLockedInBucket = true;
            
            // 【修正】只有當 bucketEvent 不是 normalBucket 時才放回
            ForceFishBackToBucket();
            
            // 重置 bucketEvent，避免下次被誤用
            bucketEvent = null;
            
            // 這裡可以添加視覺/聽覺反饋
            // 例如播放"嗡"的錯誤音效、魚閃爍紅色等
            
            return;
        }
        
        isGrabbed = true;
        isLockedInBucket = false;
        
        Debug.Log($"[GrabbableFish] {fishColor} 被抓取了！");
        
        // 停用魚的移動腳本
        if (disableMovementWhenGrabbed && fishMovement != null)
        {
            fishMovement.enabled = false;
        }
        
        // 停止任何現有的速度
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    /// <summary>
    /// 檢查魚是否被任何水桶鎖定
    /// </summary>
    private bool IsLockedInAnyBucket()
    {
        // 如果有 MultiBucketManager 且為困難模式
        if (MultiBucketManager.Instance != null && MultiBucketManager.Instance.IsHardMode)
        {
            // 檢查所有困難模式水桶
            for (int i = 0; i < MultiBucketManager.Instance.GetActiveBucketCount(); i++)
            {
                BucketEvent bucket = MultiBucketManager.Instance.GetBucketEvent(i);
                if (bucket != null && bucket.IsFishLocked(gameObject))
                {
                    bucketEvent = bucket; // 記錄是哪個桶
                    return true;
                }
            }
        }
        
        // // 備用：檢查預設的 bucketEvent（但排除 normalBucket）
        // BucketEvent normalModeBucket = MultiBucketManager.Instance?.GetNormalModeBucketEvent();
        
        // if (bucketEvent != null && bucketEvent.IsFishLocked(gameObject) && bucketEvent != normalModeBucket)
        // {
        //     return true;
        // }
        
        // // 【修改】更完整的檢查：找到所有 BucketEvent 並檢查，但排除 normalBucket
        // BucketEvent[] allBuckets = FindObjectsByType<BucketEvent>(FindObjectsSortMode.None);
        // foreach (var bucket in allBuckets)
        // {
        //     // 【重要】跳過 normalBucket，避免在困難模式下意外觸發
        //     if (bucket == normalModeBucket)
        //         continue;
                
        //     if (bucket.IsFishLocked(gameObject))
        //     {
        //         bucketEvent = bucket; // 記錄是哪個桶
        //         return true;
        //     }
        // }
        
        return false;
    }
    
    /// <summary>
    /// 強制將魚放回桶內
    /// </summary>
    private void ForceFishBackToBucket()
    {
        if (bucketEvent != null)
        {
            // 【修正】確認 bucketEvent 不是 normalBucket 才執行
            BucketEvent normalModeBucket = MultiBucketManager.Instance?.GetNormalModeBucketEvent();
            if (bucketEvent == normalModeBucket)
            {
                Debug.LogWarning($"[GrabbableFish] 警告：bucketEvent 指向 normalBucket，不應該在困難模式下執行！");
                return;
            }
            
            // 將魚傳送回桶內中心
            transform.position = bucketEvent.transform.position;
            
            // 重置速度
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// 當魚被放開時呼叫
    /// 從 Grabbable 的 UnityEvent 中綁定
    /// </summary>
    public void OnFishReleased()
    {
        isGrabbed = false;
        
        Debug.Log($"[GrabbableFish] {fishColor} 被放開了！");
        
        // 選項：放開後銷毀魚（模擬收集）
        if (destroyOnRelease)
        {
            // 通知 Generator 魚被收集了
            UpdateFishCaughtCount();
            
            Destroy(gameObject);
            return;
        }
        
        // 重新啟用移動
        if (disableMovementWhenGrabbed && fishMovement != null)
        {
            fishMovement.enabled = true;
        }
        
        // 給魚一些釋放的速度
        if (rb != null && releaseVelocityMultiplier > 0)
        {
            rb.linearVelocity *= releaseVelocityMultiplier;
        }
    }

    /// <summary>
    /// 更新魚的捕獲計數
    /// </summary>
    private void UpdateFishCaughtCount()
    {
        if (fishSpawnManager == null) return;
        
        Fish fishData = fishSpawnManager.GetFishByColor(fishColor);
        if (fishData != null)
        {
            fishData.IncrementCaught();
            Debug.Log($"[GrabbableFish] {fishData.ToString()}");
        }
    }

    /// <summary>
    /// 當魚被選中（Hover）時呼叫
    /// 可以用來顯示視覺反饋
    /// </summary>
    public void OnFishHoverEnter()
    {
        Debug.Log($"[GrabbableFish] {fishColor} 被指向了");
        // 可以在這裡添加高亮效果
        // 例如：改變材質、播放音效等
    }

    /// <summary>
    /// 當魚不再被選中時呼叫
    /// </summary>
    public void OnFishHoverExit()
    {
        Debug.Log($"[GrabbableFish] {fishColor} 不再被指向");
        // 移除高亮效果
    }

    /// <summary>
    /// 檢查魚是否被抓住
    /// </summary>
    public bool IsGrabbed => isGrabbed;
    
    /// <summary>
    /// 檢查魚是否在困難模式下被鎖定
    /// </summary>
    public bool IsLockedInBucket
    {
        get
        {
            if (isLockedInBucket)
                return true;
            
            // 【修正】確認 bucketEvent 不是 normalBucket 才檢查
            if (bucketEvent != null)
            {
                BucketEvent normalModeBucket = MultiBucketManager.Instance?.GetNormalModeBucketEvent();
                if (bucketEvent != normalModeBucket && bucketEvent.IsFishLocked(gameObject))
                {
                    return true;
                }
            }
            
            return false;
        }
    }

    /// <summary>
    /// 設置是否在放開時銷毀魚
    /// </summary>
    public void SetDestroyOnRelease(bool destroy)
    {
        destroyOnRelease = destroy;
    }

    private void OnDestroy()
    {
        // 清理
        if (isGrabbed)
        {
            Debug.Log($"[GrabbableFish] {fishColor} 被銷毀（可能是被收集了）");
        }
    }

#if UNITY_EDITOR
    // 在 Inspector 中可以測試的按鈕
    [ContextMenu("Test Grab")]
    private void TestGrab()
    {
        OnFishGrabbed();
    }

    [ContextMenu("Test Release")]
    private void TestRelease()
    {
        OnFishReleased();
    }
#endif
}
