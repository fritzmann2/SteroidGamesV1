using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;

public class PlayerSaveHandler : NetworkBehaviour
{
    [SerializeField] private string playerName = "Fritzmann"; 
    private Inventory inventory;
    private ItemInventory itemInventory;
    public event UnityAction dataLoaded;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inventory = GetComponent<Inventory>();
        if (inventory != null)
        {
            itemInventory = inventory.itemInventory;
        }
    }

    [ContextMenu("Save Inventory")]
    public void SaveInventory()
    {
        if (itemInventory == null) return;

        InventorySaveData saveData = new InventorySaveData();

        for (int i = 0; i < itemInventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = itemInventory.inventorySlots[i];

            if (!slot.IsEmpty && slot.InventoryItemInstance != null)
            {
                ItemSaveData itemData = slot.GetSaveData();
                
                if (itemData != null)
                {
                    InventorySlotSaveData data = new InventorySlotSaveData(
                        i, 
                        slot.StackSize, 
                        itemData
                    );
                    saveData.invslots.Add(data);
                }
            }
        }

        for (int i = 0; i < itemInventory.equipmentSlots.Count; i++)
        {
            EquipmentSlot slot = itemInventory.equipmentSlots[i];

            if (!slot.IsEmpty && slot.InventoryItemInstance != null)
            {
                ItemSaveData itemData = slot.GetSaveData();

                if (itemData != null)
                {
                    InventorySlotSaveData data = new InventorySlotSaveData(
                        i, 
                        slot.StackSize, 
                        itemData
                    );
                    saveData.equipmentSlots.Add(data);
                }
            }
        }

        string json = JsonUtility.ToJson(saveData, true);

        string filename = $"inventory_save_{playerName}.json";
        string path = Path.Combine(Application.persistentDataPath, filename);

        File.WriteAllText(path, json);
        Debug.Log("Inventar gespeichert unter: " + path);
    }

    [ContextMenu("Load Inventory")]
    public void LoadInventory()
    {
        if (itemInventory == null) return;

        string filename = $"inventory_save_{playerName}.json";
        string path = Path.Combine(Application.persistentDataPath, filename);

        if (!File.Exists(path))
        {
            Debug.LogWarning("Kein Speicherstand gefunden.");
            return;
        }

        string json = File.ReadAllText(path);
        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

        foreach (var slot in itemInventory.inventorySlots) slot.clearSlot();
        foreach (var slot in itemInventory.equipmentSlots) slot.clearSlot();

        foreach (var slotData in saveData.invslots)
        {
            RestoreItemToSlot(itemInventory.inventorySlots, slotData);
        }

        foreach (var slotData in saveData.equipmentSlots)
        {
            RestoreItemToSlot(itemInventory.equipmentSlots, slotData);
        }
        
        Debug.Log("Inventar geladen.");
        dataLoaded?.Invoke();
    }

    private void RestoreItemToSlot<T>(List<T> slots, InventorySlotSaveData data) where T : InventorySlot
    {
        if (data.slotIndex >= slots.Count) return;

        ItemData baseData = inventory.getItemByID(data.savedData.itemID);

        if (baseData != null)
        {
            InventoryItemInstance instance = null;

            if (data.savedData.itemtype == Itemtype.Equipment)
            {
                if (baseData is EquipmentData equipData)
                {
                    EquipmentInstance eqInstance = new EquipmentInstance(equipData);
                    
                    if (data.savedData.weaponStats.weapondamage > 0) eqInstance.weaponStats = data.savedData.weaponStats;
                    
                    instance = eqInstance;
                }
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