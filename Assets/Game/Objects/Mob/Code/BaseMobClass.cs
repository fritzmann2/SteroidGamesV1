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
    private DamageTextManager damageTextManager;


    virtual public void Awake()
    {
        health.OnValueChanged += OnHealthChanged;
        damageTextManager = FindAnyObjectByType<DamageTextManager>();
        if (IsServer)
        {
            health.Value = maxHealth;
        }
    }

    
    // Schaden nehmen
    public virtual void TakeDamage(float damage, bool isCrit)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Kein Server/Client gestartet! Schlag ignoriert.");
            return;
        }

        if (!IsSpawned) 
        {
            Debug.LogWarning("Map-Objekt ist noch nicht gespawnt! (Warte auf Sync)");
        }
        

        TakeDamageServerRpc((int)damage, isCrit);
    }

    [Rpc(SendTo.Server)]
    public virtual void TakeDamageServerRpc(int damage, bool isCrit)
    {
        damageTextManager.ShowDamageText(damage, transform.position, isCrit);
        
        health.Value -= damage;
    }
    //Todesabfrage
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
    public float defense { get; set; }
    public float spellresistance { get; set; }    
}

