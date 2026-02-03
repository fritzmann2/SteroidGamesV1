using UnityEngine;
using Unity.Netcode;


abstract public class Weapon : InventoryItem
{

    [SerializeField] public WeaponStats weaponstats;
    public PlayerStats playerStats;
    public float attackmulti = 1f;
    public Animator anim;
    public Collider2D bx;
    private Transform handPosition;
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
            PlayAnimationClientsAndHostRpc(_attacktype);
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void PlayAnimationClientsAndHostRpc(string attacktype)
    {
        if (anim != null)
        {
            anim.SetTrigger(attacktype.ToString());
            GetAnimationLength(attacktype);
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
        if (other.gameObject == playerStats.gameObject) 
        {
            return; 
        }
        BaseEntety mob = other.GetComponent<BaseEntety>();
        Debug.Log(other.tag);
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

    protected virtual void LateUpdate()
    {
        if (transform.parent == null) return;
        Vector2 inputVector = controls.Gameplay.Move.ReadValue<Vector2>();
        moveInput = inputVector.x;
        if (moveInput != 0f)
        {
            LastmoveInput = moveInput;
        }
        if (handPosition != null)
        {
            Vector3 handPos = handPosition.position;
        
            Vector3 finalPos = handPos + (handPosition.rotation * animOffset * LastmoveInput);

            transform.position = finalPos;
        }
    }
}