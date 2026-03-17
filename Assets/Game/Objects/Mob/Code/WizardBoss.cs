using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WizardBoss : BaseBoss
{
    [Header("Teleport Settings")]
    public float teleportRadius = 15f;           
    public float minTeleportDistanceToPlayer = 8f; 
    public int maxTeleportAttempts = 20;  
    public Vector2 teleportClearance = new Vector2(1.5f, 2f);
    public float teleportTimer;
    private float teleportTimerCounter = 0f;

    [Header("Flying Settings")]
    public float obstacleCheckRadius = 1.5f; 
    public float obstacleCheckDistance = 1.5f;
    public float idealDistanceMax;

    [Header("Wizard Settings")]
    public GameObject projectilePrefab;
    protected float fireballHightf = 4f;


    public override void Reset()
    {
        bossName = "Death Wizard";
        id = "WizardBoss";
        itemSpawnPosition = new Vector3(-5, 981, 0);
        maxHealth = 1000;
        
        maxdistance = 60f;     
        attackDistance = 20f; 
        idealDistanceMax = 10f;
        mindistance = 7f;  
        
        attackCooldown = 2f;
        damage = 40f;
        movementSpeed = 1f; 
        fireballHightf = 0;
        teleportRadius = 8f;        
        minTeleportDistanceToPlayer = 6f; 
        maxTeleportAttempts = 20; 
        teleportTimer = 15f;
        baseXpReward = 500;
        health.Value = maxHealth;


        #if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("Projectile t:GameObject");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            else
            {
                Debug.LogError("WizardScript: Konnte kein Prefab mit Namen 'WizardProjectile' finden!");
            }
        #endif
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Reset();
    }

    public override void FixedUpdate()
    {
        if (!IsServer) return;

        targetPlayer = getNerestPlayer();
        
        if (targetPlayer != null)
        {
            FlyMovement();
            checkAttack(); 
        }
        else
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        if (teleportTimerCounter > 0f)
        {
            teleportTimerCounter -= Time.deltaTime;
        }
    }

    public override void Attack()
    {
        if (!IsServer) return;
        if (health.Value > maxHealth * 2/3)
        {
            possibleAttackManagement(1);
            attackCooldown = 1.5f;
        }
        else if (health.Value > maxHealth * 1/3)
        {
            possibleAttackManagement(2); 
            attackCooldown = 1.5f;
        }
        else
        {
            possibleAttackManagement(2);
            attackCooldown = 1.2f;
            teleportTimer = 12f;
        }
    }

    protected void possibleAttackManagement(int possibleAttacks)
    {
        if (possibleAttacks == 1)
        {
            spawnAttackProjectile();
        }
        else if (possibleAttacks == 2)
        {
            int randomNumber = Random.Range(1, 3);
            if (randomNumber == 1 && teleportTimerCounter <= 0)
            {
                teleportAttack();
            }
            else
            {
                spawnAttackProjectile();
            }
        }
    }

    protected void spawnAttackProjectile()
    {
        if (targetPlayer == null) return;

        Vector2 playerDir = new Vector2(targetPlayer.position.x - transform.position.x, 0f).normalized;
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(playerDir.x * 0.3f, fireballHightf);
        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        NetworkObject netObj = projectileInstance.GetComponent<NetworkObject>();
        netObj.Spawn(); 
        projectileInstance.GetComponent<BaseSpell>().Init(targetPlayer.position, damage, targetPlayer);        
    }

    private void FlyMovement()
    {
        Vector2 desiredDir = Vector2.zero;
        float dist = Vector2.Distance(transform.position, targetPlayer.position);

        if (dist > maxdistance)
        {
            desiredDir = Vector2.zero;
        }
        else if (dist > idealDistanceMax)
        {
            desiredDir = (targetPlayer.position - transform.position).normalized;
        }
        else if (dist < mindistance)
        {
            desiredDir = (transform.position - targetPlayer.position).normalized;
        }

        float facingDirection = (targetPlayer.position.x - transform.position.x) > 0 ? 1f : -1f;
        transform.localScale = new Vector3(facingDirection, 1f, 1f);

        if (desiredDir != Vector2.zero)
        {
            LayerMask wallMask = LayerMask.GetMask("Wall", "Ground");
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, obstacleCheckRadius, desiredDir, obstacleCheckDistance, wallMask);
            if (hit.collider != null)
            {
                Vector2 tangent = new Vector2(-hit.normal.y, hit.normal.x);
                if (Vector2.Dot(tangent, desiredDir) < 0)
                {
                    tangent = -tangent;
                }
                desiredDir = (tangent + hit.normal * 0.5f).normalized; 
                RaycastHit2D safetyHit = Physics2D.CircleCast(transform.position, obstacleCheckRadius, desiredDir, obstacleCheckDistance * 0.5f, wallMask);
                if (safetyHit.collider != null)
                {
                    desiredDir = Vector2.zero;
                }
            }
        }
        rb.linearVelocity = desiredDir * movementSpeed;
    }   

    protected void teleportAttack()
    {
        if (targetPlayer == null) return;
        Vector2 bestSpot = transform.position;
        bool foundValidSpot = false;
        LayerMask obstacleMask = LayerMask.GetMask("Ground", "Wall");

        for (int i = 0; i < maxTeleportAttempts; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * teleportRadius;
            Vector2 testPos = (Vector2)transform.position + randomOffset;
            if (Vector2.Distance(testPos, targetPlayer.position) < minTeleportDistanceToPlayer)
            {
                continue;
            }
            Vector2 directionToSpot = testPos - (Vector2)transform.position;
            float distanceToSpot = directionToSpot.magnitude;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToSpot.normalized, distanceToSpot, obstacleMask);
            
            if (hit.collider != null)
            {
                continue;
            }
            Collider2D obstacle = Physics2D.OverlapBox(testPos, teleportClearance, 0f, obstacleMask);
            
            if (obstacle == null)
            {
                bestSpot = testPos;
                foundValidSpot = true;
                break;
            }
        }

        if (foundValidSpot)
        {
            transform.position = bestSpot;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            TeleportEffectClientRpc(bestSpot);
            teleportTimerCounter = teleportTimer;
        }
    }

    [ClientRpc]
    private void TeleportEffectClientRpc(Vector2 newPos)
    {
        transform.position = newPos;
    }

    public override void OnHealthChanged(float previousValue, float newValue)
    {
        
        base.OnHealthChanged(previousValue, newValue);
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Teleport-Bereich (Cyan) - Runder Kreis
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, teleportRadius);

        // 2. Vorschau der benötigten Boss-Größe (Gelb)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(teleportClearance.x, teleportClearance.y, 0));

        // (Falls du die Flug-Gizmos aus der vorherigen Antwort nutzt, lass sie hier einfach drunter stehen!)
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, mindistance); // Flucht-Radius
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, maxdistance); // Verfolgungs-Radius
    }
}