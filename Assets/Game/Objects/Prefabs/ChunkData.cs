using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChunkData : MonoBehaviour
{
    private List<NetworkObject> myMobs = new List<NetworkObject>();
    private List<NetworkObject> myPortals = new List<NetworkObject>();
    public List<GameObject> posibleMobs;
    public bool isBossArena = false;
    private bool boosSpawned = false;

    [Header("Portal Settings")]
    public GameObject bossPortalPrefab;


    public void SpawnMyMobs(WorldGenerator generator)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        MobSpawnPoint[] spawnPoints = transform.GetComponentsInChildren<MobSpawnPoint>();
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
            if (spawnPoint.possibleMobsNames != null && spawnPoint.possibleMobsNames.Count > 0)
            {
                string mobName = spawnPoint.getRandomMobName();
                
                foreach (GameObject mobPrefab in posibleMobs)
                {
                    BaseEnemy enemyScript = mobPrefab.GetComponent<BaseEnemy>();
                    if (enemyScript != null && enemyScript.id == mobName)
                    {
                        mobToSpawn = Instantiate(mobPrefab, spawnPos.position, Quaternion.identity);
                        break; 
                    }
                }
                
                if (mobToSpawn == null) 
                {
                    Debug.LogWarning($"[ChunkData] Fehler: Mob mit der ID '{mobName}' wurde nicht in der 'posibleMobs' Liste gefunden!");
                }
            }
            if (mobToSpawn == null)
            {
                if (posibleMobs != null && posibleMobs.Count > 0)
                {
                    int randomIndex = Random.Range(0, posibleMobs.Count);
                    mobToSpawn = Instantiate(posibleMobs[randomIndex], spawnPos.position, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("[ChunkData] ACHTUNG: Der SpawnPoint hat keine Namen UND die ChunkData hat keine 'posibleMobs'. Es kann nichts gespawnt werden!");
                    continue; 
                }
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
                        if (isBossArena)
                        {
                            enemyScript.canSpawnItem = false;
                        }
                    }
                }
                else
                {
                    Debug.LogError($"[ChunkData] Das Prefab {mobToSpawn.name} hat keine NetworkObject Komponente!");
                    Destroy(mobToSpawn);
                }
            }
        }
        if (!isBossArena)
        {
            SpawnPortal();
        }
    }

    public void SpawnPortal()
    {
        Transform portalSpawnPoint = transform.Find("PortalSpawnPoint");
        
        if (portalSpawnPoint != null && bossPortalPrefab != null)
        {
            GameObject portalInstance = Instantiate(bossPortalPrefab, portalSpawnPoint.position, Quaternion.identity);
            BossPortals portalScript = portalInstance.GetComponent<BossPortals>();
            PortalSpawner spawnerScript = portalSpawnPoint.GetComponent<PortalSpawner>();
            if (portalScript != null && spawnerScript != null)
            {
                portalScript.destinationCoordinate = spawnerScript.teleportDestination;
            }
            NetworkObject portalNetObj = portalInstance.GetComponent<NetworkObject>();
            if (portalNetObj != null)
            {
                portalNetObj.Spawn();
                myPortals.Add(portalNetObj);
            }
        }
    }

    public void RegisterMob(NetworkObject mob)
    {
        myMobs.Add(mob);
    }

    public void DespawnEveryThing()
    {
        DespawnAllMobs();
        DespawnAllPortals();
    }

    public void DespawnAllMobs()
    {
        for (int i = myMobs.Count - 1; i >= 0; i--)
        {
            NetworkObject mob = myMobs[i];
            if (mob != null && mob.IsSpawned)
            {
                mob.Despawn();
            }
        }
        myMobs.Clear();
    }

    public void DespawnAllPortals()
    {
        for (int i = myPortals.Count - 1; i >= 0; i--)
        {
            NetworkObject portal = myPortals[i];
            if (portal != null && portal.IsSpawned)
            {
                portal.Despawn();
            }
        }
        myPortals.Clear();
    }

    public void DespawnMob(NetworkObject mob)
    {
        if (myMobs.Contains(mob))
        {
            myMobs.Remove(mob);
            if (mob != null && mob.IsSpawned)
            {
                mob.Despawn();
            }
        }
    }
}