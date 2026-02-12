using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 中級模式語音資料庫
/// 儲存所有「請幫我撈 X 隻 [顏色] 的魚」的完整語音
/// </summary>
[CreateAssetMenu(fileName = "IntermediateVoiceDatabase", menuName = "Voice/Intermediate Voice Database")]
public class IntermediateVoiceDatabase : ScriptableObject
{
    [Header("紅色魚語音")]
    [Tooltip("請幫我撈 1 隻紅色的魚")]
    public AudioClip voice_1_Red;
    public AudioClip voice_2_Red;
    public AudioClip voice_3_Red;
    public AudioClip voice_4_Red;
    public AudioClip voice_5_Red;
    
    [Header("藍色魚語音")]
    [Tooltip("請幫我撈 1 隻藍色的魚")]
    public AudioClip voice_1_Blue;
    public AudioClip voice_2_Blue;
    public AudioClip voice_3_Blue;
    public AudioClip voice_4_Blue;
    public AudioClip voice_5_Blue;
    
    [Header("綠色魚語音")]
    [Tooltip("請幫我撈 1 隻綠色的魚")]
    public AudioClip voice_1_Green;
    public AudioClip voice_2_Green;
    public AudioClip voice_3_Green;
    public AudioClip voice_4_Green;
    public AudioClip voice_5_Green;
    
    [Header("階段連接詞")]
    [Tooltip("用於高級模式連接多個階段，例如：頓號停頓")]
    public AudioClip connector_And;
    
    /// <summary>
    /// 根據數量和顏色獲取對應的語音
    /// </summary>
    public AudioClip GetVoiceClip(int count, FishColor color)
    {
        switch (color)
        {
            case FishColor.Red:
                return GetRedVoice(count);
            case FishColor.Blue:
                return GetBlueVoice(count);
            case FishColor.Green:
                return GetGreenVoice(count);
            default:
                Debug.LogWarning($"[IntermediateVoiceDatabase] 未知顏色: {color}");
                return null;
        }
    }
    
    /// <summary>
    /// 根據數量和顏色字串獲取語音（用於 TaskData）
    /// </summary>
    public AudioClip GetVoiceClip(int count, string colorKey)
    {
        FishColor color = FishColorHelper.GetColorFromTag(colorKey);
        return GetVoiceClip(count, color);
    }
    
    private AudioClip GetRedVoice(int count)
    {
        switch (count)
        {
            case 1: return voice_1_Red;
            case 2: return voice_2_Red;
            case 3: return voice_3_Red;
            case 4: return voice_4_Red;
            case 5: return voice_5_Red;
            default: return null;
        }
    }
    
    private AudioClip GetBlueVoice(int count)
    {
        switch (count)
        {
            case 1: return voice_1_Blue;
            case 2: return voice_2_Blue;
            case 3: return voice_3_Blue;
            case 4: return voice_4_Blue;
            case 5: return voice_5_Blue;
            default: return null;
        }
    }
    
    private AudioClip GetGreenVoice(int count)
    {
        switch (count)
        {
            case 1: return voice_1_Green;
            case 2: return voice_2_Green;
            case 3: return voice_3_Green;
            case 4: return voice_4_Green;
            case 5: return voice_5_Green;
            default: return null;
        }
    }
    
    /// <summary>
    /// 驗證所有音檔是否已設置
    /// </summary>
    public bool ValidateDatabase()
    {
        List<string> missingClips = new List<string>();
        
        // 檢查紅色
        if (voice_1_Red == null) missingClips.Add("voice_1_Red");
        if (voice_2_Red == null) missingClips.Add("voice_2_Red");
        if (voice_3_Red == null) missingClips.Add("voice_3_Red");
        if (voice_4_Red == null) missingClips.Add("voice_4_Red");
        if (voice_5_Red == null) missingClips.Add("voice_5_Red");
        
        // 檢查藍色
        if (voice_1_Blue == null) missingClips.Add("voice_1_Blue");
        if (voice_2_Blue == null) missingClips.Add("voice_2_Blue");
        if (voice_3_Blue == null) missingClips.Add("voice_3_Blue");
        if (voice_4_Blue == null) missingClips.Add("voice_4_Blue");
        if (voice_5_Blue == null) missingClips.Add("voice_5_Blue");
        
        // 檢查綠色
        if (voice_1_Green == null) missingClips.Add("voice_1_Green");
        if (voice_2_Green == null) missingClips.Add("voice_2_Green");
        if (voice_3_Green == null) missingClips.Add("voice_3_Green");
        if (voice_4_Green == null) missingClips.Add("voice_4_Green");
        if (voice_5_Green == null) missingClips.Add("voice_5_Green");
        
        // 檢查連接詞
        if (connector_And == null) missingClips.Add("connector_And");
        
        if (missingClips.Count > 0)
        {
            Debug.LogWarning($"[IntermediateVoiceDatabase] 缺少 {missingClips.Count} 個音檔: {string.Join(", ", missingClips)}");
            return false;
        }
        
        return true;
    }
}
