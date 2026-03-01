using UnityEngine;
using Unity.Netcode;


public class InventoryItem : NetworkBehaviour
{
    public ItemData itemData;
    public PlayerStats playerStats;


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

    public void initScript(PlayerStats _playerStats)
    {
        playerStats = _playerStats;
        Debug.Log("PlayerStats set");
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
        itemtype = _data.Type;
    }
}

[System.Serializable]
public class EquipmentInstance : InventoryItemInstance
{
    public WeaponStats weaponStats;
    public ArmorStats armorStats;
    public AccessoryStats accessoryStats;
    public new EquipmentData itemData
    {
        get { return (EquipmentData)base.itemData; }
        set { base.itemData = value; }
    }

    public EquipmentInstance(EquipmentData _data) : base(_data)
    {
        if (_data.equipmentType == EquipmentType.Bow || _data.equipmentType == EquipmentType.Scythe || _data.equipmentType == EquipmentType.Sword)
        {
            this.itemtype = Itemtype.Weapon;
        }
        else if (_data.equipmentType == EquipmentType.Helmet || _data.equipmentType == EquipmentType.Chestplate || _data.equipmentType == EquipmentType.Leggings || _data.equipmentType == EquipmentType.Boots)
        {
            this.itemtype = Itemtype.Armor;
        }
        else if (_data.equipmentType == EquipmentType.Ring || _data.equipmentType == EquipmentType.Necklace)
        {
            this.itemtype = Itemtype.Accessory;
        }
        else
        {
            this.itemtype = Itemtype.None;
        }

        if (_data is WeaponData wData) weaponStats = wData.weaponStats.Clone();
        else if (_data is ArmorData aData) armorStats = aData.armorStats.Clone();
        else if (_data is AccessoryData accData) accessoryStats = accData.accessoryStats.Clone();
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
    public void SetEquipmentStats(EquipmentStats _stats)
    {
        if (_stats is WeaponStats wStats)
        {
            weaponStats = wStats;
        }
        else if (_stats is ArmorStats aStats)
        {
            armorStats = aStats;
        }
        else if (_stats is AccessoryStats accStats)
        {
            accessoryStats = accStats;
        }
    }

}

