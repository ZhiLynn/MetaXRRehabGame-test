# VoiceManager 語音管理器實施指南

## 概述

已實施**方案C（混合模式）**的語音管理系統：
- **簡單/中級模式**：任務開始時播放一次完整語音
- **高級模式**：任務開始時播放所有階段的完整語音，每完成一個階段時重複播放下一階段提示

---

## 語音輸出格式

### 簡單模式（CountOnly）
```
任務開始時播放：
"請幫我撈 3 隻魚"
```

### 中級模式（ColorCount）
```
任務開始時播放：
"請幫我撈 4 隻紅色的魚"
```

### 高級模式（MultiStage）- 假設3個階段
```
任務開始時播放（完整任務）：
"請幫我撈 2 隻紅色的魚、3 隻藍色的魚、1 隻綠色的魚"

第一階段完成後播放：
"現在請撈 3 隻藍色的魚"

第二階段完成後播放：
"現在請撈 1 隻綠色的魚"
```

---

## 音檔命名規範

### 📁 建議資料夾結構
```
Assets/
  Audio/
    Voice/
      Prefix/
        Voice_Please.wav       (請幫我撈)
        Voice_Now.wav          (現在請撈)
      
      Numbers/
        Voice_Number_1.wav     (1 隻)
        Voice_Number_2.wav     (2 隻)
        Voice_Number_3.wav     (3 隻)
        Voice_Number_4.wav     (4 隻)
        Voice_Number_5.wav     (5 隻)
      
      Colors/
        Voice_Color_Red.wav    (紅色的)
        Voice_Color_Blue.wav   (藍色的)
        Voice_Color_Green.wav  (綠色的)
        Voice_Color_Yellow.wav (黃色的)
      
      Connectors/
        Voice_Connector_And.wav (、頓號停頓)
      
      Suffix/
        Voice_Suffix_Fish.wav  (魚)
```

---

## 音檔內容清單

### 前綴（2個）
| 檔案名稱 | 內容 | 用途 |
|---------|------|------|
| Voice_Please.wav | "請幫我撈" | 任務開始時使用 |
| Voice_Now.wav | "現在請撈" | 高級模式階段提示 |

### 數字（5個）
| 檔案名稱 | 內容 | 備註 |
|---------|------|------|
| Voice_Number_1.wav | "1 隻" | 一隻 |
| Voice_Number_2.wav | "2 隻" | 兩隻 |
| Voice_Number_3.wav | "3 隻" | 三隻 |
| Voice_Number_4.wav | "4 隻" | 四隻 |
| Voice_Number_5.wav | "5 隻" | 五隻 |

### 顏色（4個）
| 檔案名稱 | 內容 | 備註 |
|---------|------|------|
| Voice_Color_Red.wav | "紅色的" | 包含"的"字 |
| Voice_Color_Blue.wav | "藍色的" | 包含"的"字 |
| Voice_Color_Green.wav | "綠色的" | 包含"的"字 |
| Voice_Color_Yellow.wav | "黃色的" | 包含"的"字 |

### 連接詞（1個）
| 檔案名稱 | 內容 | 用途 |
|---------|------|------|
| Voice_Connector_And.wav | "、"（頓號停頓，約0.3秒） | 高級模式連接多個階段 |

### 後綴（1個）
| 檔案名稱 | 內容 | 用途 |
|---------|------|------|
| Voice_Suffix_Fish.wav | "魚" | 所有任務結尾 |

**總計：13 個音檔**

---

## Unity 場景配置步驟

### 1. 創建 VoiceManager GameObject
1. 在場景 Hierarchy 中創建空物件，命名為 `VoiceManager`
2. 添加 `VoiceManager.cs` 組件
3. 系統會自動添加 `AudioSource` 組件

### 2. 配置音檔（Inspector 面板）
在 VoiceManager 組件的 Inspector 中：

#### 前綴音檔
- **Voice Prefix Please**: 拖入 `Voice_Please.wav`
- **Voice Prefix Now**: 拖入 `Voice_Now.wav`

#### 數字音檔
- **Voice Number 1**: 拖入 `Voice_Number_1.wav`
- **Voice Number 2**: 拖入 `Voice_Number_2.wav`
- **Voice Number 3**: 拖入 `Voice_Number_3.wav`
- **Voice Number 4**: 拖入 `Voice_Number_4.wav`
- **Voice Number 5**: 拖入 `Voice_Number_5.wav`

#### 顏色音檔
- **Voice Color Red**: 拖入 `Voice_Color_Red.wav`
- **Voice Color Blue**: 拖入 `Voice_Color_Blue.wav`
- **Voice Color Green**: 拖入 `Voice_Color_Green.wav`
- **Voice Color Yellow**: 拖入 `Voice_Color_Yellow.wav`

#### 連接詞音檔
- **Voice Connector And**: 拖入 `Voice_Connector_And.wav`

#### 後綴音檔
- **Voice Suffix Fish**: 拖入 `Voice_Suffix_Fish.wav`

### 3. 播放設定
- **Delay Between Clips**: 0.1 秒（片段之間的延遲，可調整）
- **Enable Voice**: ✅ 勾選（啟用語音）

### 4. 註冊到 GameBootstrapper
1. 選擇場景中的 `GameBootstrapper` 物件
2. 在 Inspector 的 **Manager References** 區塊
3. 將 `VoiceManager` 拖入 **Voice Manager** 欄位

---

## 錄音建議

### 錄音要求
- **格式**: WAV（未壓縮）或 OGG（壓縮）
- **採樣率**: 44100 Hz 或 48000 Hz
- **位元深度**: 16-bit 或 24-bit
- **聲道**: 單聲道（Mono）
- **音量**: 統一正規化到 -3dB ~ -6dB

### 錄音技巧
1. **清晰發音**：咬字清晰，語速適中
2. **情緒一致**：所有片段使用相同的語氣和情緒（建議：友善、鼓勵）
3. **靜音處理**：
   - 數字音檔：前後各留 0.1 秒靜音
   - 顏色音檔：前後各留 0.1 秒靜音
   - 連接詞：前後各留 0.05 秒靜音
4. **降噪處理**：錄音後進行降噪和去除爆音

### 範例句子（用於測試連貫性）
錄完所有片段後，試著組合播放以下句子，確認流暢度：
- "請幫我撈 3 隻魚"
- "請幫我撈 4 隻紅色的魚"
- "請幫我撈 2 隻紅色的魚、3 隻藍色的魚、1 隻綠色的魚"

---

## 程式碼工作流程

```
任務生成
  ↓
TaskManager.GenerateRandomTask()
  ↓
TaskManager.OnTaskGenerated.Invoke(task)
  ↓
VoiceManager.OnTaskGenerated(task)
  ↓
根據 task.taskType 選擇播放模式：
  - CountOnly → PlaySimpleTask()
  - ColorCount → PlayColorTask()
  - MultiStage → PlayFullMultiStageTask()
  ↓
組合音檔片段 → PlayClipSequence()
  ↓
PlayQueueCoroutine() 依序播放
```

### 高級模式階段完成流程
```
MultiBucketManager.OnBucketStageCompleted.Invoke(index)
  ↓
VoiceManager.OnStageCompleted(index)
  ↓
獲取下一階段 SubTask
  ↓
PlayCurrentStagePrompt(stage)
  ↓
播放 "現在請撈 X 隻 [顏色] 的魚"
```

---

## 測試建議

### 測試案例 1：簡單模式
1. 開始遊戲，選擇簡單模式
2. 預期語音：`"請幫我撈 [1-5] 隻魚"`
3. 檢查音檔片段是否流暢連接

### 測試案例 2：中級模式
1. 開始遊戲，選擇中級模式
2. 預期語音：`"請幫我撈 [1-5] 隻 [顏色] 的魚"`
3. 確認顏色音檔正確對應

### 測試案例 3：高級模式 - 任務開始
1. 開始遊戲，選擇高級模式
2. 預期語音（假設3個階段）：
   ```
   "請幫我撈 2 隻紅色的魚、3 隻藍色的魚、1 隻綠色的魚"
   ```
3. 確認連接詞（頓號）的停頓自然

### 測試案例 4：高級模式 - 階段提示
1. 完成第一個水桶的任務
2. 預期語音：`"現在請撈 [數量] 隻 [顏色] 的魚"`
3. 確認前綴從"請幫我撈"切換到"現在請撈"

### 測試案例 5：語音開關
1. 在遊戲中調用 `VoiceManager.SetVoiceEnabled(false)`
2. 確認語音停止播放
3. 重新啟用後確認恢復正常

---

## 擴展功能建議

### 未來可添加功能
1. **音量控制**：添加 Inspector 滑桿控制語音音量
2. **語速調整**：使用 `AudioSource.pitch` 調整播放速度
3. **重複播放按鈕**：允許玩家重新聽一次任務語音
4. **多語言支援**：根據系統語言切換音檔資料夾
5. **完整句子模式**：如果有預錄完整句子的音檔，可切換到完整句子模式（設置 `useFullSentenceAudio = true`）

### 效能優化
- 使用 `AudioClip.LoadInBackground()` 異步加載音檔
- 音檔壓縮格式建議：對話語音使用 Vorbis 壓縮

---

## 常見問題

### Q1：音檔沒有播放？
**檢查清單：**
- ✅ 所有音檔都已拖入 Inspector
- ✅ AudioSource 組件存在且啟用
- ✅ Enable Voice 已勾選
- ✅ VoiceManager 已註冊到 ServiceLocator（檢查 Console 是否有訂閱成功訊息）

### Q2：音檔播放不流暢，有明顯停頓？
**解決方案：**
- 調整 `Delay Between Clips` 參數（預設 0.1 秒）
- 確認音檔前後靜音時長一致
- 檢查音檔是否有壓縮延遲（建議使用未壓縮 WAV 格式）

### Q3：高級模式階段提示沒有播放？
**檢查：**
- MultiBucketManager 的 `OnBucketStageCompleted` 事件是否正常觸發
- Console 是否有 `[VoiceManager] 階段 X 完成` 的訊息

### Q4：如何臨時關閉語音？
```csharp
// 在任何腳本中調用
VoiceManager voiceManager = ServiceLocator.Instance.Get<VoiceManager>();
voiceManager.SetVoiceEnabled(false);
```

### Q5：顏色對應錯誤？
確認 `FishColorHelper.GetColorFromTag()` 的映射正確：
- `redFish` → `FishColor.Red` → `voiceColor_Red`
- `blueFish` → `FishColor.Blue` → `voiceColor_Blue`
- `greenFish` → `FishColor.Green` → `voiceColor_Green`
- `yellowFish` → `FishColor.Yellow` → `voiceColor_Yellow`

---

## 檔案清單

### 新增檔案
- ✅ `Assets/Scripts/Managers/VoiceManager.cs` - 語音管理器
- ✅ `Assets/Documents/VoiceManager_Implementation_Guide.md` - 本說明文件

### 修改檔案
- ✅ `Assets/Scripts/Core/GameBootstrapper.cs` - 添加 VoiceManager 註冊

### 需要準備
- ⏳ 13 個語音音檔（依照上述命名規範）
- ⏳ Unity 場景中創建 VoiceManager GameObject 並配置音檔

---

## 編譯狀態
✅ 程式碼已編譯通過，無錯誤

---

## 作者與日期
- **實施日期**: 2026-01-12
- **版本**: 1.0
- **適用專案**: MetaXRRehabGame
