using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
    public GameObject PickUpItem;
    public static ItemManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnPickUpItem(string id, Vector3 spawnPosition)
    {
        GameObject pickupitem = Instantiate(PickUpItem, spawnPosition, Quaternion.identity);
        pickupitem.name = id;
        ItemPickUp itemPickUp = pickupitem.GetComponent<ItemPickUp>();
        int itemRarity = 1;
        if (id == "WizardBoss")
        {
            itemRarity = 2;
        }
        if (itemPickUp != null) itemPickUp.setitem(itemRarity, 1, null);
        
        var netObj = pickupitem.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }
    public void SpawnPickUpItem(InventoryItemInstance _item, Vector3 _spawnPosition, int _amount, string id)
    {
        GameObject pickupitem = Instantiate(PickUpItem, _spawnPosition, Quaternion.identity);
        pickupitem.name = id;
        ItemPickUp itemPickUp = pickupitem.GetComponent<ItemPickUp>();
        int itemRarity = 1;
        if (id == "WizardBoss")
        {
            itemRarity = 2;
        }
        if (itemPickUp != null) itemPickUp.setitem(itemRarity, 1, null);
        
        var netObj = pickupitem.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }


    public void dropItem(InventoryItemInstance _item, Vector3 _spawnPosition, int _amount)
    {
        string itemID = _item.itemData.ID; 
        bool isEquipment = _item is EquipmentInstance;
        string serializedStats = "";

        if (isEquipment)
        {
            serializedStats = JsonUtility.ToJson((EquipmentInstance)_item);
        }
        if (IsServer)
        {
            SpawnDropItemOnServer(itemID, isEquipment, serializedStats, _spawnPosition, _amount);
        }
        else
        {
            RequestDropItemServerRpc(itemID, isEquipment, serializedStats, _spawnPosition, _amount);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestDropItemServerRpc(string itemID, bool isEquipment, string serializedStats, Vector3 spawnPosition, int amount)
    {
        SpawnDropItemOnServer(itemID, isEquipment, serializedStats, spawnPosition, amount);
    }

    private void SpawnDropItemOnServer(string itemID, bool isEquipment, string serializedStats, Vector3 _spawnPosition, int _amount)
    {
        ItemData baseItemData = GetItemDataByID(itemID); 
        if (baseItemData == null)
        {
            Debug.LogError($"ItemData für ID {itemID} nicht gefunden!");
            return;
        }

        GameObject dropItemObj = Instantiate(PickUpItem, _spawnPosition, Quaternion.identity); 
        ItemPickUp pickUpComp = dropItemObj.GetComponent<ItemPickUp>();
        
        if (pickUpComp != null)
        {
            pickUpComp.setitem(itemID, _amount, isEquipment, serializedStats);
            
            pickUpComp.itemRarity = 1; 
        }
        
        var netObj = dropItemObj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    } 

    private ItemData GetItemDataByID(string id)
    {
        Inventory playerInventory = FindAnyObjectByType<Inventory>();
        
        if (playerInventory != null)
        {
            return playerInventory.getItemByID(id);
        }
        return null; 
    }
}
