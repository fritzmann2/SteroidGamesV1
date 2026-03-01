using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerStats : BaseMobClass
{
    [Header("Verbindungen")]
    private PlayerSaveHandler playerSaveHandler;

    [Header("Debugging")]
    [SerializeField] private List<EquipmentInstance> equipmentDatas = new List<EquipmentInstance>();
    [SerializeField] protected Playerstats totalStats;
    
    [SerializeField] protected Playerstats baseStats; 
    [SerializeField] protected PlayerStatsUI statsUI;
    private ItemInventory itemInventory;
    private bool isCrit;
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private int playerXP = 0;
    public NetworkVariable<float> mana = new NetworkVariable<float>(
        100f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    protected float maxMana = 0;
    private float regenInterval = 1f;
    [Header("Spieler Info")]
    public string playerName;

    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn(); 
        if (!IsOwner) return;
        StartCoroutine(HealthRegenerationRoutine());
        playerSaveHandler = GetComponent<PlayerSaveHandler>(); 
        itemInventory = GetComponentInParent<Inventory>().itemInventory;
        statsUI = FindAnyObjectByType<PlayerStatsUI>();
        playerSaveHandler.dataLoaded += Init;
        itemInventory.OnEquipmentChanged += Initbase;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsOwner) return;
        playerSaveHandler.dataLoaded -= Init;
        itemInventory.OnEquipmentChanged -= Initbase;
    }

    private System.Collections.IEnumerator HealthRegenerationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenInterval);
            
            bool needsUpdate = false;
            float currentHealth = health.Value;
            float currentMana = mana.Value;
            if (totalStats.healthRegen > 0 && currentHealth > 0 && currentHealth < maxHealth)
            {
                currentHealth += totalStats.healthRegen;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                needsUpdate = true;
            }
            if (totalStats.manaRegen > 0 && currentMana < maxMana)
            {
                currentMana += totalStats.manaRegen;
                if (currentMana > maxMana) currentMana = maxMana;
                needsUpdate = true;
            }
            if (needsUpdate)
            {
                ApplyRegenServerRpc(currentHealth, currentMana);
            }
        }
    }

    [ServerRpc]
    private void ApplyRegenServerRpc(float newHealth, float newMana)
    {
        health.Value = newHealth;
        mana.Value = newMana;
    }

    private void Init()
    {
        Initbase();

        calculateLevel(baseStats);
        statsUI.UpdateHealthUI((int)health.Value, maxHealth);
        statsUI.UpdateXPUI(playerXP, GetRequiredXP(), playerLevel);
    }

    private void Initbase()
    {
        equipmentDatas = new List<EquipmentInstance>();

        for (int i = 0; i < itemInventory.equipmentSlots.Count; i++)
        {
            EquipmentSlot slot = itemInventory.getEquipmentSlot(i);
            
            equipmentDatas.Add(slot.EquipInstance);
        }
        calculateBaseStats();
        RecalculateTotalStats();
    }


    public void LoadSaveData(PlayerStatsSaveData savedData)
    {
        if (savedData != null && savedData.baseStats != null)
        {
            this.baseStats = savedData.baseStats;
        }
    }


    public void UpdateStatsFromEquipment(int slotIndex)
    {
        Debug.Log("updating slot nummer: " + slotIndex);
        equipmentDatas[slotIndex] = itemInventory.equipmentSlots[slotIndex].EquipInstance;
        RecalculateTotalStats();
    }

    private void RecalculateTotalStats()
    {
        totalStats = new Playerstats
        {
            strength = baseStats.strength,
            critChance = baseStats.critChance,
            critDamage = baseStats.critDamage,
            attackSpeed = baseStats.attackSpeed,
            weapondamage = baseStats.weapondamage,
            defense = baseStats.defense,
            spellresistance = baseStats.spellresistance,
            movementSpeed = baseStats.movementSpeed,
            mana = baseStats.mana,
            manaRegen = baseStats.manaRegen,
            health = baseStats.health,
            healthRegen = baseStats.healthRegen
        };
        foreach (var data in equipmentDatas)
        {
            if (data == null) continue;

            EquipmentStats stats = data.GetEquipmentStats();
            if (stats == null) continue;
            if (stats is WeaponStats w)
            {
                totalStats.weapondamage += w.weapondamage;
                totalStats.strength += w.strength;
                totalStats.critChance += w.critChance;
                totalStats.critDamage += w.critDamage;
                totalStats.attackSpeed += w.attackSpeed;
            }
            else if (stats is ArmorStats a)
            {
                totalStats.defense += a.defense;
                totalStats.spellresistance += a.spellresistance;
            }
            else if (stats is AccessoryStats acc)
            {
                totalStats.health += acc.health;
                totalStats.mana += acc.mana;
                totalStats.manaRegen += acc.manaRegen;
                totalStats.healthRegen += acc.healthRegen;
                totalStats.movementSpeed += acc.movementSpeed;
                totalStats.attackSpeed += acc.attackSpeed;
                totalStats.critChance += acc.critChance;
                totalStats.critDamage += acc.critDamage;
                totalStats.strength += acc.strength;
                totalStats.defense += acc.defence;
                totalStats.spellresistance += acc.spellresistance;
            }
        }
        maxHealth = (int)totalStats.health;
        maxMana = totalStats.mana;
    }

    public override void TakeDamage(float _damage, bool isCrit)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Kein Server/Client gestartet! Schlag ignoriert.");
            return;
        }

        int damage = (int)(_damage / (1 + totalStats.defense / 100f));

        TakeDamageServerRpc((int)damage, isCrit);
    }

    public void DealotherDamage(BaseEntety mob, float attackmulti)
    {
        int damage = calculateDamage(attackmulti);
        if (damage <= 0) damage = 40; 
        mob.TakeDamage(damage, isCrit);
        mob.GetComponent<BaseEnemy>().SetLastAttacker(this.transform);
    }

    public int calculateDamage(float attackmulti)
    {
        
        float multiplier = 1f;
        if (getcrit() == 1)
        {
            multiplier += totalStats.critDamage / 100f;
            isCrit = true;
        }
        else
        {
            isCrit = false;
        }
        multiplier *= 1+ totalStats.strength / 100f;

        float damage = totalStats.weapondamage * multiplier * attackmulti;
        

        return Mathf.RoundToInt(damage);
    }

    public int getcrit()
    {
        float critRoll = Random.Range(0f, 100f);
        return (critRoll <= totalStats.critChance) ? 1 : 0;
    }

    public int getLevel()
    {
        return playerLevel;
    }

    private void calculateLevel(Playerstats stats)
    {
        playerXP = stats.totalexperience;
        playerLevel = 1;
        while (playerXP >= GetRequiredXP())
        {
            playerLevel++;
            playerXP -= GetRequiredXP();
        }
    }

    [ClientRpc]
    public void ReceiveXPClientRpc(int amount)
    {
        if (IsOwner)
        {
            gainXP(amount);
        }
    }
    public void gainXP(int amount)
    {
        playerXP += amount;
        while (playerXP >= GetRequiredXP())
        {
            playerXP -= GetRequiredXP();
            playerLevel++;
            Debug.Log($"Level Up! Du bist jetzt Level {playerLevel}");
        }
        baseStats.totalexperience += amount;
        statsUI.UpdateXPUI(playerXP, GetRequiredXP(), playerLevel);
    }

    public PlayerStatsSaveData GetSaveData()
    {
        PlayerStatsSaveData playerStatsSaveData = new PlayerStatsSaveData
        {
            baseStats = this.baseStats
        };
        return playerStatsSaveData;
    }

    private void calculateBaseStats()
    {
        baseStats.health = health.Value;
        baseStats.calculateBaseStats(playerLevel);
        maxMana = baseStats.mana;
        mana.Value = maxMana;
    }

    private int GetRequiredXP()
    {
        float requiredXP = (playerLevel * playerLevel * 0.3f) + (100 + (playerLevel * 10));
        return Mathf.RoundToInt(requiredXP);
    }

    public void ResetToDefault()
    {
        baseStats.totalexperience = 0;
        if (health != null) health.Value = maxHealth;
        calculateLevel(baseStats); 
        Initbase();
        Debug.Log("Spieler-Stats wurden erfolgreich auf Level 1 zurückgesetzt!");
    }

    public override void OnHealthChanged(float previousValue, float newValue)
    {
        base.OnHealthChanged(previousValue, newValue);
        if (IsOwner && statsUI != null)
        {
            statsUI.UpdateHealthUI((int)newValue, maxHealth);
        }
        if (newValue <= 0)
        {
            if (IsServer)
            {
                Vector3 spawnPos = new Vector3(0, 2f, 0);
                health.Value = maxHealth;
                TeleportClientRpc(spawnPos);
            }
        }
    }

    public void ResetHP()
    {
        health.Value = maxHealth;
        statsUI.UpdateHealthUI((int)health.Value, maxHealth);
    }


    [ClientRpc]
    private void TeleportClientRpc(Vector3 newPos)
    {
        if (TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
        }
        transform.position = newPos;

        if (IsOwner)
        {
            CameraFollow cam = FindAnyObjectByType<CameraFollow>();
            if (cam != null)
            {
                cam.ResetZoom();
            }
        }
    }
}



