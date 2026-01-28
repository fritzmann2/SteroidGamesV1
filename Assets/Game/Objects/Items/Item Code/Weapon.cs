using UnityEngine;
using Unity.Netcode;
using System;


abstract public class Weapon : InventoryItem
{

    [SerializeField] public WeaponStats weaponstats;
    public PlayerStats playerStats;
    public float attackmulti = 1f;
    public Animator anim;
    public Collider2D bx;
    private string attacktype;
    private Transform visualTarget;
    public Vector3 animOffset;
    private GameControls controls;
    private float moveInput;
    private float LastmoveInput = 1;
    public EquipmentType type;
    public bool isAttacking = false;
 
    virtual public void Attack1()
    {}
    virtual public void Attack2()
    {}
    virtual public void Attack3()
    {}
    virtual public void Attack4()
    {}

    protected virtual void Awake()
    {
        bx = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        controls = new GameControls();
        playerStats = GetComponentInParent<PlayerStats>();
    }
    
    public void init(WeaponStats _weaponstats)
    {
        weaponstats = _weaponstats;
        playerStats.UpdateStatsFromEquipment(4);

    }

    
    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    public void performattack(string _attacktype)
    {        
        if(anim != null && !isAttacking)
        {   
            attacktype = _attacktype;
            PlayAnimationClientsAndHostRpc(attacktype);
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void PlayAnimationClientsAndHostRpc(string attacktype)
    {
        if (anim != null)
        {
            anim.SetTrigger(attacktype.ToString());
        }
    }
    
    public virtual void EnableHitbox()
    {
        if (bx != null) bx.enabled = true;
    }

    public virtual void DisableHitbox()
    {
        if (bx != null) bx.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseEntety mob = other.GetComponent<BaseEntety>();

        if(other.CompareTag("Player"))
        {
            playerStats.DealotherDamage(mob, attackmulti);
            Debug.Log("Hit Player");

        }
        if (other.CompareTag("mob"))
        {
            playerStats.DealotherDamage(mob, attackmulti);
        }   
    }

    public float GetAnimationLength()
    {
        if (anim == null || anim.runtimeAnimatorController == null) return 0f;

        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == attacktype.ToString())
            {
                return clip.length;
            }
        }
        Debug.LogWarning("Animation nicht gefunden: " + attacktype.ToString());
        return 0f;
    }
    public void SetFollowTarget(Transform target)
    {
        visualTarget = target;
    }

    protected virtual void LateUpdate()
    {
        if (transform.parent == null) return;
        Vector2 inputVector = controls.Gameplay.Move.ReadValue<Vector2>();
        moveInput = inputVector.x;
        if (moveInput != 0f)
        {
            LastmoveInput = moveInput;
        }
        if (visualTarget != null)
        {
            Vector3 handPos = visualTarget.position;
        
            Vector3 finalPos = handPos + (visualTarget.rotation * animOffset * LastmoveInput);

            transform.position = finalPos;
        }
    }
}