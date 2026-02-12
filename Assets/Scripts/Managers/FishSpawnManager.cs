using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FishSpawnManager : MonoBehaviour
{
    [Header("Bucket Settings")]
    [SerializeField] private GameObject bucketReturnPoint;
    
    [Header("Spawn Settings")]
    public GameObject[] fishPrefab;
    
    [Tooltip("場景中的生成點（空的 GameObjects）")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Tooltip("每種魚的最小和最大生成數量（如果為0則使用生成點數量）")]
    [SerializeField] private int minSpawnCount = 0;
    [SerializeField] private int maxSpawnCount = 0;
    
    [Header("Randomization Settings")]
    [Tooltip("Y 軸隨機偏移範圍（上下浮動）")]
    [SerializeField] private float yAxisRandomOffset = 0.1f;
    
    
    [Tooltip("生成後等待物理穩定的時間")]
    [SerializeField] private float spawnDelay = 0.1f;
    
    [Header("Spawn Point Settings")]
    [Tooltip("是否隨機打亂生成點順序")]
    [SerializeField] private bool shuffleSpawnPoints = true;
    
    [Tooltip("是否允許重複使用生成點")]
    [SerializeField] private bool allowReuseSpawnPoints = false;
    
    [Header("Task System Settings")]
    [Tooltip("每种颜色最少生成的鱼数量（确保任务可完成）")]
    [SerializeField] private int minFishPerColor = 5;
    
    private List<Fish> fish = new List<Fish>();
    // 動態獲取所有可用的魚類標籤（確保與其他系統同步）
    private string[] fishname => FishTags.GetAllFishTags();
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private bool isDataInitialized = false; // 標記 Fish 資料是否已初始化
    
    // 任务系统：控制生成哪些颜色的鱼
    private List<string> enabledColors = new List<string>();
    
    // 是否允許在 OnEnable 時自動生成魚（由 GameModeManager 控制）
    private bool autoSpawnOnEnable = true;
    
    void Awake()
    {
        // 在 Awake 中初始化 Fish 資料（同步執行，確保 Start 時資料已準備好）
        InitializeFishData();
    }
    
    void OnEnable()
    {
        // 只有在允許自動生成時才生成魚
        // GameModeManager 會先禁用此組件，設定配置後再啟用，並手動調用 RegenerateAllFish()
        if (autoSpawnOnEnable && spawnPoints != null && spawnPoints.Length > 0 && fishPrefab != null)
        {
            StartCoroutine(SpawnFishWithDelay());
        }
        else if (!autoSpawnOnEnable)
        {
            Debug.Log("[FishSpawnManager] 自動生成已禁用，等待手動調用 RegenerateAllFish()");
        }
        else
        {
            Debug.LogError("[FishSpawnManager] 請在 Inspector 中設置 Spawn Points 和 Fish Prefabs！");
        }
    }
    
    /// <summary>
    /// 設定是否在 OnEnable 時自動生成魚
    /// </summary>
    public void SetAutoSpawnOnEnable(bool enabled)
    {
        autoSpawnOnEnable = enabled;
    }
    
    /// <summary>
    /// 初始化 Fish 資料（在生成 GameObject 之前）
    /// </summary>
    private void InitializeFishData()
    {
        fish.Clear();
        
        // 默认启用所有颜色（如果未设置）
        if (enabledColors.Count == 0)
        {
            enabledColors.AddRange(fishname);
        }
        
        // 計算要生成的魚數量
        int totalSpawnPointsCount = spawnPoints != null ? spawnPoints.Length : 0;
        int enabledFishCount = enabledColors.Count;
        
        // 計算總需求數量
        int totalRequiredFish = enabledFishCount * minFishPerColor;
        
        // 如果生成點不足且未啟用重複使用，發出警告並自動啟用
        if (totalRequiredFish > totalSpawnPointsCount && !allowReuseSpawnPoints)
        {
            Debug.LogWarning($"[FishSpawnManager] ⚠️ 生成點不足！需要 {totalRequiredFish} 個點，但只有 {totalSpawnPointsCount} 個");
            Debug.LogWarning($"[FishSpawnManager] 自動啟用 'Allow Reuse Spawn Points' 以確保任務可完成");
            allowReuseSpawnPoints = true;
        }
        
        // 為每種啟用的魚預先創建資料物件（只遍歷啟用的顏色）
        for (int i = 0; i < enabledColors.Count && i < fishPrefab.Length; i++)
        {
            string fishColor = enabledColors[i];
            
            // 檢查該顏色是否有對應的預製體
            int prefabIndex = System.Array.IndexOf(fishname, fishColor);
            if (prefabIndex < 0 || prefabIndex >= fishPrefab.Length)
            {
                Debug.LogWarning($"[FishSpawnManager] {fishColor} 沒有對應的預製體，跳過生成");
                continue;
            }
            
            int spawnCount;
            
            // 如果設置了 min/max spawn count，使用隨機數量
            if (maxSpawnCount > 0)
            {
                spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
            }
            else
            {
                // 否則，平均分配生成點給每種魚
                int pointsPerFishType = totalSpawnPointsCount / enabledFishCount;
                spawnCount = pointsPerFishType;
            }
            
            // 任務系統需要：確保每種顏色至少生成足夠的魚來完成任務
            spawnCount = Mathf.Max(spawnCount, minFishPerColor);
            
            fish.Add(new Fish(fishColor, spawnCount, prefabIndex + 1));
            
            Debug.Log($"[FishSpawnManager] 初始化 Fish 資料: {fishColor} - 預計生成 {spawnCount} 隻");
        }
        
        isDataInitialized = true;
        Debug.Log($"[FishSpawnManager] Fish 資料初始化完成，總共 {fish.Count} 種魚，可用生成點：{totalSpawnPointsCount} 個");
        
        // 最終檢查
        if (totalRequiredFish > totalSpawnPointsCount)
        {
            Debug.Log($"[FishSpawnManager] ✅ 已啟用生成點重複使用，可滿足 {totalRequiredFish} 條魚的需求");
        }
    }

    /// <summary>
    /// 使用場景中的生成點生成魚
    /// 新流程：
    /// 1. 準備生成點列表（可選擇是否打亂順序）
    /// 2. 根據生成點位置分配給每種魚
    /// 3. 加入 Y 軸隨機偏移讓魚看起來隨機
    /// 4. 延遲生成避免物理碰撞
    /// </summary>
    private IEnumerator SpawnFishWithDelay()
    {
        // 確保 Fish 資料已初始化
        if (!isDataInitialized)
        {
            Debug.LogError("[FishSpawnManager] Fish 資料尚未初始化！");
            yield break;
        }
        
        // 確保有生成點
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[FishSpawnManager] 沒有設置生成點！請在 Inspector 中添加 Spawn Points");
            yield break;
        }
        
        // Step 1: 準備生成點列表
        List<Transform> availableSpawnPoints = PrepareSpawnPointsList();
        
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("[FishSpawnManager] 沒有可用的生成點！");
            yield break;
        }
        
        Debug.Log($"[FishSpawnManager] 總共 {GetTotalSpawnedCount()} 隻魚，可用生成點：{availableSpawnPoints.Count} 個");
        
        // Step 2: 依序分配生成點給每種魚並生成 GameObject
        int spawnPointIndex = 0;
        List<GameObject> spawnedFishObjects = new List<GameObject>();
        
        for(int i = 0; i < fish.Count && i < fishPrefab.Length; i++)
        {
            Fish fishData = fish[i];
            int spawnCount = fishData.spawnedAmount;

            Debug.Log($"[FishSpawnManager] 開始生成 {fishData.color}: {spawnCount} 隻");
            
            // 為這種魚生成對應數量的 GameObject
            for(int j = 0; j < spawnCount; j++)
            {
                // 檢查是否還有可用的生成點
                if (spawnPointIndex >= availableSpawnPoints.Count)
                {
                    if (allowReuseSpawnPoints)
                    {
                        // 如果允許重複使用，回到列表開頭
                        spawnPointIndex = 0;
                        Debug.Log($"[FishSpawnManager] 生成點用完，開始重複使用（第 {j+1}/{spawnCount} 隻 {fishData.color}）");
                    }
                    else
                    {
                        Debug.LogError($"[FishSpawnManager] ❌ 生成點不足！無法生成所有 {fishData.color}");
                        Debug.LogError($"[FishSpawnManager] 已生成 {j}/{spawnCount}，缺少 {spawnCount - j} 隻");
                        Debug.LogError($"[FishSpawnManager] 解決方案：");
                        Debug.LogError($"[FishSpawnManager] 1. 在 Inspector 中啟用 'Allow Reuse Spawn Points'");
                        Debug.LogError($"[FishSpawnManager] 2. 或增加場景中的 Spawn Points 數量");
                        
                        // 更新實際生成數量
                        for (int k = j; k < spawnCount; k++)
                        {
                            fishData.DecrementSpawned();
                        }
                        break;
                    }
                }
                
                // Step 3: 從生成點獲取位置，並加入隨機偏移
                Transform spawnPoint = availableSpawnPoints[spawnPointIndex];
                //Vector3 spawnPosition = GetRandomizedPosition(spawnPoint.position);
                Vector3 spawnPosition = spawnPoint.position;

                // Step 4: 生成魚 GameObject
                GameObject spawnedFish = Instantiate(fishPrefab[i], spawnPosition, Quaternion.identity);
                
                if (spawnedFish == null)
                {
                    Debug.LogError($"[FishSpawnManager] 無法生成 {fishData.color} 的 GameObject");
                    fishData.DecrementSpawned();
                    continue;
                }
                
                // 添加 FishData 组件（任务系统需要）
                FishData fishDataComponent = spawnedFish.GetComponent<FishData>();
                if (fishDataComponent == null)
                {
                    fishDataComponent = spawnedFish.AddComponent<FishData>();
                }
                fishDataComponent.SetPrefabName(fishData.color);

                // 注入 bucketReturnPoint
                FishForwardMovement movement = spawnedFish.GetComponent<FishForwardMovement>();
                if (movement != null && bucketReturnPoint != null)
                {
                    movement.SetBucketReturnPoint(bucketReturnPoint);
                }

                spawnedFishObjects.Add(spawnedFish);
                
                // 初始化 Rigidbody
                Rigidbody rb = spawnedFish.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    // 延遲初始化移動
                    StartCoroutine(InitializeFishMovement(spawnedFish, 0.2f));
                }
                
                Debug.Log($"[FishSpawnManager] 生成 {fishData.color} #{j+1} 在位置 {spawnPosition}");
                
                spawnPointIndex++;
                
                // 等待一小段時間再生成下一隻魚
                yield return new WaitForSeconds(spawnDelay);
            }
        }
        
        // 輸出總生成數量
        Debug.Log($"[FishSpawnManager] GameObject 生成完成，總共 {spawnedFishObjects.Count} 隻魚");
    }
    
    /// <summary>
    /// 準備生成點列表（可選擇是否打亂順序）
    /// </summary>
    private List<Transform> PrepareSpawnPointsList()
    {
        List<Transform> points = new List<Transform>();
        
        // 過濾掉 null 的生成點
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                points.Add(spawnPoint);
            }
            else
            {
                Debug.LogWarning("[FishSpawnManager] 發現 null 的生成點，已跳過");
            }
        }
        
        // 如果設置了打亂順序
        if (shuffleSpawnPoints)
        {
            ShuffleList(points);
            Debug.Log("[FishSpawnManager] 已隨機打亂生成點順序");
        }
        
        return points;
    }
    
    /// <summary>
    /// 打亂列表順序（Fisher-Yates shuffle）
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// 從生成點位置獲取隨機化的位置（加入 Y 軸和可選的 X/Z 軸偏移）
    /// </summary>
    private Vector3 GetRandomizedPosition(Vector3 basePosition)
    {
        //float offsetX = Random.Range(-xzAxisRandomOffset, xzAxisRandomOffset);
        float offsetY = Random.Range(-yAxisRandomOffset, yAxisRandomOffset);
        //float offsetZ = Random.Range(-xzAxisRandomOffset, xzAxisRandomOffset);
        
        return new Vector3(
            basePosition.x ,
            basePosition.y + offsetY,  // 主要的 Y 軸隨機偏移
            basePosition.z
        );
    }


    /// <summary>
    /// 延遲初始化魚的移動
    /// </summary>
    private IEnumerator InitializeFishMovement(GameObject fish, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 检查 GameObject 是否仍然存在（可能在等待期间被销毁）
        if (fish == null)
        {
            yield break; // 如果已被销毁，直接退出协程
        }
        
        FishMovement movement = fish.GetComponent<FishMovement>();
        if (movement != null)
        {
            // 確保魚的移動腳本已啟動
            movement.enabled = true;
        }
    }

    /// <summary>
    /// Get all Fish data
    /// </summary>
    public List<Fish> GetFish()
    {
        return fish;
    }

    /// <summary>
    /// Get Fish data by color
    /// </summary>
    public Fish GetFishByColor(string color)
    {
        return fish.Find(f => f.color == color);
    }
    
    /// <summary>
    /// 获取场景中实际存在的特定颜色的鱼数量（实时统计）
    /// </summary>
    public int GetActualFishCountByColor(string color)
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag(color);
        return fishes != null ? fishes.Length : 0;
    }
    
    /// <summary>
    /// 获取场景中实际存在的所有鱼数量（实时统计）
    /// </summary>
    public int GetActualTotalFishCount()
    {
        int total = 0;
        foreach (string color in fishname)
        {
            total += GetActualFishCountByColor(color);
        }
        return total;
    }

    /// <summary>
    /// Get total spawned count
    /// </summary>
    public int GetTotalSpawnedCount()
    {
        int total = 0;
        foreach (Fish f in fish)
        {
            total += f.spawnedAmount;
        }
        return total;
    }

    /// <summary>
    /// Get total caught count
    /// </summary>
    public int GetTotalCaughtCount()
    {
        int total = 0;
        foreach (Fish f in fish)
        {
            total += f.caughtAmount;
        }
        return total;
    }

    /// <summary>
    /// 清理已生成的魚（用於重新生成）
    /// </summary>
    public void ClearAllFish()
    {
        // 停止所有协程，避免访问已销毁的对象
        StopAllCoroutines();
        
        // 找到所有魚並銷毀
        foreach (string fishTag in fishname)
        {
            GameObject[] fishes = GameObject.FindGameObjectsWithTag(fishTag);
            foreach (GameObject f in fishes)
            {
                Destroy(f);
            }
        }
        
        fish.Clear();
        isDataInitialized = false;
        
        Debug.Log("[FishSpawnManager] 已清除所有魚并停止所有协程");
    }

    /// <summary>
    /// 手動觸發重新生成
    /// </summary>
    [ContextMenu("Regenerate All Fish")]
    public void RegenerateAllFish()
    {
        ClearAllFish();
        InitializeFishData(); // 重新初始化資料
        StartCoroutine(SpawnFishWithDelay());
    }
    
    /// <summary>
    /// 檢查 Fish 資料是否已初始化
    /// </summary>
    public bool IsDataInitialized()
    {
        return isDataInitialized;
    }
    
    /// <summary>
    /// 在 Scene View 中顯示生成點的輔助線（用於調試）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        
        Gizmos.color = Color.cyan;
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                // 繪製球體標記生成點
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.1f);
                
                // 繪製向上的箭頭
                Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[i].position + Vector3.up * 0.2f);
                
                // 繪製 Y 軸隨機範圍
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(spawnPoints[i].position + Vector3.up * yAxisRandomOffset, 0.05f);
                Gizmos.DrawWireSphere(spawnPoints[i].position - Vector3.up * yAxisRandomOffset, 0.05f);
                Gizmos.color = Color.cyan;
            }
        }
    }
    
    // ========== 任务系统接口 ==========
    
    /// <summary>
    /// 应用鱼生成配置 - 使用数据驱动方式
    /// </summary>
    public void ApplySpawnConfig(FishSpawnConfig config)
    {   
        // 设置启用的颜色
        enabledColors.Clear();
        if (config.EnabledColors != null && config.EnabledColors.Length > 0)
        {
            enabledColors.AddRange(config.EnabledColors);
            Debug.Log($"[FishSpawnManager] 应用配置：启用 {config.EnabledColors.Length} 种颜色：{string.Join(", ", config.EnabledColors)}");
        }
        else
        {
            // 如果未指定，启用所有颜色
            enabledColors.AddRange(fishname);
            Debug.Log("[FishSpawnManager] 未指定颜色配置，启用所有颜色");
        }
        
        // 设置最小鱼数量
        if (config.MinFishPerColor > 0)
        {
            minFishPerColor = config.MinFishPerColor;
        }
        
        // 重新初始化数据
        InitializeFishData();
    }
    
    /// <summary>
    /// 重新生成指定颜色和数量的鱼（当 ClearBucket 销毁鱼后使用）
    /// </summary>
    /// <param name="fishToRegenerate">需要重新生成的鱼（颜色 -> 数量）</param>
    public void RegenerateFish(Dictionary<string, int> fishToRegenerate)
    {
        if (fishToRegenerate == null || fishToRegenerate.Count == 0)
        {
            Debug.LogWarning("[FishSpawnManager] 没有需要重新生成的鱼");
            return;
        }
        
        Debug.Log($"[FishSpawnManager] 开始重新生成 {fishToRegenerate.Count} 种鱼");
        
        // 更新 Fish 数据的 spawnedAmount
        foreach (var kvp in fishToRegenerate)
        {
            string color = kvp.Key;
            int count = kvp.Value;
            
            Fish fishData = fish.Find(f => f.color == color);
            if (fishData != null)
            {
                fishData.IncrementSpawned(count);
                Debug.Log($"[FishSpawnManager] 更新 {color} 计数，增加 {count}");
            }
            else
            {
                Debug.LogWarning($"[FishSpawnManager] 找不到颜色 {color} 的 Fish 数据");
            }
        }
        
        // 启动协程生成新鱼
        StartCoroutine(SpawnSpecificFish(fishToRegenerate));
    }
    
    /// <summary>
    /// 生成指定颜色和数量的鱼（协程）
    /// </summary>
    private IEnumerator SpawnSpecificFish(Dictionary<string, int> fishToSpawn)
    {
        // 确保有生成点
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[FishSpawnManager] 没有设置生成点！");
            yield break;
        }
        
        // 准备生成点列表
        List<Transform> availableSpawnPoints = PrepareSpawnPointsList();
        
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("[FishSpawnManager] 没有可用的生成点！");
            yield break;
        }
        
        int spawnPointIndex = Random.Range(0, availableSpawnPoints.Count); // 随机起始点
        int totalSpawned = 0;
        
        // 遍历需要生成的鱼
        foreach (var kvp in fishToSpawn)
        {
            string color = kvp.Key;
            int count = kvp.Value;
            
            // 找到对应的鱼 prefab 索引
            int prefabIndex = -1;
            for (int i = 0; i < fish.Count; i++)
            {
                if (fish[i].color == color)
                {
                    prefabIndex = i;
                    break;
                }
            }
            
            if (prefabIndex == -1 || prefabIndex >= fishPrefab.Length)
            {
                Debug.LogWarning($"[FishSpawnManager] 找不到颜色 {color} 的 prefab");
                continue;
            }
            
            Debug.Log($"[FishSpawnManager] 重新生成 {color}: {count} 只");
            
            // 生成指定数量的鱼
            for (int j = 0; j < count; j++)
            {
                // 检查生成点
                if (spawnPointIndex >= availableSpawnPoints.Count)
                {
                    if (allowReuseSpawnPoints)
                    {
                        spawnPointIndex = 0;
                    }
                    else
                    {
                        Debug.LogWarning($"[FishSpawnManager] 生成点不足，剩余 {count - j} 只 {color} 未生成");
                        break;
                    }
                }
                
                // 获取生成点位置
                Transform spawnPoint = availableSpawnPoints[spawnPointIndex];
                Vector3 spawnPosition = spawnPoint.position;
                
                // 生成鱼 GameObject
                GameObject spawnedFish = Instantiate(fishPrefab[prefabIndex], spawnPosition, Quaternion.identity);
                
                if (spawnedFish == null)
                {
                    Debug.LogError($"[FishSpawnManager] 无法生成 {color} 的 GameObject");
                    continue;
                }
                
                // 添加 FishData 组件
                FishData fishDataComponent = spawnedFish.GetComponent<FishData>();
                if (fishDataComponent == null)
                {
                    fishDataComponent = spawnedFish.AddComponent<FishData>();
                }
                fishDataComponent.SetPrefabName(color);
                
                // 注入 bucketReturnPoint
                FishForwardMovement movement = spawnedFish.GetComponent<FishForwardMovement>();
                if (movement != null && bucketReturnPoint != null)
                {
                    movement.SetBucketReturnPoint(bucketReturnPoint);
                }
                
                // 初始化 Rigidbody
                Rigidbody rb = spawnedFish.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    // 延迟初始化移动
                    StartCoroutine(InitializeFishMovement(spawnedFish, 0.2f));
                }
                
                Debug.Log($"[FishSpawnManager] 重新生成 {color} #{j+1} 在位置 {spawnPosition}");
                
                spawnPointIndex++;
                totalSpawned++;
                
                // 延迟
                yield return new WaitForSeconds(spawnDelay);
            }
        }
        
        Debug.Log($"[FishSpawnManager] 重新生成完成，总共 {totalSpawned} 只鱼");
    }
    
    /// <summary>
    /// 设置生成模式（难度）- 控制生成哪些颜色的鱼
    /// [已弃用] 请使用 ApplySpawnConfig 替代
    /// </summary>
    /// <param name="difficulty">0=Easy(单色), 1=Normal(3-4色), 2=Hard(3-4色)</param>
    [System.Obsolete("请使用 ApplySpawnConfig 替代")]
    public void SetSpawnMode(int difficulty)
    {
        enabledColors.Clear();
        
        switch (difficulty)
        {
            case 0: // Easy - 只生成一种颜色
                string singleColor = fishname[Random.Range(0, fishname.Length)];
                enabledColors.Add(singleColor);
                Debug.Log($"[FishSpawnManager] 初級模式：只生成 {singleColor}");
                break;
                
            case 1: // Normal - 随机生成 3-4 种颜色
            case 2: // Hard - 随机生成 3-4 种颜色
                int colorCount = Random.Range(3, Mathf.Min(5, fishname.Length + 1));
                
                List<string> availableColors = new List<string>(fishname);
                for (int i = 0; i < colorCount && availableColors.Count > 0; i++)
                {
                    int randomIndex = Random.Range(0, availableColors.Count);
                    enabledColors.Add(availableColors[randomIndex]);
                    availableColors.RemoveAt(randomIndex);
                }
                
                Debug.Log($"[FishSpawnManager] 中/高級模式：生成 {colorCount} 種顏色：{string.Join(", ", enabledColors)}");
                break;
        }
        
        // 只重新初始化数据，不立即生成鱼
        // 生成鱼需要调用 RegenerateAllFish()
        InitializeFishData();
    }
}
