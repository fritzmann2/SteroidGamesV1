using System.Collections.Generic;
using UnityEngine;

public class EquipmentSlot_UI : InventorySlot_UI
{
    public int slotIndex;
    [SerializeField] public List<EquipmentType> allowedEqTypes;

}
