using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class ItemInventory
{
    public List<InventorySlot> inventorySlots;
    public List<EquipmentSlot> equipmentSlots;
    public event Action OnEquipmentChanged;

    public ItemInventory()
    {
        inventorySlots = new List<InventorySlot>();
        equipmentSlots = new List<EquipmentSlot>();
        int i = 0;
        foreach (var slot in inventorySlots)
        {
            slot.slotnum = i;
            slot.clearSlot();
            i++;
        }
        i = 0;
        foreach (var slot in equipmentSlots)
        {
            slot.slotnum = i;
            slot.clearSlot();
            i++;
        }
    }
    public InventorySlot getInventorySlot(int index)
    {
        return inventorySlots[index];
    }
    public EquipmentSlot getEquipmentSlot(int index)
    {
        return equipmentSlots[index];
    }
    public bool addItem(InventoryItemInstance _itemInstance, int amount)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].IsEmpty) continue;
            if (inventorySlots[i].InventoryItemInstance.itemData == null) continue;

            if (inventorySlots[i].InventoryItemInstance.itemData.ID == _itemInstance.itemData.ID)
            {
                int amountToAdd;
                bool fitsCompletely = inventorySlots[i].RoomLeftInStack(amount, out amountToAdd);

                if (amountToAdd > 0)
                {
                    inventorySlots[i].addToStack(amountToAdd);
                    amount -= amountToAdd;
                }

                if (amount <= 0)
                {
                    return true;
                }
            }
        }

        while (amount > 0)
        {
            int emptySlotIndex = -1;

            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].IsEmpty)
                {
                    emptySlotIndex = i;
                    break;
                }
            }

            if (emptySlotIndex == -1)
            {
                Debug.Log("Inventory Full - Rest amount lost: " + amount);
                return false; 
            }

            int maxStack = _itemInstance.itemData.MaxStackSize;
            int amountForThisSlot = Mathf.Min(amount, maxStack);

            inventorySlots[emptySlotIndex].UpdateInventorySlot(_itemInstance, amountForThisSlot);
            
            amount -= amountForThisSlot;
        }

        return true; 
    }
    public bool removeItem()
    {
        return true;
    }

    public bool switchItem(int index1,int index2, bool eqfirst, bool eqsecond)
    {
//        Debug.Log("try switch item " + eqfirst + " " + eqsecond);
        InventorySlot slot1;
        if (eqfirst) slot1 = equipmentSlots[index1];
        else         slot1 = inventorySlots[index1];

        InventorySlot slot2;
        if (eqsecond) slot2 = equipmentSlots[index2];
        else          slot2 = inventorySlots[index2];


        if (!slot1.IsEmpty && !slot2.IsEmpty && slot1.InventoryItemInstance.itemData?.ID == slot2.InventoryItemInstance.itemData?.ID)
        {
            int amountToAdd;
            int amount = slot1.StackSize;
            slot2.RoomLeftInStack(amount, out amountToAdd);
            slot2.addToStack(amountToAdd);
            slot1.removeFromStack(amountToAdd);
        }
        else
        {
            InventoryItemInstance temp = slot1.InventoryItemInstance;
            int stacksize = slot1.StackSize;
            slot1.UpdateInventorySlot(slot2.InventoryItemInstance, slot2.StackSize);
            slot2.UpdateInventorySlot(temp, stacksize);
        }
        if (eqfirst || eqsecond)
        {
            OnEquipmentChanged.Invoke();
        }
        Debug.Log("switched slot:" + index1 + " und slot:" + index2);
        return true;
    }

}
