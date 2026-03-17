using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WorldGenerator : NetworkBehaviour
{
    [Header("Einstellungen")]
    public float chunkSize = 50f;
    public float updateInterval = 0.5f;
    [Header("Editor Setup Pfade")]
    public string mobFolderPath = "Assets/Game/Objects/Mob/Prefabs";
    public string chunkFolderPath = "Assets/Game/TileLevels/RockTiles";

    [Header("Prefabs")]
    public GameObject baseNetworkChunkPrefab; 
    public GameObject[] allMobPrefabs;
    public GameObject[] allChunkPrefabs;
    
    [Header("Chunk Loading Settings")]
    private float loadDistance = 40f; 
    private float despawnDelay = 10f;   

    private Dictionary<string, GameObject> mobDictionary = new Dictionary<string, GameObject>();
    private Dictionary<Vector2Int, GameObject> chunkPrefabDictionary = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeDictionaries();
            StartCoroutine(ChunkUpdateLoop());
        }
    }

    private void InitializeDictionaries()
    {
        foreach (GameObject mob in allMobPrefabs)
        {
            if (mob != null) mobDictionary[mob.name] = mob;
        }
        Debug.Log($"[WorldGen] {mobDictionary.Count} Mobs initialisiert.");

        foreach (GameObject chunk in allChunkPrefabs)
        {
            if (chunk == null) continue;

            string[] parts = chunk.name.Split('_'); 
            
            if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                chunkPrefabDictionary[new Vector2Int(x, y)] = chunk;
            }
            else
            {
                Debug.LogWarning($"[WorldGen] Chunk-Name '{chunk.name}' hat nicht das Format 'Chunk_X_Y'. Er wird ignoriert.");
            }
        }
        Debug.Log($"[WorldGen] {chunkPrefabDictionary.Count} Chunks initialisiert.");
    }
 
    
    private Dictionary<Vector2Int, float> chunkLastSeenTimes = new Dictionary<Vector2Int, float>();

    private IEnumerator ChunkUpdateLoop()
    {
        while (true)
        {
            UpdateChunks();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdateChunks()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        if (allPlayers.Length == 0) return;

        HashSet<Vector2Int> currentlyVisibleChunks = new HashSet<Vector2Int>();
        foreach (Vector2Int chunkCoord in chunkPrefabDictionary.Keys)
        {
            Vector2 chunkCenterPos = new Vector2(chunkCoord.x * chunkSize, chunkCoord.y * chunkSize);
            
            bool isAnyPlayerNear = false;

            foreach (GameObject player in allPlayers)
            {
                float distance = Vector2.Distance(player.transform.position, chunkCenterPos);
                if (distance <= loadDistance)
                {
                    isAnyPlayerNear = true;
                    break; 
                }
            }

            if (isAnyPlayerNear)
            {
                currentlyVisibleChunks.Add(chunkCoord);
                chunkLastSeenTimes[chunkCoord] = Time.time;
                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    SpawnChunk(chunkCoord);
                }
            }
        }

        List<Vector2Int> chunksToDespawn = new List<Vector2Int>();
        foreach (Vector2Int activeCoord in activeChunks.Keys)
        {
            if (!currentlyVisibleChunks.Contains(activeCoord))
            {
                if (chunkLastSeenTimes.TryGetValue(activeCoord, out float lastSeenTime))
                {
                    if (Time.time >= lastSeenTime + despawnDelay)
                    {
                        chunksToDespawn.Add(activeCoord);
                    }
                }
                else
                {
                    chunksToDespawn.Add(activeCoord);
                }
            }
        }

        foreach (Vector2Int coord in chunksToDespawn)
        {
            DespawnChunk(coord);
            chunkLastSeenTimes.Remove(coord);
        }
    }

    private void SpawnChunk(Vector2Int coord)
    {
        if (chunkPrefabDictionary.TryGetValue(coord, out GameObject chunkVisualPrefab))
        {
            float offsetX = chunkSize / 2f;
            float offsetY = chunkSize / 2f;
            Vector3 spawnPos = new Vector3(
                (coord.x * chunkSize) - offsetX, 
                (coord.y * chunkSize) + offsetY, 
                0
            );
            GameObject networkChunk = Instantiate(baseNetworkChunkPrefab, spawnPos, Quaternion.identity);
            networkChunk.name = $"Chunk_{coord.x}_{coord.y}";
            GameObject visualContainer = new GameObject("VisualContainer");
            visualContainer.transform.SetParent(networkChunk.transform);
            visualContainer.transform.localPosition = Vector3.zero; 
            visualContainer.transform.localScale = new Vector3(6.25f, 6.25f, 1f); 

            Instantiate(chunkVisualPrefab, visualContainer.transform);

            GameObject mobContainer = new GameObject("MobContainer");
            mobContainer.transform.SetParent(networkChunk.transform);
            mobContainer.transform.localPosition = Vector3.zero;

            networkChunk.GetComponent<NetworkObject>().Spawn();

            activeChunks.Add(coord, networkChunk);
            
            SpawnMobsInChunk(networkChunk);
        }
    }

    private void SpawnMobsInChunk(GameObject chunk)
    {
        ChunkData chunkData = chunk.GetComponentInChildren<ChunkData>();
        
        if (chunkData == null) return; 

        MobSpawnPoint[] spawners = chunkData.GetSpawnPoints();
        Transform mobContainer = chunk.transform.Find("MobContainer");

        foreach (MobSpawnPoint spawner in spawners)
        {
            if (mobDictionary.TryGetValue(spawner.getMobName(), out GameObject mobPrefab))
            {
                GameObject newMob = Instantiate(mobPrefab, spawner.transform.position, Quaternion.identity);
                NetworkObject mobNetObj = newMob.GetComponent<NetworkObject>();
                
                mobNetObj.Spawn();

                if (mobContainer != null)
                {
                    mobNetObj.TrySetParent(mobContainer);
                }
                else
                {
                    mobNetObj.TrySetParent(chunk.transform);
                }

                chunkData.RegisterMob(mobNetObj);
                BaseEnemy enemyScript = newMob.GetComponent<BaseEnemy>();
                if (enemyScript != null)
                {
                    enemyScript.parentChunk = chunkData;
                }
            }
            else
            {
                Debug.LogError($"[WorldGen] Mob '{spawner.getMobName()}' nicht im Inspector zugewiesen!");
            }
        }
    }

    private void DespawnChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject chunkToDestroy))
        {
            ChunkData chunkData = chunkToDestroy.GetComponentInChildren<ChunkData>();
            if (chunkData != null)
            {
                chunkData.DespawnAllMobs();
            }

            if (chunkToDestroy.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Despawn(true);
            }
            else
            {
                Destroy(chunkToDestroy);
            }
            
            activeChunks.Remove(coord);
        }
    }

    #if UNITY_EDITOR
    [ContextMenu("Lade Prefabs aus Ordnern")]
    public void LoadPrefabsFromFolders()
    {
        allMobPrefabs = LoadPrefabsAtPath(mobFolderPath);
        allChunkPrefabs = LoadPrefabsAtPath(chunkFolderPath, "Chunk");
        UnityEditor.EditorUtility.SetDirty(this); 
        Debug.Log($"[WorldGen Editor] Erfolgreich {allMobPrefabs.Length} Mobs und {allChunkPrefabs.Length} Chunks geladen!");
    }

    private GameObject[] LoadPrefabsAtPath(string path, string namePrefix = "")
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameObject", new[] { path });
        List<GameObject> loadedPrefabs = new List<GameObject>();
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                if (string.IsNullOrEmpty(namePrefix) || prefab.name.StartsWith(namePrefix))
                {
                    loadedPrefabs.Add(prefab);
                }
            }
        }
        return loadedPrefabs.ToArray();
    }
    #endif
}