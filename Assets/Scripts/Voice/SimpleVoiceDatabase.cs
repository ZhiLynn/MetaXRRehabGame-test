using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 簡單模式語音資料庫
/// 儲存所有「請幫我撈 X 隻魚」的完整語音
/// </summary>
[CreateAssetMenu(fileName = "SimpleVoiceDatabase", menuName = "Voice/Simple Voice Database")]
public class SimpleVoiceDatabase : ScriptableObject
{
    [Header("簡單模式完整語音")]
    [Tooltip("請幫我撈 1 隻魚")]
    public AudioClip voice_1Fish;
    
    [Tooltip("請幫我撈 2 隻魚")]
    public AudioClip voice_2Fish;
    
    [Tooltip("請幫我撈 3 隻魚")]
    public AudioClip voice_3Fish;
    
    [Tooltip("請幫我撈 4 隻魚")]
    public AudioClip voice_4Fish;
    
    [Tooltip("請幫我撈 5 隻魚")]
    public AudioClip voice_5Fish;
    
    /// <summary>
    /// 根據數量獲取對應的語音
    /// </summary>
    public AudioClip GetVoiceClip(int count)
    {
        switch (count)
        {
            case 1: return voice_1Fish;
            case 2: return voice_2Fish;
            case 3: return voice_3Fish;
            case 4: return voice_4Fish;
            case 5: return voice_5Fish;
            default:
                Debug.LogWarning($"[SimpleVoiceDatabase] 沒有數量 {count} 的語音");
                return null;
        }
    }
    
    /// <summary>
    /// 驗證所有音檔是否已設置
    /// </summary>
    public bool ValidateDatabase()
    {
        List<string> missingClips = new List<string>();
        
        if (voice_1Fish == null) missingClips.Add("voice_1Fish");
        if (voice_2Fish == null) missingClips.Add("voice_2Fish");
        if (voice_3Fish == null) missingClips.Add("voice_3Fish");
        if (voice_4Fish == null) missingClips.Add("voice_4Fish");
        if (voice_5Fish == null) missingClips.Add("voice_5Fish");
        
        if (missingClips.Count > 0)
        {
            Debug.LogWarning($"[SimpleVoiceDatabase] 缺少音檔: {string.Join(", ", missingClips)}");
            return false;
        }
        
        return true;
    }
}
