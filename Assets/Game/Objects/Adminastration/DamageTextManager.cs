using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class DamageTextManager : NetworkBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    [SerializeField] private GameObject damageTextPrefab;


    private void Awake()
    {
        Instance = this;
        if (Instance != null && Instance != this) 
        {
            Debug.LogError("DamageTextManager Deaktivating");
            Destroy(gameObject);
            return;
        }

    }

 

    public void ShowDamageText(int damageAmount, Vector3 position, bool isCrit)
    {
        // Nur der Server darf den RPC senden
        if (IsServer)
        {
            SpawnDamageTextClientRpc(damageAmount, position, isCrit);
        }
    }

    [ClientRpc]
    private void SpawnDamageTextClientRpc(int damageAmount, Vector3 position, bool isCrit)
    {
        GameObject damagePopupObj = Instantiate(damageTextPrefab, position, Quaternion.identity);
        
        DamagePopup popup = damagePopupObj.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(damageAmount, isCrit);
        }
        else
        {
            Debug.LogError("DamagePopup Script fehlt auf dem Prefab!");
        }
    }
}