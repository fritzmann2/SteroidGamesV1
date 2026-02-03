using System.Collections.Generic;
using UnityEngine;

public class EquipmentSlot_UI : InventorySlot_UI
{
    public int slotIndex;
    [SerializeField] public List<EquipmentType> allowedEqTypes;
    private PlayerStats playerStats;

    public bool Init(EquipmentSlot slot, int _slotnum)
    {
        foreach (var eqtype in allowedEqTypes)
        {
            if (eqtype == slot.EquipInstance.itemData.equipmentType)
            {
                int index = slot.StackSize;
                base.Init(slot, _slotnum); 
                trigerstatupdate();
                return true;
            }
        }
        
        Debug.Log("Wrong item type for this equipment slot");
        InventorySlot emptySlot = new InventorySlot(); 
        base.Init(emptySlot, _slotnum);
        return false;
    }

    private void trigerstatupdate()
    {
        if (playerStats != null)
        {
            playerStats.UpdateStatsFromEquipment(slotIndex);
        }
        else
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
            playerStats.UpdateStatsFromEquipment(slotIndex);
        }
    }
}
