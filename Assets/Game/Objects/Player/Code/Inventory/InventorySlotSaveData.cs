using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;



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

    public ItemSaveData(string _id)
    {
        itemID = _id;
        equipmentType = EquipmentType.None;
    }

    // Konstruktor für Waffen
    public ItemSaveData(string _id, EquipmentType _type, WeaponStats _wStats)
    {
        itemID = _id;
        equipmentType = _type;
        weaponStats = _wStats;
        itemtype = Itemtype.Equipment;
    }
    public ItemSaveData(string _id, EquipmentType _type, ArmorStats _aStats)
    {
        itemID = _id;
        equipmentType = _type;
        armorStats = _aStats;
        itemtype = Itemtype.Equipment;
    }
    public ItemSaveData(string _id, EquipmentType _type, AccessoryStats _aStats)
    {
        itemID = _id;
        equipmentType = _type;
        accessoryStats = _aStats;
        itemtype = Itemtype.Equipment;
    }
    public ItemSaveData(string _id, Itemtype _type)
    {
        itemID = _id;
        itemtype = _type;
    }
}

[System.Serializable]
public struct InventorySlotSaveData
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