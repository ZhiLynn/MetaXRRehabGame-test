# 魚類標籤配置優化方案

## 問題分析
之前代碼中硬編碼了魚類標籤，分散在多個文件中：
```csharp
// ❌ 舊做法 - 硬編碼，難以擴展
if (collision.gameObject.CompareTag("redFish") || 
    collision.gameObject.CompareTag("grayFish") || 
    collision.gameObject.CompareTag("greenFish"))
```

## 解決方案：統一的 FishTags 配置類

### 1. 新增文件：`FishTags.cs`
位置：`Assets/Scripts/Constants/FishTags.cs`

**功能：**
- 集中管理所有魚類標籤
- 提供多種檢查方式（Tag、Component）
- 易於擴展

**主要方法：**
```csharp
// 檢查物體是否為魚類
FishTags.IsFish(gameObject)

// 獲取所有魚類標籤
FishTags.GetAllFishTags()

// 基於組件檢查（備用方法）
FishTags.IsFishByComponent(gameObject)
```

### 2. 更新檔案

#### ScoopFish.cs
**之前：**
```csharp
if (collision.gameObject.CompareTag("redFish") || 
    collision.gameObject.CompareTag("grayFish") || 
    collision.gameObject.CompareTag("greenFish"))
```

**之後：**
```csharp
if (FishTags.IsFish(collision.gameObject))
```

#### FishSpawnManager.cs
**之前：**
```csharp
private string[] fishname = {"redFish", "grayFish", "greenFish"};
```

**之後：**
```csharp
private string[] fishname => FishTags.GetAllFishTags();
```

#### GameResultUI.cs
**之前：**
```csharp
string[] fishTags = { "redFish", "grayFish", "greenFish", "yellowFish" };
```

**之後：**
```csharp
foreach (string fishTag in FishTags.GetAllFishTags())
```

## 擴展流程

### 如何添加新魚種？

**步驟 1：** 在 `FishTags.cs` 中添加新標籤
```csharp
private static readonly string[] AllFishTags = 
{
    "redFish",
    "blueFish",
    "grayFish",
    "greenFish",
    "yellowFish"  // ✅ 新增
};
```

**步驟 2：** 完成！所有使用 `FishTags.IsFish()` 的代碼會自動支持新魚種

無需修改其他文件！

## 改進優點

| 項目 | 之前 | 之後 |
|-----|-----|-----|
| **可維護性** | 5個地方硬編碼 | 1個地方配置 |
| **可擴展性** | 添加新魚需改多個文件 | 只需改 FishTags.cs |
| **查詢效率** | 多次 CompareTag 調用 | HashSet 快速查詢 |
| **代碼簡潔度** | 冗長的 OR 判斷 | 簡單的方法調用 |
| **出錯風險** | 容易遺漏某個文件 | 集中管理，降低風險 |

## 額外建議

### TaskDisplayUI.cs 的改進
目前 TaskDisplayUI 使用 switch-case 處理魚種，可考慮改用字典：

```csharp
private Dictionary<string, Sprite> fishSpriteMap;

void Start()
{
    fishSpriteMap = new Dictionary<string, Sprite>
    {
        {"redfish", redFishSprite},
        {"bluefish", blueFishSprite},
        // ...
    };
}
```

這樣也不需要修改 FishTags.cs 就能自動支持新魚種。
