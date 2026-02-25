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

    override public void Attack1()
    {
        if (canshoot)
        {
            canshoot = false;

            if (IsOwner)
            {
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                mouseWorldPos.z = 0f;
                Vector3 direction = mouseWorldPos - transform.position;
                
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (transform.parent.localScale.x == -1)
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
        if (!IsOwner) return;
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

    public void shootArrow()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;
        Vector3 direction = mouseWorldPos - transform.position;

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