using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;


public class InventoryItem : NetworkBehaviour
{
    public ItemData itemData;

    public void Initialize(string _id)
    {        
        if (itemData != null)
        {
            itemData.ID = _id;
        }
        else
        {
            Debug.LogError("ItemData ist im Inspector nicht zugewiesen!");
        }
        
    }
}

[System.Serializable]
public class InventoryItemInstance
{
    public ItemData itemData; 
    public Itemtype itemtype;

    public InventoryItemInstance(ItemData _data)
    {
        itemData = _data;
    }
}

public class EquipmentInstance : InventoryItemInstance
{
    public WeaponStats weaponStats;
    public ArmorStats armorStats;
    public AccessoryStats accessoryStats;

    // Hilfsvariable für den genauen Equipment Typ
    public EquipmentType equipmentType;

    public EquipmentInstance(EquipmentData _data) : base(_data)
    {
        itemtype = Itemtype.Equipment;
        equipmentType = _data.equipmentType;

        if (_data is WeaponData wData) weaponStats = wData.weaponStats;
        else if (_data is ArmorData aData) armorStats = aData.armorStats;
        else if (_data is AccessoryData accData) accessoryStats = accData.accessoryStats;
    }
}

