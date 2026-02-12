using UnityEngine;
using UnityEngine.InputSystem;

public class ScoopFish : MonoBehaviour
{
    [Header("References")]
    public Transform snapPoint; // Pole snap location

    private FishForwardMovement hoveredFish; // Lantern near pole
    private FishForwardMovement heldFish;    // Lantern currently held
    [SerializeField]private GrabPoleKeyboardTrigger keyboardpanel;
    private GrabPoleKeyboardTrigger triggeredKeyboard; // 已觸發的鍵盤（仿照 pressedButton）

    private ButtonEvent hoveredButton; // 懸停的按鈕
    private ButtonEvent pressedButton; // 正在按下的按鈕

    [Header("Controller Settings")]
    public InputActionProperty grabAction; // Assign grip/trigger

    private static FishForwardMovement snappedFish = null; // Only one lantern can snap

    void OnEnable() => grabAction.action.Enable();
    void OnDisable() => grabAction.action.Disable();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float grabValue = grabAction.action.ReadValue<float>();
        
        // Debug: 顯示當前狀態
        if (hoveredButton != null && Time.frameCount % 60 == 0) // 每 60 幀顯示一次
        {
            Debug.Log($"[ScoopFish] hoveredButton={hoveredButton.gameObject.name}, grabValue={grabValue:F2}, pressedButton={(pressedButton != null ? pressedButton.gameObject.name : "null")}");
        }

        // Snap any hovered lantern on grab, no basket check
        if (hoveredFish != null && grabValue > 0.6f && heldFish == null && snappedFish == null)
        {
            heldFish = hoveredFish;
            SnapFish(heldFish);
        }

        // Release held lantern
        if (heldFish != null && grabValue < 0.2f)
        {
            ReleaseFish(heldFish);
            heldFish = null;
        }
        
        // 按鈕互動邏輯
        // 當懸停在按鈕上且抓取值 > 0.8 時，按下按鈕
        if (hoveredButton != null && grabValue > 0.6f && pressedButton == null)
        {   
            Debug.Log($"[ScoopFish] 嘗試按下按鈕, grabValue={grabValue:F2}");
            pressedButton = hoveredButton;
            PressButton(pressedButton);
        }
        
        // 當抓取值 < 0.2 時，釋放按鈕
        if (pressedButton != null && grabValue < 0.2f)
        {
            Debug.Log($"[ScoopFish] 嘗試釋放按鈕, grabValue={grabValue:F2}");
            ReleaseButton(pressedButton);
            pressedButton = null;
        }

        // 鍵盤面板互動邏輯（仿照按鈕）
        // 當懸停在鍵盤上且抓取值 > 0.6f 時，觸發鍵盤（且尚未觸發過）
        if (keyboardpanel != null && grabValue > 0.6f && triggeredKeyboard == null)
        {
            Debug.Log($"[ScoopFish] 嘗試打開keyboard, grabValue={grabValue:F2}");
            triggeredKeyboard = keyboardpanel;
            triggeredKeyboard.OnTriggerPressed();
        }
        
        // 當抓取值 < 0.2 時，允許再次觸發
        if (triggeredKeyboard != null && grabValue < 0.2f)
        {
            Debug.Log($"[ScoopFish] 鍵盤觸發狀態重置, grabValue={grabValue:F2}");
            triggeredKeyboard = null;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[ScoopFish] OnCollisionEnter: {collision.gameObject.name}");
        HandleCollisionEnter(collision.gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ScoopFish] OnTriggerEnter: {other.gameObject.name}");
        
        // 【修正】在困難模式下排除 normalBucket
        if (IsNormalBucketInHardMode(other.gameObject))
        {
            Debug.LogWarning($"[ScoopFish] 檢測到 normalBucket 觸發，在困難模式下忽略");
            return;
        }
        
        HandleCollisionEnter(other.gameObject);
        
        // 特別處理鍵盤
        GrabPoleKeyboardTrigger keyboard = other.gameObject.GetComponent<GrabPoleKeyboardTrigger>();
        if (keyboard == null && other.transform.parent != null)
            keyboard = other.transform.parent.GetComponent<GrabPoleKeyboardTrigger>();

        if (keyboard != null)
            keyboardpanel = keyboard;    
    }

    void OnCollisionExit(Collision collision)
    {
        HandleCollisionExit(collision.gameObject);
    }
    
    void OnTriggerExit(Collider other)
    {
        HandleCollisionExit(other.gameObject);
        
        GrabPoleKeyboardTrigger keyboard = other.gameObject.GetComponent<GrabPoleKeyboardTrigger>();
        if (keyboard == null && other.transform.parent != null)
            keyboard = other.transform.parent.GetComponent<GrabPoleKeyboardTrigger>();

        if (keyboard == keyboardpanel)
        {
            keyboardpanel = null;
            triggeredKeyboard = null; // ✅ 離開時重置觸發狀態
            Debug.Log($"[ScoopFish] 離開鍵盤觸發區域");
        }
    }

    private void SnapFish(FishForwardMovement fish)
    {
        if (snapPoint == null || fish == null) return;

        // Snap to pole immediately
        fish.SnapTo(snapPoint);

        snappedFish = fish;
        fish.selected = true;

        hoveredFish = null; // ✅ prevent re-snapping
        Debug.Log("Fish Caught");
    }

    private void ReleaseFish(FishForwardMovement fish)
    {
        if (fish == null) return;

        snappedFish = null;
        fish.selected = false;

        // Return only if NOT in basket
        if (!fish.isInBucket)
            fish.ReturnToOriginal();
        else
            fish.GoToNewPosition();
    }
    
    /// <summary>
    /// 【新增】統一的碰撞進入處理函數
    /// </summary>
    private void HandleCollisionEnter(GameObject other)
    {
        if (other == null) return;
        
        // 檢測魚（使用統一的 FishTags 配置）
        if (FishTags.IsFish(other))
        {
            hoveredFish = other.GetComponent<FishForwardMovement>();
        }
        
        // 檢測按鈕
        ButtonEvent button = other.GetComponent<ButtonEvent>();
        if (button != null)
        {
            hoveredButton = button;
            Debug.Log($"[ScoopFish] 懸停在按鈕：{other.name}");
        }
    }
    
    /// <summary>
    /// 【新增】統一的碰撞離開處理函數
    /// </summary>
    private void HandleCollisionExit(GameObject other)
    {
        if (other == null) return;
        
        // 魚離開
        if (other.GetComponent<FishForwardMovement>() == hoveredFish)
        {
            hoveredFish = null;
        }
        
        // 按鈕離開
        ButtonEvent button = other.GetComponent<ButtonEvent>();
        if (button != null && button == hoveredButton)
        {
            hoveredButton = null;
            Debug.Log($"[ScoopFish] 離開按鈕：{other.name}");
        }
    }
    
    /// <summary>
    /// 【新增】檢查是否為 normalBucket 在困難模式下
    /// </summary>
    private bool IsNormalBucketInHardMode(GameObject other)
    {
        if (other == null) return false;
        
        // 只在困難模式下檢查
        if (MultiBucketManager.Instance == null || !MultiBucketManager.Instance.IsHardMode)
            return false;
        
        BucketEvent normalModeBucket = MultiBucketManager.Instance.GetNormalModeBucketEvent();
        if (normalModeBucket == null)
            return false;
        
        BucketEvent otherBucket = other.GetComponent<BucketEvent>();
        return otherBucket == normalModeBucket;
    }
    
    /// <summary>
    /// 按下按鈕（調用按鈕的 isPressed 設定）
    /// </summary>
    private void PressButton(ButtonEvent button)
    {
        if (button == null) return;
        
        // 直接設定 isPressed 為 true
        button.isPressed = true;
        
        // 調用按鈕的公開方法來觸發按下事件
        button.PressButton();

        Debug.Log($"[ScoopFish] 按下按鈕：{button.gameObject.name}");
        
    }
    
    /// <summary>
    /// 釋放按鈕（調用按鈕的 isPressed 設定）
    /// </summary>
    private void ReleaseButton(ButtonEvent button)
    {
        if (button == null) return;
        
        // 直接設定 isPressed 為 false
        button.isPressed = false;
        
        Debug.Log($"[ScoopFish] 釋放按鈕：{button.gameObject.name}");
    }
}
