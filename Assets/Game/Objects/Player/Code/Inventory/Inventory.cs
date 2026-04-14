using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
    private GameControls controls;
    private BoxCollider2D bx;
    public MouseItemData mouseItemData;
    public WorldGenerator worldGenerator;
    private PlayerStats playerStats;

    private void Awake()
    {
        
        Transform childTransform = transform.Find("ItemPickupRange");
        bx = childTransform.GetComponent<BoxCollider2D>();
        bx.isTrigger = true;
        worldGenerator = FindAnyObjectByType<WorldGenerator>();
        playerStats = GetComponent<PlayerStats>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        controls = InputManager.Instance.Controls;
        if(!IsOwner) Debug.LogWarning("safety fail");
        InventoryUI inventoryUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (inventoryUI != null)
        {
            inventoryUI.initplayer(this);
        }
        else
        {
            Debug.LogWarning("InventoryUI not found");
        }

        mouseItemData = FindAnyObjectByType<MouseItemData>(FindObjectsInactive.Include);
        if (mouseItemData != null)
        {
            mouseItemData.ItemChange += trySwitchItem;
        }
    }

    public override void OnNetworkDespawn() 
    {      
        if (!IsOwner) return;
        if (mouseItemData != null)
        {
            mouseItemData.ItemChange -= trySwitchItem;
        }
    }
    
    public override void OnDestroy()
    {
        if (mouseItemData != null)
        {
            mouseItemData.ItemChange -= trySwitchItem;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (mouseItemData != null && mouseItemData.hasitem && controls.Gameplay.Teleport.WasPressedThisFrame())
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                dropItem();
            }
        }
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

    public void sortInventory()
    {
        itemInventory.SortInventory();
    }


    [ServerRpc]
    public void tryAddItemServerRPC(NetworkObjectReference itemObjectRef)
    {
        if (itemObjectRef.TryGet(out NetworkObject targetObject))
        {
            ItemPickUp itemPickUp = targetObject.GetComponent<ItemPickUp>();
            
            if (itemPickUp != null)
            {
                ItemPickUpData data = itemPickUp.getItemData(playerStats.playerLevel);
                targetObject.Despawn();
                tryAddItemClientRPC(data.id, data.amount, data.isEquipment, data.serializedStats);
            }
        }
    }

    [ClientRpc]
    public void tryAddItemClientRPC(string ID, int amount, bool isEquipment = false, string serializedStats = "")
    {
        if (IsOwner || IsServer)
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
            
            if (itemDataToAdd == null) return;

            bool isAdded = false;

            if (isEquipment && !string.IsNullOrEmpty(serializedStats))
            {
                EquipmentData equipData = (EquipmentData)itemDataToAdd;
                EquipmentInstance equip = new EquipmentInstance(equipData);
                
                JsonUtility.FromJsonOverwrite(serializedStats, equip);
                equip.itemData = equipData;
                equip.itemtype = equipData.Type;

                isAdded = itemInventory.addItem(equip, amount);
            }
            else
            {
                InventoryItemInstance itemInstance = new InventoryItemInstance(itemDataToAdd); 
                isAdded = itemInventory.addItem(itemInstance, amount);
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
    public ItemData getRandomID()
    {
        ItemData itemDataToReturn = itemDatabase[(int)Random.Range(0, itemDatabase.Count)];
        return itemDataToReturn;
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (!IsOwner) return;
        if (bx.IsTouching(other) && other.CompareTag("ItemPickUp") && controls.Gameplay.pickupitem.IsPressed())
        {
            bool allowedToPickUp = false;
            var currentDevice = controls.Gameplay.pickupitem.activeControl.device;
            if (currentDevice is Mouse)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                if (other.OverlapPoint(mouseWorldPos))
                {
                    allowedToPickUp = true;
                }
            }
            else
            {
                allowedToPickUp = true;
            }


            if (allowedToPickUp)
            {
                NetworkObject itemNetObj = other.GetComponent<NetworkObject>();
                if (itemNetObj != null)
                {
                    tryAddItemServerRPC(itemNetObj);
                }
            }
        }
    }

    private void trySwitchItem(int slot1, int slot2, bool eqfirst, bool eqsecond)
    {
        if (IsOwner)
        {
            bool success = itemInventory.switchItem(slot1, slot2, eqfirst, eqsecond);
            
            if (success)
            {
                changesuccess?.Invoke();
                
                if (!IsServer)
                {
                    SwitchItemServerRpc(slot1, slot2, eqfirst, eqsecond);
                }
            }
        }
    }

    [ServerRpc]
    public void SwitchItemServerRpc(int slot1, int slot2, bool eqfirst, bool eqsecond)
    {
        itemInventory.switchItem(slot1, slot2, eqfirst, eqsecond);
    }
    
    public void dropItem()
    {
        if (mouseItemData.hasitem)
        {
            int index = mouseItemData.indexslot;
            mouseItemData.ClearSlot();
            InventoryItemInstance inventoryItemInstance = itemInventory.inventorySlots[index].InventoryItemInstance;
            int amount = itemInventory.inventorySlots[index].StackSize;
            itemInventory.inventorySlots[index].clearSlot();
            itemInventory.inventorySlots[index].inventoryUI.updateSlot(index, false);
            ItemManager.Instance.dropItem(inventoryItemInstance, transform.position + new Vector3 (1f, 0f, 0f), amount);
        }
    }

    public void dropItemFromSlot(int index, bool isEquipment)
    {
        InventorySlot targetSlot = isEquipment ? itemInventory.equipmentSlots[index] : itemInventory.inventorySlots[index];
        if (targetSlot.IsEmpty || targetSlot.InventoryItemInstance == null) return;
        InventoryItemInstance itemToDrop = targetSlot.InventoryItemInstance;
        int amountToDrop = targetSlot.StackSize;
        targetSlot.clearSlot();
        targetSlot.inventoryUI.updateSlot(index, isEquipment);
        if (isEquipment)
        {
            itemInventory.TriggerEquipmentChanged();
        }
        Vector3 spawnOffset = new Vector3(transform.localScale.x * 1.5f, 0f, 0f); 
        ItemManager.Instance.dropItem(itemToDrop, transform.position + spawnOffset, amountToDrop);
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