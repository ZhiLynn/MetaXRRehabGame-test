# ⚡ 快速設置檢查清單

> 使用此清單快速驗證重構後的場景配置是否正確

---

## 📋 場景設置（5分鐘）

### ✅ 步驟 1: GameBootstrapper（必須）
```
創建步驟：
1. Hierarchy → 右鍵 → Create Empty
2. 命名為 "GameBootstrapper"
3. Add Component → GameBootstrapper

Inspector 檢查：
□ GameBootstrapper 組件已添加
□ 所有 Manager References 顯示為空（正常，會自動查找）
□ GameObject 激活狀態（Active）
```

### ✅ 步驟 2: 清理舊依賴（3分鐘）
```
在以下組件的 Inspector 中移除所有 Manager 引用：

GameModeManager:
□ 移除 Game Manager
□ 移除 Fish Spawn Manager
□ 移除 Task Manager
□ 移除 Score Manager
□ 移除 Difficulty Manager

DifficultyManager:
□ 移除 Fish Spawn Manager
□ 移除 Task Manager
□ 移除 Score Manager

TaskDisplayUI:
□ 移除 Task Manager

BucketEvent:
□ 移除 Fish Spawn Manager

FishStatisticsManager:
□ 移除 Fish Spawn Manager
□ 移除 Bucket Event

ConfirmButtonHandler:
□ 移除 Task Manager
□ 移除 Bucket Event
□ 移除 Game Mode Manager

RetryButtonHandler:
□ 移除 Task Manager
□ 移除 Bucket Event

GrabbableFish:
□ 移除 Fish Spawn Manager
```

### ✅ 步驟 3: 保留 UI 綁定（2分鐘）
```
確認以下 UI 組件仍正確綁定：

TaskDisplayUI:
□ Task Description Text (TextMeshProUGUI)
□ Error Message Text (TextMeshProUGUI)

ScoreDisplayUI:
□ Score Text (TextMeshProUGUI)

BucketEvent:
□ Bucket Text (TextMeshProUGUI)
□ Statistics Text (TextMeshProUGUI)

GameResultUI:
□ 所有結果顯示文本組件

GameModeManager:
□ Difficulty Selection UI (GameObject[])
□ Time Selection UI (GameObject[])
```

---

## 🧪 運行測試（2分鐘）

### ✅ 啟動測試
```
1. 按下 Play 按鈕
2. 檢查 Console 日誌

預期結果：
□ [GameBootstrapper] all services have been registered.
□ [TaskManager] ...（正常初始化）
□ [FishSpawnManager] ...（正常初始化）
□ 無 NullReferenceException 錯誤
□ 無 MissingReferenceException 錯誤
```

### ✅ 功能測試
```
遊戲流程測試：
□ 難度選擇按鈕可點擊
□ 時間選擇按鈕可點擊
□ 遊戲開始後魚正常生成
□ 任務文本正確顯示
□ 抓魚放入桶子功能正常
□ 確認按鈕驗證任務正常
□ 分數計算正常
□ 遊戲結束畫面顯示正常
```

---

## 🐛 快速問題排查

### ❌ 問題：Manager is null
```
檢查：
□ GameBootstrapper 存在於場景中
□ Console 有 "all services have been registered" 日誌
□ 對應的 Manager GameObject 存在且激活

解決：
1. 確認 GameBootstrapper 在場景中
2. 確認所有 Manager 都在場景中並激活
3. 重新運行場景
```

### ❌ 問題：UI 不顯示
```
檢查：
□ TaskDisplayUI 的 Text 組件已綁定
□ ScoreDisplayUI 的 Text 組件已綁定
□ UI GameObject 是激活狀態
□ Canvas 設置正確

解決：
1. 重新綁定對應的 Text 組件
2. 檢查 GameObject 的 Active 狀態
3. 檢查 Canvas Scaler 設置
```

### ❌ 問題：任務不生成
```
檢查：
□ TaskManager 存在且激活
□ GameModeManager 正確初始化
□ DifficultyManager 已設置難度
□ Console 無錯誤日誌

解決：
1. 檢查 GameModeManager.Start() 是否執行
2. 檢查難度選擇流程
3. 手動調用 TaskManager.GenerateRandomTask()
```

### ❌ 問題：魚不生成
```
檢查：
□ FishSpawnManager 存在且激活
□ Spawn Points 已設置
□ Fish Prefabs 已設置
□ DifficultyConfig 正確應用

解決：
1. 檢查 FishSpawnManager Inspector 設置
2. 確認 Spawn Points 數量足夠
3. 檢查 Fish Prefabs 引用
```

---

## 📊 完成確認

### 最終檢查清單
```
場景配置：
□ GameBootstrapper 已創建並配置
□ 所有舊的 Manager SerializeField 已清空
□ 所有 UI Text 組件保持綁定

代碼狀態：
□ 無編譯錯誤
□ 無運行時錯誤
□ Console 無紅色錯誤日誌

功能測試：
□ 難度選擇正常
□ 任務生成正常
□ 魚生成正常
□ 分數計算正常
□ 遊戲流程完整

性能檢查：
□ 場景啟動順暢
□ 無明顯卡頓
□ FPS 穩定
```

---

## 🎯 下一步

完成所有檢查後：
1. ✅ **提交代碼** - 將重構結果提交到版本控制
2. ✅ **測試所有關卡** - 確保每個場景都正常運行
3. ✅ **更新文檔** - 記錄任何特殊配置
4. ✅ **團隊同步** - 通知團隊成員新的架構變更

---

## 📞 需要幫助？

如果遇到無法解決的問題：
1. 檢查 Console 完整錯誤堆疊
2. 參考完整文檔：`REFACTORING_GUIDE.md`
3. 檢查 Scene Hierarchy 是否缺少組件

---

**預計完成時間**: 10-15 分鐘  
**難度等級**: ⭐⭐☆☆☆（簡單）

**Good Luck! 🚀**
