using UnityEngine;

/// <summary>
/// 單一水桶的重試按鈕 - 困難模式平行任務專用
/// 允許玩家獨立重置某個水桶而不影響其他水桶
/// 使用 ButtonEvent 機制（物理碰撞觸發）
/// </summary>
public class BucketRetryButton : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("對應的水桶索引（0-based）")]
    [SerializeField] private int bucketIndex = 0;
    
    [Header("ButtonEvent 參考")]
    [Tooltip("按鈕的 ButtonEvent 組件（自動獲取或手動指定）")]
    [SerializeField] private ButtonEvent buttonEvent;
    
    [Header("UI 參考（選用）")]
    [Tooltip("按鈕文字")]
    [SerializeField] private TMPro.TMP_Text buttonText;
    
    [Tooltip("錯誤時顯示的物件")]
    [SerializeField] private GameObject errorIndicator;
    
    [Tooltip("按鈕視覺物件（用於顯示/隱藏，不影響事件訂閱）")]
    [SerializeField] private GameObject visualObject;
    
    private MultiBucketManager multiBucketManager;
    
    private void Awake()
    {
        // 如果未手動指定 ButtonEvent，嘗試自動獲取
        if (buttonEvent == null)
        {
            buttonEvent = GetComponent<ButtonEvent>();
        }
        
        if (buttonEvent == null)
        {
            Debug.LogWarning($"[BucketRetryButton] 未找到 ButtonEvent 組件！請確保此物件或子物件包含 ButtonEvent。");
        }
    }
    
    private void Start()
    {
        multiBucketManager = MultiBucketManager.Instance;
        
        if (multiBucketManager == null)
        {
            Debug.LogWarning($"[BucketRetryButton] MultiBucketManager 未找到！");
        }
        
        // 訂閱 ButtonEvent 的按下事件
        if (buttonEvent != null)
        {
            buttonEvent.onButtonPressed.AddListener(OnRetryButtonPressed);
            Debug.Log($"[BucketRetryButton] 已訂閱 ButtonEvent.onButtonPressed (水桶 {bucketIndex})");
        }
        
        // 訂閱錯誤事件以顯示/隱藏按鈕
        if (multiBucketManager != null)
        {
            multiBucketManager.OnBucketError.AddListener(OnBucketError);
            multiBucketManager.OnBucketStageCompleted.AddListener(OnBucketCompleted);
        }
        
        // 預設隱藏錯誤指示器
        if (errorIndicator != null)
        {
            errorIndicator.SetActive(false);
        }
        
        // 預設隱藏按鈕視覺（但保持物件啟用以接收事件）
        HideButton();
        
        Debug.Log($"[BucketRetryButton] 水桶 {bucketIndex} 重試按鈕已初始化（預設隱藏）");
    }
    
    private void OnDestroy()
    {
        // 取消訂閱 ButtonEvent
        if (buttonEvent != null)
        {
            buttonEvent.onButtonPressed.RemoveListener(OnRetryButtonPressed);
        }
        
        // 取消訂閱 MultiBucketManager
        if (multiBucketManager != null)
        {
            multiBucketManager.OnBucketError.RemoveListener(OnBucketError);
            multiBucketManager.OnBucketStageCompleted.RemoveListener(OnBucketCompleted);
        }
    }
    
    /// <summary>
    /// 當 ButtonEvent 被觸發（玩家按下按鈕）
    /// </summary>
    public void OnRetryButtonPressed()
    {
        if (multiBucketManager == null)
        {
            Debug.LogError("[BucketRetryButton] MultiBucketManager 為 null！");
            return;
        }
        
        Debug.Log($"[BucketRetryButton] 玩家按下重試按鈕（水桶 {bucketIndex + 1}）");
        
        // 呼叫 MultiBucketManager 重置單一水桶，清空桶內全部的魚
        multiBucketManager.ResetSingleBucket(bucketIndex);
        
        // 隱藏錯誤指示器
        if (errorIndicator != null)
        {
            errorIndicator.SetActive(false);
        }
        
        // 【修改】移除隱藏按鈕的邏輯，讓重試按鈕保持可被按下
        // HideButton();
        
        // 更新按鈕文字
        UpdateButtonText(false);
    }
    
    /// <summary>
    /// 當水桶發生錯誤時
    /// </summary>
    private void OnBucketError(int errorBucketIndex, string errorMessage)
    {
        // 只處理對應的水桶
        if (errorBucketIndex == bucketIndex)
        {
            Debug.Log($"[BucketRetryButton] 水桶 {bucketIndex + 1} 發生錯誤: {errorMessage}");
            
            // 顯示錯誤指示器
            if (errorIndicator != null)
            {
                errorIndicator.SetActive(true);
            }
            
            // 顯示按鈕本身（讓玩家可以按下重試）
            ShowButton();
            
            // 更新按鈕文字
            UpdateButtonText(true);
        }
    }
    
    /// <summary>
    /// 當水桶完成時
    /// </summary>
    private void OnBucketCompleted(int completedBucketIndex)
    {
        // 只處理對應的水桶
        if (completedBucketIndex == bucketIndex)
        {
            Debug.Log($"[BucketRetryButton] 水桶 {bucketIndex + 1} 已完成");
            
            // 隱藏錯誤指示器
            if (errorIndicator != null)
            {
                errorIndicator.SetActive(false);
            }
            
            // 【修改】移除隱藏按鈕的邏輯，讓按鈕保持可被按下
            
            // 更新按鈕文字
            UpdateButtonText(false);
        }
    }
    
    /// <summary>
    /// 更新按鈕文字
    /// </summary>
    private void UpdateButtonText(bool showError)
    {
        if (buttonText != null)
        {
            if (showError)
            {
                buttonText.text = $"重試水桶{bucketIndex + 1}";
            }
            else
            {
                buttonText.text = $"水桶{bucketIndex + 1}";
            }
        }
    }
    
    /// <summary>
    /// 設置按鈕對應的水桶索引（動態設定用）
    /// </summary>
    public void SetBucketIndex(int index)
    {
        bucketIndex = index;
        UpdateButtonText(false);
    }
    
    /// <summary>
    /// 獲取當前水桶索引
    /// </summary>
    public int GetBucketIndex() => bucketIndex;
    
    /// <summary>
    /// 顯示按鈕（啟用 ButtonEvent 和視覺物件）
    /// </summary>
    private void ShowButton()
    {
        // 啟用 ButtonEvent 組件
        if (buttonEvent != null)
        {
            buttonEvent.enabled = true;
            // 重置按鈕狀態，清除冷卻時間
            buttonEvent.ResetButton();
        }
        
        // 顯示視覺物件
        if (visualObject != null)
        {
            visualObject.SetActive(true);
        }
        else
        {
            // 如果沒有設置 visualObject，則顯示整個物件
            // 但這可能導致子物件的其他組件也被啟用
            gameObject.SetActive(true);
        }
        
        Debug.Log($"[BucketRetryButton] 水桶 {bucketIndex} 重試按鈕已顯示（已重置冷卻）");
    }
    
    /// <summary>
    /// 隱藏按鈕（禁用 ButtonEvent 但保持物件啟用以接收事件）
    /// </summary>
    private void HideButton()
    {
        // 禁用 ButtonEvent 組件（防止誤觸）
        if (buttonEvent != null)
        {
            buttonEvent.enabled = false;
        }
        
        // 隱藏視覺物件
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        // 注意：不隱藏 gameObject 本身，以保持事件訂閱有效
        
        Debug.Log($"[BucketRetryButton] 水桶 {bucketIndex} 重試按鈕已隱藏");
    }
}
