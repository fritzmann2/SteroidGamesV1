using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class ChunkData : MonoBehaviour
{
    private List<NetworkObject> myMobs = new List<NetworkObject>();
    private List<NetworkObject> myPortals = new List<NetworkObject>();
    public List<GameObject> posibleMobs;
    public bool isBossArena = false;
    private bool boosSpawned = false;

    [Header("Portal Settings")]
    public GameObject bossPortalPrefab;

    private WorldGenerator myGenerator;
    private MobSpawnPoint[] cachedSpawnPoints;

    private Dictionary<string, GameObject> mobPrefabLookup = new Dictionary<string, GameObject>();
    private List<string> fallbackMobIds = new List<string>();

    private void Awake()
    {
        cachedSpawnPoints = transform.GetComponentsInChildren<MobSpawnPoint>(true);

        foreach (GameObject prefab in posibleMobs)
        {
            if (prefab == null) continue;
            
            BaseEnemy enemyScript = prefab.GetComponent<BaseEnemy>();
            if (enemyScript != null)
            {
                if (!mobPrefabLookup.ContainsKey(enemyScript.id))
                {
                    mobPrefabLookup.Add(enemyScript.id, prefab);
                    fallbackMobIds.Add(enemyScript.id); 
                }
            }
        }
    }

    public void SpawnMyMobs(WorldGenerator generator)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        myGenerator = generator;
        StartCoroutine(SpawnMobsRoutine(generator));
    }

    private IEnumerator SpawnMobsRoutine(WorldGenerator generator)
    {
        foreach (MobSpawnPoint spawnPoint in cachedSpawnPoints)
        {
            if (isBossArena && spawnPoint.isBossSpawner)
            {
                if (boosSpawned) continue;
                else boosSpawned = true;
            }
            
            Transform spawnPos = spawnPoint.transform;
            GameObject mobToSpawn = null;
            
            if (spawnPoint.possibleMobsNames != null && spawnPoint.possibleMobsNames.Count > 0)
            {
                string mobName = spawnPoint.getRandomMobName();
                
                if (mobPrefabLookup.TryGetValue(mobName, out GameObject matchedPrefab))
                {
                    mobToSpawn = generator.GetOrCreateMob(mobName, matchedPrefab, spawnPos.position);
                }
                else
                {
                    Debug.LogWarning($"[ChunkData] Fehler: Mob '{mobName}' nicht in 'posibleMobs'!");
                }
            }
            
            if (mobToSpawn == null && fallbackMobIds.Count > 0)
            {
                int randomIndex = Random.Range(0, fallbackMobIds.Count);
                string randomId = fallbackMobIds[randomIndex];
                GameObject fallbackPrefab = mobPrefabLookup[randomId];
                
                mobToSpawn = generator.GetOrCreateMob(randomId, fallbackPrefab, spawnPos.position);
            }

            if (mobToSpawn != null)
            {
                NetworkObject netObj = mobToSpawn.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    RegisterMob(netObj);
                    
                    BaseEnemy enemyScript = mobToSpawn.GetComponent<BaseEnemy>();
                    if (enemyScript != null)
                    {
                        enemyScript.SetParentChunk(this);
                        enemyScript.Setparrent(generator);
                        if (isBossArena) enemyScript.canSpawnItem = false;
                    }
                }
            }

            yield return null; 
        }

        if (!isBossArena) SpawnPortal();
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

    public void RegisterMob(NetworkObject mob) { myMobs.Add(mob); }

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
            if (mob != null)
            {
                BaseEnemy enemy = mob.GetComponent<BaseEnemy>();
                if (enemy != null && myGenerator != null)
                {
                    myGenerator.ReturnMobToPool(enemy.id, mob.gameObject);
                }
            }
        }
        myMobs.Clear();
    }

    public void DespawnMob(NetworkObject mob)
    {
        if (myMobs.Contains(mob))
        {
            myMobs.Remove(mob);
            if (mob != null)
            {
                BaseEnemy enemy = mob.GetComponent<BaseEnemy>();
                if (enemy != null && myGenerator != null)
                {
                    myGenerator.ReturnMobToPool(enemy.id, mob.gameObject);
                }
            }
        }
    }

    public void DespawnAllPortals()
    {
        for (int i = myPortals.Count - 1; i >= 0; i--)
        {
            NetworkObject portal = myPortals[i];
            if (portal != null && portal.IsSpawned) portal.Despawn();
        }
        myPortals.Clear();
    }
}