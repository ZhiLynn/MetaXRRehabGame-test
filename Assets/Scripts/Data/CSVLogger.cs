using UnityEngine;
using System;
using System.IO;
using TMPro;

public class CSVLogger : MonoBehaviour
{
    public static CSVLogger Instance;

    private string filePath;

    [Header("Optional: Assign Player Name Input Field")]
    public TMP_InputField playerNameInput;

    // Game data fields
    public string _playerName;
    public string _sceneName;
    public string _gameMode;
    public string _answerSituation;  // 每題反應時間 (毫秒)
    public string _score;
    public string _taskCompletion;   // 完成情況 (例: "2, 3" = 3個任務完成2個)

    // 時間追蹤
    private float taskStartTime = 0f;
    private string lastLoggedLine = "";
    private string lastLoggedSecond = "";
    private bool hasLoggedPlayerName = false;
    private bool isGameActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[CSVLogger] Initialized and waiting for player name input...");
    }

    // ==============================
    //   PLAYER NAME / FILE CREATION
    // ==============================
    public void SetPlayerNameFromSceneSelection(string name)
    {
        // 如果玩家ID為空，使用時間戳作為ID
        if (string.IsNullOrEmpty(name))
        {
            name = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            Debug.LogWarning($"[CSVLogger] Empty player name detected. Using timestamp as ID: {name}");
        }

        _playerName = name;
        hasLoggedPlayerName = true;

        CreateNewCSVFile();

        LogPlayerName();
    }

    private void CreateNewCSVFile()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"GameLog_{_playerName}_{timestamp}.csv");

        string header = "PlayerName,SceneName,GameMode,ReactionTime(ms),TaskCompletion,Score,TimeStamp";
        File.WriteAllText(filePath, header + "\n");

        Debug.Log($"[CSVLogger] New CSV file created: {filePath}");
    }

    private void LogPlayerName()
    {
        string currentSecond = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = $"{_playerName},{_sceneName},{_gameMode},0,0,0,{currentSecond}";
        File.AppendAllText(filePath, line + "\n");

        lastLoggedLine = line;
        lastLoggedSecond = currentSecond;
        isGameActive = true;
        taskStartTime = Time.time;

        Debug.Log($"[CSVLogger] Logged initial player name: {line}");
    }

    // ==============================
    //   GAME DATA LOGGING
    // ==============================

    public string SceneName
    {
        get => _sceneName;
        set
        {
            if (_sceneName != value)
            {
                _sceneName = value;
                LogIfChanged();
            }
        }
    }

    public string GameMode
    {
        get => _gameMode;
        set
        {
            if (_gameMode != value)
            {
                _gameMode = value;
                LogIfChanged();
            }
        }
    }

    public string AnswerSituation
    {
        get => _answerSituation;
        set
        {
            _answerSituation = value;
            LogAnswerWithReactionTime();
        }
    }

    public string TaskCompletion
    {
        get => _taskCompletion;
        set
        {
            if (_taskCompletion != value)
            {
                _taskCompletion = value;
                LogIfChanged();
            }
        }
    }

    public string Score
    {
        get => _score;
        set
        {
            if (_score != value)
            {
                _score = value;
                LogIfChanged();
            }
        }
    }

    private void LogIfChanged()
    {
        // 如果還沒初始化 CSV 檔案，自動使用隨機 ID 初始化
        if (string.IsNullOrEmpty(filePath))
        {
            string randomID = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            Debug.LogWarning($"[CSVLogger] filePath is null. Auto-initializing with random ID: {randomID}");
            SetPlayerNameFromSceneSelection(randomID);
            return; // 初始化後會自動記錄，這次先返回
        }

        string currentSecond = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Prevent multiple identical logs in the same second
        if (currentSecond == lastLoggedSecond)
            return;

        string line = $"{_playerName},{_sceneName},{_gameMode},0,{_taskCompletion},{_score},{currentSecond}";
        File.AppendAllText(filePath, line + "\n");

        lastLoggedLine = line;
        lastLoggedSecond = currentSecond;

        Debug.Log($"[CSVLogger] Logged: {line}");
    }

    /// <summary>
    /// 記錄答案及反應時間 (每次按下 OK 按鈕時呼叫)
    /// </summary>
    private void LogAnswerWithReactionTime()
    {
        if (!isGameActive) return;

        // 如果還沒初始化 CSV 檔案，自動使用隨機 ID 初始化
        if (string.IsNullOrEmpty(filePath))
        {
            string randomID = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            Debug.LogWarning($"[CSVLogger] filePath is null. Auto-initializing with random ID: {randomID}");
            SetPlayerNameFromSceneSelection(randomID);
            return; // 初始化後會自動記錄，這次先返回
        }

        string currentSecond = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 計算反應時間 (毫秒)
        float reactionTime = (Time.time - taskStartTime) * 1000f;
        int reactionTimeMs = Mathf.RoundToInt(reactionTime);

        // 防止同一秒內記錄多筆
        if (currentSecond == lastLoggedSecond)
            return;

        string line = $"{_playerName},{_sceneName},{_gameMode},{reactionTimeMs},{_taskCompletion},{_score},{currentSecond}";
        File.AppendAllText(filePath, line + "\n");

        lastLoggedLine = line;
        lastLoggedSecond = currentSecond;

        Debug.Log($"[CSVLogger] Logged Answer - ReactionTime: {reactionTimeMs}ms, TaskCompletion: {_taskCompletion}");
        
        // 重置任務開始時間以便下一題
        taskStartTime = Time.time;
    }

    /// <summary>
    /// 記錄遊戲結束 (時間用完)
    /// </summary>
    public void LogGameEnd()
    {
        if (!isGameActive) return;

        // 如果還沒初始化 CSV 檔案，自動使用隨機 ID 初始化
        if (string.IsNullOrEmpty(filePath))
        {
            string randomID = "Player_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            Debug.LogWarning($"[CSVLogger] filePath is null. Auto-initializing with random ID: {randomID}");
            SetPlayerNameFromSceneSelection(randomID);
        }

        string currentSecond = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = $"{_playerName},{_sceneName},{_gameMode},0,{_taskCompletion},{_score},{currentSecond}";
        File.AppendAllText(filePath, line + "\n");

        Debug.Log($"[CSVLogger] Game ended. Final log: {line}");
        isGameActive = false;
    }

    // ==============================
    //   GAME SESSION MANAGEMENT
    // ==============================

    public void EndGameAndPrepareNewLog()
    {
        Debug.Log("[CSVLogger] Game ended. Preparing for new session...");
        
        // 記錄遊戲結束
        LogGameEnd();
        
        hasLoggedPlayerName = false;
        _sceneName = "";
        _gameMode = "";
        _answerSituation = "";
        _taskCompletion = "";
        _score = "";
        lastLoggedLine = "";
        lastLoggedSecond = "";
        filePath = null;
        isGameActive = false;
    }

    /// <summary>
    /// 外部呼叫：遊戲時間用完時
    /// </summary>
    public void OnGameTimeExpired()
    {
        Debug.Log("[CSVLogger] Game time expired!");
        LogGameEnd();
    }

    public string GetFilePath() => filePath;
}
