using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChunkData : MonoBehaviour
{
    private List<NetworkObject> myMobs = new List<NetworkObject>();
    public List<GameObject> posibleMobs;
    public bool isBossArena = false;
    private bool boosSpawned = false;

    [Header("Portal Settings")]
    public GameObject bossPortalPrefab;


    public void SpawnMyMobs(WorldGenerator generator)
    {
        MobSpawnPoint[] spawnPoints = transform.parent.GetComponentsInChildren<MobSpawnPoint>();

        foreach (MobSpawnPoint spawnPoint in spawnPoints)
        {
            if (isBossArena)
            {
                if (spawnPoint.isBossSpawner)
                {
                    if (boosSpawned) continue;
                    else boosSpawned = true;
                }
            }

            Transform spawnPos = spawnPoint.transform;
            GameObject mobToSpawn = null;
            
            if (spawnPoint.possibleMobsNames.Count != 0)
            {
                string mobName = spawnPoint.getRandomMobName();
                
                foreach (GameObject mobPrefab in posibleMobs)
                {
                    if (mobPrefab.GetComponent<BaseEnemy>().id == mobName)
                    {
                        mobToSpawn = Instantiate(mobPrefab, spawnPos.position, Quaternion.identity);
                        break;
                    }
                }
                
                if (mobToSpawn == null) Debug.LogWarning($"[ChunkData] Fehler: Mob mit der ID '{mobName}' wurde nicht in der 'posibleMobs' Liste gefunden!");
            }
            else if (posibleMobs.Count != 0)
            {
                int randomIndex = Random.Range(0, posibleMobs.Count);
                mobToSpawn = Instantiate(posibleMobs[randomIndex], spawnPos.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[ChunkData] ACHTUNG: Der SpawnPoint hat keine Namen UND die ChunkData hat keine 'posibleMobs'. Es kann nichts gespawnt werden!");
            }

            if (mobToSpawn != null)
            {
                NetworkObject netObj = mobToSpawn.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn(); 
                    RegisterMob(netObj);
                    
                    BaseEnemy enemyScript = mobToSpawn.GetComponent<BaseEnemy>();
                    if (enemyScript != null)
                    {
                        enemyScript.SetParentChunk(this);
                        enemyScript.Setparrent(generator);
                    }
                }
                else
                {
                    Debug.LogError($"[ChunkData] Das Prefab {mobToSpawn.name} hat keine NetworkObject Komponente!");
                }
            }
        }

        Transform portalSpawnPoint = transform.parent.Find("PortalSpawnPoint");
        if (portalSpawnPoint != null && bossPortalPrefab != null)
        {
            GameObject portalInstance = Instantiate(bossPortalPrefab, portalSpawnPoint.position, Quaternion.identity);
            NetworkObject portalNetObj = portalInstance.GetComponent<NetworkObject>();
            if (portalNetObj != null)
            {
                portalNetObj.Spawn();
                RegisterMob(portalNetObj); 
            }
        }
    }

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
    public void DespawnMob(NetworkObject mob)
    {
        if (myMobs.Contains(mob))
        {
            if (mob != null && mob.IsSpawned)
            {
                mob.Despawn();
            }
            myMobs.Remove(mob);
        }
    }
}
