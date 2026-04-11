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
    public GameObject bossPortalPrefab;
    public GameObject[] allMobPrefabs;
    public GameObject[] allChunkPrefabs;
    public GameObject[] allBossArenaPrefabs;
    
    [Header("Chunk Loading Settings")]
    private float loadDistance = 60f; 
    private float despawnDelay = 10f;   

    [Header("Teleport Settings")]
    [Tooltip("Wie viele Sekunden ein Spieler warten muss, bevor er wieder teleportiert werden kann.")]
    public float teleportCooldown = 3f; 
    private Dictionary<ulong, float> lastTeleportTimes = new Dictionary<ulong, float>();

    private Dictionary<string, GameObject> mobDictionary = new Dictionary<string, GameObject>();
    private Dictionary<Vector2Int, GameObject> chunkPrefabDictionary = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, float> chunkLastSeenTimes = new Dictionary<Vector2Int, float>();
    private Dictionary<Vector2Int, Vector3> arenaReturnPoints = new Dictionary<Vector2Int, Vector3>();

    public void Awake()
    {
        InitializeDictionaries(); 
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
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

        foreach (GameObject arena in allBossArenaPrefabs)
        {
            if (arena == null) continue;

            string numberString = string.Empty;
            foreach (char c in arena.name)
            {
                if (char.IsDigit(c)) numberString += c;
            }
            if (int.TryParse(numberString, out int arenaNumber))
            {
                int index = arenaNumber - 1; 
                int worldX = index * 200;
                int xCoord = Mathf.RoundToInt(worldX / chunkSize); 
                int worldY = 1000;
                int yCoord = Mathf.RoundToInt(worldY / chunkSize);
                chunkPrefabDictionary[new Vector2Int(xCoord, yCoord)] = arena;
                Debug.Log($"[WorldGen] {arena.name} erfolgreich auf Chunk-Koordinate {xCoord}_{yCoord} (Welt: {worldX}|{worldY}) registriert!");
            }
            else
            {
                Debug.LogWarning($"[WorldGen] Konnte keine Zahl im Namen von '{arena.name}' finden. Sie wird ignoriert.");
            }
        }
        
        Debug.Log($"[WorldGen] {chunkPrefabDictionary.Count} Chunks (inklusive Arenen) initialisiert.");
    }
 
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
            Vector2 trueCenterPos = new Vector2(
                chunkCoord.x * chunkSize, 
                chunkCoord.y * chunkSize 
            );
            
            bool isAnyPlayerNear = false;
            foreach (GameObject player in allPlayers)
            {
                if (Vector2.Distance(player.transform.position, trueCenterPos) <= loadDistance)
                {
                    isAnyPlayerNear = true;
                    break; 
                }
            }

            if (isAnyPlayerNear)
            {
                currentlyVisibleChunks.Add(chunkCoord);
            }
        }

        HashSet<Vector2Int> linkedChunksToLoad = new HashSet<Vector2Int>();
        foreach (Vector2Int visibleCoord in currentlyVisibleChunks)
        {
            if (activeChunks.TryGetValue(visibleCoord, out GameObject chunkObj))
            {
                ChunkData cd = chunkObj.GetComponentInChildren<ChunkData>();
                if (cd != null)
                {
                    foreach (Vector2Int linkedCoord in cd.linkedChunks)
                    {
                        linkedChunksToLoad.Add(linkedCoord);
                    }
                }
            }
        }
        currentlyVisibleChunks.UnionWith(linkedChunksToLoad);

        foreach (Vector2Int coord in currentlyVisibleChunks)
        {
            chunkLastSeenTimes[coord] = Time.time;
            if (!activeChunks.ContainsKey(coord))
            {
                SpawnChunk(coord);
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
            
            ChunkData chunkDataScript = networkChunk.GetComponent<ChunkData>();
            
            bool isArena = false;
            foreach (GameObject arenaPrefab in allBossArenaPrefabs)
            {
                if (arenaPrefab == chunkVisualPrefab)
                {
                    isArena = true;
                    break;
                }
            }

            if (chunkDataScript != null)
            {
                chunkDataScript.isBossArena = isArena; 
            }

            GameObject visualContainer = new GameObject("VisualContainer");
            visualContainer.transform.SetParent(networkChunk.transform);
            visualContainer.transform.localPosition = Vector3.zero; 
            visualContainer.transform.localScale = new Vector3(6.25f, 6.25f, 1f); 

            Instantiate(chunkVisualPrefab, visualContainer.transform);

            GameObject mobContainer = new GameObject("MobContainer");
            mobContainer.transform.SetParent(networkChunk.transform);
            mobContainer.transform.localPosition = Vector3.zero;

            networkChunk.GetComponent<NetworkObject>().Spawn();

            if (chunkDataScript != null)
            {
                chunkDataScript.gridCoordinate.Value = coord; 
            }

            activeChunks.Add(coord, networkChunk);
            
            SpawnMobsInChunk(networkChunk);
            
            SpawnPortalsInChunk(networkChunk, false); 
        }
    }

public void SpawnPortalsInChunk(GameObject chunk, bool forceSpawn)
    {
        ChunkData chunkData = chunk.GetComponentInChildren<ChunkData>();
        if (chunkData == null) return;
        if (chunkData.isBossArena && !forceSpawn) 
        {
    //        Debug.Log($"[WorldGen] Portal-Spawn in Arena {chunk.name} unterdrückt (Boss lebt noch).");
            return;
        }
        PortalSpawner[] spawners = chunk.GetComponentsInChildren<PortalSpawner>();
        foreach (PortalSpawner spawner in spawners)
        {
            spawner.Reset();
            if (bossPortalPrefab != null)
            {
                GameObject portalObj = Instantiate(bossPortalPrefab, spawner.transform.position, Quaternion.identity);
                NetworkObject portalNetObj = portalObj.GetComponent<NetworkObject>();
                portalNetObj.Spawn();
                portalNetObj.TrySetParent(chunk.transform);
                BossPortals bossPortalScript = portalObj.GetComponent<BossPortals>();
                if (bossPortalScript != null)
                {
                    if (chunkData.isBossArena && arenaReturnPoints.TryGetValue(chunkData.gridCoordinate.Value, out Vector3 returnPos))
                    {
                        bossPortalScript.destinationCoordinate = returnPos;
                        arenaReturnPoints.Remove(chunkData.gridCoordinate.Value);
                    }
                    else
                    {
                        bossPortalScript.destinationCoordinate = spawner.teleportDestination;
                    }
                }
                int targetX = Mathf.RoundToInt(bossPortalScript.destinationCoordinate.x / chunkSize);
                int targetY = Mathf.RoundToInt(bossPortalScript.destinationCoordinate.y / chunkSize);
                if (!chunkData.linkedChunks.Contains(new Vector2Int(targetX, targetY)))
                {
                    chunkData.linkedChunks.Add(new Vector2Int(targetX, targetY));
                }
                chunkData.RegisterPortal(portalNetObj);
            }
        }
    }

    private void SpawnMobsInChunk(GameObject chunk)
    {
        ChunkData chunkData = chunk.GetComponentInChildren<ChunkData>();
        
        if (chunkData == null) return; 

        MobSpawnPoint[] spawners = chunkData.GetSpawnPoints();
//        Debug.Log($"[WorldGen] Chunk {chunk.name} hat {spawners.Length} Spawnpunkte gefunden!");
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
                chunkData.DespawnAllPortals();
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

    public void SpawnVisualForClient(GameObject networkChunk, Vector2Int coord)
    {
        if (IsServer) return;
        if (chunkPrefabDictionary.TryGetValue(coord, out GameObject chunkVisualPrefab))
        {
            networkChunk.name = $"Chunk_{coord.x}_{coord.y}"; 
            GameObject visualContainer = new GameObject("VisualContainer");
            visualContainer.transform.SetParent(networkChunk.transform);
            visualContainer.transform.localPosition = Vector3.zero; 
            visualContainer.transform.localScale = new Vector3(6.25f, 6.25f, 1f); 
            Instantiate(chunkVisualPrefab, visualContainer.transform);
        }
    }
    
    public void TeleportToBoss(Vector2 destinationCoordinate, Transform player)
    {
        if (!IsServer) return;
        NetworkObject playerNetObj = player.GetComponent<NetworkObject>();
        if (playerNetObj != null)
        {
            ulong playerId = playerNetObj.NetworkObjectId;
            if (lastTeleportTimes.TryGetValue(playerId, out float lastTime))
            {
                if (Time.time < lastTime + teleportCooldown)
                {
                    Debug.Log($"[WorldGen] Teleport blockiert: Spieler ist noch im Cooldown.");
                    return; 
                }
            }
            lastTeleportTimes[playerId] = Time.time;
        }
        int targetX = Mathf.RoundToInt(destinationCoordinate.x / chunkSize);
        int targetY = Mathf.RoundToInt(destinationCoordinate.y / chunkSize);
        Vector2Int arenaCoord = new Vector2Int(targetX, targetY);
        if (!arenaReturnPoints.ContainsKey(arenaCoord))
        {
            arenaReturnPoints[arenaCoord] = player.position;
        }
        player.position = new Vector3(destinationCoordinate.x, destinationCoordinate.y, 0);
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        Debug.Log($"[WorldGen] Spieler zu Boss-Arena {destinationCoordinate} teleportiert! Rückkehrpunkt gespeichert.");
    }

    #if UNITY_EDITOR
    [ContextMenu("Lade Prefabs aus Ordnern")]
    public void LoadPrefabsFromFolders()
    {
        allMobPrefabs = LoadPrefabsAtPath(mobFolderPath);
        allChunkPrefabs = LoadPrefabsAtPath(chunkFolderPath, "Chunk");
        allBossArenaPrefabs = LoadPrefabsAtPath(chunkFolderPath, "BossArena");
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