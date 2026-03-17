using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ItemPickUp : NetworkBehaviour
{
    public int itemRarity = 0;
    public int amount = 1;
    public string id;
    
    public string serializedStats = "";
    public bool isEquipment = false;

    private BoxCollider2D bx;
    private SpriteRenderer sr; 

    public void Awake()
    {
        bx = GetComponent<BoxCollider2D>();
        bx.isTrigger = true;
        
        sr = GetComponent<SpriteRenderer>();
    }
    
    public void setitem(int _itemRarity, int _amount, string _id, int playerLevel = 1)
    {
        itemRarity  = _itemRarity;
        amount = _amount;
        id = _id;

        Inventory anyInventory = FindAnyObjectByType<Inventory>();
        if (anyInventory == null) return;

        ItemData baseItemData = anyInventory.getItemByID(id);
        if (baseItemData == null) return;

        if (sr != null) 
        {
            sr.sprite = baseItemData.Icon; 
        }

        if (baseItemData is EquipmentData eqData)
        {
            isEquipment = true;
            EquipmentInstance itemInstance = new EquipmentInstance(eqData);
            Itemtype itemtype = itemInstance.itemtype;
            EquipmentStats stats = null;
            
            if (itemtype == Itemtype.Weapon) stats = new WeaponStats();
            else if (itemtype == Itemtype.Armor) stats = new ArmorStats();
            else if (itemtype == Itemtype.Accessory) stats = new AccessoryStats();
            
            float adjustedRarity = itemRarity + (playerLevel * 0.1f);
            if (adjustedRarity > 2.5f) adjustedRarity = 2.5f;
            stats.generateStats(adjustedRarity);
            
            itemInstance.SetEquipmentStats(stats);
            serializedStats = JsonUtility.ToJson(itemInstance);
        }
    }

    public void setitem(string _itemID, int _amount, bool _isEquipment, string _serializedStats)
    {
        Inventory anyInventory = FindAnyObjectByType<Inventory>();
        if (anyInventory == null) return;

        ItemData baseItemData = anyInventory.getItemByID(_itemID);

        if (baseItemData == null) return;

        if (sr != null) 
        {
            sr.sprite = baseItemData.Icon; 
        }
        amount = _amount;
        isEquipment = _isEquipment;
        serializedStats = _serializedStats;
    }

    public ItemPickUpData getItemData()
    {
        return new ItemPickUpData
        {
            itemRarity = itemRarity,
            amount = amount,
            id = id,
            serializedStats = serializedStats, 
            isEquipment = isEquipment        
        };
    }  
}

public class ItemPickUpData
{
    public int itemRarity;
    public int amount;
    public string id;
    public string serializedStats;
    public bool isEquipment;
}