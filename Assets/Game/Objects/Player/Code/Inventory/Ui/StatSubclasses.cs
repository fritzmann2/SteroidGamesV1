using System.Collections.Generic;
using System.Reflection; 

[System.Serializable]
public class Playerstats
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
    public float mana;
    public float manaRegen;
    public float health;

    public void calculateBaseStats(int level)
    {
        attackSpeed = level/100f;
        critChance = level/100f;
        critDamage = level * 2f;
        strength = level;
        defense = level;
        spellresistance = level/100f; 
        mana = level;
        manaRegen = level/100f;    
    }
}

[System.Serializable]
public class PlayerStatsSaveData
{
    public Playerstats baseStats;
}