using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;

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
        if (_itemInstance is EquipmentInstance equipmentInstance)
            {
                EquipmentStats stats = equipmentInstance.GetEquipmentStats();
            }


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

    public bool switchItem(int index1, int index2, bool eqfirst, bool eqsecond)
    {
        InventorySlot slot1 = eqfirst ? equipmentSlots[index1] : inventorySlots[index1];
        InventorySlot slot2 = eqsecond ? equipmentSlots[index2] : inventorySlots[index2];

        if (eqsecond && !slot1.IsEmpty && !((EquipmentSlot)slot2).CanEquip(slot1.InventoryItemInstance))
        {
            Debug.LogWarning("Tausch abgebrochen: Item 1 passt nicht in Equipment-Slot 2!");
            return false;
        }

        if (eqfirst && !slot2.IsEmpty && !((EquipmentSlot)slot1).CanEquip(slot2.InventoryItemInstance))
        {
            Debug.LogWarning("Tausch abgebrochen: Item 2 passt nicht in Equipment-Slot 1!");
            return false;
        }

        if (!slot1.IsEmpty && !slot2.IsEmpty && slot1.InventoryItemInstance.itemData?.ID == slot2.InventoryItemInstance.itemData?.ID)
        {
            int amountToAdd;
            int amount = slot1.StackSize;
            slot2.RoomLeftInStack(amount, out amountToAdd);
            if (amountToAdd <= 0)
            {
                swap2slots(slot1, slot2);
            }
            else
            {
                slot2.addToStack(amountToAdd);
                slot1.removeFromStack(amountToAdd);
            }
            
        }
        else
        {
            swap2slots(slot1, slot2);
        }

        if (eqfirst || eqsecond)
        {
            OnEquipmentChanged?.Invoke();
        }

        return true;
    }

    private void swap2slots(InventorySlot slot1, InventorySlot slot2)
    {
        InventoryItemInstance temp = slot1.InventoryItemInstance;
            int stacksize = slot1.StackSize;
            
            slot1.UpdateInventorySlot(slot2.InventoryItemInstance, slot2.StackSize);
            slot2.UpdateInventorySlot(temp, stacksize);
    }

    public void SortInventory()
    {
        List<(InventoryItemInstance item, int amount)> itemsToSort = new List<(InventoryItemInstance, int)>();

        foreach (var slot in inventorySlots)
        {
            if (!slot.IsEmpty && slot.InventoryItemInstance != null && slot.InventoryItemInstance.itemData != null)
            {
                itemsToSort.Add((slot.InventoryItemInstance, slot.StackSize));
            }
        }
        itemsToSort.Sort((a, b) =>
        {
            var itemA = a.item;
            var itemB = b.item;

            bool isAEquip = itemA is EquipmentInstance;
            bool isBEquip = itemB is EquipmentInstance;

            if (isAEquip && !isBEquip) return -1; 
            if (!isAEquip && isBEquip) return 1;  

            int nameComparison = string.Compare(itemA.itemData.ID, itemB.itemData.ID, StringComparison.OrdinalIgnoreCase);
            if (nameComparison != 0) return nameComparison;

            if (isAEquip && isBEquip)
            {
                EquipmentStats statsA = ((EquipmentInstance)itemA).GetEquipmentStats();
                EquipmentStats statsB = ((EquipmentInstance)itemB).GetEquipmentStats();

                int valA = statsA != null ? statsA.compleatValue : 0;
                int valB = statsB != null ? statsB.compleatValue : 0;

                return valB.CompareTo(valA);
            }

            return b.amount.CompareTo(a.amount);
        });

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < itemsToSort.Count)
            {
                inventorySlots[i].UpdateInventorySlot(itemsToSort[i].item, itemsToSort[i].amount);
            }
            else
            {
                inventorySlots[i].UpdateInventorySlot(null, 0);
            }
        }
    }
    public void TriggerEquipmentChanged()
    {
        OnEquipmentChanged?.Invoke();
    }
}
