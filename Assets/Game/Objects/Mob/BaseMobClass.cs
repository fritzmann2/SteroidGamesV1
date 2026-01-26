using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

abstract public class BaseEntety : NetworkBehaviour
{
    public NetworkVariable<float> health = new NetworkVariable<float>(
        100f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public int maxHealth;
    private BoxCollider2D bx;
    private DamageTextManager damageTextManager;

    virtual public void Awake()
    {
        bx = GetComponent<BoxCollider2D>();
        health.OnValueChanged += OnHealthChanged;
        damageTextManager = FindAnyObjectByType<DamageTextManager>();
        // Startwerte setzen (nur Server)
        if (IsServer)
        {
            health.Value = maxHealth;
        }
    }
    // Schaden nehmen
    public virtual void TakeDamage(float damage, bool isCrit)
    {
        // 1. Check: Läuft das Netzwerk überhaupt schon?
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
//        Debug.Log("Mob took " + damage + " damage.");
        damageTextManager.ShowDamageText(damage, transform.position, isCrit);
        
        health.Value -= damage;
//        Debug.Log(health + " HP remains");
    }
    //Todesabfrage
    virtual public void OnHealthChanged(float previousValue, float newValue)
    {
        if(newValue <= 0)
        {
//            Debug.Log("Mob is dead.");
            if (this.tag == "mob")
            {
//                Debug.LogError("Mob tries dieing");
                Destroy(gameObject);
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

