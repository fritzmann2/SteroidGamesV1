using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public int availableSkillPoints = 5; 
    private Dictionary<string, int> unlockedSkills = new Dictionary<string, int>();
    private SkillNode[] allNodes;

    private void Start()
    {
        allNodes = GetComponentsInChildren<SkillNode>();

        foreach (SkillNode node in allNodes)
        {
            node.Initialize(this);
        }
        RefreshAllNodes();
    }

    public int GetSkillLevel(string skillID)
    {
        if (unlockedSkills.TryGetValue(skillID, out int level))
        {
            return level;
        }
        return 0;
    }

    public bool ArePrerequisitesMet(SkillData data)
    {
        if (data.prerequisites == null || data.prerequisites.Length == 0)
            return true;
        foreach (SkillData req in data.prerequisites)
        {
            if (GetSkillLevel(req.skillID) == 0)
            {
                return false; 
            }
        }
        return true;
    }

    public void TryUnlockSkill(SkillNode node)
    {
        if (availableSkillPoints <= 0)
        {
            Debug.Log("Nicht genug Skillpunkte!");
            return;
        }

        string id = node.skillData.skillID;
        int currentLevel = GetSkillLevel(id);

        if (currentLevel < node.skillData.maxLevel && ArePrerequisitesMet(node.skillData))
        {
            unlockedSkills[id] = currentLevel + 1;
            availableSkillPoints--;
            Debug.Log($"Skill {node.skillData.skillName} gelevelt auf {currentLevel + 1}");
            RefreshAllNodes(); 
        }
    }

    private void RefreshAllNodes()
    {
        foreach (SkillNode node in allNodes)
        {
            node.UpdateUI();
        }
    }

    public SkillTreeSaveData GetSaveData()
    {
        SkillTreeSaveData data = new SkillTreeSaveData();
        data.availableSkillPoints = this.availableSkillPoints;

        foreach (KeyValuePair<string, int> kvp in unlockedSkills)
        {
            data.unlockedSkillIDs.Add(kvp.Key);
            data.unlockedSkillLevels.Add(kvp.Value);
        }
        return data;
    }

    public void LoadSaveData(SkillTreeSaveData data)
    {
        if (data == null) return;

        this.availableSkillPoints = data.availableSkillPoints;
        unlockedSkills.Clear();
        for (int i = 0; i < data.unlockedSkillIDs.Count; i++)
        {
            unlockedSkills[data.unlockedSkillIDs[i]] = data.unlockedSkillLevels[i];
        }

        RefreshAllNodes();
    }
}