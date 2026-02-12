using UnityEngine;

/// <summary>
/// 遊戲事件定義（使用 struct 避免 GC）
/// </summary>

public struct TaskGeneratedEvent
{
    public TaskData TaskData;
}

public struct TaskValidatedEvent
{
    public TaskValidationResult Result;
}

public struct TaskFailedEvent
{
    public TaskData TaskData;
}

public struct ScoreChangedEvent
{
    public int NewScore;
    public int Delta;
}

public struct GameEndEvent
{
    public GameResult Result;
}

public struct FishCaughtEvent
{
    public GameObject Fish;
    public string FishColor;
}

public struct DifficultyChangedEvent
{
    public DifficultyConfig NewDifficulty;
}