using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// [已棄用] 此類已被 BucketEvent 取代
/// 保留僅供參考，建議使用 BucketEvent 處理魚的碰撞事件
/// </summary>
[System.Obsolete("此類已被 BucketEvent 取代，請使用 BucketEvent")]
public class FishEvent : MonoBehaviour
{
    [SerializeField]private GameManager gameEvent;
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fish"))
        {
            gameEvent.score += 1;
            Debug.LogWarning("Fish caught! Score: " + gameEvent.score);
            Destroy(collision.gameObject);
        }
    }
}