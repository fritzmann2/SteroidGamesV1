using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChunkData : NetworkBehaviour
{
    [Header("Network Sync")]
    public NetworkVariable<Vector2Int> gridCoordinate = new NetworkVariable<Vector2Int>(
        new Vector2Int(9999, 9999), 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    [Header("Chunk Content")]
    public List<NetworkObject> myMobs = new List<NetworkObject>();
    public List<NetworkObject> myPortals = new List<NetworkObject>();
    public List<Vector2Int> linkedChunks = new List<Vector2Int>();

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer) 
        {
            WorldGenerator wg = FindAnyObjectByType<WorldGenerator>();
            if (wg != null)
            {
                wg.SpawnVisualForClient(gameObject, gridCoordinate.Value);
            }
        }
    }

    public MobSpawnPoint[] GetSpawnPoints()
    {
        return GetComponentsInChildren<MobSpawnPoint>(true);
    }

    public void RegisterMob(NetworkObject mob)
    {
        myMobs.Add(mob);
    }

    public void RegisterPortal(NetworkObject portal)
    {
        myPortals.Add(portal);
    }

    public void DespawnAllMobs()
    {
        foreach (NetworkObject mob in myMobs)
        {
            if (mob != null && mob.IsSpawned) mob.Despawn(true);
        }
        myMobs.Clear();
    }
    
    public void BossDead()
    {
        DespawnAllMobs();
        SpawnPortal();
    }

    private void SpawnPortal()
    {
    }

    public void DespawnAllPortals()
    {
        foreach (NetworkObject portal in myPortals)
        {
            if (portal != null && portal.IsSpawned) portal.Despawn(true);
        }
        myPortals.Clear();
    }
}