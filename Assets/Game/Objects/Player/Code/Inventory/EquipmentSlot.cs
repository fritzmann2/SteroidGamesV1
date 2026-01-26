[System.Serializable]
public class EquipmentSlot : InventorySlot
{
    public EquipmentType allowedType;

    public EquipmentInstance EquipInstance => inventoryItemInstance as EquipmentInstance;


    public EquipmentSlot()
    {
        isequipment = true;
    }

    public bool TryEquip(InventoryItemInstance itemInstance)
    {
        if (itemInstance is EquipmentInstance eqInstance)
        {
            EquipmentData data = eqInstance.itemData as EquipmentData;

            if (data != null && data.equipmentType == allowedType)
            {
                UpdateInventorySlot(itemInstance, 1);
                return true;
            }
        }

        return false;
    }

    public bool IsValid()
    {
        return inventoryItemInstance is EquipmentInstance;
    }
}