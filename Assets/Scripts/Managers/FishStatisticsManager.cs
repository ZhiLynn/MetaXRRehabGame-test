using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Fish Statistics Manager - Used to display and manage fish statistics in the game
/// Optional, provides more advanced statistical functions
/// </summary>
public class FishStatisticsManager : MonoBehaviour
{
    [Header("References")]
    private FishSpawnManager fishSpawnManager;
    private BucketEvent bucketEvent;
    
    [Header("UI Elements")]
    [SerializeField] private TMP_Text overallProgressText;
    [SerializeField] private TMP_Text redFishText;
    [SerializeField] private TMP_Text blueFishText;
    [SerializeField] private TMP_Text greenFishText;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private float updateInterval = 0.5f; // UI update interval
    
    private float timer = 0f;
    private List<Fish> fishes;
    private bool isInitialized = false;

    private void Start()
    {
        // 通过 ServiceLocator 获取依赖
        fishSpawnManager = ServiceLocator.Instance.Get<FishSpawnManager>();
        bucketEvent = ServiceLocator.Instance.Get<BucketEvent>();
        
        if (fishSpawnManager != null)
        {
            fishes = fishSpawnManager.GetFish();
            isInitialized = true;
            
            // initialize UI
            UpdateStatisticsUI();
        }
    }

    private void Update()
    {
        // make sure initialized.
        if (!isInitialized) return;
        
        timer += Time.deltaTime;
        
        if (timer >= updateInterval && showDebugInfo)
        {
            UpdateStatisticsUI();
            timer = 0f;
        }
    }

    /// <summary>
    /// update statistics UI
    /// </summary>
    private void UpdateStatisticsUI()
    {
        if (fishes == null || fishes.Count == 0) return;

        // update overall progress
        if (overallProgressText != null && bucketEvent != null)
        {
            float progress = bucketEvent.GetOverallProgress();
            overallProgressText.text = $"All Progress: {progress:P0}";
        }

        // update individual fish stats
        foreach (Fish fish in fishes)
        {
            TMP_Text targetText = GetTextForFish(fish.color);
            if (targetText != null)
            {
                targetText.text = FormatFishStatistics(fish);
            }
        }
    }

    /// <summary>
    /// format fish statistics string
    /// </summary>
    private string FormatFishStatistics(Fish fish)
    {
        string displayName = GetFishDisplayName(fish.color);
        string stats = $"{displayName}\n";
        stats += $"caught: {fish.caughtAmount}/{fish.spawnedAmount}\n";
        stats += $"progress: {fish.GetProgress():P0}";
        
        if (fish.targetAmount > 0)
        {
            stats += $"\target: {fish.targetAmount}";
        }
        
        return stats;
    }

    /// <summary>
    /// Get the corresponding Text component according to the color of the fish
    /// </summary>
    private TMP_Text GetTextForFish(string color)
    {
        switch (color)
        {
            case "redFish": return redFishText;
            case "blueFish": return blueFishText;
            case "greenFish": return greenFishText;
            default: return null;
        }
    }

    /// <summary>
    /// Get the display name for the fish based on its tag
    /// </summary>
    private string GetFishDisplayName(string tag)
    {
        switch (tag)
        {
            case "redFish": return "red_GoldFish";
            case "blueFish": return "blue_GoldFish";
            case "greenFish": return "green_GoldFish";
            default: return tag;
        }
    }

    /// <summary>
    /// show full report
    /// </summary>
    public void ShowFullReport()
    {
        if (fishes == null) return;

        Debug.Log("==================== Full Report ====================");
        
        foreach (Fish fish in fishes)
        {
            Debug.Log(fish.ToString());
        }
        
        if (fishSpawnManager != null)
        {
            int totalCaught = fishSpawnManager.GetTotalCaughtCount();
            int totalSpawned = fishSpawnManager.GetTotalSpawnedCount();
            float overallProgress = totalSpawned > 0 ? (float)totalCaught / totalSpawned : 0f;
            
            Debug.Log($"\nTotal Progress: {totalCaught}/{totalSpawned} ({overallProgress:P0})");
        }
        
        if (bucketEvent != null && bucketEvent.IsAllFishCaught())
        {
            Debug.Log("\n Congratulations! All fish have been caught!");
        }
        
        Debug.Log("====================================================");
    }

    /// <summary>
    /// check if the game is complete
    /// </summary>
    public bool IsGameComplete()
    {
        return bucketEvent != null && bucketEvent.IsAllFishCaught();
    }

    /// <summary>
    /// Get fish information by color
    /// </summary>
    public Fish GetFishInfo(string color)
    {
        if (fishes == null) return null;
        return fishes.Find(f => f.color == color);
    }

    // Debug: In the Inspector, you can press a button to display the report
    [ContextMenu("Show Full Report")]
    private void DebugShowReport()
    {
        ShowFullReport();
    }
}
