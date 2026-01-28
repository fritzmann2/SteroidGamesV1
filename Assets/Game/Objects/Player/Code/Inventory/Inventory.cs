using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;





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
    
    private void Awake()
    {
        Transform childTransform = transform.Find("ItemPickupRange");
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
            if (ID == null)
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
            InventoryItemInstance itemInstance = new InventoryItemInstance(itemDataToAdd);

            bool isAdded = itemInventory.addItem(itemInstance, amount);
//            Debug.Log("try to add clientRPC");
        
            if(isAdded)
            {
                othercollider.GetComponent<NetworkObject>().Despawn();
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
        return itemDatabase[(int)Random.Range(0, itemDatabase.Count)];
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
    
        
    





#if UNITY_EDITOR
    [ContextMenu("Auto Fill Database")]
    public void AutoFillDatabase()
    {
        int i = 0;
        itemDatabase = new List<ItemData>();
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if(item != null) itemDatabase.Add(item);
            i++;
        }
        Debug.Log("Added " + i + " items to database.");
        EditorUtility.SetDirty(this);
    }
    #endif
}
