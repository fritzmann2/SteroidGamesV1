using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class BaseSpell : NetworkBehaviour
{
    [Header("Spell Settings")]
    protected float speed = 7f;
    protected float damage = 0;
    protected float despawnTime = 5f;

    public NetworkVariable<int> spelltype = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    protected float baseAnimationTimer = 0.5f;
    protected float animationTimer;
    
    [Header("Homing Settings")]
    public float rotateSpeed = 40f;

    protected Vector2 direction = Vector2.zero;
    protected Rigidbody2D rb;
    private Transform target;
    public List<Sprite> spellSprites;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animationTimer = baseAnimationTimer;
    }
    public virtual void Init(Vector2 targetPosition, float _damage)
    {
        spelltype.Value = 0;
        spriteRenderer.sprite = spellSprites[0];
        damage = _damage; 
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        SetMovementClientRpc(dir, 0);
    }

    public void Init(Vector2 targetPosition, float _damage, Transform _target)
    {
        speed += speed * 0.1f;
        if (IsServer)
        {
            spelltype.Value = 1; 
        }
        spriteRenderer.sprite = spellSprites[2];
        target = _target;
        damage = _damage;
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        ulong targetNetId = 0;
        if (_target != null && _target.TryGetComponent(out NetworkObject netObj))
        {
            targetNetId = netObj.NetworkObjectId;
        }
        SetMovementClientRpc(dir, targetNetId);
    }

    [ClientRpc]
    private void SetMovementClientRpc(Vector2 dir, ulong targetNetId)
    {
        direction = dir;
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        if (targetNetId != 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out NetworkObject targetObj))
        {
            target = targetObj.transform;
        }
    }

    public virtual void FixedUpdate()
    {
        if (target != null && rb != null)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotateSpeed * Time.fixedDeltaTime);
            direction = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));

            rb.linearVelocity = direction * speed;
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }

        if (spelltype.Value == 0)
        {
            if (animationTimer <= 0f)
            {
                if (spriteRenderer.sprite != spellSprites[1])
                {
                    spriteRenderer.sprite = spellSprites[1];
                }
                else
                {
                    spriteRenderer.sprite = spellSprites[0];
                }
                animationTimer = baseAnimationTimer;
            }
        }
        else if (spelltype.Value == 1)
        {
            if (spriteRenderer.sprite != spellSprites[2])
            {
                spriteRenderer.sprite = spellSprites[2];
            }
        }

        if (animationTimer > 0f)
        {
            animationTimer -= Time.fixedDeltaTime;
        }

        if (!IsServer) return; 

        if (despawnTime > 0f)
        {
            despawnTime -= Time.fixedDeltaTime;
        }
        else if (IsSpawned)
        {
            Despawn();
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<BaseEntety>().TakeDamage(damage, false);
            Despawn();
        }
        else if (other.CompareTag("Obstacle"))
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}