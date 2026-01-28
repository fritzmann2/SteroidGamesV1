using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class Scythe : Weapon
{
    private Transform player;
    private movement movement;
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
        attackmulti = 0.7f;
        Throw();
    }

    protected override void Awake()
    {
        movement = GetComponentInParent<movement>();
        type = EquipmentType.Scythe;
        base.Awake();
        player = transform.parent.parent;
        isThrown = false;
        
    }
    void Update()
    {
        if (isThrown)
        {
            HandleMovement();
        }

    }
    private void Throw()
    {
        anim.enabled = false;
        EnableHitbox();
        viewdir = movement.LooksRight ? 1 : -1;
        isAttacking = true;
        isThrown = true;
        isReturning = false;
        throwDirection = new Vector2(viewdir*1f, -0.1f).normalized;
        throwTime = 0f;

        transform.parent = null;
        Debug.Log("Thrown");
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
            Debug.Log("rotating forward");
        }
        else
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime * viewdir);
            Debug.Log("rotating backward");
        }
    }
    void Catch()
    {
        isThrown = false;
        isReturning = false;
        isAttacking = false;
        transform.parent = player;
        anim.enabled = true;
        DisableHitbox();
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity; 
        throwTime = 0f;   
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