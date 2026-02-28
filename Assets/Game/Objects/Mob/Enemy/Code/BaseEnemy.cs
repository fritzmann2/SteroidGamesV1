using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;


abstract public class BaseEnemy : BaseEntety
{
    [Header("General Settings")]
    public Rigidbody2D rb;
    public LevelManager levelManager;
    public GameObject hpbarfiller;
    public Transform targetPlayer;
    public WorldGenerator worldgen;
    public ChunkData parentChunk;
    private List<Transform> activePlayers = new List<Transform>();
    public string id = "Testsubject";
    public bool canSpawnItem = true;

    public virtual void Reset()    
    {
        health.Value = maxHealth;
        hpbarfiller = transform.GetChild(0).GetChild(0).gameObject;
        customGravity = 35f; 
        maxFallSpeed = 25f;
        baseXpReward = 50;

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
        rb = GetComponent<Rigidbody2D>();
        levelManager.onPlayerRegistered += updatePlayerList;
        updatePlayerList();
        attackDistance = attackDistance + Random.Range(10, 0)*0.05f;
        wallLayer = LayerMask.GetMask("Wall", "Ground");
        groundLayer = LayerMask.GetMask("Ground");
        attackCooldownTimer = attackCooldown;
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
        targetPlayer = getNerestPlayer();
        if (targetPlayer != null)
        {
            move();
            checkAttack();
            checkGravity();
            if (canJump)
            {
                checkForJump();
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

    private void checkForJump()
    {
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(0, groundCheckPos);
        isGrounded = Physics2D.OverlapBox(boxCenter, groundCheckSize, 0, groundLayer);

        float direction = transform.localScale.x > 0 ? 1 : -1;
        
        Vector2 wallCheckOrigin = (Vector2)transform.position + new Vector2(0, wallCheckHeight);

        bool hitsWall = Physics2D.Raycast(wallCheckOrigin, Vector2.right * direction, wallCheckDistance, wallLayer);

        if (isGrounded && hitsWall && rb.linearVelocity.y < 0.1f)
        {
            jump();
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

            Vector2 voidOrigin = new Vector2(
                transform.position.x + (voidCheckOffsetx * facingDirection), 
                transform.position.y + voidCheckStartY
            );
            bool isGroundAhead = Physics2D.Raycast(voidOrigin, Vector2.down, voidCheckDistance, groundLayer);

            bool distanceCheck = Mathf.Abs(direction.x) > attackDistance && Mathf.Abs(direction.x) < maxdistance;

            if (distanceCheck && isGroundAhead)
            {
                RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, groundLayer);
                
                if (groundHit.collider != null)
                {
                    Vector2 groundNormal = groundHit.normal;
                    
                    Vector2 slopeDirection = new Vector2(groundNormal.y, -groundNormal.x);
                    
                    if (facingDirection < 0) slopeDirection = -slopeDirection;

                    rb.linearVelocity = slopeDirection * movementSpeed;
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
        if (newValue <= 0)
        {
            if (canSpawnItem)
            {
                int randomnum = Random.Range(0, 2);
                if (randomnum == 0)
                {
                    worldgen.SpawnPickUpItem(id, transform);
                }
            }
            parentChunk.DespawnMob(this.GetComponent<NetworkObject>());
            DistributeXP();
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
        parentChunk.DespawnMob(this.GetComponent<NetworkObject>());
    }
}
