using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChunkData : MonoBehaviour
{
    public List<NetworkObject> myMobs = new List<NetworkObject>();

    public MobSpawnPoint[] GetSpawnPoints()
    {
        return GetComponentsInChildren<MobSpawnPoint>(true);
    }

    public void RegisterMob(NetworkObject mob)
    {
        myMobs.Add(mob);
    }

    public void DespawnAllMobs()
    {
        foreach (NetworkObject mob in myMobs)
        {
            if (mob != null && mob.IsSpawned)
            {
                mob.Despawn(true);
            }
        }
        myMobs.Clear();
    }
}