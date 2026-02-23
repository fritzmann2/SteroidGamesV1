using System;
using UnityEngine;

[System.Serializable]
public class EquipmentSlot : InventorySlot
{
    public EquipmentType allowedType;
    public EquipmentInstance EquipInstance => inventoryItemInstance as EquipmentInstance;
    public event Action<int> OnEquipmentChanged;

    public EquipmentSlot()
    {
        isequipment = true;
    }

    public bool CanEquip(InventoryItemInstance itemInstance)
    {
        if (itemInstance == null) return true; 
        
        if (itemInstance is EquipmentInstance eqInstance && eqInstance.itemData is EquipmentData data)
        {
            return data.equipmentType == allowedType;
        }
        return false;
    }

    public bool TryEquip(InventoryItemInstance itemInstance)
    {
        if (CanEquip(itemInstance))
        {
            UpdateInventorySlot(itemInstance, 1);
            OnEquipmentChanged?.Invoke(slotnum);
            Debug.Log("Equiped Item in slot: " + slotnum);
            return true;
        }
        return false;
    }

    public bool IsValid()
    {
        return inventoryItemInstance is EquipmentInstance;
    }
}