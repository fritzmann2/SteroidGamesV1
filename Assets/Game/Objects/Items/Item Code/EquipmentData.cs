using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Equipment")]
public class EquipmentData : ItemData
{
    public EquipmentType equipmentType;
    public override int MaxStackSize => 1;
}

public class WeaponData : EquipmentData
{
    public WeaponStats weaponStats;
}

public class ArmorData : EquipmentData
{
    public ArmorStats armorStats;
}

public class AccessoryData : EquipmentData
{
    public AccessoryStats accessoryStats;
}