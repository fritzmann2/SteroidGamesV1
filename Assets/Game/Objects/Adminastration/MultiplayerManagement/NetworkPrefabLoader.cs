#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkPrefabLoader : MonoBehaviour
{
    [Header("Einstellungen")]
    public string chunksFolderPath = "Assets/Game/Prefabs/Chunks"; 
    
    public NetworkPrefabsList targetList;

    
    [ContextMenu("Lade alle Chunks aus Ordner")]
    void LoadPrefabsFromFolder()
    {
#if UNITY_EDITOR
        if (targetList == null)
        {
            Debug.LogError("Bitte ziehe zuerst deine 'DefaultNetworkPrefabs' Liste in den Slot!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { chunksFolderPath });
        
        int addedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            if (prefab.GetComponent<NetworkObject>() == null)
            {
                Debug.LogWarning($"Übersprungen: '{prefab.name}' hat keine NetworkObject Komponente!");
                continue;
            }

            bool alreadyInList = false;
            foreach (var item in targetList.PrefabList)
            {
                if (item.Prefab == prefab)
                {
                    alreadyInList = true;
                    break;
                }
            }

            if (!alreadyInList)
            {
                targetList.Add(new NetworkPrefab { Prefab = prefab });
                addedCount++;
            }
        }

        EditorUtility.SetDirty(targetList);
        
        Debug.Log($"Fertig! Es wurden {addedCount} neue Prefabs zur Liste hinzugefügt.");
#endif
    }
}