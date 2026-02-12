using UnityEngine;
using System.Collections;

/// <summary>
/// Fish Profile Category - Tracks spawn and catch statistics for fish of a specific color
/// </summary>
[System.Serializable]
public class Fish
{
    public string color { get; private set; }
    public int spawnedAmount { get; private set; }
    public int caughtAmount { get; private set; }
    public int targetAmount { get; private set; }
    public int order { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="color">fish color</param>
    /// <param name="spawnedAmount">generated amount</param>
    /// <param name="order">fish order</param>
    /// <param name="targetAmount">target amount（default: 0）</param>
    public Fish(string color, int spawnedAmount, int order, int targetAmount = 0)
    {
        this.color = color;
        this.spawnedAmount = spawnedAmount;
        this.caughtAmount = 0; 
        this.order = order;
        this.targetAmount = targetAmount;
    }

    /// <summary>
    /// increment caught amount (when fish enters the bucket)
    /// </summary>
    /// <param name="amount">increment amount（Default: 1）</param>
    /// <returns>returns true if caught amount is not greater than spawned amount</returns>
    public bool IncrementCaught(int amount = 1)
    {
        caughtAmount += amount;
        
        //avoid caught amount exceeding spawned amount
        if (caughtAmount > spawnedAmount)
        {
            Debug.LogWarning($"[Fish] {color} caught amount ({caughtAmount}) over spawned amount ({spawnedAmount})!");
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// DecrementCaught（When fish leaves the bucket）
    /// </summary>
    /// <param name="amount">Decrement amount（default: 1）</param>
    public void DecrementCaught(int amount = 1)
    {
        caughtAmount -= amount;
        
        // make sure caught amount is not less than 0
        if (caughtAmount < 0)
        {
            Debug.LogWarning($"[Fish] {color} caught amount became negative, resetting to 0!");
            caughtAmount = 0;
        }
    }

    /// <summary>
    /// 減少已生成數量（當生成失敗時使用）
    /// </summary>
    /// <param name="amount">減少的數量（default: 1）</param>
    public void DecrementSpawned(int amount = 1)
    {
        spawnedAmount -= amount;
        
        // 確保生成數量不小於 0
        if (spawnedAmount < 0)
        {
            Debug.LogWarning($"[Fish] {color} spawned amount became negative, resetting to 0!");
            spawnedAmount = 0;
        }
        
        // 確保 caught amount 不超過 spawned amount
        if (caughtAmount > spawnedAmount)
        {
            Debug.LogWarning($"[Fish] {color} caught amount ({caughtAmount}) exceeds spawned amount ({spawnedAmount}), adjusting caught amount!");
            caughtAmount = spawnedAmount;
        }
    }

    /// <summary>
    /// 增加已生成數量（當重新生成魚時使用）
    /// </summary>
    /// <param name="amount">增加的數量（default: 1）</param>
    public void IncrementSpawned(int amount = 1)
    {
        spawnedAmount += amount;
        Debug.Log($"[Fish] {color} spawned amount increased by {amount}, now: {spawnedAmount}");
    }

    /// <summary>
    /// get progress（0.0 ~ 1.0）
    /// </summary>
    /// <returns>progress</returns>
    public float GetProgress()
    {
        if (spawnedAmount == 0) return 0f;
        return (float)caughtAmount / spawnedAmount;
    }

    /// <summary>
    /// GetTargetProgress（0.0 ~ 1.0）
    /// </summary>
    /// <returns>The target completion percentage, or return if there is no target -1</returns>
    public float GetTargetProgress()
    {
        if (targetAmount == 0) return -1f;
        return (float)caughtAmount / targetAmount;
    }

    /// <summary>
    /// check if target is completed
    /// </summary>
    /// <returns>is target completed</returns>
    public bool IsTargetComplete()
    {
        if (targetAmount == 0) return false;
        return caughtAmount >= targetAmount;
    }

    /// <summary>
    /// check if all fish are caught
    /// </summary>
    /// <returns>is all fish caught</returns>
    public bool IsAllCaught()
    {
        return caughtAmount >= spawnedAmount;
    }

    /// <summary>
    /// get remaining amount
    /// </summary>
    public int GetRemainingAmount()
    {
        return Mathf.Max(0, spawnedAmount - caughtAmount);
    }

    /// <summary>
    /// get fish info string
    /// </summary>
    public override string ToString()
    {
        string targetInfo = targetAmount > 0 ? $" | target: {targetAmount}" : "";
        return $"[{color}] spawned: {spawnedAmount} | caught: {caughtAmount} | remaining: {GetRemainingAmount()}{targetInfo} | progress: {GetProgress():P0}";
    }
} 