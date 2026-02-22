using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;

public class PlayerSaveHandler : NetworkBehaviour
{
    [Header("Spieler Info")]
    [SerializeField] private string playerName = "Fritzmann"; 
    
    [Header("Auto Save")]
    [SerializeField] private float autoSaveInterval = 30f; // Speichert alle 30 Sekunden
    private Coroutine autoSaveCoroutine;

    private Inventory inventory;
    private ItemInventory itemInventory;
    public event UnityAction dataLoaded;

    // ==========================================================
    // 1. SETUP & LOGIN (Wird beim Spawnen des Spielers aufgerufen)
    // ==========================================================
    public override void OnNetworkSpawn()
    {
        inventory = GetComponent<Inventory>();
        if (inventory != null)
        {
            itemInventory = inventory.itemInventory;
        }

        // SERVER: Sucht das Savegame auf seiner Festplatte und schickt es an den Client
        if (IsServer)
        {
            string path = Path.Combine(Application.persistentDataPath, $"inventory_save_{playerName}.json");
            string json = "";

            if (File.Exists(path))
            {
                json = File.ReadAllText(path);
                Debug.Log($"[Server] Lade Daten für {playerName} und sende an Client...");
            }
            else
            {
                Debug.Log($"[Server] Kein Speicherstand für {playerName} gefunden. Neues Profil.");
            }

            // Sende Daten nur an den Besitzer dieses Spielers
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { OwnerClientId } }
            };
            
            ReceiveDataFromServerClientRpc(json, clientRpcParams);
        }

        // CLIENT: Startet den unsichtbaren Auto-Save-Loop
        if (IsOwner)
        {
            dataLoaded.Invoke();
            autoSaveCoroutine = StartCoroutine(AutoSaveLoop());
        }
    }

    public override void OnNetworkDespawn()
    {
        // Wenn der Spieler verschwindet, stoppen wir den Auto-Save-Loop
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }
    }

    // ==========================================================
    // 2. AUTO-SAVE & LOGOUT (Client sendet Daten an Server)
    // ==========================================================

    // Endlosschleife für den Auto-Save im Hintergrund
    private System.Collections.IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            Debug.Log("[Client] Führe unsichtbaren Auto-Save aus...");
            SyncDataWithServer();
        }
    }

    // Wird automatisch von Unity aufgerufen, wenn das Spiel per 'X' oder Alt+F4 beendet wird
    private void OnApplicationQuit()
    {
        if (IsOwner && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log("[Client] Spiel wird geschlossen! Letzter Notfall-Save...");
            SyncDataWithServer();
        }
        Debug.Log("close Game");
    }

    // Für deinen manuellen Logout-Button im Spielmenü
    public void RequestLogoutAndSave()
    {
        if (!IsOwner) return;

        Debug.Log("[Client] Manueller Logout! Speichere und trenne Verbindung...");
        SyncDataWithServer();
        
        // Optional: NetworkManager.Singleton.Shutdown();
    }

    // Die Kern-Funktion: Generiert JSON und sendet es an den Server
    public void SyncDataWithServer()
    {
        if (!IsOwner) return;
        string json = GenerateSaveJson();
        SendSaveDataToServerRpc(json);
    }

    // Server empfängt den JSON-String und schreibt ihn auf die Festplatte
    [ServerRpc]
    private void SendSaveDataToServerRpc(string json)
    {
        string path = Path.Combine(Application.persistentDataPath, $"inventory_save_{playerName}.json");
        File.WriteAllText(path, json);
        Debug.Log($"[Server] Daten für {playerName} erfolgreich auf dem Server gespeichert!");
    }

    // Wandelt das aktuelle Inventar in einen Text (JSON) um
    private string GenerateSaveJson()
    {
        if (itemInventory == null) return "";

        InventorySaveData saveData = new InventorySaveData();

        for (int i = 0; i < itemInventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = itemInventory.inventorySlots[i];
            if (!slot.IsEmpty && slot.InventoryItemInstance != null)
            {
                ItemSaveData itemData = new ItemSaveData(slot.InventoryItemInstance);
                if (itemData != null) saveData.invslots.Add(new InventorySlotSaveData(i, slot.StackSize, itemData));
            }
        }

        for (int i = 0; i < itemInventory.equipmentSlots.Count; i++)
        {
            EquipmentSlot slot = itemInventory.equipmentSlots[i];
            if (!slot.IsEmpty && slot.InventoryItemInstance != null)
            {
                ItemSaveData itemData = new ItemSaveData(slot.InventoryItemInstance);
                if (itemData != null) saveData.equipmentSlots.Add(new InventorySlotSaveData(i, slot.StackSize, itemData));
            }
        }

        return JsonUtility.ToJson(saveData, true);
    }

    // ==========================================================
    // 3. LADEN & ANWENDEN (Client empfängt Daten vom Server)
    // ==========================================================

    // Client empfängt den JSON-String, den der Server in OnNetworkSpawn geschickt hat
    [ClientRpc]
    private void ReceiveDataFromServerClientRpc(string json, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return; 

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("[Client] Kein Speicherstand erhalten. Starte mit leerem Inventar.");
            return;
        }

        Debug.Log("[Client] Speicherstand vom Server erhalten! Wende Daten an...");
        ApplySaveData(json);
    }

    // Wandelt den Text wieder in ein Inventar um
    private void ApplySaveData(string json)
    {
        if (itemInventory == null) return;

        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

        foreach (var slot in itemInventory.inventorySlots) slot.clearSlot();
        foreach (var slot in itemInventory.equipmentSlots) slot.clearSlot();

        foreach (var slotData in saveData.invslots) RestoreItemToSlot(itemInventory.inventorySlots, slotData);
        foreach (var slotData in saveData.equipmentSlots) RestoreItemToSlot(itemInventory.equipmentSlots, slotData);
        
        dataLoaded?.Invoke();
    }

    // Stellt die Items aus dem Savegame in den echten Slots wieder her
    private void RestoreItemToSlot<T>(List<T> slots, InventorySlotSaveData data) where T : InventorySlot
    {
        if (data.slotIndex >= slots.Count) return;

        ItemData baseData = inventory.getItemByID(data.savedData.itemID);

        if (baseData != null)
        {
            InventoryItemInstance instance = null;

            if (baseData is EquipmentData equipData)
            {
                EquipmentInstance eqInstance = new EquipmentInstance(equipData);
                
                // Klone die spezifischen Stats für den Multiplayer-Schutz in den Arbeitsspeicher
                if (data.savedData.itemtype == Itemtype.Weapon && data.savedData.weaponStats != null)
                {
                    eqInstance.weaponStats = data.savedData.weaponStats.Clone();
                }
                else if (data.savedData.itemtype == Itemtype.Armor && data.savedData.armorStats != null)
                {
                    eqInstance.armorStats = data.savedData.armorStats.Clone();
                }
                else if (data.savedData.itemtype == Itemtype.Accessory && data.savedData.accessoryStats != null)
                {
                    eqInstance.accessoryStats = data.savedData.accessoryStats.Clone();
                }
                
                instance = eqInstance;
            }
            else
            {
                instance = new InventoryItemInstance(baseData);
            }

            if (instance != null)
            {
                slots[data.slotIndex].UpdateInventorySlot(instance, data.amount);
            }
        }
    }
}