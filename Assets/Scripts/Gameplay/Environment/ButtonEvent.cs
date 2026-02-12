using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Oculus.Haptics;
using Oculus.Interaction;

public class ButtonEvent : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("檢測模式：Trigger / Collision / Both")]
    public DetectionMode detectionMode = DetectionMode.Trigger;
    
    [Tooltip("觸發按鈕的標籤（例如：Player, Hand, GrabPole）")]
    [SerializeField] private string triggerTag = "Player";
    
    [Tooltip("按鈕是否被按下")]
    public bool isPressed = false;
    
    [Tooltip("是否可以重複按下")]
    [SerializeField] private bool canRepeatPress = true;
    
    [Tooltip("按鈕冷卻時間（秒）")]
    [SerializeField] private float cooldownTime = 2f;
    
    private float lastPressTime = -999f; // 上次按下的時間
    
    public enum DetectionMode
    {
        Trigger,
        Collision,
        Both,
        MetaXR  // 使用 Meta XR Grabbable 組件
    }
    
    [Header("Meta XR Settings")]
    [Tooltip("Grabbable 組件引用（用於 MetaXR 檢測模式）")]
    [SerializeField] private Grabbable grabbable;
    
    [Tooltip("自動獲取 Grabbable 組件")]
    [SerializeField] private bool autoGetGrabbable = true;
    
    [Header("Controller Settings")]
    [Tooltip("抓取動作（分配 grip 或 trigger）")]
    public InputActionProperty grabAction;
    
    private bool isHoveredByHand = false; // 手是否懸停在按鈕上
    
    [Header("Visual Effects")]
    [Tooltip("啟用視覺效果（發光、材質切換、動畫）")]
    [SerializeField] private bool enableVisualEffects = false;
    
    [Tooltip("發光顏色")]
    public Color glowColor = Color.yellow;
    
    [Tooltip("發光強度")]
    public float glowIntensity = 2.0f;
    
    [Tooltip("選中時的材質")]
    public Material selectedMaterial;
    
    [Tooltip("正常狀態的材質")]
    public Material normalMaterial;
    
    [Tooltip("是否被選中")]
    public bool isSelected = false;
    
    [Header("Hover Animation")]
    [Tooltip("啟用懸停動畫")]
    [SerializeField] private bool enableHoverAnimation = false;
    
    [Tooltip("懸停時的縮放")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    
    [Tooltip("懸停時的浮動高度")]
    public float floatHeight = 0.05f;
    
    [Tooltip("動畫速度")]
    public float animationSpeed = 6f;
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 targetScale;
    private Vector3 targetPosition;
    private bool isHovered = false;
    
    private Renderer buttonRenderer;
    private Material buttonMaterial;
    private Color originalEmission;
    private bool isGlowing = false;
    
    [Header("Haptic Feedback")]
    [Tooltip("啟用觸覺反饋（需要 Oculus Haptics）")]
    [SerializeField] private bool enableHaptics = false;
    
    [Tooltip("震動片段")]
    [SerializeField] public HapticClip hapticClip;
    
    private HapticClipPlayer hapticPlayer;
    
    [Header("Audio")]
    [Tooltip("按鈕按下時的音效")]
    [SerializeField] private AudioClip buttonPressSound;
    
    [Tooltip("按鈕釋放時的音效（可選）")]
    [SerializeField] private AudioClip buttonReleaseSound;
    
    [Tooltip("AudioSource 組件（自動獲取或手動指定）")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("音效音量（0-1）")]
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;
    
    [Header("Events")]
    [Tooltip("按鈕被按下時觸發")]
    public UnityEvent onButtonPressed;
    
    [Tooltip("按鈕被釋放時觸發")]
    public UnityEvent onButtonReleased;
    
    void Start()
    {
        Debug.Log($"[ButtonEvent] {gameObject.name} 已初始化，觸發標籤：{triggerTag}");
        
        // 初始化視覺效果
        if (enableVisualEffects)
        {
            InitializeVisualEffects();
        }
        
        // 初始化懸停動畫
        if (enableHoverAnimation)
        {
            originalScale = transform.localScale;
            originalPosition = transform.localPosition;
            targetScale = originalScale;
            targetPosition = originalPosition;
        }
        
        // 初始化觸覺反饋
        if (enableHaptics && hapticClip != null)
        {
            hapticPlayer = new HapticClipPlayer(hapticClip);
        }
        
        // 初始化 Meta XR Grabbable
        if (detectionMode == DetectionMode.MetaXR)
        {
            InitializeMetaXRGrabbable();
        }
        
        // 自動獲取 AudioSource 組件
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            // 如果沒有 AudioSource，嘗試添加一個
            if (audioSource == null && (buttonPressSound != null || buttonReleaseSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                Debug.Log($"[ButtonEvent] 已自動添加 AudioSource 組件");
            }
        }
    }
    
    void Update()
    {
        // 處理視覺效果
        if (enableVisualEffects && buttonRenderer != null)
        {
            // 材質選擇
            buttonRenderer.material = isSelected ? selectedMaterial : normalMaterial;
        }
        
        // 處理懸停動畫
        if (enableHoverAnimation)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * animationSpeed);
        }
        
    }
    
    /// <summary>
    /// 初始化視覺效果系統
    /// </summary>
    private void InitializeVisualEffects()
    {
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
        {
            buttonMaterial = buttonRenderer.material;
            buttonMaterial.EnableKeyword("_EMISSION");
            originalEmission = buttonMaterial.GetColor("_EmissionColor");
        }
    }
    
    /// <summary>
    /// 初始化 Meta XR Grabbable 組件
    /// </summary>
    private void InitializeMetaXRGrabbable()
    {
        // 自動獲取 Grabbable 組件
        if (autoGetGrabbable && grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
            if (grabbable == null)
            {
                Debug.LogWarning($"[ButtonEvent] {gameObject.name} 設定為 MetaXR 模式，但找不到 Grabbable 組件！");
                Debug.LogWarning($"[ButtonEvent] 請在 Inspector 中添加 Grabbable 組件，並將 OnButtonGrabbed 綁定到 'When Select' 事件");
                Debug.LogWarning($"[ButtonEvent] 將 OnButtonReleased 綁定到 'When Unselect' 事件");
                return;
            }
        }
        
        if (grabbable != null)
        {
            Debug.Log($"[ButtonEvent] {gameObject.name} 已找到 Grabbable 組件");
            Debug.Log($"[ButtonEvent] 請確保在 Grabbable 組件的 UnityEvent 中綁定：");
            Debug.Log($"  - 'When Select' → OnButtonGrabbed()");
            Debug.Log($"  - 'When Unselect' → OnButtonReleased()");
        }
    }
    
    /// <summary>
    /// 當按鈕被抓取時調用（從 Grabbable 的 When Select 事件綁定）
    /// </summary>
    public void OnButtonGrabbed()
    {
        // 檢查冷卻時間
        if (Time.time - lastPressTime < cooldownTime)
        {
            Debug.Log($"[ButtonEvent] 按鈕在冷卻中，剩餘時間：{cooldownTime - (Time.time - lastPressTime):F1}秒");
            return;
        }
        
        // 如果不允許重複按下且已經按下，則忽略
        if (!canRepeatPress && isPressed)
        {
            Debug.Log($"[ButtonEvent] 按鈕已按下，不允許重複按下");
            return;
        }
        
        isPressed = true;
        
        Debug.Log($"[ButtonEvent] Meta XR Grabbable - isPressed 設為 true（觸發按下事件）");
        
        // 觸發按下事件
        onButtonPressed?.Invoke();
        lastPressTime = Time.time;
    }
    
    /// <summary>
    /// 當按鈕被釋放時調用（從 Grabbable 的 When Unselect 事件綁定）
    /// </summary>
    public void OnButtonReleased()
    {
        if (canRepeatPress)
        {
            isPressed = false;
        }
        
        Debug.Log($"[ButtonEvent] Meta XR Grabbable - isPressed 設為 false（觸發釋放事件）");
        
        // 觸發釋放事件
        onButtonReleased?.Invoke();
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 只在 Trigger 或 Both 模式下處理
        if (detectionMode == DetectionMode.Collision)
            return;
        
        // 只有當 tag 正確時才播放音效和觸覺反饋
        if (other.CompareTag(triggerTag))
        {
            hapticPlay();
            PlayButtonPressSound();
        }
            
        HandleEnter(other.CompareTag(triggerTag));
    }
    
    void OnTriggerExit(Collider other)
    {
        PlayButtonReleaseSound();
        // 只在 Trigger 或 Both 模式下處理
        if (detectionMode == DetectionMode.Collision)
            return;
            
        HandleExit(other.CompareTag(triggerTag));
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // 只在 Collision 或 Both 模式下處理
        if (detectionMode == DetectionMode.Trigger)
            return;
        
        // 只有當 tag 正確時才播放音效和觸覺反饋
        if (collision.gameObject.CompareTag(triggerTag))
        {
            hapticPlay();
            PlayButtonPressSound();
        }
            
        HandleEnter(collision.gameObject.CompareTag(triggerTag));
    }
    
    void OnCollisionExit(Collision collision)
    {
        PlayButtonReleaseSound();
        // 只在 Collision 或 Both 模式下處理
        if (detectionMode == DetectionMode.Trigger)
            return;
            
        HandleExit(collision.gameObject.CompareTag(triggerTag));
    }
    
    /// <summary>
    /// 統一處理進入事件（只處理視覺效果，不觸發按下）
    /// </summary>
    private void HandleEnter(bool isValidTag)
    {
        if (!isValidTag)
            return;
        
        // 設置為選中狀態（僅視覺效果）
        isSelected = true;
        isHovered = true;
        isHoveredByHand = true; // 標記手懸停在按鈕上
        
        Debug.Log($"[ButtonEvent] Trigger Enter - isSelected 設為 true（視覺效果），isHoveredByHand = true");
        
        // 視覺效果
        if (enableVisualEffects)
        {
            SetGlow(true);
        }
        
        // 懸停動畫
        if (enableHoverAnimation)
        {
            targetScale = originalScale * hoverScale.x;
            targetPosition = originalPosition + new Vector3(0, 0, 0);
        }
    }
    
    /// <summary>
    /// 統一處理離開事件（只處理視覺效果）
    /// </summary>
    private void HandleExit(bool isValidTag)
    {
        if (!isValidTag)
            return;
        
        // 恢復選中狀態（僅視覺效果）
        isSelected = false;
        isHovered = false;
        isHoveredByHand = false; // 標記手離開按鈕
        
        Debug.Log($"[ButtonEvent] Trigger Exit - isSelected 設為 false（恢復視覺），isHoveredByHand = false");
        
        // 視覺效果
        if (enableVisualEffects)
        {
            SetGlow(false);
        }
        
        // 懸停動畫
        if (enableHoverAnimation)
        {
            targetScale = originalScale;
            targetPosition = originalPosition;
        }
    }
    
    /// <summary>
    /// 設置發光效果
    /// </summary>
    private void SetGlow(bool glow)
    {
        if (buttonMaterial == null) return;

        if (glow && !isGlowing)
        {
            buttonMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
            DynamicGI.SetEmissive(buttonRenderer, glowColor * glowIntensity);
            isGlowing = true;
        }
        else if (!glow && isGlowing)
        {
            buttonMaterial.SetColor("_EmissionColor", originalEmission);
            DynamicGI.SetEmissive(buttonRenderer, originalEmission);
            isGlowing = false;
        }
    }
    
    /// <summary>
    /// 按下按鈕（由 Update 中的 InputAction 調用）
    /// </summary>
    private void HandleButtonPress()
    {
        if (!canRepeatPress && isPressed)
        {
            return;
        }
        
        isPressed = true;
        
        Debug.Log($"[ButtonEvent] 按鈕被按下（InputAction grabValue > 0.8）");
        
        // 觸覺反饋
        if (enableHaptics && hapticPlayer != null)
        {
            hapticPlayer.Play(Controller.Right);
        }
        
        // 播放按下音效
        PlayButtonPressSound();
        
        // 觸發按下事件
        onButtonPressed?.Invoke();
        lastPressTime = Time.time;
    }
    
    /// <summary>
    /// 釋放按鈕（由 Update 中的 InputAction 調用）
    /// </summary>
    private void HandleButtonRelease()
    {
        if (!isPressed)
            return;
        
        if (canRepeatPress)
        {
            isPressed = false;
        }
        
        Debug.Log($"[ButtonEvent] 按鈕被釋放（InputAction grabValue < 0.2）");
        
        // 播放釋放音效
        PlayButtonReleaseSound();
        
        // 觸發釋放事件
        onButtonReleased?.Invoke();
    }
    
    
    /// <summary>
    /// 手動重置按鈕狀態（用於外部調用）
    /// </summary>
    public void ResetButton()
    {
        isPressed = false;
        lastPressTime = -999f; // 重置冷卻時間
    }
    
    /// <summary>
    /// 手動按下按鈕（用於外部調用）
    /// </summary>
    public void PressButton()
    {
        // 檢查冷卻時間
        if (Time.time - lastPressTime < cooldownTime)
        {
            return;
        }
        
        if (!isPressed || canRepeatPress)
        {
            isPressed = true;
            onButtonPressed?.Invoke();
            lastPressTime = Time.time;
        }
    }
    
    /// <summary>
    /// 檢查按鈕是否在冷卻中
    /// </summary>
    public bool IsOnCooldown()
    {
        return Time.time - lastPressTime < cooldownTime;
    }
    
    /// <summary>
    /// 獲取剩餘冷卻時間
    /// </summary>
    public float GetRemainingCooldown()
    {
        float remaining = cooldownTime - (Time.time - lastPressTime);
        return Mathf.Max(0f, remaining);
    }
    
    /// <summary>
    /// 播放按鈕按下音效
    /// </summary>
    private void PlayButtonPressSound()
    {
        if (buttonPressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonPressSound, soundVolume);
        }
    }
    
    /// <summary>
    /// 播放按鈕釋放音效
    /// </summary>
    private void PlayButtonReleaseSound()
    {
        if (buttonReleaseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonReleaseSound, soundVolume);
        }
    }

    void hapticPlay(){
      // 觸覺反饋
        if (enableHaptics && hapticPlayer != null)
        {
            hapticPlayer.Play(Controller.Right);
            Debug.Log($"[ButtonEvent] 播放觸覺反饋");
        }
   }
}
