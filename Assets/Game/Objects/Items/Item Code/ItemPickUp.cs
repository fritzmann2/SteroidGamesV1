using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System.Collections; // WICHTIG: Wird für die Coroutine benötigt!

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ItemPickUp : NetworkBehaviour
{
    [Header("Item Settings")]
    public int itemRarity = 0;
    public int amount = 1;
    public string id;
    private string tempid;
    
    public string serializedStats = "";
    public bool isEquipment = false;

    [Header("Despawn Settings")]
    [Tooltip("Zeit in Sekunden, bis das Item automatisch despawnt")]
    public float autoDespawnTime = 20f;

    private BoxCollider2D bx;
    private SpriteRenderer sr; 

    public NetworkVariable<FixedString64Bytes> netItemID = new NetworkVariable<FixedString64Bytes>(
        "", 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public void Awake()
    {
        bx = GetComponent<BoxCollider2D>();
        bx.isTrigger = true;
        
        sr = GetComponent<SpriteRenderer>();
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            UpdateSpriteForClient(netItemID.Value.ToString());
        }
        netItemID.OnValueChanged += (oldVal, newVal) => 
        {
            UpdateSpriteForClient(newVal.ToString());
        };
        if (IsServer)
        {
            StartCoroutine(AutoDespawnRoutine());
        }
    }

    private IEnumerator AutoDespawnRoutine()
    {
        yield return new WaitForSeconds(autoDespawnTime);
        if (IsSpawned && NetworkObject != null)
        {
            NetworkObject.Despawn(true); 
        }
    }

    private void UpdateSpriteForClient(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;
        Inventory anyInventory = FindAnyObjectByType<Inventory>();
        if (anyInventory != null)
        {
            ItemData baseItemData = anyInventory.getItemByID(itemID);
            if (baseItemData != null && sr != null)
            {
                sr.sprite = baseItemData.Icon;
                sr.color = Color.white;
            }
        }
    }

    public void setitem(int _itemRarity, int _amount, string _id, int playerLevel = 1)
    {
        itemRarity  = _itemRarity;
        amount = _amount;
        id = _id;

        Inventory anyInventory = FindAnyObjectByType<Inventory>();
        if (anyInventory == null) 
        {
            Debug.Log("anyInventory is null");
            return;
        }
        
        ItemData baseItemData;
        if (string.IsNullOrEmpty(id)) 
        {
            baseItemData = anyInventory.getRandomID();
            id = baseItemData.ID;
            tempid = null;
        }
        else
        {
            baseItemData = anyInventory.getItemByID(id);
        }
        if (baseItemData == null)
        {
            Debug.Log("baseItemData is null");
            return;
        }
        if (sr != null) 
        {
            sr.sprite = baseItemData.Icon; 
            sr.color = Color.white;
        }
        if (IsServer)
        {
            netItemID.Value = new FixedString64Bytes(id);
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
        tempid = "not null";
        Inventory anyInventory = FindAnyObjectByType<Inventory>();
        if (anyInventory == null) return;
        ItemData baseItemData = anyInventory.getItemByID(_itemID);
        if (baseItemData == null) return;
        if (sr != null) 
        {
            sr.sprite = baseItemData.Icon; 
            sr.color = Color.white;
        }
        amount = _amount;
        isEquipment = _isEquipment;
        serializedStats = _serializedStats;
        id = _itemID;
        if (IsServer)
        {
            netItemID.Value = new FixedString64Bytes(id);
        }
    }

    public ItemPickUpData getItemData(int playerLevel)
    {
        if (tempid == null)
        {
            Inventory anyInventory = FindAnyObjectByType<Inventory>();
            ItemData baseItemData = anyInventory.getItemByID(id);
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