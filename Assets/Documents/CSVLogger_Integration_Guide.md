# CSVLogger 整合指南

## 📊 CSV 記錄格式

```
PlayerName,SceneName,GameMode,ReactionTime(ms),TaskCompletion,Score,TimeStamp
```

## 🎮 各事件中的呼叫方式

### 1️⃣ **遊戲開始時** (在 GameModeManager.StartGameWithDifficulty)
```csharp
// 設定遊戲模式
CSVLogger.Instance.SceneName = "GameScene";
CSVLogger.Instance.GameMode = selectedDifficulty; // "Easy" / "Normal" / "Hard"
// 記錄開始時間
// (自動在 SetPlayerNameFromSceneSelection 時呼叫)
```

### 2️⃣ **任務完成時** (在 TaskManager 或 HardModeManager.OnTaskValidated)
```csharp
// 設定完成情況 (格式: "已完成, 總數")
// 例: 3個子任務完成了2個 → "2, 3"
CSVLogger.Instance.TaskCompletion = $"{completedCount}, {totalCount}";

// 設定分數
CSVLogger.Instance.Score = currentScore.ToString();

// 這會自動計算反應時間並記錄
CSVLogger.Instance.AnswerSituation = "任務完成"; // 標籤（可選）
```

### 3️⃣ **按下 OK 按鈕時** (在 ConfirmButtonHandler 或 ButtonEvent)
```csharp
// 按下確認按鈕時
CSVLogger.Instance.TaskCompletion = $"{completedStages}, {totalStages}";
CSVLogger.Instance.Score = scoreManager.GetCurrentScore().ToString();
CSVLogger.Instance.AnswerSituation = "按下確認"; // 觸發反應時間計算
```

### 4️⃣ **遊戲時間用完時** (在 GameManager 倒數結束時)
```csharp
// 當 GameManager 倒數計時結束
CSVLogger.Instance.OnGameTimeExpired();
```

### 5️⃣ **遊戲結束/重新開始** (在 GameModeManager.RestartGame)
```csharp
CSVLogger.Instance.EndGameAndPrepareNewLog();
```

---

## 📋 實際範例

### 簡單模式 (CountOnly)
```
玩家,GameScene,Easy,2500,1,100,2025-12-21 10:30:45
玩家,GameScene,Easy,3000,2,200,2025-12-21 10:30:48
玩家,GameScene,Easy,0,2,300,2025-12-21 10:34:22  // 時間用完
```

### 困難模式 (MultiStage)
```
玩家,GameScene,Hard,1500,1,50,2025-12-21 10:35:00
玩家,GameScene,Hard,2000,2,100,2025-12-21 10:35:02
玩家,GameScene,Hard,2500,3,150,2025-12-21 10:35:05
玩家,GameScene,Hard,0,3,150,2025-12-21 10:39:00  // 時間用完
```

---

## 🔧  需要修改的地方

### 1. 在 GameModeManager 中
- StartGameWithDifficulty() 時設定 SceneName 和 GameMode
- OnTaskValidated() 時更新 TaskCompletion 和 Score

### 2. 在 HardModeManager 中
- OnStageComplete() 時更新 TaskCompletion
- ValidateCurrentStage() 時呼叫 AnswerSituation

### 3. 在 GameManager 中
- 倒數計時結束時呼叫 CSVLogger.Instance.OnGameTimeExpired()

### 4. 在 ConfirmButtonHandler 中 (如果存在)
- 按下 OK 按鈕時設定 TaskCompletion

---

## ⏱️ 反應時間計算

- 從任務開始（LogPlayerName）到按下 OK 按鈕
- 單位：毫秒 (ms)
- 範例: 2500 ms = 2.5 秒

