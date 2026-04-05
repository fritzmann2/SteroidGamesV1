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
    protected List<Transform> activePlayers = new List<Transform>();
    public string id = "Testsubject";
    public bool canSpawnItem = true;

    [Header("Optimization")]
    public float activationDistance = 20f;
    private float timeSinceSpawn = 0f;

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
    public float movementSpeed = 6f;
    public float jumpforce = 10f;
    [SerializeField] protected bool canJump = true;
    [SerializeField] protected bool isGrounded;
    [SerializeField] protected bool isWallAhed;
    [SerializeField] protected bool isVoidAhed;
    [SerializeField] protected float mindistance = 1f;
    [SerializeField] protected float attackDistance = 8f;
    [SerializeField] protected float maxdistance = 20f;
    
    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    protected float attackCooldownTimer = 0f;
    [SerializeField] protected float damage = 5f;

    [Header("XP System")]
    public int baseXpReward = 50;
    public float sharedXpRadius = 15f;
    public float sharedXpPercentage = 0.5f;
    private Transform lastAttacker;

    public virtual void Reset()    
    {
        hpbarfiller = transform.GetChild(0).GetChild(0).gameObject;
        if (IsSpawned && IsServer)
        {
            health.Value = maxHealth;
        }
        customGravity = 35f; 
        maxFallSpeed = 25f;
        baseXpReward = 50;
        
        timeSinceSpawn = 0f;
    }


    override public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        anim = GetComponent<Animator>();
        if (!IsServer) return;
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
        if (levelManager != null)
        {
            levelManager.onPlayerRegistered -= updatePlayerList;
        }
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
            checkGravity();
            if (canJump)
            {
                jumpCheck();
            }
            move();
            checkAttack();
        }
    }

    private void checkCollisions()
    {
        groundCheck();
        isWallAhed = wallCheck(wallCheckHeight);
        voidCheck();
    }

    private bool wallCheck(float _wallCheckHeight)
    {
        int facingDirection = transform.localScale.x > 0 ? 1 : -1;

        Vector3 origin = new Vector3(0f, _wallCheckHeight, 0f);
        Vector2 direction = Vector2.right * facingDirection; 
        RaycastHit2D hit = Physics2D.Raycast(transform.position + origin, direction, wallCheckDistance, wallLayer);
        if (hit.collider != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void voidCheck()
    {
        int facingDirection = transform.localScale.x > 0 ? 1 : -1;

        Vector3 origin = new Vector3 (voidCheckOffsetx * facingDirection, voidCheckStartY, 0f);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + origin, Vector2.down, voidCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            isVoidAhed = false;
        }
        else
        {
            isVoidAhed = true;
        }
    }

    private void groundCheck()
    {
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(0, groundCheckPos);
        Collider2D hit = Physics2D.OverlapBox(boxCenter, groundCheckSize, 0f, groundLayer);
        if (hit != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void jumpCheck()
    {
        if (isGrounded && !isVoidAhed && isWallAhed)
        {
            if (!wallCheck(1)) 
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpforce);
            }
        }
    }

    

    protected virtual void move()
    {
        if (activePlayers.Count > 0 && targetPlayer != null)
        {
            Vector3 direction = targetPlayer.position - transform.position;
            
            float distanceX = Mathf.Abs(direction.x);
            
            if (distanceX > 0.1f) 
            {
                float facingDirection = direction.x > 0 ? 1f : -1f;
                transform.localScale = new Vector3(facingDirection, 1f, 1f);
                
                if (!isWallAhed && !isVoidAhed)
                {
                    float distance = Vector3.Distance(transform.position, targetPlayer.position);
                    if (distance < maxdistance && distance > attackDistance)
                    {
                        rb.linearVelocity = new Vector2(facingDirection * movementSpeed, rb.linearVelocity.y);
                    }
                }
                else
                {
                    stop();
                }
            }
            else
            {
                stop();
            }
        }
        else
        {
            stop();
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


    protected void stop() { rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); }

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
        if (newValue <= 0 && IsServer)
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
        if (transform.tag == "Player") return;
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

}