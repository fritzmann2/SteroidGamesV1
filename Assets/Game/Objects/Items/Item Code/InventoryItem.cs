using UnityEngine;
using Unity.Netcode;


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
    public new EquipmentData itemData;


    public EquipmentInstance(EquipmentData _data) : base(_data)
    {
        itemData.equipmentType = _data.equipmentType;
        if (itemData.equipmentType == EquipmentType.Bow || itemData.equipmentType == EquipmentType.Scythe || itemData.equipmentType == EquipmentType.Sword)
        {
            itemData.Type = Itemtype.Weapon;
        }
        else if (itemData.equipmentType == EquipmentType.Helmet || itemData.equipmentType == EquipmentType.Chestplate || itemData.equipmentType == EquipmentType.Leggings || itemData.equipmentType == EquipmentType.Boots)
        {
            itemData.Type = Itemtype.Armor;
        }
        else if (itemData.equipmentType == EquipmentType.Ring || itemData.equipmentType == EquipmentType.Necklace)
        {
            itemData.Type = Itemtype.Accessory;
        }

        if (_data is WeaponData wData) weaponStats = wData.weaponStats;
        else if (_data is ArmorData aData) armorStats = aData.armorStats;
        else if (_data is AccessoryData accData) accessoryStats = accData.accessoryStats;
    }
    public EquipmentStats GetEquipmentStats()
    {   
        if (itemData.Type == Itemtype.Weapon)
        {
            return weaponStats;
        }
        else if (itemData.Type == Itemtype.Armor)
        {
            return armorStats;
        }
        else if (itemData.Type == Itemtype.Accessory)
        {
            return accessoryStats;
        }
        else return null;
    }

}

