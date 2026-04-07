using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections.Generic;


public class Scythe : Weapon
{
    public LayerMask groundLayer;
    private float throwSpeed = 10f;
    private float returnSpeed = 14f;
    private float maxDistance = 6f;
    private float rotationSpeed = 1080f;
    private float throwTime = 0f;
    private int viewdir;
    private bool isThrown = false;      
    private bool isReturning = false;  
    private Vector3 throwDirection;
    public BoxCollider2D groundcheck;

    [Header("Auto Aim")]
    [SerializeField] private float autoAimRadius = 20f;


    override public void Attack1()
    {
        if (isThrown || isAttacking) return; 
        attackmulti = 1f;
        performattack(AttackTypeScythe.Slash.ToString());
    }
    
    override public void Attack2()
    {
        if (isThrown || isAttacking) return;
        attackmulti = 0.9f;
        performattack(AttackTypeScythe.Round.ToString());
    }
    
    override public void Attack3()
    {
        if (isThrown || isAttacking) return;
        attackmulti = 1.5f;
        performattack(AttackTypeScythe.Charge.ToString());
    }
    
    override public void Attack4()
    {
        if (isThrown || isAttacking) return; 
        attackmulti = 0.7f;
        Vector3 direction = GetAimDirection().normalized;
        float currentSpeed = 1f;
        if (playerStats != null)
        {
            currentSpeed = playerStats.getTotalStats().attackSpeed;
            if (currentSpeed < 1f) currentSpeed = 1f;
        }
        if (IsServer)
        {
            ThrowClientRpc(direction, currentSpeed);
        }
        else
        {
            Debug.Log("try Throwing");
            ThrowServerRpc(direction, currentSpeed);
        }
    }

    protected override void Awake()
    {
        type = EquipmentType.Scythe;
        base.Awake();
        isThrown = false;
        groundLayer = LayerMask.GetMask("Ground");       
        DisableHitbox(); 
    }

    protected override void Start()
    {
        base.Start();
    }
    void Update()
    {
        if (isThrown)
        {
            HandleMovement();
        }
    }

    private Vector3 GetAimDirection()
    {
        if (Gamepad.current != null)
        {
            Transform nearestEnemy = GetNearestEnemy();
            if (nearestEnemy != null)
            {
                Vector3 aimDir = nearestEnemy.position - transform.position;
                aimDir.z = 0f;
                return aimDir;
            }
            else
            {
                float facingDir = transform.parent != null ? transform.parent.localScale.x : 1f;
                return new Vector3(facingDir, 0f, 0f);
            }
        }
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;
        return mouseWorldPos - transform.position;
    }

    private Transform GetNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, autoAimRadius);
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Mob"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = hit.transform;
                }
            }
        }
        return nearest;
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ThrowServerRpc(Vector3 direction, float currentSpeed)
    {
        ThrowClientRpc(direction, currentSpeed);
    }

    [ClientRpc]
    private void ThrowClientRpc(Vector3 direction, float currentSpeed)
    {
        attackspeed = currentSpeed;
        anim.enabled = false;
        
        EnableHitbox();
        viewdir = transform.localScale.x > 0 ? 1 : -1;
        throwDirection = direction;
        
        throwTime = 0f;
        isAttacking = true;
        isThrown = true;
        isReturning = false;
        transform.parent = null;
    }

    private void HandleMovement()
    {
        HandleRotation();
        throwTime += Time.deltaTime;

        float currentThrowSpeed = throwSpeed * attackspeed;
        float currentReturnSpeed = returnSpeed * attackspeed;
        if (throwTime >= maxDistance / currentThrowSpeed)
        {
            isReturning = true;
        }
        if (!groundcheck.IsTouchingLayers(groundLayer) && throwTime < maxDistance / currentThrowSpeed)
        {
            if (!isReturning)
            {
                transform.position += throwDirection * currentThrowSpeed * Time.deltaTime;
            }
        }
        else if (isReturning)
        {
            if (player == null)
            {
                Debug.LogWarning("Player not found");
            }
            transform.position = Vector3.MoveTowards(transform.position, player.position, currentReturnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, player.position) < 0.1f)
            {
                Catch();
            }
        }
    }

    public void HandleRotation()
    {
        float currentRotationSpeed = rotationSpeed * attackspeed;

        if (!isReturning)
        {
            transform.Rotate(0, 0, -currentRotationSpeed * Time.deltaTime * viewdir);
        }
        else
        {
            transform.Rotate(0, 0, currentRotationSpeed * Time.deltaTime * viewdir);
        }
    }
    
    void Catch()
    {
        isThrown = false;
        isAttacking = false;
        isReturning = false;
        throwTime = 0f;   
        addParent();
    }

    public override void EnableHitbox()
    {
        BoxCollider2D[] hitboxes = GetComponentsInChildren<BoxCollider2D>();

        foreach (BoxCollider2D bx in hitboxes)
        {
            bx.enabled = true;
        }
    }
    public override void DisableHitbox()
    {
        BoxCollider2D[] hitboxes = GetComponentsInChildren<BoxCollider2D>();
        hittedTargets = new List<Transform>();
        Debug.Log("Hitbox disabled");
        foreach (BoxCollider2D bx in hitboxes)
        {
            bx.enabled = false;
        }
    }
}

public enum AttackTypeScythe
{
    Round,   
    Slash,
    Charge,
    Throw
}