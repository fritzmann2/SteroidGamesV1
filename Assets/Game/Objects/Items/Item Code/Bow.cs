using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;


public class Bow : Weapon
{
    [SerializeField] protected Vector3 bowTransform;
    public GameObject Arrow;
    private bool canshoot = true;
    private bool isAiming = false;
    private float currentAimAngle = 0f;
    [SerializeField] private float autoAimRadius = 20f;
    
    [Header("Aiming Settings")]
    [SerializeField] private float dropCompensationFactor = 0.005f;

    override public void Attack1()
    {
        if (canshoot)
        {
            canshoot = false;

            if (playerStats != null && playerStats.IsOwner)
            {
                isAiming = true;
                
                Vector3 direction = GetAimDirection();
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (transform.parent != null && transform.parent.localScale.x == -1)
                {
                    angle += 180;
                    if (angle > 360) angle -= 360;
                }
                SyncBowRotationServerRpc(angle);
            }

            performattack(AttackTypeBow.normal_shot.ToString());    
        }
    }

    
    override public void Attack2()
    {
        performattack(AttackTypeBow.bow_uppercut.ToString());
    }
    override public void Attack3()
    {
        performattack(AttackTypeBow.Charge.ToString());
    }

    protected override void Awake()
    {
        type = EquipmentType.Bow;
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }

    void FixedUpdate()
    {
        if (playerStats == null) return;
        if (!playerStats.IsOwner) return;
        if (transform.parent != null)
        {
            transform.localPosition = handPosition.localPosition + animOffset + bowTransform;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SyncBowRotationServerRpc(float angle)
    {
        SyncBowRotationClientRpc(angle);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SyncBowRotationClientRpc(float angle)
    {
        currentAimAngle = angle;
        isAiming = true;
    }

    private Vector3 GetAimDirection()
    {
        Vector3 aimDir = Vector3.zero;
        if (Gamepad.current != null)
        {
            Transform nearestEnemy = GetNearestEnemy();
            if (nearestEnemy != null)
            {
                aimDir = nearestEnemy.position - transform.position;
                aimDir.z = 0f;
            }
            else
            {
                float facingDir = transform.parent != null ? transform.parent.localScale.x : 1f;
                aimDir = new Vector3(facingDir, 0f, 0f);
            }
        }
        else
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;
            aimDir = mouseWorldPos - transform.position;
        }
        float distanceX = Mathf.Abs(aimDir.x);
        aimDir.y += distanceX * distanceX * dropCompensationFactor;

        return aimDir;
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

    public void shootArrow()
    {
        if (playerStats == null || !playerStats.IsOwner) return;

        Vector3 direction = GetAimDirection();
        Vector2 exactClientSpawnPos = (Vector2)transform.position + new Vector2(0.1f, 0);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (transform.parent != null && transform.parent.localScale.x == -1)
        {
            angle += 180;
            if (angle > 360) angle -= 360;
        }
        SyncBowRotationServerRpc(angle); 

        if (IsServer)
        {
            ShootServer(exactClientSpawnPos, direction);
        }
        else
        {
            ShootServerRpc(exactClientSpawnPos, direction);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShootServerRpc(Vector2 spawnPos, Vector3 direction)
    {
        ShootServer(spawnPos, direction);
    }

    private void ShootServer(Vector2 spawnPos, Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion arrowRotation = Quaternion.Euler(0, 0, angle);
        GameObject projectileInstance = Instantiate(Arrow, spawnPos, arrowRotation);
        NetworkObject netObj = projectileInstance.GetComponent<NetworkObject>();
        netObj.Spawn(); 
        InitArrowClientRpc(netObj.NetworkObjectId, direction);
    }

    [ClientRpc]
    private void InitArrowClientRpc(ulong arrowNetworkId, Vector3 direction)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(arrowNetworkId, out NetworkObject arrowObj))
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion arrowRotation = Quaternion.Euler(0, 0, angle);

            arrowObj.GetComponent<BaseArrow>().init(direction, arrowRotation, player);
        }
    }

    public void setCanshoot()
    {
        canshoot = true;
        isAiming = false;
        
        transform.localRotation = Quaternion.Euler(0, 0, 0); 
    }

    void LateUpdate()
    {
        if (isAiming)
        {
            if (playerStats != null && playerStats.IsOwner)
            {
                Vector3 direction = GetAimDirection();
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (transform.parent != null && transform.parent.localScale.x == -1)
                {
                    angle += 180;
                    if (angle > 360) angle -= 360;
                }
                currentAimAngle = angle;
            }
            transform.rotation = Quaternion.Euler(0, 0, currentAimAngle);
        }
    }
}


public enum AttackTypeBow
{
    normal_shot,
    bow_uppercut,
    Charge
}