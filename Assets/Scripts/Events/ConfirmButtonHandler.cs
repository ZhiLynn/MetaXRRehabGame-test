using UnityEngine;

/// <summary>
/// 確認按鈕處理器 - 只處理用戶輸入，不包含業務邏輯
/// Event Layer - 將用戶指令傳遞給 Business Layer
/// 
/// 職責：
/// 1. 處理用戶按鈕輸入
/// 2. 調用 BucketManager (Business Layer) 執行驗證
/// 3. 處理音效反饋（UI 層面）
/// 4. 不直接操作 Data Layer (BucketEvent)
/// </summary>
public class ConfirmButtonHandler : MonoBehaviour
{
    [Header("依賴（Business Layer）")]
    private BucketManager bucketManager;
    
    [Header("音效設置")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;
    [SerializeField] private AudioClip resetSound;
    
    private void Awake()
    {
        // 獲取 Business Layer 服務
        if (!ServiceLocator.Instance.TryGet(out bucketManager))
        {
            Debug.LogError("[ConfirmButtonHandler] BucketManager 未找到！");
        }
        
        // 訂閱 Business Layer 的事件（音效反饋）
        if (bucketManager != null)
        {
            bucketManager.OnValidationSuccess.AddListener(PlayCorrectSound);
            bucketManager.OnValidationFailed.AddListener(PlayResetSound);
            bucketManager.OnValidationIncomplete.AddListener(PlayIncorrectSound);
        }
        
        Debug.Log("[ConfirmButtonHandler] ✅ Event Layer 初始化完成");
    }
    
    private void OnDestroy()
    {
        // 取消訂閱
        if (bucketManager != null)
        {
            bucketManager.OnValidationSuccess.RemoveListener(PlayCorrectSound);
            bucketManager.OnValidationFailed.RemoveListener(PlayResetSound);
            bucketManager.OnValidationIncomplete.RemoveListener(PlayIncorrectSound);
        }
    }
    
    /// <summary>
    /// 確認按鈕按下（由 ButtonEvent 的 UnityEvent 調用）
    /// ✅ 只處理輸入，將業務邏輯委託給 BucketManager
    /// </summary>
    public void OnConfirmButtonPressed()
    {
        Debug.Log("[ConfirmButtonHandler] 玩家按下確認按鈕");
        
        if (bucketManager == null)
        {
            Debug.LogError("[ConfirmButtonHandler] BucketManager 為 null！");
            return;
        }
        
        // ✅ 將驗證指令傳遞給 Business Layer
        // 不再直接操作 BucketEvent 或包含業務邏輯
        bucketManager.ValidateActiveBucket();
    }
    
    // ========== UI 反饋方法（由 BucketManager 事件觸發）==========
    
    private void PlayCorrectSound()
    {
        if (audioSource != null && correctSound != null)
        {
            audioSource.PlayOneShot(correctSound);
        }
    }
    
    private void PlayResetSound()
    {
        if (audioSource != null && resetSound != null)
        {
            audioSource.PlayOneShot(resetSound);
        }
    }
    
    private void PlayIncorrectSound()
    {
        if (audioSource != null && incorrectSound != null)
        {
            audioSource.PlayOneShot(incorrectSound);
        }
    }
}
