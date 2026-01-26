using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerStats : BaseMobClass
{
    [Header("Verbindungen")]
    private PlayerSaveHandler playerSaveHandler;

    [Header("Debugging")]
    [SerializeField] private List<equipmentStatsSlot> equipmentStats = new List<equipmentStatsSlot>();
    [SerializeField] private playerstats totalStats = new playerstats();
    
    // Basis-Stats (Stats ohne Ausrüstung)
    [SerializeField] private playerstats baseStats = new playerstats(); 
    private bool isCrit;




    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
       
        playerSaveHandler = GetComponent<PlayerSaveHandler>();        
        playerSaveHandler.dataLoaded += Init;
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerSaveHandler.dataLoaded -= Init;
    }

    private void Init()
    {
        {
            Debug.LogWarning("PlayerStats: Init fehlgeschlagen - Kein InventoryHolder oder EquipedSlots gefunden.");
            return;
        }


    }

    private void HandleEquipmentChanged(InventorySlot slot)
    {
    }

    public void UpdateStatsFromEquipment(int slotIndex)
    {
        // 1. Hole das echte Item aus dem InventoryHolder
        
        // 2. Finde oder erstelle den lokalen Cache-Eintrag
        equipmentStatsSlot localSlot = equipmentStats.FirstOrDefault(x => x.slotIndex == slotIndex);
        
        if (localSlot == null)
        {
            localSlot = new equipmentStatsSlot { slotIndex = slotIndex };
            equipmentStats.Add(localSlot);
        }


        // 4. Alles neu berechnen
        RecalculateTotalStats();
    }

    private void RecalculateTotalStats()
    {
        totalStats = new playerstats
        {
            strength = baseStats.strength,
            critChance = baseStats.critChance,
            critDamage = baseStats.critDamage,
            attackSpeed = baseStats.attackSpeed,
            weapondamage = baseStats.weapondamage,
        };

        
    }

    // --- Kampf-Methoden ---

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
            multiplier += (totalStats.critDamage / 100f);
            isCrit = true;
        }
        else
        {
            isCrit = false;
        }
        multiplier *= (1+ totalStats.strength / 100f);

        float damage = totalStats.weapondamage * multiplier * attackmulti;
        

        return Mathf.RoundToInt(damage);
    }

    public int getcrit()
    {
        float critRoll = Random.Range(0f, 100f);
        return (critRoll <= totalStats.critChance) ? 1 : 0;
    }

    public PlayerStatsSaveData GetSaveData()
    {
        PlayerStatsSaveData playerStatsSaveData = new PlayerStatsSaveData
        {
            baseStats = this.baseStats
        };
        return playerStatsSaveData;
    }
}



