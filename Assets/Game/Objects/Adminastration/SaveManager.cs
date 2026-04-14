using UnityEngine;
using Unity.Netcode;
using System.IO;
using System.Collections.Generic;

public class SaveManager : NetworkBehaviour
{
    public static SaveManager Instance { get; private set; }
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameSaveData LoadPlayerData(string playerName)
    {
        string safeName = string.Join("_", playerName.Split(Path.GetInvalidFileNameChars()));
        string fileName = $"save_{safeName}.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            try { return JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path)); }
            catch { return null; }
        }
        return null;
    }

    public void SaveGameData(GameSaveData data)
    {
        string safeName = string.Join("_", data.playerName.Split(Path.GetInvalidFileNameChars()));
        string fileName = $"save_{safeName}.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        try 
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            Debug.Log($"<color=green>Gespeichert:</color> {data.playerName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fehler beim Speichern: {e.Message}");
        }
    }
}

[System.Serializable]
public class GameSaveData
{
    public string playerName;
    public InventorySaveData inventoryData;
    public PlayerStatsSaveData statsData; 
    public SkillTreeSaveData skillTreeData;
}

[System.Serializable]
public class SkillTreeSaveData
{
    public int availableSkillPoints;
    public int skillPointsUsed; 
    public List<string> unlockedSkillIDs = new List<string>();
    public List<int> unlockedSkillLevels = new List<int>();
}