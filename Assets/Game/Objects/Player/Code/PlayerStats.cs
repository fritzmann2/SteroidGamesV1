using System.Collections.Generic;
using UnityEngine;


public class PlayerStats : BaseMobClass
{
    [Header("Verbindungen")]
    private PlayerSaveHandler playerSaveHandler;

    [Header("Debugging")]
    [SerializeField] private List<EquipmentInstance> equipmentDatas = new List<EquipmentInstance>();
    [SerializeField] protected Playerstats totalStats;
    
    [SerializeField] protected Playerstats baseStats; 
    private ItemInventory itemInventory;
    private bool isCrit;
    private int playerLevel = 1;
    private int playerXP = 0;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerSaveHandler = GetComponent<PlayerSaveHandler>(); 
        itemInventory = GetComponentInParent<Inventory>().itemInventory;
        playerSaveHandler.dataLoaded += Init;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerSaveHandler.dataLoaded -= Init;
    }

    private void Init()
    {
        for (int i = 0; i < itemInventory.equipmentSlots.Count; i++)
        {
            EquipmentSlot slot = itemInventory.getEquipmentSlot(i);
            
            equipmentDatas.Add(slot.EquipInstance);
            
            slot.OnEquipmentChanged -= UpdateStatsFromEquipment;
            slot.OnEquipmentChanged += UpdateStatsFromEquipment;
        }

        // 3. Werte berechnen
        calculateLevel(baseStats);
        calculateBaseStats();
        RecalculateTotalStats();
    }




    public void UpdateStatsFromEquipment(int slotIndex)
    {
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
                totalStats.movementSpeed += acc.movementSpeed;
                totalStats.attackSpeed += acc.attackSpeed;
                totalStats.critChance += acc.critChance;
                totalStats.critDamage += acc.critDamage;
                totalStats.strength += acc.strength;
                totalStats.defense += acc.defence;
                totalStats.spellresistance += acc.spellresistance;
                
            }
        }
        
        Debug.Log($"Neue Stats berechnet. Total Strength: {totalStats.strength}");
    }


    public void DealotherDamage(BaseEntety mob, float attackmulti)
    {
        int damage = calculateDamage(attackmulti);
        if (damage <= 0) damage = 5; 
        mob.TakeDamage(damage, isCrit);
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
        while (playerXP >= playerLevel * playerLevel *0.3f + 100 * playerLevel)
        {
            playerLevel++;
            playerXP -= (int)(playerLevel * playerLevel *0.3f + 100 * playerLevel);
        }
    }
    public void gainXP(int amount)
    {
        playerXP += amount;
        if (playerXP >= playerLevel * playerLevel *0.3f + 100 * playerLevel)
        {
            playerLevel++;
            playerXP -= (int)(playerLevel * playerLevel *0.3f + 100 * playerLevel);
        }
        baseStats.totalexperience += amount;
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
        Debug.Log("stats calculated");
    }
}



