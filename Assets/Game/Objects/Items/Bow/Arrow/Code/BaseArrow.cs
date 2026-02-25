using UnityEngine;
using Unity.Netcode;

public class BaseArrow : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float movementspeed; 
    [SerializeField] private float gravity;

    private float despawnTimer = 15f;
    private BoxCollider2D bx;
    private bool hasHitWall = false;
    private Transform owner;
    private Vector3 velocity;

    private bool isInitialized = false; 

    void Reset()
    {
        movementspeed = 20f;
        gravity = 7f;
    }

    void Awake()
    {
        bx = GetComponent<BoxCollider2D>();
        bx.isTrigger = true;
        bx.enabled = true;
    }

    public void init(Vector3 _direction, Quaternion _rotation, Transform _owner)
    {
        velocity = _direction.normalized * movementspeed;
        transform.rotation = _rotation;
        owner = _owner;
        isInitialized = true; 
    }

    void FixedUpdate()
    {
        if (isInitialized)
        {
            if (!hasHitWall)
            {
                velocity.y -= gravity * Time.fixedDeltaTime;
                transform.position += velocity * Time.fixedDeltaTime;
                
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle); 
            }
            
            if (IsServer)
            {
                despawnTimer -= Time.fixedDeltaTime;
                if (despawnTimer <= 0)
                {
                    GetComponent<NetworkObject>().Despawn();
                }
                
                if (owner != null && Vector3.Distance(owner.position, transform.position) > 60f)
                {
                    GetComponent<NetworkObject>().Despawn();
                }
            }
        }
    }

    private void hitWall()
    {
        hasHitWall = true;
        bx.enabled = false;
        if (despawnTimer > 3f)
        {
            despawnTimer = 3f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            hitWall();
            return;
        }

        if (!IsServer) return; 
        if (owner == null) return; 
        if (other.TryGetComponent<BaseEntety>(out BaseEntety mob))
        {
            if (other.transform != owner)
            {
                owner.GetComponent<PlayerStats>().DealotherDamage(mob, 0.4f);
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}