using System;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float changeTargetInterval = 3.0f;
    
    [Header("Physics Settings")]
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private float drag = 1.0f;
    [SerializeField] private float angularDrag = 0.5f;
    
    private Vector3 velocity;
    private float timer;
    private Rigidbody rb;
    private bool isInitialized = false;

    void Start()
    {
        InitializeFish();
    }

    private void InitializeFish()
    {
        // get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // set Rigidbody properties
        rb.useGravity = false; // because fish are in water and don't need gravity
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        
        // make sure there is a collider
        if (GetComponent<Collider>() == null)
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.1f; // fish model size change
        }
        
        // initialize velocity
        velocity = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized * speed;
        
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || !enabled) return;
        
        if (usePhysics && rb != null)
        {
            // use Rigidbody to move
            rb.linearVelocity = velocity;
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
        
        // handle direction change
        timer += Time.deltaTime;
        if (timer >= changeTargetInterval)
        {
            ChangeDirection();
            timer = 0f;
        }
    }

    private void ChangeDirection()
    {
        // random new direction
        velocity = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Tank") || collision.gameObject.CompareTag("Bucket"))
        {
            // calculate reflection vector
            Debug.Log("[FishMovement] 碰到牆壁，改變方向");
            Vector3 normal = collision.contacts[0].normal;
            velocity = Vector3.Reflect(velocity, normal).normalized * speed;
            
            //reset timer to avoid immediate direction change
            timer = 0f;
        }
    }

    /// <summary>
    /// stop fish movement
    /// </summary>
    public void StopMovement()
    {
        velocity = Vector3.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// resume fish movement
    /// </summary>
    public void ResumeMovement()
    {
        ChangeDirection();
    }

    /// <summary>
    /// set fish speed
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        velocity = velocity.normalized * speed;
    }

    private void OnDisable()
    {
        // when script is disabled, stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        // when script is enabled, resume movement
        if (isInitialized && velocity.magnitude < 0.1f)
        {
            ResumeMovement();
        }
    }
}
