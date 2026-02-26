using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerSaveHandler : NetworkBehaviour
{
    [Header("Spieler Info")]
    public string playerName;
    
    [Header("Auto Save")]
    [SerializeField] private float autoSaveInterval = 30f; 
    private Coroutine autoSaveCoroutine;

    private Inventory inventory;
    private ItemInventory itemInventory;
    private PlayerStats playerStats;
    public event UnityAction dataLoaded;

    public override void OnNetworkSpawn()
    {
        inventory = GetComponent<Inventory>();
        playerStats = GetComponent<PlayerStats>();
        
        if (inventory != null)
        {
            itemInventory = inventory.itemInventory;
        }

        if (IsOwner)
        {
            string myName = PlayerPrefs.GetString("PlayerName", "UnknownPlayer");
            
            RequestLoadDataServerRpc(myName);

            autoSaveCoroutine = StartCoroutine(AutoSaveLoop());
            PauseManager pauseManager = FindAnyObjectByType<PauseManager>();
            if (pauseManager != null)
            {
                pauseManager.RegisterPlayerSaveHandler(this);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && !string.IsNullOrEmpty(this.playerName))
        {
            LevelManager.Instance.UnregisterPlayer(this.playerName);
        }

        if (autoSaveCoroutine != null) StopCoroutine(autoSaveCoroutine);
    }

    private System.Collections.IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SyncDataWithServer();
        }
    }
    public void SyncDataWithServer()
    {
        if (!IsOwner) return;
        
        GameSaveData data = GenerateFullSaveData();
        string json = JsonUtility.ToJson(data);
        
        byte[] jsonData = System.Text.Encoding.UTF8.GetBytes(json);
        SendSaveDataToServerRpc(jsonData);
    }


    private void OnApplicationQuit()
    {
        if (!IsOwner) return;

        Debug.Log("[Client] Spiel wird geschlossen! Letzter Notfall-Save...");
        GameSaveData data = GenerateFullSaveData();

        if (IsServer)
        {
            SaveManager.Instance.SaveGameData(data);
        }
        else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            byte[] jsonData = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
            SendSaveDataToServerRpc(jsonData);
        }
    }

    [ServerRpc]
    private void RequestLoadDataServerRpc(string clientPlayerName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (!LevelManager.Instance.TryRegisterPlayer(clientPlayerName, this.transform))
        {
            Debug.Log($"[Server] Name '{clientPlayerName}' ist bereits online! Werfe Client raus.");
            
            RejectLoginClientRpc("Dieser Name ist bereits auf dem Server online!", new ClientRpcParams 
            { 
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } 
            });

            NetworkManager.Singleton.DisconnectClient(clientId);
            
            return;
        }

        this.playerName = clientPlayerName;

        GameSaveData data = SaveManager.Instance.LoadPlayerData(this.playerName);

        if (data != null)
        {
            ApplyFullSaveData(data);
        }
        string json = data != null ? JsonUtility.ToJson(data) : "";
        byte[] jsonData = System.Text.Encoding.UTF8.GetBytes(json);

        ClientRpcParams clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };
        
        ReceiveDataFromServerClientRpc(jsonData, clientRpcParams);
    }

    [ClientRpc]
    private void RejectLoginClientRpc(string errorMessage, ClientRpcParams rpcParams = default)
    {
        Debug.LogWarning($"[Client] Login fehlgeschlagen: {errorMessage}");
        if (NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
    }

    

    public void RequestLogoutAndSave()
    {
        if (!IsOwner) return;
        SyncDataWithServer();
    }

    [ServerRpc]
    private void SendSaveDataToServerRpc(byte[] jsonData)
    {
        string json = System.Text.Encoding.UTF8.GetString(jsonData);
        
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        SaveManager.Instance.SaveGameData(data);
    }

    private GameSaveData GenerateFullSaveData()
    {
        GameSaveData data = new GameSaveData();
        data.playerName = this.playerName;

        if (playerStats != null)
        {
            data.statsData = playerStats.GetSaveData();
        }

        data.inventoryData = GenerateInventorySaveData();
        
        return data;
    }

    private InventorySaveData GenerateInventorySaveData()
    {
        InventorySaveData invData = new InventorySaveData();
        if (itemInventory == null) return invData;

        for (int i = 0; i < itemInventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = itemInventory.inventorySlots[i];
            if (!slot.IsEmpty && slot.InventoryItemInstance != null)
            {
                ItemSaveData itemData = new ItemSaveData(slot.InventoryItemInstance);
                if (itemData != null) invData.invslots.Add(new InventorySlotSaveData(i, slot.StackSize, itemData));
            }
        }

        for (int i = 0; i < itemInventory.equipmentSlots.Count; i++)
        {
            EquipmentSlot slot = itemInventory.equipmentSlots[i];
            if (!slot.IsEmpty && slot.InventoryItemInstance != null)
            {
                ItemSaveData itemData = new ItemSaveData(slot.InventoryItemInstance);
                if (itemData != null) invData.equipmentSlots.Add(new InventorySlotSaveData(i, slot.StackSize, itemData));
            }
        }
        return invData;
    }

    [ClientRpc]
    private void ReceiveDataFromServerClientRpc(byte[] jsonData, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return; 
        if (jsonData == null || jsonData.Length == 0) return;
        string json = System.Text.Encoding.UTF8.GetString(jsonData);
        if (string.IsNullOrEmpty(json)) return;
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        ApplyFullSaveData(data);
    }

    private void ApplyFullSaveData(GameSaveData data)
    {       

        if (playerStats != null && data.statsData != null)
        {
            playerStats.LoadSaveData(data.statsData);
        }

        if (itemInventory != null && data.inventoryData != null)
        {
            foreach (var slot in itemInventory.inventorySlots) slot.clearSlot();
            foreach (var slot in itemInventory.equipmentSlots) slot.clearSlot();

            foreach (var slotData in data.inventoryData.invslots) RestoreItemToSlot(itemInventory.inventorySlots, slotData);
            foreach (var slotData in data.inventoryData.equipmentSlots) RestoreItemToSlot(itemInventory.equipmentSlots, slotData);
        }
        playerStats.playerName = playerName;
        dataLoaded?.Invoke();
    }

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
                
                if (data.savedData.itemtype == Itemtype.Weapon && data.savedData.weaponStats != null)
                    eqInstance.weaponStats = data.savedData.weaponStats.Clone();
                else if (data.savedData.itemtype == Itemtype.Armor && data.savedData.armorStats != null)
                    eqInstance.armorStats = data.savedData.armorStats.Clone();
                else if (data.savedData.itemtype == Itemtype.Accessory && data.savedData.accessoryStats != null)
                    eqInstance.accessoryStats = data.savedData.accessoryStats.Clone();
                
                instance = eqInstance;
            }
            else
            {
                instance = new InventoryItemInstance(baseData);
            }

            if (instance != null) slots[data.slotIndex].UpdateInventorySlot(instance, data.amount);
        }
    }

    public void RequestPlayerReset()
    {
        if (!IsOwner) return;
        ResetPlayerServerRpc();
    }

    [ServerRpc]
    private void ResetPlayerServerRpc()
    {
        PerformResetLocally();

        GameSaveData emptyData = GenerateFullSaveData();
        SaveManager.Instance.SaveGameData(emptyData);

        ResetPlayerClientRpc();
    }

    [ClientRpc]
    private void ResetPlayerClientRpc()
    {
        if (!IsOwner) return;
        PerformResetLocally();
    }

    private void PerformResetLocally()
    {
        if (itemInventory != null)
        {
            foreach (var slot in itemInventory.inventorySlots) slot.clearSlot();
            foreach (var slot in itemInventory.equipmentSlots) slot.clearSlot();
        }

        if (playerStats != null)
        {
            playerStats.ResetToDefault();
        }

        dataLoaded?.Invoke();
    }
}