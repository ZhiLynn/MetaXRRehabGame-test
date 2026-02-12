using UnityEngine;
using System.Collections;

public class FishForwardMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float turnSpeed = 1.0f;

    [Header("Wall Avoidance")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallLayerMask;

    [Header("Behavioral Randomness")]
    [SerializeField] private float minChangeDirTime = 2.0f;
    [SerializeField] private float maxChangeDirTime = 5.0f;
    [SerializeField] private float randomTurnArc = 20.0f;

    [Header("States")]
    public bool hovered;
    public bool selected;
    public bool isInBucket = false;
    private bool arrivedAtBucketPoint = false;

    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public Quaternion initialRotation;
    [HideInInspector] public Transform initialParent;
    [HideInInspector] public Vector3 initialScale;

    [Header("Return Settings")]
    public float returnSpeed = 2f;

    private Rigidbody rb;
    private Vector3 targetDirection;
    private float timeToChangeDirection;
    private Animator animator;
    public GameObject bucketReturnPoint;
        // 生成時注入 bucketReturnPoint
        public void SetBucketReturnPoint(GameObject point)
        {
            bucketReturnPoint = point;
        }
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        transform.Rotate(0, Random.Range(0, 360), 0);
        targetDirection = transform.forward;

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialParent = transform.parent;
        initialScale = transform.localScale;

        ScheduleNextDirectionChange();
    }

    void FixedUpdate()
    {
        if (selected) return;

        // If inside bucket but not yet at the snap point, skip normal movement
        if (isInBucket && !arrivedAtBucketPoint)
            return;

        UpdateTargetDirection();
        RotateFish();
        MoveFish();
    }

    void UpdateTargetDirection()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, wallCheckDistance, wallLayerMask))
        {
            Vector3 reflectDir = Vector3.Reflect(transform.forward, hit.normal).normalized;
            transform.rotation = Quaternion.LookRotation(reflectDir, Vector3.up);
            targetDirection = reflectDir;
            ScheduleNextDirectionChange();
            return;
        }

        timeToChangeDirection -= Time.fixedDeltaTime;
        if (timeToChangeDirection <= 0)
        {
            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(-randomTurnArc, randomTurnArc),
                Random.Range(-randomTurnArc, randomTurnArc),
                0
            );
            targetDirection = (randomRotation * transform.forward).normalized;
            ScheduleNextDirectionChange();
        }
    }

    void RotateFish()
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
    }

    void MoveFish()
    {
        rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
    }

    void ScheduleNextDirectionChange()
    {
        timeToChangeDirection = Random.Range(minChangeDirTime, maxChangeDirTime);
    }

    public void SnapTo(Transform snapPoint)
    {
        transform.SetParent(snapPoint, true);
        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;
        transform.localScale = initialScale;
        selected = true;
        animator.SetBool("OnScoop", selected);
    }

    public void ReturnToOriginal()
    {
        selected = false;
        animator.SetBool("OnScoop", selected);
        transform.SetParent(initialParent, true);
        transform.localScale = initialScale;
        StopAllCoroutines();
        StartCoroutine(ReturnSmooth(initialPosition, initialRotation));
    }

    public void GoToNewPosition()
    {
        Vector3 newPosition = transform.position;
        Quaternion newRotation = transform.rotation;
        if(isInBucket){
            Debug.Log("Fish is in bucket , return to the bucket");
            newPosition = bucketReturnPoint.transform.position;
            newRotation = bucketReturnPoint.transform.rotation;
        }
        
        selected = false;
        isInBucket = false;
        arrivedAtBucketPoint = false;
        animator.SetBool("OnScoop", selected);
        transform.SetParent(initialParent, true);
        transform.localScale = initialScale;
        StopAllCoroutines();
        StartCoroutine(ReturnSmooth(newPosition, newRotation));
    }

    private IEnumerator ReturnSmooth(Vector3 targetPos, Quaternion targetRot)
    {
        float t = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bucket"))
        {
            isInBucket = true;
            arrivedAtBucketPoint = true;
            
            // 動態更新返回點：從碰撞到的桶子取得對應的返回點
            UpdateBucketReturnPoint(other.gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bucket"))
        {
            isInBucket = true;
            arrivedAtBucketPoint = true;
            
            // 動態更新返回點：從碰撞到的桶子取得對應的返回點
            UpdateBucketReturnPoint(other.gameObject);
        }
    }
    
    /// <summary>
    /// 從桶子物件取得對應的返回點並更新
    /// </summary>
    private void UpdateBucketReturnPoint(GameObject bucket)
    {
        // 【修正】排除 normalBucket，防止困難模式下被誤設定
        if (bucket == null)
            return;
        
        BucketEvent bucketEventComponent = bucket.GetComponent<BucketEvent>();
        BucketEvent normalModeBucket = MultiBucketManager.Instance?.GetNormalModeBucketEvent();
        
        // ✅ 只在困難模式下才排除 normalBucket
        if (MultiBucketManager.Instance != null && MultiBucketManager.Instance.IsHardMode)
        {
            // 如果這個桶是 normalBucket，在困難模式下應該被排除
            if (normalModeBucket != null && bucketEventComponent == normalModeBucket)
            {
                Debug.LogWarning($"[FishForwardMovement] 檢測到 normalBucket，在困難模式下應被排除，不更新返回點");
                return;
            }
        }
        
        // 優先從 BucketEvent 取得設定的返回點
        BucketEvent bucketEvent = bucket.GetComponent<BucketEvent>();
        if (bucketEvent == null)
        {
            // 嘗試從父物件取得
            bucketEvent = bucket.GetComponentInParent<BucketEvent>();
        }
        
        if (bucketEvent != null)
        {
            Transform returnPoint = bucketEvent.GetFishReturnPoint();
            if (returnPoint != null)
            {
                bucketReturnPoint = returnPoint.gameObject;
                Debug.Log($"[FishForwardMovement] 更新返回點為: {bucketReturnPoint.name} (來自 {bucket.name})");
                return;
            }
        }
        
        // 備用方案：嘗試找桶子的子物件 "FishReturnPoint"
        Transform childReturnPoint = bucket.transform.Find("FishReturnPoint");
        if (childReturnPoint != null)
        {
            bucketReturnPoint = childReturnPoint.gameObject;
            Debug.Log($"[FishForwardMovement] 更新返回點為子物件: {bucketReturnPoint.name}");
            return;
        }
        
        // 最後備用：使用桶子本身作為返回點
        bucketReturnPoint = bucket;
        Debug.Log($"[FishForwardMovement] 使用桶子本身作為返回點: {bucket.name}");
    }
}
