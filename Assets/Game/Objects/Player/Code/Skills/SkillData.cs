using UnityEngine;

public enum StatType
{
    None, Health, Mana, Strength, Defense, MovementSpeed, AttackSpeed, CritChance, CritDamage
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill Tree/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillID;
    public string skillName;
    [TextArea] public string description;
    public Sprite skillIcon;
    public int maxLevel = 1;
    
    [Header("Abhängigkeiten")]
    public SkillData[] prerequisites; 

    [Header("Stat Boost pro Level")]
    public StatType targetStat = StatType.None; 
    public float valuePerLevel = 0f; 
}