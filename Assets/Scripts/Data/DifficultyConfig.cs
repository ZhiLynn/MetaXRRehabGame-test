using UnityEngine;

[System.Serializable]
public abstract class DifficultyConfig
{
    [Header("基础设置")]
    public string difficultyName { get; private set; }
    public int difficultyIndex;
    public TaskType taskType;
    
    [Header("时间设置")]
    public float timeLimit;
    
    [Header("分数设置")]
    public float scoreMultiplier;
    
    [Header("鱼生成设置")]
    public int minFishPerColor = 5;
    
    // Remove direct dependency on Manager
    // Change to return configuration information
    
    public virtual FishSpawnConfig GetFishSpawnConfig()
    {
        return new FishSpawnConfig
        {
            MinFishPerColor = minFishPerColor,
            EnabledColors = GetEnabledColors()
        };
    }
    
    public virtual TaskConfig GetTaskConfig()
    {
        return new TaskConfig
        {
            TaskType = taskType,
            MinFishPerColor = minFishPerColor
        };
    }
    
    protected abstract string[] GetEnabledColors();
    
    // Abstract method for description
    public abstract string GetDescription();
    
    // Getter methods for accessing config properties
    public string GetDifficultyName() => difficultyName;
    public int GetDifficultyIndex() => difficultyIndex;
    public TaskType GetTaskType() => taskType;
    public float GetTimeLimit() => timeLimit;
    public float GetScoreMultiplier() => scoreMultiplier;
    public int GetMinFishPerColor() => minFishPerColor;
    
    // Setter methods for customization
    protected void SetDifficultyName(string name) => difficultyName = name;
    public void SetTimeLimit(float time) => timeLimit = time;
}
