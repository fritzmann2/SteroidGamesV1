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


    [Header("Collider Settings")]
    [SerializeField] private const float groundCheckPos = -0.5f; 
    [SerializeField] private  Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private const float wallCheckDistance = 0.6f;
    [SerializeField] private const float wallCheckHeight = -0.4f;
    [SerializeField] private const float voidCheckOffsetx = 0.6f;
    [SerializeField] private const float voidCheckStartY = -0.4f;
    [SerializeField] private const float voidCheckDistance = 1.5f;

    [Header("LayerMask")]
    private LayerMask groundLayer;
    private LayerMask wallLayer;

    [Header("Movement")]
    private float movementSpeed = 6f;
    public float jumpforce = 5f;
    private bool canJump = true;
    private float mindistance = 2;
    private float maxdistance = 10f;
    
    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float attackCooldownTimer = 0f;
    private float damage = 5f;

    


    override public void Awake()
    {
        base.Awake();
        worldgen = FindAnyObjectByType<WorldGenerator>();
        levelManager = FindAnyObjectByType<LevelManager>();
        rb = GetComponent<Rigidbody2D>();
        levelManager.onPlayerRegistered += updatePlayerList;
        updatePlayerList();
        mindistance = mindistance + Random.Range(10, 0)*0.05f;
        wallLayer = LayerMask.GetMask("Wall", "Ground");
        groundLayer = LayerMask.GetMask("Ground");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        levelManager.onPlayerRegistered -= updatePlayerList;
    }

    virtual public void FixedUpdate()
    {
        targetPlayer = getNerestPlayer();
        if (targetPlayer != null)
        {
            move();
            checkAttack();
            if (canJump)
            {
                checkForJump();
            }
        }
    }

    private void checkForJump()
    {
        bool isGrounded = Physics2D.BoxCast(transform.position, groundCheckSize, 0, Vector2.down, Mathf.Abs(groundCheckPos), groundLayer);
        
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

    public void move()
    {
        if (activePlayers.Count > 0 && targetPlayer != null)
        {
            Vector3 direction = targetPlayer.position - transform.position;
            float facingDirection = direction.x > 0 ? 1f : -1f;

            if (direction.x != 0) transform.localScale = new Vector3(facingDirection, 1f, 1f);

            Vector2 origin = new Vector2(
                transform.position.x + (voidCheckOffsetx * facingDirection), 
                transform.position.y + voidCheckStartY
            );

            bool isGroundAhead = Physics2D.Raycast(origin, Vector2.down, voidCheckDistance, groundLayer);

            bool distanceCheck = (direction.x > mindistance || direction.x < -mindistance) && 
                                 (direction.x < maxdistance || direction.x > -maxdistance);

            if (distanceCheck && isGroundAhead)
            {
                rb.linearVelocity = new Vector2(direction.normalized.x * movementSpeed, rb.linearVelocity.y);
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
            if (Vector3.Distance(targetPlayer.position, transform.position) < mindistance + mindistance / 10 && attackCooldownTimer <= 0f)
            {
                Attack();
                attackCooldownTimer = attackCooldown;
            }
        }
        
    }
    virtual public void Attack()
    {
        targetPlayer.GetComponent<BaseEntety>().TakeDamage(damage, false);
    }
    public override void OnHealthChanged(float previousValue, float newValue)
    {
        if (newValue <= 0)
        {
           worldgen.SpawnPickUpItem(id, transform);
           parentChunk.DespawnMob(this.GetComponent<NetworkObject>());
        }
        base.OnHealthChanged(previousValue, newValue);
        if (hpbarfiller != null)
        {
            hpbarfiller.transform.localScale = new Vector3 (newValue / maxHealth, 1f, 1f);
        }
    }

    public void Setparrent(WorldGenerator parrentworldgen)
    {
        this.worldgen = parrentworldgen;
    }

    public Transform getNerestPlayer()
    {
        Transform nearestPlayer = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Transform player in activePlayers)
        {
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
        activePlayers = levelManager.GetActivePlayers();
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
