[System.Serializable]
public class equipmentStatsSlot
{
    public int slotIndex;
}


public class playerstats
{
    public int totalexperience;
    public float movementSpeed;
    public float weapondamage;
    public float attackSpeed;
    public float critChance;
    public float critDamage;
    public float strength;
    public float defense;
    public float spellresistance;
}

public class PlayerStatsSaveData
{
    public playerstats baseStats;
}