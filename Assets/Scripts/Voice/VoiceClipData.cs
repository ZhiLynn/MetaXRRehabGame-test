using UnityEngine;

/// <summary>
/// 單個語音片段資料
/// </summary>
[CreateAssetMenu(fileName = "VoiceClip", menuName = "Voice/Voice Clip Data")]
public class VoiceClipData : ScriptableObject
{
    [Header("識別資訊")]
    [Tooltip("唯一識別碼，例如：Simple_1, Intermediate_2_Red")]
    public string identifier;
    
    [Tooltip("語音音檔")]
    public AudioClip clip;
    
    [Header("描述資訊（僅供參考）")]
    [Tooltip("語音內容說明，例如：請幫我撈1隻魚")]
    [TextArea(2, 4)]
    public string description;
    
    [Tooltip("語音類型")]
    public VoiceType voiceType;
}

/// <summary>
/// 語音類型
/// </summary>
public enum VoiceType
{
    Simple,         // 簡單模式：只有數量
    Intermediate,   // 中級模式：數量+顏色
    Advanced        // 高級模式（暫不使用，保留擴展）
}
