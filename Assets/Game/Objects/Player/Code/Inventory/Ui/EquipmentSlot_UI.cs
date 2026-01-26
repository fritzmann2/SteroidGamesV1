using System;
using UnityEngine;

public class EquipmentSlot_UI : InventorySlot_UI
{
    public int slotIndex;
    [SerializeField] public Itemtype equipmentType;

    
    public override bool Init(InventorySlot slot, int _slotnum)
    {
        if (slot.InventoryItemInstance == null || slot.InventoryItemInstance.itemData == null)
        {
            int index = slot.StackSize;
//            Debug.Log("Slot ist leer");
            base.Init(slot, _slotnum); 
            return true;
        }
        if (equipmentType == slot.InventoryItemInstance.itemData.Type)
        {
            
        }
        
        else
        {
            Debug.Log("Wrong item type for this equipment slot");
            InventorySlot emptySlot = new InventorySlot(); 
            base.Init(emptySlot, _slotnum);
        }
        return false;
    }

    private void trigerstatupdate()
    {
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.UpdateStatsFromEquipment(slotIndex);
        }
    }
}
