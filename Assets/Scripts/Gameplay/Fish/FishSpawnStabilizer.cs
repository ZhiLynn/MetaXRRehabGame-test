using UnityEngine;

/// <summary>
/// 防止魚在生成時因為物理碰撞而彈飛出 Tank
/// 附加到每個魚的 Prefab 上
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FishSpawnStabilizer : MonoBehaviour
{
    [Header("Stabilization Settings")]
    [Tooltip("生成後的穩定時間")]
    [SerializeField] private float stabilizationTime = 1.0f;
    
    [Tooltip("在穩定期間限制最大速度")]
    [SerializeField] private float maxVelocityDuringStabilization = 0.5f;
    
    [Tooltip("在穩定期間限制最大角速度")]
    [SerializeField] private float maxAngularVelocityDuringStabilization = 0.5f;
    
    private Rigidbody rb;
    private float spawnTime;
    private bool isStabilizing = true;
    private FishMovement fishMovement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        fishMovement = GetComponent<FishMovement>();
        spawnTime = Time.time;
    }

    private void Start()
    {
        // 在生成時暫時降低魚的物理反應
        if (rb != null)
        {
            // 增加阻尼以快速消除初始碰撞的能量
            rb.linearDamping = 5.0f;
            rb.angularDamping = 5.0f;
        }
        
        // 暫時禁用魚的移動腳本
        if (fishMovement != null)
        {
            fishMovement.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!isStabilizing) return;

        float timeSinceSpawn = Time.time - spawnTime;
        
        // 在穩定期間限制速度
        if (rb != null)
        {
            // 限制線性速度
            if (rb.linearVelocity.magnitude > maxVelocityDuringStabilization)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxVelocityDuringStabilization;
            }
            
            // 限制角速度
            if (rb.angularVelocity.magnitude > maxAngularVelocityDuringStabilization)
            {
                rb.angularVelocity = rb.angularVelocity.normalized * maxAngularVelocityDuringStabilization;
            }
        }

        // 檢查是否已經穩定
        if (timeSinceSpawn >= stabilizationTime)
        {
            CompleteStabilization();
        }
    }

    /// <summary>
    /// 完成穩定過程
    /// </summary>
    private void CompleteStabilization()
    {
        isStabilizing = false;
        
        if (rb != null)
        {
            // 恢復正常的阻尼值
            rb.linearDamping = 1.0f;
            rb.angularDamping = 0.5f;
            
            // 清除任何剩餘的速度
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // 重新啟用魚的移動腳本
        if (fishMovement != null)
        {
            fishMovement.enabled = true;
        }
        
        // 完成後移除此組件（不再需要）
        Destroy(this);
        
        Debug.Log($"[FishSpawnStabilizer] {gameObject.name} 穩定完成");
    }

    /// <summary>
    /// 在碰撞時減少衝擊力
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (!isStabilizing) return;
        
        // 在穩定期間減少碰撞反應
        if (rb != null)
        {
            // 大幅減少碰撞後的速度
            rb.linearVelocity *= 0.1f;
            rb.angularVelocity *= 0.1f;
        }
        
        Debug.Log($"[FishSpawnStabilizer] {gameObject.name} 在穩定期間與 {collision.gameObject.name} 碰撞");
    }

    /// <summary>
    /// 強制立即完成穩定（用於測試）
    /// </summary>
    [ContextMenu("Force Complete Stabilization")]
    public void ForceCompleteStabilization()
    {
        CompleteStabilization();
    }
}
