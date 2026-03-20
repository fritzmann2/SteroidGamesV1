using UnityEngine;
using Unity.Netcode;

public class BaseArrow : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float movementspeed = 20f; 
    [SerializeField] private float gravity = 7f;

    private LayerMask groundLayerMask;
    private float despawnTimer = 15f;
    private BoxCollider2D bx;
    private SpriteRenderer sr;
    private bool hasHitWall = false;
    private Transform owner;
    private Vector3 velocity;
    private bool hasHit = false;

    private bool isInitialized = false; 

    void Awake()
    {
        bx = GetComponent<BoxCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        
        bx.isTrigger = true;
        bx.enabled = true;
        groundLayerMask = LayerMask.GetMask("Ground");
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
        if (isInitialized && !hasHitWall && !hasHit)
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
                if (NetworkObject != null && NetworkObject.IsSpawned)
                    NetworkObject.Despawn();
            }
            
            if (owner != null && Vector3.Distance(owner.position, transform.position) > 60f)
            {
                if (NetworkObject != null && NetworkObject.IsSpawned)
                    NetworkObject.Despawn();
            }
        }
    }

    private void hitWall()
    {
        hasHitWall = true;
        hasHit = true;
        bx.enabled = false;
        velocity = Vector3.zero;

        if (despawnTimer > 3f)
        {
            despawnTimer = 3f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        bool hitGroundLayer = ((1 << other.gameObject.layer) & groundLayerMask) != 0;
        if (other.CompareTag("Obstacle") || hitGroundLayer)
        {
            hitWall();
            return;
        }

        if (owner == null) return; 
        
        NetworkObject ownerNetObj = owner.GetComponent<NetworkObject>();
        
        if (ownerNetObj != null && ownerNetObj.IsOwner)
        {
            if (other.TryGetComponent<BaseEntety>(out BaseEntety mob))
            {
                if (other.transform != owner)
                {
                    hasHit = true;
                    
                    owner.GetComponent<PlayerStats>().DealotherDamage(mob, 0.6f);
                    velocity = Vector3.zero; 
                    if (sr != null) sr.enabled = false;
                    bx.enabled = false;
                    RequestDespawnServerRpc();
                }
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestDespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}