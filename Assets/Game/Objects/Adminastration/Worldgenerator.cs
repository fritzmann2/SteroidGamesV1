using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WorldGenerator : NetworkBehaviour
{
    [Header("Alle Prefabs hier reinziehen")]
    public GameObject[] allMapChunks;
    public GameObject dummy;
    public GameObject PickUpItem;
    public GameObject DropItem;

    [Header("Optimization")]
    public float chunkCheckInterval = 0.5f;

    [Header("Boss Arenen")]
    public GameObject[] bossArenaPrefabs;
    
    private Dictionary<Vector2Int, Vector2Int> bossArenaAnchors = new Dictionary<Vector2Int, Vector2Int>();
    private HashSet<Vector2Int> knownBossCoordinates = new HashSet<Vector2Int>();
    
    private HashSet<Vector2Int> noSpawnZones = new HashSet<Vector2Int>();

    [Header("Settings")]
    public int chunkSize = 50;
    public GameObject defaultChunkPrefab; 
    public CameraFollow cam;


    private Dictionary<Vector2Int, GameObject> mapLookup = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    private Vector2Int[] ChunkOffsets = new Vector2Int[]
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
        
        foreach (GameObject prefab in bossArenaPrefabs)
        {
            if (prefab == null) continue;

            string[] parts = prefab.name.Split('_');
            
            int x = 0, y = 0, w = 1, h = 1;
            bool parsed = false;

            if (parts.Length >= 5 &&
                int.TryParse(parts[parts.Length - 4], out x) &&
                int.TryParse(parts[parts.Length - 3], out y) &&
                int.TryParse(parts[parts.Length - 2], out w) &&
                int.TryParse(parts[parts.Length - 1], out h))
            {
                parsed = true;
            }
            else if (parts.Length >= 3 &&
                int.TryParse(parts[parts.Length - 2], out x) &&
                int.TryParse(parts[parts.Length - 1], out y))
            {
                parsed = true;
            }

            if (parsed)
            {
                Vector2Int anchorCoord = new Vector2Int(x, y);
                if (!bossArenaAnchors.ContainsKey(anchorCoord))
                {
                    knownBossCoordinates.Add(anchorCoord);
                    mapLookup.Add(anchorCoord, prefab); 

                    int startX = x - w / 2;
                    int endX = x + (w - 1) / 2;
                    int startY = y - h / 2;
                    int endY = y + (h - 1) / 2;

                    for (int i = startX; i <= endX; i++)
                    {
                        for (int j = startY; j <= endY; j++)
                        {
                            Vector2Int occupiedCoord = new Vector2Int(i, j);
                            bossArenaAnchors[occupiedCoord] = anchorCoord;
                        }
                    }

                    int buffer = 2;
                    for (int i = startX - buffer; i <= endX + buffer; i++)
                    {
                        for (int j = startY - buffer; j <= endY + buffer; j++)
                        {
                            Vector2Int bufferCoord = new Vector2Int(i, j);
                            noSpawnZones.Add(bufferCoord);
                        }
                    }
                }
            }
        }
        StartCoroutine(ChunkCheckRoutine());
    }

    private System.Collections.IEnumerator ChunkCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(chunkCheckInterval);

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) continue;

            HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null) continue;

                Transform playerTransform = client.PlayerObject.transform;
                
                int pX = Mathf.FloorToInt(playerTransform.position.x / chunkSize);
                int pY = Mathf.RoundToInt(playerTransform.position.y / chunkSize);

                Vector2Int playerGridPos = new Vector2Int(pX, pY);

                if (bossArenaAnchors.TryGetValue(playerGridPos, out Vector2Int anchorPos))
                {
                    chunksToKeep.Add(anchorPos); 
                }
                else
                {
                    foreach (Vector2Int offset in ChunkOffsets)
                    {
                        Vector2Int targetChunk = playerGridPos + offset;
                        
                        if (!noSpawnZones.Contains(targetChunk))
                        {
                            chunksToKeep.Add(targetChunk);
                        }
                    }
                }
            }

            foreach (Vector2Int coord in chunksToKeep)
            {
                if (!activeChunks.ContainsKey(coord))
                {
                    SpawnChunkByCoord(coord);
                }
            }

            List<Vector2Int> chunksToRemove = new List<Vector2Int>();
            foreach (var kvp in activeChunks)
            {
                if (!chunksToKeep.Contains(kvp.Key))
                {
                    chunksToRemove.Add(kvp.Key);
                }
            }

            foreach (Vector2Int coord in chunksToRemove)
            {
                RemoveChunk(coord);
            }
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

        ChunkData chunkData = newChunk.GetComponentInChildren<ChunkData>();
        if (chunkData != null)
        {
            chunkData.SpawnMyMobs(this);
        }
    }

    void RemoveChunk(Vector2Int coord)
    {
        if (activeChunks.TryGetValue(coord, out GameObject chunkObj))
        {
            if (chunkObj != null)
            {
                ChunkData chunkData = chunkObj.GetComponentInChildren<ChunkData>();
                if(chunkData != null) chunkData.DespawnEveryThing();

                NetworkObject netObj = chunkObj.GetComponent<NetworkObject>();
                if (netObj != null) netObj.Despawn();
                Destroy(chunkObj);
            }
            activeChunks.Remove(coord);
        }
    }

    public void SpawnPickUpItem(string id, Vector3 spawnPosition)
    {
        GameObject pickupitem = Instantiate(PickUpItem, spawnPosition, Quaternion.identity);
        pickupitem.name = id;
        ItemPickUp itemPickUp = pickupitem.GetComponent<ItemPickUp>();
        int itemRarity = 1;
        if (id == "WizardBoss")
        {
            itemRarity = 2;
        }
        if (itemPickUp != null) itemPickUp.setitem(itemRarity, 1, null);
        
        var netObj = pickupitem.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }

    public void TeleportToBoss(Vector3 _bossArenaCords, Transform PlayerToTeleport)
    {
        if (!IsServer) return;
        NetworkObject playerNetObj = PlayerToTeleport.GetComponent<NetworkObject>();
        if (playerNetObj != null)
        {
            ulong clientId = playerNetObj.OwnerClientId;
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };
            TeleportSinglePlayerClientRpc(_bossArenaCords, clientRpcParams);
        }
    }

    [ClientRpc]
    private void TeleportSinglePlayerClientRpc(Vector3 destination, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            GameObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
            Rigidbody2D rb = localPlayer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            localPlayer.transform.position = destination;
            localPlayer.GetComponent<PlayerMovement>().pauseGravity();

            if (cam != null)
            {
                cam.SetZoom(7f);
            }
            else
            {
                Debug.LogWarning("camera not found");
            }
        }
    }

    public void dropItem(InventoryItemInstance _item, Vector3 _spawnPosition, int _amount)
    {
        string itemID = _item.itemData.ID; 
        bool isEquipment = _item is EquipmentInstance;
        string serializedStats = "";

        if (isEquipment)
        {
            serializedStats = JsonUtility.ToJson((EquipmentInstance)_item);
        }
        if (IsServer)
        {
            SpawnDropItemOnServer(itemID, isEquipment, serializedStats, _spawnPosition, _amount);
        }
        else
        {
            RequestDropItemServerRpc(itemID, isEquipment, serializedStats, _spawnPosition, _amount);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestDropItemServerRpc(string itemID, bool isEquipment, string serializedStats, Vector3 spawnPosition, int amount)
    {
        SpawnDropItemOnServer(itemID, isEquipment, serializedStats, spawnPosition, amount);
    }

    private void SpawnDropItemOnServer(string itemID, bool isEquipment, string serializedStats, Vector3 _spawnPosition, int _amount)
    {

        ItemData baseItemData = GetItemDataByID(itemID); 

        if (baseItemData == null)
        {
            Debug.LogError($"ItemData für ID {itemID} nicht gefunden!");
            return;
        }

        InventoryItemInstance reconstructedItem;

        if (isEquipment)
        {
            EquipmentInstance equip = new EquipmentInstance((EquipmentData)baseItemData);
            JsonUtility.FromJsonOverwrite(serializedStats, equip);
            reconstructedItem = equip;
        }
        else
        {
            reconstructedItem = new InventoryItemInstance(baseItemData);
        }

        GameObject dropItemObj = Instantiate(DropItem, _spawnPosition, Quaternion.identity); 
        dropItemObj.GetComponent<DropItem>().init(reconstructedItem, _amount);
        
        var netObj = dropItemObj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }

    private ItemData GetItemDataByID(string id)
    {
        Inventory playerInventory = FindAnyObjectByType<Inventory>();
        
        if (playerInventory != null)
        {
            return playerInventory.getItemByID(id);
        }
        return null; 
    }
}