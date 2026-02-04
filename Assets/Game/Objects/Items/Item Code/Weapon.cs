using UnityEngine;
using Unity.Netcode;
using System.Collections;


abstract public class Weapon : InventoryItem
{

    [SerializeField] public WeaponStats weaponstats;
    protected PlayerMovement movement;

    public PlayerStats playerStats;
    protected Transform player;

    public float attackmulti = 1f;
    public Animator anim;
    public Collider2D bx;
    protected Transform handPosition;
    public Vector3 animOffset;
    protected GameControls controls;
    protected float moveInput;
    protected float LastmoveInput = 1;
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
        bx.enabled = false;
        anim = GetComponent<Animator>();
        controls = new GameControls();
        playerStats = GetComponentInParent<PlayerStats>();
        movement = GetComponentInParent<PlayerMovement>();
        handPosition = transform.parent.GetChild(0).transform;
        player = transform.parent.parent;
        transform.localPosition = handPosition.localPosition;
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
        anim.enabled = true;
        if(anim != null && !isAttacking)
        {   
            PlayAnimationClientsAndHostRpc(_attacktype);
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
        Debug.Log("Enable Hitbox");
    }

    public virtual void DisableHitbox()
    {
        if (bx != null) bx.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == playerStats.gameObject) 
        {
            return; 
        }
        BaseEntety mob = other.GetComponent<BaseEntety>();
        if (other.CompareTag("Mob"))
        {
            Debug.Log("Hit Mob");
            playerStats.DealotherDamage(mob, attackmulti);
        } 
        if(other.CompareTag("Player"))
        {
            playerStats.DealotherDamage(mob, attackmulti);
            Debug.Log("Hit Player");
        }
    }

    public float GetAnimationLength(string attacktype)
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
        handPosition = target;
    }

    protected virtual void addParent()
    {
        isAttacking = false;
        transform.parent = player;
        DisableHitbox();
        transform.localRotation = Quaternion.identity; 
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (transform.parent != null)
        {
            transform.localPosition = handPosition.localPosition + animOffset;
        }
    }
}