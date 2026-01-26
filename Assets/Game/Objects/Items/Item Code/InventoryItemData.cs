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



[System.Serializable]
public class ArmorStats : INetworkSerializable
{
    public float defense;
    public float spellresistance;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref defense);
        serializer.SerializeValue(ref spellresistance);
    }
}
[System.Serializable]
public class AccessoryStats : INetworkSerializable
{
    public float health;
    public float mana;
    public float healthRegen;
    public float manaRegen;
    public float movementSpeed;
    public float attackSpeed;
    public float critChance;
    public float critDamage;
    public float strength;
    public float defense;
    public float spellresistance;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref health);
        serializer.SerializeValue(ref mana);
        serializer.SerializeValue(ref healthRegen);
        serializer.SerializeValue(ref manaRegen);
        serializer.SerializeValue(ref movementSpeed);
        serializer.SerializeValue(ref attackSpeed);
        serializer.SerializeValue(ref critChance);
        serializer.SerializeValue(ref critDamage);
        serializer.SerializeValue(ref strength);
        serializer.SerializeValue(ref defense);
        serializer.SerializeValue(ref spellresistance);
    }
}
