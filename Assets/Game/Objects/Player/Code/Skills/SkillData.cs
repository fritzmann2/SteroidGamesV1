using UnityEngine;

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
}