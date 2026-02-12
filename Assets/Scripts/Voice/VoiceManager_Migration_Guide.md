# VoiceManager 重構指南 - 方案二：ScriptableObject 資料驅動

## 📋 概述

本指南將協助您從現有的 `VoiceManager`（程式碼合成音效）遷移至 `VoiceManagerV2`（ScriptableObject 資料驅動），使用完整的預錄音檔。

### 設計架構

```
VoiceManagerV2
├── SimpleVoiceDatabase（簡單模式）
│   ├── 請幫我撈 1 隻魚
│   ├── 請幫我撈 2 隻魚
│   ├── ... (共 5 個音檔)
│
├── IntermediateVoiceDatabase（中級模式）
│   ├── 紅色魚 (5 個音檔)
│   ├── 藍色魚 (5 個音檔)
│   ├── 綠色魚 (5 個音檔)
│   ├── 黃色魚 (5 個音檔)
│   └── 連接詞 (1 個音檔)
│
└── 高級模式（複用中級音檔）
    └── 使用 IntermediateVoiceDatabase 串接播放
```

---

## 🎯 需要準備的音檔清單

### 簡單模式（5 個音檔）
| 檔案名稱建議 | 語音內容 | 用途 |
|------------|---------|------|
| `Simple_1Fish.wav` | 請幫我撈 1 隻魚 | 簡單模式 1 隻 |
| `Simple_2Fish.wav` | 請幫我撈 2 隻魚 | 簡單模式 2 隻 |
| `Simple_3Fish.wav` | 請幫我撈 3 隻魚 | 簡單模式 3 隻 |
| `Simple_4Fish.wav` | 請幫我撈 4 隻魚 | 簡單模式 4 隻 |
| `Simple_5Fish.wav` | 請幫我撈 5 隻魚 | 簡單模式 5 隻 |

### 中級模式（20 個音檔）
| 檔案名稱建議 | 語音內容 | 用途 |
|------------|---------|------|
| `Inter_1Red.wav` | 請幫我撈 1 隻紅色的魚 | 中級 1 紅 |
| `Inter_2Red.wav` | 請幫我撈 2 隻紅色的魚 | 中級 2 紅 |
| `Inter_3Red.wav` | 請幫我撈 3 隻紅色的魚 | 中級 3 紅 |
| `Inter_4Red.wav` | 請幫我撈 4 隻紅色的魚 | 中級 4 紅 |
| `Inter_5Red.wav` | 請幫我撈 5 隻紅色的魚 | 中級 5 紅 |
| `Inter_1Blue.wav` | 請幫我撈 1 隻藍色的魚 | 中級 1 藍 |
| `Inter_2Blue.wav` | 請幫我撈 2 隻藍色的魚 | 中級 2 藍 |
| ... | ... | ... |
| `Inter_5Yellow.wav` | 請幫我撈 5 隻黃色的魚 | 中級 5 黃 |

### 連接詞（1 個音檔）
| 檔案名稱建議 | 語音內容 | 用途 |
|------------|---------|------|
| `Connector_And.wav` | 、（頓號停頓音效） | 高級模式串接 |

**總計：26 個音檔**

---

## 📁 步驟 1：建立資料夾結構

在 Unity 專案中建立以下資料夾：

```
Assets/
├── Scripts/
│   └── Voice/              ← 新建資料夾
│       ├── VoiceClipData.cs
│       ├── SimpleVoiceDatabase.cs
│       ├── IntermediateVoiceDatabase.cs
│       └── VoiceManagerV2.cs
│
├── Audio/
│   └── Voice/              ← 新建資料夾（放音檔）
│       ├── Simple/
│       ├── Intermediate/
│       └── Connector/
│
└── Resources/
    └── VoiceDatabases/     ← 新建資料夾（放 ScriptableObject）
```

---

## 📦 步驟 2：建立 ScriptableObject 資料庫

### 2.1 建立 SimpleVoiceDatabase

1. 在 Unity Project 視窗右鍵
2. 選擇 `Create > Voice > Simple Voice Database`
3. 命名為 `SimpleVoiceDatabase`
4. 將其移動到 `Assets/Resources/VoiceDatabases/`

### 2.2 建立 IntermediateVoiceDatabase

1. 在 Unity Project 視窗右鍵
2. 選擇 `Create > Voice > Intermediate Voice Database`
3. 命名為 `IntermediateVoiceDatabase`
4. 將其移動到 `Assets/Resources/VoiceDatabases/`

---

## 🎵 步驟 3：匯入並分配音檔

### 3.1 匯入音檔到 Unity

1. 將所有錄製好的音檔拖曳到對應資料夾：
   - 簡單模式音檔 → `Assets/Audio/Voice/Simple/`
   - 中級模式音檔 → `Assets/Audio/Voice/Intermediate/`
   - 連接詞音檔 → `Assets/Audio/Voice/Connector/`

2. 選擇所有音檔，在 Inspector 設定：
   - **Load Type**: `Decompress On Load`（小檔案）或 `Compressed In Memory`（大檔案）
   - **Preload Audio Data**: ✅ 勾選
   - **Compression Format**: `Vorbis` 或 `PCM`

### 3.2 分配音檔到 SimpleVoiceDatabase

1. 選擇 `SimpleVoiceDatabase` ScriptableObject
2. 在 Inspector 中逐一分配：
   - `Voice_1Fish` ← 拖曳 `Simple_1Fish.wav`
   - `Voice_2Fish` ← 拖曳 `Simple_2Fish.wav`
   - ... 以此類推

### 3.3 分配音檔到 IntermediateVoiceDatabase

1. 選擇 `IntermediateVoiceDatabase` ScriptableObject
2. 分配 20 個顏色音檔：
   - **紅色區塊**: `Voice_1_Red` ~ `Voice_5_Red`
   - **藍色區塊**: `Voice_1_Blue` ~ `Voice_5_Blue`
   - **綠色區塊**: `Voice_1_Green` ~ `Voice_5_Green`
   - **黃色區塊**: `Voice_1_Yellow` ~ `Voice_5_Yellow`
3. 分配連接詞：
   - `Connector_And` ← 拖曳 `Connector_And.wav`

---

## 🔧 步驟 4：更新場景設定

### 4.1 更新現有的 VoiceManager GameObject

找到場景中的 `VoiceManager` GameObject，執行以下操作：

**方法 A：直接替換組件**
1. 選擇 GameObject
2. 移除舊的 `VoiceManager` 組件（如果確定不再需要）
3. 點擊 `Add Component` → 搜尋 `VoiceManagerV2`
4. 添加組件

**方法 B：新建 GameObject（推薦）**
1. 建立新 GameObject：`GameObject > Create Empty`
2. 重新命名為 `VoiceManagerV2`
3. 添加 `VoiceManagerV2` 組件
4. 暫時保留舊的 `VoiceManager`（測試後再刪除）

### 4.2 設定 VoiceManagerV2 Inspector

選擇 `VoiceManagerV2` GameObject，在 Inspector 中設定：

```
VoiceManagerV2 (Script)
├── Audio Source: [自動生成或手動拖曳]
├── Simple Voice DB: [拖曳 SimpleVoiceDatabase]
├── Intermediate Voice DB: [拖曳 IntermediateVoiceDatabase]
├── Delay Between Clips: 0.15
└── Enable Voice: ✅
```

---

## ✅ 步驟 5：驗證設定

### 5.1 檢查音檔完整性

1. 進入 Play Mode
2. 查看 Console，應顯示：
   ```
   [VoiceManagerV2] 所有語音資料庫驗證通過
   [VoiceManagerV2] 已訂閱 TaskManager 事件
   [VoiceManagerV2] 已訂閱 MultiBucketManager 事件
   [VoiceManagerV2] 已訂閱 HardModeManager 事件
   ```

3. 如果出現警告訊息，檢查對應的音檔是否已正確分配

### 5.2 測試各模式語音

- **簡單模式**: 開始遊戲，選擇簡單難度，確認語音播放
- **中級模式**: 選擇中級難度，確認顏色語音正確
- **高級模式**: 選擇高級難度，確認多階段語音串接流暢

---

## 🔄 步驟 6：遷移完成後的清理

當確認 `VoiceManagerV2` 運作正常後：

1. **刪除舊組件**
   - 移除場景中的舊 `VoiceManager` GameObject

2. **（可選）保留舊腳本作為備份**
   - 將 `VoiceManager.cs` 重新命名為 `VoiceManager_OLD.cs`
   - 移動到 `Assets/Scripts/_Deprecated/` 資料夾

3. **更新文檔**
   - 在專案文檔中記錄此次重構
   - 更新音檔命名規範供團隊使用

---

## 📊 方案比較總結

| 項目 | 舊 VoiceManager | 新 VoiceManagerV2 |
|------|----------------|-------------------|
| 音檔數量 | ~15 個片段 | 26 個完整句子 |
| 語音自然度 | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| 程式碼複雜度 | 高 | 低 |
| 擴展性 | 低 | 高 |
| 音檔管理 | Inspector 手動配置 | ScriptableObject 集中管理 |
| 高級模式 | 即時合成 | 複用中級音檔串接 |

---

## 🐛 常見問題排查

### Q1: 播放語音時沒有聲音
**檢查項目：**
- AudioSource 是否有 Output 設定
- 音檔是否已正確 Import
- `Enable Voice` 是否勾選
- 音量設定是否為 0

### Q2: 高級模式語音斷斷續續
**調整：**
- 增加 `Delay Between Clips` 值（建議 0.1 ~ 0.3 秒）
- 確認連接詞音檔不為空

### Q3: Console 顯示 "缺少音檔" 警告
**解決：**
- 打開對應的 Database ScriptableObject
- 檢查哪個欄位是空的
- 拖曳對應音檔進去

### Q4: 找不到 "Create > Voice" 選單
**解決：**
- 確認已將新腳本放入 `Assets/Scripts/Voice/` 資料夾
- 等待 Unity 編譯完成
- 檢查腳本是否有編譯錯誤

---

## 📝 後續擴展建議

### 如果需要增加更多數量（6~10 隻）

1. 編輯 `SimpleVoiceDatabase.cs`，新增欄位：
   ```csharp
   public AudioClip voice_6Fish;
   public AudioClip voice_7Fish;
   // ...
   ```

2. 更新 `GetVoiceClip()` 方法加入新的 case

3. 錄製並匯入新音檔

### 如果需要支援更多顏色（紫色、橘色等）

1. 編輯 `IntermediateVoiceDatabase.cs`
2. 新增對應顏色區塊（例如 Purple、Orange）
3. 更新 `GetVoiceClip()` 方法

---

## ✨ 完成！

按照以上步驟，您的 VoiceManager 已成功重構為資料驅動架構。現在可以：

- ✅ 使用完整、自然的預錄語音
- ✅ 透過 ScriptableObject 集中管理音檔
- ✅ 輕鬆擴展新的語音內容
- ✅ 高級模式複用中級音檔，節省儲存空間

如有任何問題，請參考 `VoiceManagerV2.cs` 中的註解說明。
