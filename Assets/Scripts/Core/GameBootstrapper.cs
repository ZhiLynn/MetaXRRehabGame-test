using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Game Launcher - Responsible for registering all services to ServiceLocator
/// Executed earliest in the scenario
/// </summary>
[DefaultExecutionOrder(-100)] // 確保此腳本在其他腳本之前執行
public class GameBootstrapper : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private GameModeManager gameModeManager;
    [SerializeField] private DifficultyManager difficultyManager;
    [SerializeField] private FishSpawnManager fishSpawnManager;
    [SerializeField] private MultiBucketManager multiBucketManager;
    [SerializeField] private BucketManager bucketManager;  // ✅ Business Layer 水桶管理器
    [SerializeField] private VoiceManagerV2 voiceManagerV2;    // ✅ 語音管理器
    
    [Header("UI References")]
    [SerializeField] private TaskDisplayUI taskDisplayUI;
    
    [Header("Handler References")]
    [SerializeField] private ConfirmButtonHandler confirmButtonHandler;
    [SerializeField] private RetryButtonHandler retryButtonHandler;

    void Awake()
    {
        // Automatically find all Managers (if not specified).
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();
        if (taskManager == null) taskManager = FindFirstObjectByType<TaskManager>();
        if (gameModeManager == null) gameModeManager = FindFirstObjectByType<GameModeManager>();
        if (difficultyManager == null) difficultyManager = FindFirstObjectByType<DifficultyManager>();
        if (fishSpawnManager == null) fishSpawnManager = FindFirstObjectByType<FishSpawnManager>();
        if (multiBucketManager == null) multiBucketManager = FindFirstObjectByType<MultiBucketManager>();
        if (bucketManager == null) bucketManager = FindFirstObjectByType<BucketManager>();
        if (voiceManagerV2 == null) voiceManagerV2 = FindFirstObjectByType<VoiceManagerV2>();  // ✅ 語音管理器
        
        // UI and Handlers
        if (taskDisplayUI == null) taskDisplayUI = FindFirstObjectByType<TaskDisplayUI>();
        if (confirmButtonHandler == null) confirmButtonHandler = FindFirstObjectByType<ConfirmButtonHandler>();
        if (retryButtonHandler == null) retryButtonHandler = FindFirstObjectByType<RetryButtonHandler>();

        // Register all services
        RegisterServices();
    }

    private void RegisterServices()
    {
        var locator = ServiceLocator.Instance;

        if (gameManager != null) locator.Register(gameManager);
        if (scoreManager != null) locator.Register(scoreManager);
        if (taskManager != null) locator.Register(taskManager);
        if (gameModeManager != null) locator.Register(gameModeManager);
        if (difficultyManager != null) locator.Register(difficultyManager);
        if (fishSpawnManager != null) locator.Register(fishSpawnManager);
        if (multiBucketManager != null) locator.Register(multiBucketManager);
        if (bucketManager != null) locator.Register(bucketManager);
        if (voiceManagerV2 != null) locator.Register(voiceManagerV2);  // ✅ 語音管理器
        
        // UI and Handlers
        if (taskDisplayUI != null) locator.Register(taskDisplayUI);
        if (confirmButtonHandler != null) locator.Register(confirmButtonHandler);
        if (retryButtonHandler != null) locator.Register(retryButtonHandler);

        Debug.Log("[GameBootstrapper] ✅ 所有服務已註冊（含 BucketManager、VoiceManagerV2）");
    }

    // Clear services when uninstalling a scenario
    void OnDestroy()
    {
        // ServiceLocator.Instance.Clear();
    }
}