using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WizardScript : BaseEnemy
{
    [Header("Wizard Settings")]
    public GameObject projectilePrefab;

    public override void Reset()    
    {
        id = "Wizard";
        maxHealth = 10;
        maxdistance = 15f; 
        mindistance = 5f;
        attackCooldown = 3f;
        base.Reset();

        #if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("Projectile t:GameObject");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log($"WizardScript: Prefab '{projectilePrefab.name}' automatisch gefunden und zugewiesen!");
            }
            else
            {
                Debug.LogError("WizardScript: Konnte kein Prefab mit Namen 'WizardProjectile' finden!");
            }
        #endif
    } 
    public override void Awake()
    {
        base.Awake();
    }

    public override void Attack()
    {
        if (!IsServer) return;

        spawnAttackProjectile();
    }

    private void spawnAttackProjectile()
    {
        if (targetPlayer == null) return;

        Vector2 playerDir = new Vector2(targetPlayer.position.x - transform.position.x, 0f).normalized;
        
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(playerDir.x * 0.3f, 4f);

        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        NetworkObject netObj = projectileInstance.GetComponent<NetworkObject>();
        netObj.Spawn(); 
        projectileInstance.GetComponent<BaseSpell>().Init(targetPlayer.transform.position, damage);        
    }
}