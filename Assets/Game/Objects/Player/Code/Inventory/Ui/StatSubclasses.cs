using System.Collections.Generic;
using System.Reflection; 
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
    public float mana;
    public float manaRegen;
    public float health;
}

public class PlayerStatsSaveData
{
    public playerstats baseStats;
}

public static class StatExtensions
{
    public static Dictionary<string, string> GetActiveStats(this EquipmentStats stats)
    {
        Dictionary<string, string> activeStats = new Dictionary<string, string>();

        if (stats == null) return activeStats;

        System.Type type = stats.GetType();

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            string statName = field.Name;
            object value = field.GetValue(stats);

            if (value is float fValue)
            {
                if (fValue != 0) 
                {
                    activeStats.Add(FormatName(statName), fValue.ToString("0.##")); 
                }
            }
            else if (value is int iValue)
            {
                if (statName == "count") continue; 

                if (iValue != 0)
                {
                    activeStats.Add(FormatName(statName), iValue.ToString());
                }
            }
        }

        return activeStats;
    }

    private static string FormatName(string name)
    {
        return char.ToUpper(name[0]) + name.Substring(1);
    }
}