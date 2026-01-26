using System;
using NUnit.Framework;
using UnityEngine;



[System.Serializable]
public class InventorySlot
{
    [SerializeField] protected InventoryItemInstance inventoryItemInstance;
    [SerializeField] protected int stacksize;
    public InventoryUI inventoryUI;
    public bool IsEmpty = true;
    public InventoryItemInstance InventoryItemInstance => inventoryItemInstance;
    
    public int StackSize => stacksize;

    public int slotnum;
    protected bool isequipment = false;


    public InventorySlot (InventoryItemInstance source, int amount)
    {
        inventoryItemInstance = source;
        stacksize = amount;
        Debug.Log ("Constructor wird aufgerufen" + amount);
    }
    public InventorySlot ()
    {
        clearSlot();
    }

    public void clearSlot()
    {
//        Debug.Log("clearing slot");
        inventoryItemInstance = null;
        stacksize = -1;
        IsEmpty = true;
    }
    public void initInventoryUI(InventoryUI _inventoryUI, int _slotnum)
    {
        Debug.Log("Intialize inventoryUI");
        inventoryUI = _inventoryUI;
        slotnum = _slotnum;
    }

    public virtual void UpdateInventorySlot(InventoryItemInstance data, int amount)
    {
        inventoryItemInstance = data;
        
        if (inventoryItemInstance == null || inventoryItemInstance.itemData == null)
        {
            IsEmpty = true;
        }
        else
        {
            IsEmpty = false;
        }
        if (IsEmpty) 
        {
            Debug.Log("slot updated to empty");
            clearSlot();
        }
        
        stacksize = amount;
        if (inventoryUI == null)
        {
            Debug.Log ("inventoryUI is null");
        }
        inventoryUI.updateSlot(slotnum, isequipment);
    }
    public bool RoomLeftInStack(int ammountToAdd, out int ammountRemaining)
    {
        if (inventoryItemInstance.itemData == null)
        {
            ammountRemaining = 0;
            return false;
        }
        ammountRemaining = inventoryItemInstance.itemData.MaxStackSize - stacksize;
        return RoomLeftInStack(ammountToAdd);
    }
    public bool RoomLeftInStack(int ammountToAdd)
    {
        if (stacksize + ammountToAdd <= inventoryItemInstance.itemData.MaxStackSize) return true;
        else return false;
    }
    public void addToStack(int amount)
    {
        stacksize += amount;
        inventoryUI.updateSlot(slotnum, isequipment);
    }
    public void removeFromStack(int amount)
    {
        stacksize -= amount;
        if (stacksize <= 0) clearSlot();
        inventoryUI.updateSlot(slotnum, isequipment);
    }
    public ItemSaveData GetSaveData()
    {
        ItemSaveData itemSaveData = new ItemSaveData();
        itemSaveData.itemtype = inventoryItemInstance.itemData.Type;
        return itemSaveData;
    }
}