using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 統一管理魚類標籤（Tag），提高可擴充性
/// 如需新增魚種，只需在 AllFishTags 中添加新標籤
/// </summary>
public static class FishTags
{
    // 所有魚類標籤集合（易於擴展：只需添加新的標籤）
    private static readonly string[] AllFishTags = 
    {
        "redFish",
        "blueFish",
        "greenFish",
        // "grayFish",
        // 未來可擴展：
        // "yellowFish",
        // "orangeFish",
        // etc.
    };

    // 將陣列轉換為 HashSet（提高查詢效率）
    private static readonly HashSet<string> FishTagSet = new HashSet<string>(AllFishTags);

    /// <summary>
    /// 獲取所有魚類標籤
    /// </summary>
    public static string[] GetAllFishTags() => AllFishTags;

    /// <summary>
    /// 獲取所有魚類標籤（List 形式）
    /// </summary>
    public static List<string> GetAllFishTagsList() => AllFishTags.ToList();

    /// <summary>
    /// 檢查物體是否為魚類（更高效的方式）
    /// 建議在 OnCollisionEnter/OnTriggerEnter 中使用此方法
    /// </summary>
    /// <param name="gameObject">要檢查的物體</param>
    /// <returns>是否為魚類</returns>
    public static bool IsFish(UnityEngine.GameObject gameObject)
    {
        if (gameObject == null) return false;

        // 方法 1：檢查 Tag（快速）
        return FishTagSet.Contains(gameObject.tag);
    }

    /// <summary>
    /// 檢查物體是否為魚類（基於組件存在）
    /// 更精確，不依賴 Tag，建議作為備用檢查方法
    /// </summary>
    /// <param name="gameObject">要檢查的物體</param>
    /// <returns>是否有 FishForwardMovement 組件</returns>
    public static bool IsFishByComponent(UnityEngine.GameObject gameObject)
    {
        if (gameObject == null) return false;
        return gameObject.GetComponent<FishForwardMovement>() != null;
    }

    /// <summary>
    /// 檢查物體標籤是否為指定魚種
    /// </summary>
    /// <param name="gameObject">要檢查的物體</param>
    /// <param name="fishTag">要查詢的魚種標籤</param>
    /// <returns>是否匹配</returns>
    public static bool IsFishType(UnityEngine.GameObject gameObject, string fishTag)
    {
        if (gameObject == null || string.IsNullOrEmpty(fishTag)) return false;
        return gameObject.CompareTag(fishTag);
    }

    /// <summary>
    /// 獲取所有魚的種類數量
    /// </summary>
    public static int GetFishCount() => AllFishTags.Length;
}
