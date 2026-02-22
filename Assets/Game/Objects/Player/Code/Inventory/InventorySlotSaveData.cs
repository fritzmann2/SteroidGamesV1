using System.Collections.Generic;



[System.Serializable]
public class ItemSaveData
{
    public string itemID;
    public EquipmentType equipmentType;
    public Itemtype itemtype;
    
    public WeaponStats weaponStats; 
    public ArmorStats armorStats;
    public AccessoryStats accessoryStats;

    public ItemSaveData() { }

    public ItemSaveData(InventoryItemInstance instance)
    {
        if (instance == null || instance.itemData == null) return;

        itemID = instance.itemData.ID;
        itemtype = instance.itemtype;
        equipmentType = EquipmentType.None;

        if (instance is EquipmentInstance eqInstance)
        {
            equipmentType = eqInstance.itemData.equipmentType;
            EquipmentStats stats = eqInstance.GetEquipmentStats();

            if (stats is WeaponStats w) weaponStats = w;
            else if (stats is ArmorStats a) armorStats = a;
            else if (stats is AccessoryStats acc) accessoryStats = acc;
        }
    }
}

[System.Serializable]
public class InventorySlotSaveData
{
    public int slotIndex;
    public int amount;
    public ItemSaveData savedData; 

    public InventorySlotSaveData(int _index, int _amount, ItemSaveData _saveData)
    {
        slotIndex = _index;
        amount = _amount;
        savedData = _saveData;
    }
}

[System.Serializable]
public class InventorySaveData
{
    public List<InventorySlotSaveData> invslots = new List<InventorySlotSaveData>();
    public List<InventorySlotSaveData> equipmentSlots = new List<InventorySlotSaveData>();
}