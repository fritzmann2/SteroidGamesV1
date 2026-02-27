using Unity.Netcode;
using UnityEngine;

abstract public class BaseEntety : NetworkBehaviour
{
    public NetworkVariable<float> health = new NetworkVariable<float>(
        100f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public int maxHealth;

    virtual public void Awake()
    {
        health.OnValueChanged += OnHealthChanged;
    }

    override public void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
        }
    }

    
    public virtual void TakeDamage(float damage, bool isCrit)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;
        if (!IsSpawned) return;

        if (IsServer)
        {
            ApplyDamageServer(damage, isCrit);
        }
        else
        {
            TakeDamageServerRpc(damage, isCrit);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public virtual void TakeDamageServerRpc(float damage, bool isCrit)
    {
        ApplyDamageServer(damage, isCrit);
    }

    protected virtual void ApplyDamageServer(float finalDamage, bool isCrit)
    {
        DamageTextManager.Instance.ShowDamageText((int)finalDamage, transform.position, isCrit);
        health.Value -= finalDamage;
    }

    virtual public void OnHealthChanged(float previousValue, float newValue)
    {
        if(newValue <= 0)
        {
            if (this.tag == "mob")
            {
            }
        }
    }
}

abstract public class BaseMobClass : BaseEntety
{
    public float movementSpeed { get; set; }
    public float attackSpeed { get; set; }
    public float critChance { get; set; }
    public float critDamage { get; set; }
    public float strength { get; set; }
    public float defence { get; set; }
    public float spellresistance { get; set; }    
}

