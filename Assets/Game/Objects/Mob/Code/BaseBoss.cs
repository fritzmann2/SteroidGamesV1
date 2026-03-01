using UnityEngine;
using Unity.Netcode;

public abstract class BaseBoss : BaseEnemy
{
    [Header("Boss Info")]
    public string bossName = "Unbekannter Boss";
    private float imunity = 5f;
    private int healthState = 1;
    private bool canTakeDamage = true;

    private Coroutine immunityCoroutine; 
    public Vector3 itemSpawnPosition = new Vector3(0, 0, 0);


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.ShowBoss(bossName, maxHealth, transform);
            BossUIController.Instance.changeHPcolor(healthState);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.HideBoss();
        }
    }

    protected override void ApplyDamageServer(float finalDamage, bool isCrit)
    {
        if (!IsServer || !canTakeDamage) return; 

        DamageTextManager.Instance.ShowDamageText((int)finalDamage, transform.position, isCrit);
        int newValue = (int)(health.Value - finalDamage);

        if (health.Value > maxHealth * 2/3 && newValue <= maxHealth * 2 / 3)
        {
            health.Value = maxHealth * 2/3 - 1;
            int nextPhase = 2; 
            TriggerImmunityClientRpc(imunity, nextPhase);
            
            parentChunk.SpawnMyMobs(worldgen);
        }
        else if (health.Value > maxHealth * 1/3 && newValue <= maxHealth * 1 / 3)
        {
            health.Value = maxHealth * 1/3 - 1;
            int nextPhase = 3;
            
            TriggerImmunityClientRpc(imunity * 1.5f, nextPhase);
            
            parentChunk.SpawnMyMobs(worldgen);
        }
        else
        {
            health.Value -= finalDamage;
        }
    }

    public override void OnHealthChanged(float previousValue, float newValue)
    {
        if (newValue <= 0)
        {
            if (IsServer)
            {
                worldgen.SpawnPickUpItem(id, itemSpawnPosition);
                DistributeXP();
                parentChunk.SpawnPortal();
                parentChunk.DespawnAllMobs();
            }  
        }
        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.UpdateHealth(newValue);
        }
    }
    
    [ClientRpc]
    private void TriggerImmunityClientRpc(float duration, int targetHealthState)
    {
        healthState = targetHealthState;
        StartImmunity(duration);
    }

    public void StartImmunity(float duration)
    {
        if (immunityCoroutine != null)
        {
            StopCoroutine(immunityCoroutine);
        }
        
        immunityCoroutine = StartCoroutine(ImmunityRoutine(duration));
    }

    private System.Collections.IEnumerator ImmunityRoutine(float duration)
    {
        canTakeDamage = false; 
        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.changeHPcolor(0); 
        }
        yield return new WaitForSeconds(duration);
        canTakeDamage = true;
        if (BossUIController.Instance != null)
        {
            BossUIController.Instance.changeHPcolor(healthState); 
        }

        immunityCoroutine = null;
    }
}