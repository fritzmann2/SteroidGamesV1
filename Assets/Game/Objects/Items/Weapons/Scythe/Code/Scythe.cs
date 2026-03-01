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
        attackmulti = 1f;
        performattack(AttackTypeScythe.Slash.ToString());
    }
    override public void Attack2()
    {
        attackmulti = 0.9f;
        performattack(AttackTypeScythe.Round.ToString());
    }
    override public void Attack3()
    {
        attackmulti = 1.5f;
        performattack(AttackTypeScythe.Charge.ToString());
    }
    override public void Attack4()
    {
        if (isThrown) return;
        attackmulti = 0.7f;
        Vector3 direction = GetAimDirection().normalized;
        
        if (IsServer)
        {
            ThrowClientRpc(direction);
        }
        else
        {
            Debug.Log("try Throwing");
            ThrowServerRpc(direction);
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
    private void ThrowServerRpc(Vector3 direction)
    {
        ThrowClientRpc(direction);
    }

    [ClientRpc]
    private void ThrowClientRpc(Vector3 direction)
    {
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
        if (throwTime >= maxDistance / throwSpeed)
        {
            isReturning = true;
        }
        if (!groundcheck.IsTouchingLayers(groundLayer) && throwTime < maxDistance/throwSpeed)
        {
            if (!isReturning)
            {
                transform.position += throwDirection * throwSpeed * Time.deltaTime;
            }
        }
        else if (isReturning)
        {
            if (player == null)
            {
                Debug.LogWarning("Player not found");
            }
            transform.position = Vector3.MoveTowards(transform.position, player.position, returnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, player.position) < 0.1f)
            {
                Catch();
            }
        }
    }

    public void HandleRotation()
    {
        if (!isReturning)
        {
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime * viewdir);
        }
        else
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime * viewdir);
        }
    }
    void Catch()
    {
        isThrown = false;
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