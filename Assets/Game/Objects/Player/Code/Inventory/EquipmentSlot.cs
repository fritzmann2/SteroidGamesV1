using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

[System.Serializable]
public class EquipmentSlot : InventorySlot
{
    public List<EquipmentType> allowedType;
    public EquipmentInstance EquipInstance => inventoryItemInstance as EquipmentInstance;
    public event Action<int> OnEquipmentChanged;

    public EquipmentSlot()
    {
        isequipment = true;
    }

    public bool CanEquip(InventoryItemInstance itemInstance)
    {
        if (itemInstance == null) 
        {
            Debug.LogWarning("itemInstance is null");
            return true; 
        }
        
        if (itemInstance is EquipmentInstance eqInstance && eqInstance.itemData is EquipmentData data)
        {
//            Debug.Log("item Type: " + data.equipmentType.ToString() + " allowed Type: " + allowedType.ToString());
            bool isAllowed = false;
            foreach (EquipmentType eqtype in allowedType)
            {
                if (data.equipmentType == eqtype)
                {
                    isAllowed = true;
                    break;  
                }
            }
            return isAllowed;
        }
        Debug.LogWarning("No EquipmentInstance found");
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