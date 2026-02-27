using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Services.Matchmaker.Models;


public class Bow : Weapon
{
    [SerializeField] protected Vector3 bowTransform;
    public GameObject Arrow;
    private bool canshoot = true;
    private bool isAiming = false;
    private float currentAimAngle = 0f;
    [SerializeField] private float autoAimRadius = 20f;

    override public void Attack1()
    {
        if (canshoot)
        {
            canshoot = false;

            if (playerStats != null && playerStats.IsOwner)
            {
                Vector3 direction = GetAimDirection();
                
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (transform.parent != null && transform.parent.localScale.x == -1)
                {
                    angle += 180;
                    if (angle > 360)
                    {
                        angle -= 360;
                    }
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
        if (Gamepad.current != null)
        {
            Transform nearestEnemy = GetNearestEnemy();
            
            if (nearestEnemy != null)
            {
                Vector3 aimDir = nearestEnemy.position - transform.position + new Vector3(0f, 0.5f, 0f);
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

    public void shootArrow()
    {
        if (playerStats == null || !playerStats.IsOwner) return;

        // 2. Auch hier nutzen wir einfach unsere neue Methode!
        Vector3 direction = GetAimDirection();

        if (IsServer)
        {
            ShootServer(direction);
        }
        else
        {
            ShootServerRpc(direction);
        }
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShootServerRpc(Vector3 direction)
    {
        ShootServer(direction);
    }

    private void ShootServer(Vector3 direction)
    {
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(0.1f, 0);
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