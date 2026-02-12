using UnityEngine;

/// <summary>
/// 水桶數據結構定義 - Data Layer
/// 用於在各層之間傳遞數據，不包含業務邏輯
/// </summary>

/// <summary>
/// 水桶驗證結果
/// </summary>
public class BucketValidationResult
{
    public bool IsValid;
    public TaskValidationResult Result;
    public string Message;
}

/// <summary>
/// 水桶驗證狀態（從 Data Layer 傳遞到 Business Layer）
/// </summary>
public class BucketValidationState
{
    public bool IsFull;
    public bool IsColorCorrect;
    public int CurrentCount;
    public int Capacity;
    public FishColor ActualColor;
    public FishColor TargetColor;
}

/// <summary>
/// 水桶數據變更事件
/// </summary>
public class BucketDataChangedEvent
{
    public BucketEventType EventType;
    public int BucketIndex;
    public GameObject Fish;
    public FishColor FishColor;
    public int CurrentCount;
    public int Capacity;
    public FishColor TargetColor;
    public BucketValidationState ValidationState;
}

/// <summary>
/// 水桶事件類型
/// </summary>
public enum BucketEventType
{
    FishAdded,
    FishRemoved,
    FishLocked,      // 魚被鎖定（困難模式）
    BucketCleared,   // 水桶被清空
    Cleared,         // 保留舊名稱以兼容
    Full,
    ColorMismatch
}
