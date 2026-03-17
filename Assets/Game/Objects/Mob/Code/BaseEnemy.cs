using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

abstract public class BaseEnemy : BaseEntety
{
    [Header("General Settings")]
    public LevelManager levelManager;
    public GameObject hpbarfiller;
    public Transform targetPlayer;
    public WorldGenerator worldgen;
    public ChunkData parentChunk;
    private List<Transform> activePlayers = new List<Transform>();
    public string id = "Testsubject";
    public bool canSpawnItem = true;

    [Header("Optimization")]
    public float activationDistance = 20f;
    private float timeSinceSpawn = 0f;

    public virtual void Reset()    
    {
        if (IsSpawned && IsServer)
        {
            health.Value = maxHealth;
        }
        hpbarfiller = transform.GetChild(0).GetChild(0).gameObject;
        customGravity = 35f; 
        maxFallSpeed = 25f;
        baseXpReward = 50;
        
        timeSinceSpawn = 0f;
    }

    [Header("Custom Gravity")]
    [SerializeField] private float customGravity; 
    [SerializeField] private float maxFallSpeed;


    [Header("Collider Settings")]
    [SerializeField] protected float groundCheckPos = -0.5f; 
    [SerializeField] protected Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] protected float wallCheckDistance = 0.6f;
    [SerializeField] protected float wallCheckHeight = -0.4f;
    [SerializeField] protected float voidCheckOffsetx = 0.6f;
    [SerializeField] protected float voidCheckStartY = -0.4f;
    [SerializeField] protected float voidCheckDistance = 1.5f;

    [Header("LayerMask")]
    private LayerMask groundLayer;
    private LayerMask wallLayer;

    [Header("Movement")]
    protected float movementSpeed = 6f;
    public float jumpforce = 5f;
    private bool canJump = true;
    private bool isGrounded;
    [SerializeField] protected float mindistance = 1f;
    [SerializeField] protected float attackDistance = 8f;
    [SerializeField] protected float maxdistance = 20f;
    
    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    protected float attackCooldownTimer = 0f;
    protected float damage = 5f;

    [Header("XP System")]
    public int baseXpReward = 50;
    public float sharedXpRadius = 15f;
    public float sharedXpPercentage = 0.5f;
    private Transform lastAttacker;


    override public void Awake()
    {
        Reset();
        base.Awake();
        worldgen = FindAnyObjectByType<WorldGenerator>();
        levelManager = FindAnyObjectByType<LevelManager>();
        levelManager.onPlayerRegistered += updatePlayerList;
        updatePlayerList();
        attackDistance = attackDistance + Random.Range(10, 0)*0.05f;
        
        wallLayer = LayerMask.GetMask("Wall", "Ground");
        groundLayer = LayerMask.GetMask("Ground");
        attackCooldownTimer = attackCooldown;

        if (rb != null)
        {
            rb.gravityScale = 0f; 
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
        }
    }


    virtual public void Attack() {}

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        levelManager.onPlayerRegistered -= updatePlayerList;
    }

    virtual public void FixedUpdate()
    {
        if (!IsServer) return;

        timeSinceSpawn += Time.fixedDeltaTime;

        if (timeSinceSpawn < 1f)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            return;
        }
        else if (rb.bodyType == RigidbodyType2D.Kinematic) 
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        targetPlayer = getNerestPlayer();
        
        if (targetPlayer != null)
        {
            if (Vector3.Distance(transform.position, targetPlayer.position) > activationDistance)
            {
                rb.linearVelocity = Vector2.zero; 
                return; 
            }

            checkCollisions();
            move();
            checkGravity();
            checkAttack();
        }
    }

    private void checkCollisions()
    {
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(0, groundCheckPos);
        isGrounded = Physics2D.OverlapBox(boxCenter, groundCheckSize, 0, groundLayer);
        
        if (isGrounded && canJump && rb.linearVelocity.y <= 0.1f)
        {
            float direction = transform.localScale.x > 0 ? 1 : -1;
            Vector2 wallCheckOrigin = (Vector2)transform.position + new Vector2(0, wallCheckHeight); 
            RaycastHit2D wallHit = Physics2D.Raycast(wallCheckOrigin, Vector2.right * direction, wallCheckDistance, wallLayer);

            if (wallHit.collider != null)
            {
                if (Mathf.Abs(wallHit.normal.x) > 0.7f) 
                {
                    jump();
                }
            }
        }
    }

    private void jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpforce);
    }

    private void move()
    {
        if (activePlayers.Count > 0 && targetPlayer != null)
        {
            Vector3 direction = targetPlayer.position - transform.position;
            float facingDirection = direction.x > 0 ? 1f : -1f;
            if (direction.x != 0) transform.localScale = new Vector3(facingDirection, 1f, 1f);

            bool distanceCheck = Mathf.Abs(direction.x) > attackDistance && Mathf.Abs(direction.x) < maxdistance;

            if (distanceCheck)
            {
                Vector2 voidOrigin = new Vector2(transform.position.x + (voidCheckOffsetx * facingDirection), transform.position.y + voidCheckStartY);
                bool isGroundAhead = Physics2D.Raycast(voidOrigin, Vector2.down, voidCheckDistance * 1.5f, groundLayer);
                if (isGrounded && !isGroundAhead) { stop(); return; }
                if (isGrounded)
                {
                    Vector2 slopeCheckOrigin = (Vector2)transform.position + new Vector2(0, groundCheckPos + 0.2f);
                    Vector2 slopeCheckDir = new Vector2(facingDirection, -1f).normalized;
                    RaycastHit2D slopeHit = Physics2D.Raycast(slopeCheckOrigin, slopeCheckDir, 1f, groundLayer);
                    if (slopeHit.collider != null)
                    {
                        Vector2 normal = slopeHit.normal;
                        if (normal.y < 0.99f)
                        {
                            Vector2 slopeDir = new Vector2(normal.y, -normal.x);
                            if (facingDirection < 0) slopeDir = -slopeDir;
                            
                            rb.linearVelocity = slopeDir * movementSpeed;
                            return;
                        }
                    }
                    rb.linearVelocity = new Vector2(facingDirection * movementSpeed, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(facingDirection * movementSpeed, rb.linearVelocity.y);
                }
            }
            else
            {
                stop();
            }
        }
    }

    private void checkGravity()
    {
        if (!isGrounded)
        {
            float currentGravity = customGravity;
            float newVelocityY = rb.linearVelocity.y - (currentGravity * Time.fixedDeltaTime);
            newVelocityY = Mathf.Max(newVelocityY, -maxFallSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newVelocityY);
        }
    }


    private void stop() { rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); }

    public void checkAttack()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
        else
        {
            if (Vector3.Distance(targetPlayer.position, transform.position) < attackDistance + attackDistance / 10 && attackCooldownTimer <= 0f)
            {
                Attack();
                attackCooldownTimer = attackCooldown * (1 + Random.Range(0f, 0.2f));
            }
        }
        
    }
    public override void OnHealthChanged(float previousValue, float newValue)
    {
        if (!IsServer) return;
        if (newValue <= 0)
        {
            if (canSpawnItem)
            {
                int randomnum = Random.Range(0, 2);
                if (randomnum == 0)
                {
                    ItemManager.Instance.SpawnPickUpItem(id, transform.position);
                }
            }
            if (parentChunk != null)
            {
                parentChunk.myMobs.Remove(GetComponent<NetworkObject>());
            }
            DistributeXP();
            GetComponent<NetworkObject>().Despawn();
        }
        base.OnHealthChanged(previousValue, newValue);
        if (hpbarfiller != null)
        {
            hpbarfiller.transform.localScale = new Vector3 (newValue / maxHealth, 1f, 1f);
        }
    }

    protected void DistributeXP()
    {
        for (int i = activePlayers.Count - 1; i >= 0; i--)
        {
            Transform player = activePlayers[i];
            if (player == null) 
            {
                activePlayers.RemoveAt(i);
                continue;
            }

            int xpToGive = 0;

            if (player == lastAttacker)
            {
                xpToGive = baseXpReward;
            }
            else if (Vector3.Distance(transform.position, player.position) <= sharedXpRadius)
            {
                xpToGive = Mathf.RoundToInt(baseXpReward * sharedXpPercentage);
            }

            if (xpToGive > 0)
            {
                PlayerStats pStats = player.GetComponent<PlayerStats>();
                if (pStats != null)
                {
                    pStats.ReceiveXPClientRpc(xpToGive);
                }
            }
        }
    }

    public void SetLastAttacker(Transform attacker)
    {
        lastAttacker = attacker;
    }

    public void Setparrent(WorldGenerator parrentworldgen)
    {
        worldgen = parrentworldgen;
    }

    public Transform getNerestPlayer()
    {
        Transform nearestPlayer = null;
        float nearestDistance = Mathf.Infinity;

        for (int i = activePlayers.Count - 1; i >= 0; i--)
        {
            Transform player = activePlayers[i];

            if (player == null)
            {
                activePlayers.RemoveAt(i); 
                continue;                
            }

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPlayer = player;
            }
        }
        
        return nearestPlayer;        
    }

    public void updatePlayerList()
    {
        activePlayers = new List<Transform>(levelManager.GetActivePlayers().Values);
    }

    public void SetParentChunk(ChunkData chunk)
    {
        parentChunk = chunk;
    }

    private void OnDrawGizmos()
    {
        float direction = transform.localScale.x > 0 ? 1 : -1;

        // 1. Wall Check (Red)
        Gizmos.color = Color.red;
        Vector3 wallCheckStart = transform.position + new Vector3(0, wallCheckHeight, 0);
        Gizmos.DrawLine(wallCheckStart, wallCheckStart + new Vector3(direction * wallCheckDistance, 0, 0));

        // 2. Void Check (Yellow)
        Gizmos.color = Color.yellow;
        Vector3 voidOrigin = new Vector3(
            transform.position.x + (voidCheckOffsetx * direction), 
            transform.position.y + voidCheckStartY, 
            0
        );
        Gizmos.DrawLine(voidOrigin, voidOrigin + Vector3.down * voidCheckDistance);

        // 3. Ground Check (Green)
        Gizmos.color = Color.green;
        Vector3 boxCenter = transform.position + new Vector3(0, groundCheckPos, 0);
        Gizmos.DrawWireCube(boxCenter, new Vector3(groundCheckSize.x, groundCheckSize.y, 1));
    }

    public void Kill()
    {
        //parentChunk.DespawnMob(this.GetComponent<NetworkObject>());
    }
}