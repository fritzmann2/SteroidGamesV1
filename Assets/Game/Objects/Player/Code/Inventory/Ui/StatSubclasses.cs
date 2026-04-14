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
    public float healthRegen;
    public float health;

    public void calculateBaseStats(int level)
    {
        attackSpeed = level/300f + 1;
        critChance = level/100f;
        critDamage = level * 2f;
        strength = level;
        defense = level;
        spellresistance = level/100f; 
        manaRegen = level/100f + 2f;   
        healthRegen = level/200f + 1f;
    }

    public override string ToString()
    {
        return $"Leben: {health:F0} (+{healthRegen:F1}/s)\n" +
               $"Mana: {mana:F0} (+{manaRegen:F1}/s)\n" +
               $"Bewegungsspeed: {movementSpeed:F1}\n" +
               $"Waffenschaden: {weapondamage:F0}\n" +
               $"Angriffsspeed: {attackSpeed:F2}\n" +
               $"Crit Chance: {critChance:F1}%\n" +
               $"Crit Schaden: {critDamage:F0}%\n" +
               $"Stärke: {strength:F0}\n" +
               $"Verteidigung: {defense:F0}\n" +
               $"Magieresistenz: {spellresistance:F1}";
    }
}

[System.Serializable]
public class PlayerStatsSaveData
{
    public Playerstats baseStats;
}