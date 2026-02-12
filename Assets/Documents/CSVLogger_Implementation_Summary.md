# CSVLogger 實作完成總結

## ✅ 已完成的整合

### 1️⃣ **CSVLogger.cs 更新**
- ✅ 新增 `_taskCompletion` 欄位（格式：「2, 3」表示完成2個，共3個任務）
- ✅ 修改 `AnswerSituation` 屬性為自動計算反應時間
- ✅ 新增 `LogAnswerWithReactionTime()` 方法
- ✅ 新增 `OnGameTimeExpired()` 方法
- ✅ **防止玩家ID為空**：如果玩家ID為空則自動使用時間戳 (yyyyMMdd_HHmmss)
- ✅ CSV 頭欄位：`PlayerName,SceneName,GameMode,ReactionTime(ms),TaskCompletion,Score,TimeStamp`

### 2️⃣ **GameManager.cs 整合**
- ✅ 倒數計時結束時自動呼叫 `CSVLogger.Instance.OnGameTimeExpired()`
- ✅ 記錄遊戲時間用完的事件

### 3️⃣ **GameModeManager.cs 整合**
- ✅ 遊戲開始時設定 `SceneName = "GameScene"`
- ✅ 遊戲開始時設定 `GameMode = "Easy"/"Normal"/"Hard"`
- ✅ 任務完成時自動更新 `Score` 欄位

### 4️⃣ **HardModeManager.cs 整合**
- ✅ 困難模式每個階段完成時記錄 `TaskCompletion`（格式：「已完成, 總數」）
- ✅ 任務全部完成時記錄最終完成情況

---

## 📋 CSV 記錄範例

### 遊戲開始
```
玩家ID,GameScene,Easy,0,0,0,2025-12-21 10:30:00
```

### 完成第1個任務（反應時間2500ms）
```
玩家ID,GameScene,Easy,2500,1,100,2025-12-21 10:30:02
```

### 完成第2個任務（反應時間3000ms）
```
玩家ID,GameScene,Easy,3000,2,200,2025-12-21 10:30:05
```

### 遊戲時間用完
```
玩家ID,GameScene,Easy,0,2,200,2025-12-21 10:34:22
```

---

## 🔧 玩家ID防空機制

**情況 1**：玩家輸入名稱
```csharp
CSVLogger.Instance.SetPlayerNameFromSceneSelection("玩家1");
// CSV 記錄為：玩家1,GameScene,Easy,...
```

**情況 2**：玩家ID 為空
```csharp
CSVLogger.Instance.SetPlayerNameFromSceneSelection("");
// 自動轉換為時間戳：20251221_103000
// CSV 記錄為：20251221_103000,GameScene,Easy,...
```

---

## ⏱️ 反應時間計算

- **起點**：任務開始（LogPlayerName 時的 taskStartTime）
- **終點**：按下確認按鈕時（AnswerSituation 被設定）
- **單位**：毫秒 (ms)
- **自動重置**：每次記錄後自動重置 taskStartTime 以便下一題

---

## 🎮 使用方式

### 在遊戲中呼叫：
```csharp
// 1. 遊戲開始時自動設定（GameModeManager 已處理）
CSVLogger.Instance.SceneName = "GameScene";
CSVLogger.Instance.GameMode = "Easy";

// 2. 任務完成時（GameModeManager 已處理）
CSVLogger.Instance.Score = "100";

// 3. 困難模式階段完成時（HardModeManager 已處理）
CSVLogger.Instance.TaskCompletion = "1, 3";  // 3個任務完成1個

// 4. 按下確認按鈕時（需要在 ButtonEvent 或 ConfirmButtonHandler 中呼叫）
CSVLogger.Instance.AnswerSituation = "確認";  // 自動記錄反應時間
```

---

## 📁 CSV 檔案位置

- **Windows**: `C:\Users\{使用者名稱}\AppData\PersistentDataPath\GameLog_{玩家ID}_{時間戳}.csv`
- **Android**: `/sdcard/Android/data/{應用包名}/files/GameLog_{玩家ID}_{時間戳}.csv`

使用 `CSVLogger.Instance.GetFilePath()` 獲取完整路徑。

---

## 🚀 下一步（可選）

如果需要進一步整合，可在以下位置新增呼叫：

1. **ButtonEvent.cs** - 按下 OK 按鈕時
   ```csharp
   CSVLogger.Instance.AnswerSituation = "按下確認";
   ```

2. **TaskManager.cs** - 普通模式任務完成時
   ```csharp
   CSVLogger.Instance.TaskCompletion = $"{completedCount}, {totalCount}";
   ```

