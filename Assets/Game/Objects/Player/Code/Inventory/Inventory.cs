using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;



#if UNITY_EDITOR
using UnityEditor; 
#endif


[System.Serializable]
public class Inventory : NetworkBehaviour
{
    [Header("Datenbank")]
    public List<ItemData> itemDatabase;
    [Header("Inventory Systems")]
    private Purs purs;
    [SerializeField] public ItemInventory itemInventory;
    public UnityEvent<bool> addedsuccess;
    public UnityAction changesuccess;
    private BoxCollider2D bx;
    private Collider2D othercollider;
    public MouseItemData mouseItemData;
    public WorldGenerator worldGenerator;
    
    private void Awake()
    {
        Transform childTransform = transform.Find("ItemPickupRange");
        worldGenerator = FindAnyObjectByType<WorldGenerator>();
        bx = childTransform.GetComponent<BoxCollider2D>();
        bx.isTrigger = true;
        InventoryUI inventoryUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (inventoryUI != null)
        {
            inventoryUI.initplayer(this);
        }
        mouseItemData = FindAnyObjectByType<MouseItemData>();
        mouseItemData.ItemChange += trySwitchItem;
    }
    public override void OnDestroy()
    {
        mouseItemData.ItemChange -= trySwitchItem;
    }

    public bool loadInventory(Purs _purs, ItemInventory _itemInventory)
    {
        if (itemInventory == null)
        {
            itemInventory = _itemInventory;
        }
        if (purs == null)
        {
            purs = _purs;
        }
        return true;
    }


    [ServerRpc]
    public void tryAddItemServerRPC(string ID, int rarity, int amount)
    {
        
        tryAddItemClientRPC(ID,rarity, amount);
    }


    [ClientRpc]
    public void tryAddItemClientRPC(string ID,int rarity, int amount)
    {
        if (IsOwner)
        {
            ItemData itemDataToAdd;
            if (string.IsNullOrEmpty(ID))
            {
                itemDataToAdd = getRandomID();
            }
            else
            {
                itemDataToAdd = getItemByID(ID);
            }
            if (itemDataToAdd == null)
            {
                return;
            }

            bool isAdded = false;

            if (itemDataToAdd is EquipmentData eqData)
            {
                EquipmentInstance itemInstance = new EquipmentInstance(eqData);
                Itemtype itemtype = itemInstance.itemtype;
                EquipmentStats equipmentStats = new EquipmentStats();
                if (itemtype == Itemtype.Weapon)
                {
                    equipmentStats = new WeaponStats();
                }
                else if (itemtype == Itemtype.Armor)
                {
                    equipmentStats = new ArmorStats();
                }
                else if (itemtype == Itemtype.Accessory)
                {
                    equipmentStats = new AccessoryStats();
                }
                itemInstance.SetEquipmentStats(additemstats(equipmentStats, rarity));
                isAdded = itemInventory.addItem(itemInstance, amount);
            }
            else
            {
                InventoryItemInstance itemInstance = new InventoryItemInstance(itemDataToAdd); 
                isAdded = itemInventory.addItem(itemInstance, amount);
            }
        
            if(isAdded)
            {
                if (worldGenerator != null)
                {
                    worldGenerator.DespawnItem(othercollider.gameObject);
                }
                else
                {
                    worldGenerator = FindAnyObjectByType<WorldGenerator>();
                    worldGenerator.DespawnItem(othercollider.gameObject);
                }
            }
        }
    }
    public ItemData getItemByID(string ID)
    {
        for (int i = 0; i < itemDatabase.Count; i++)
        {
            if (ID == itemDatabase[i].ID)
            {
                return itemDatabase[i];
            }
        }
        return null;
    }
    private ItemData getRandomID()
    {
        ItemData itemDataToReturn = itemDatabase[(int)Random.Range(0, itemDatabase.Count)];
        return itemDataToReturn;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (bx.IsTouching(other) && other.CompareTag("ItemPickUp"))
        {
            othercollider = other;
            ItemPickUp itemPickUp = other.GetComponent<ItemPickUp>();
            ItemPickUpData itemPickUpData = itemPickUp.getItemData();
            tryAddItemServerRPC(itemPickUpData.id, itemPickUpData.itemRarity, itemPickUpData.amount);
        }
    }

    private void trySwitchItem(int slot1, int slot2, bool eqfirst, bool eqsecond)
    {
        if (IsOwner)
        {
            bool success = itemInventory.switchItem(slot1, slot2, eqfirst, eqsecond);
            if (success)
            {
                changesuccess.Invoke();
            }
        }
    }
    
    private EquipmentStats additemstats(EquipmentStats _equipmentStats, int rarity)
    {
        PlayerStats playerStats = GetComponent<PlayerStats>();
        float adjustedRarity = rarity + playerStats.getLevel() * 0.1f;
        Debug.Log("Adjusted Rarity is: " + adjustedRarity);
        _equipmentStats.generateStats(adjustedRarity);        
        return _equipmentStats;
    }
        
    





    #if UNITY_EDITOR
        [ContextMenu("Auto Fill Database")]
        public void AutoFillDatabase()
        {
            int i = 0;
            itemDatabase = new List<ItemData>();
            
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                if (obj is ItemData item)
                {
                    if (!itemDatabase.Contains(item))
                    {
                        itemDatabase.Add(item);
                        i++;
                    }
                }
            }
            
            Debug.Log("Added " + i + " items to database.");
            EditorUtility.SetDirty(this);
        }
    #endif
}