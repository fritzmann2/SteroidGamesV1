using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class SkillTreeManager : MonoBehaviour
{
    public int availableSkillPoints = 5; 
    public int skillPointsUsed = 0;
    private Dictionary<string, int> unlockedSkills = new Dictionary<string, int>();
    private SkillNode[] allNodes;

    [Header("Player Stats & Movement")]
    private PlayerStats playerStats;
    private PlayerMovement playerMovement; 
    public GameObject playerStatsUI;

    private void Start()
    {
        InitializeManager();
        RefreshAllNodes();
    }

    private void InitializeManager()
    {
        if (allNodes == null || allNodes.Length == 0)
        {
            allNodes = GetComponentsInChildren<SkillNode>(true); 
            foreach (SkillNode node in allNodes)
            {
                node.Initialize(this);
            }
        }
        if (playerStats == null || playerMovement == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                playerStats = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerStats>();
                playerMovement = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerMovement>();
            }
        }
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
        availableSkillPoints = playerStats.playerLevel;
        if (availableSkillPoints < skillPointsUsed)
        {
            Debug.Log("Nicht genug Skillpunkte!");
            return;
        }

        string id = node.skillData.skillID;
        int currentLevel = GetSkillLevel(id);

        if (currentLevel < node.skillData.maxLevel && ArePrerequisitesMet(node.skillData))
        {
            unlockedSkills[id] = currentLevel + 1;
            skillPointsUsed++;
            Debug.Log($"Skill {node.skillData.skillName} gelevelt auf {currentLevel + 1}");
            if (id.StartsWith("stat_"))
            {
                ApplyAllSkillsToStats();
            }
            RefreshAllNodes(); 
        }
    }

    private void ApplyAbility(string skillID)
    {
        if (playerMovement == null) return;

        switch (skillID)
        {
            case "skill_dash":
                playerMovement.UnlockDash();
                break;
            case "skill_doublejump":
                playerMovement.SetMaxJumps(2);
                break;
            case "skill_triplejump": 
                playerMovement.SetMaxJumps(3);
                break;
        }
    }
    private void ApplyAllSkillsToStats()
    {
        if (playerStats == null || playerMovement == null) return;

        playerStats.ResetSkillBonuses();
        playerMovement.ResetAbilities(); 

        foreach (SkillNode node in allNodes)
        {
            int level = GetSkillLevel(node.skillData.skillID);
            if (level > 0)
            {
                if (node.skillData.targetStat != StatType.None)
                {
                    float totalBonus = node.skillData.valuePerLevel * level;
                    playerStats.AddSkillBonus(node.skillData.targetStat, totalBonus);
                }

                if (node.skillData.skillID.StartsWith("skill_"))
                {
                    ApplyAbility(node.skillData.skillID);
                }
            }
        }
        showStats();
    }

    private void RefreshAllNodes()
    {
        foreach (SkillNode node in allNodes)
        {
            node.UpdateUI();
        }
        showStats();
    }

    private void showStats()
    {
        if (playerStats == null) 
        {
            return; 
        }
        if (playerStatsUI == null) 
        {
            return;
        }

        playerStatsUI.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true).text = playerStats.getTotalStats().ToString();
    }

    public SkillTreeSaveData GetSaveData()
    {
        SkillTreeSaveData data = new SkillTreeSaveData();
        data.availableSkillPoints = this.availableSkillPoints;
        data.skillPointsUsed = this.skillPointsUsed; 

        foreach (KeyValuePair<string, int> kvp in unlockedSkills)
        {
            data.unlockedSkillIDs.Add(kvp.Key);
            data.unlockedSkillLevels.Add(kvp.Value);
        }
        return data;
    }

    public void LoadSaveData(SkillTreeSaveData data, PlayerStats stats)
    {
        if (data == null) 
        {
            Debug.LogWarning("Gespeicherte Skill-Daten sind ungültig!");
            return;
        }
        if (stats != null)
        {
            playerStats = stats;
            if (playerMovement == null)
            {
                playerMovement = stats.GetComponent<PlayerMovement>();
            }
        }
        InitializeManager();
        this.availableSkillPoints = data.availableSkillPoints;
        this.skillPointsUsed = data.skillPointsUsed; 
        
        unlockedSkills.Clear();
        for (int i = 0; i < data.unlockedSkillIDs.Count; i++)
        {
            unlockedSkills[data.unlockedSkillIDs[i]] = data.unlockedSkillLevels[i];
        }

        ApplyAllSkillsToStats(); 
        RefreshAllNodes();
    }

    public void ResetSkillTree()
    {
        if (playerStats != null)
        {
            availableSkillPoints = playerStats.playerLevel; 
        }
        skillPointsUsed = 0; 
        unlockedSkills.Clear();
        ApplyAllSkillsToStats(); 
        RefreshAllNodes();
    }
}