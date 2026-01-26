using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChunkData : MonoBehaviour
{
    private List<NetworkObject> myMobs = new List<NetworkObject>();

    public void RegisterMob(NetworkObject mob)
    {
        myMobs.Add(mob);
    }

    public void DespawnAllMobs()
    {
        foreach (var mob in myMobs)
        {
            if (mob != null && mob.IsSpawned)
            {
                mob.Despawn();
            }
        }
        myMobs.Clear();
    }
}
