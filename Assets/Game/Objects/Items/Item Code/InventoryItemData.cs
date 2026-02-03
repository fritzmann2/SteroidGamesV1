using UnityEngine;
using Unity.Netcode;


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string ID;
    public string ItemName;
    public Sprite Icon;
    public Itemtype Type;
    public GameObject itemObject;

    [SerializeField] private int _maxStackSize = 64;

    public virtual int MaxStackSize => _maxStackSize; 
}

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
