using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WorldGenerator : NetworkBehaviour
{
    [Header("Alle Prefabs hier reinziehen")]
    public GameObject[] allMapChunks; // <-- WICHTIG: Die müssen im Inspector hier drin sein!
    public GameObject dummy;
    public GameObject PickUpItem;


    [Header("Settings")]
    public int chunkSize = 50;
    public GameObject defaultChunkPrefab; 

    private Dictionary<Vector2Int, GameObject> mapLookup = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    private Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, -1), 
        new Vector2Int(-1, 0), new Vector2Int(1, 0)
    };

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            this.enabled = false;
            return;
        }

        // --- NAMEN PARSEN ---
        foreach (GameObject prefab in allMapChunks)
        {
            if (prefab == null) continue;

            string prefabName = prefab.name;
            string[] parts = prefabName.Split('_');

            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[parts.Length - 2], out int x) && 
                    int.TryParse(parts[parts.Length - 1], out int y))
                {
                    Vector2Int coord = new Vector2Int(x, y);

                    if (!mapLookup.ContainsKey(coord))
                    {
                        mapLookup.Add(coord, prefab);
                    }
                    else
                    {
                        Debug.LogWarning($"Achtung: Koordinate {coord} ist doppelt belegt durch {prefabName}!");
                    }
                }
            }
        }
        
//        Debug.Log($">>> WorldGenerator: Map initialisiert mit {mapLookup.Count} Chunks.");
    }

    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;

        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

        // 1. Liste erstellen: Was MUSS da sein?
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            Transform playerTransform = client.PlayerObject.transform;
            
            int pX = Mathf.FloorToInt(playerTransform.position.x / chunkSize);

            int pY = Mathf.RoundToInt(playerTransform.position.y / chunkSize);

            Vector2Int playerGridPos = new Vector2Int(pX, pY);

            foreach (Vector2Int offset in offsets)
            {
                chunksToKeep.Add(playerGridPos + offset);
            }
        }

        // 2. Neue Chunks spawnen (Was fehlt, wird gebaut)
        foreach (Vector2Int coord in chunksToKeep)
        {
            if (!activeChunks.ContainsKey(coord))
            {
                SpawnChunkByCoord(coord);
            }
        }

        // 3. Alte Chunks löschen (Was nicht mehr gebraucht wird, kommt weg)
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        
        foreach (var kvp in activeChunks)
        {
            // Wenn der aktive Chunk NICHT in der "Behalten"-Liste steht -> Markieren zum Löschen
            if (!chunksToKeep.Contains(kvp.Key))
            {
                chunksToRemove.Add(kvp.Key);
            }
        }

        // Jetzt wirklich löschen
        foreach (Vector2Int coord in chunksToRemove)
        {
            RemoveChunk(coord);
        }
    }

    void SpawnChunkByCoord(Vector2Int coord)
    {
        GameObject prefabToSpawn = defaultChunkPrefab;

        if (mapLookup.TryGetValue(coord, out GameObject specificPrefab))
        {
            prefabToSpawn = specificPrefab;
        }

        if (prefabToSpawn == null) return;

        Vector3 spawnPos = new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0);

        GameObject newChunk = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        newChunk.name = $"WorldChunk_{coord.x}_{coord.y}";

        NetworkObject netObj = newChunk.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }

        activeChunks.Add(coord, newChunk);
        SpawnMobsInChunk(newChunk);
    }

    void RemoveChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject chunkObj))
        {
            if (chunkObj != null)
            {
                ChunkData chunkData = chunkObj.GetComponentInChildren<ChunkData>();
                chunkData.DespawnAllMobs();

                NetworkObject netObj = chunkObj.GetComponent<NetworkObject>();
                if (netObj != null) netObj.Despawn();
                Destroy(chunkObj);
            }
            activeChunks.Remove(coord);
        }
    }

// Neue Hilfsfunktion
    void SpawnMobsInChunk(GameObject chunk)
    {
        // Sucht alle MobSpawnPoint-Skripte, die Kinder von diesem Chunk sind
        ChunkData chunkData = chunk.GetComponentInChildren<ChunkData>();
        MobSpawnPoint[] spawnPoints = chunk.GetComponentsInChildren<MobSpawnPoint>();
        foreach (MobSpawnPoint spawnPoint in spawnPoints)
        {
            Transform position = spawnPoint.transform;
            GameObject dummyobj = Instantiate(dummy, position);
            dummyobj.GetComponent<dummyscript>().Setparrent(this);
            var netObj = dummyobj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                chunkData.RegisterMob(netObj);
                netObj.Spawn();
                
            }
        }
    }
    public void SpawnPickUpItem(string id, Transform transform)
    {
        GameObject pickupitem = Instantiate(PickUpItem, transform);
        pickupitem.name = id;
        ItemPickUp itemPickUp = pickupitem.GetComponent<ItemPickUp>();
        itemPickUp.setitem(1, 1, null);
        var netObj = pickupitem.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }

        
    }
}