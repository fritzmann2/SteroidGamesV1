using UnityEngine;
using Unity.Netcode;





public enum EquipmentType
{
    Sword,
    Bow,
    Scythe, 
    Helmet,
    Chestplate,
    Leggings,
    Boots,
    Ring,
    Necklace,
    None
}

public enum Itemtype
{
    Armor,
    Weapon,
    Accessory,
    None

}
[System.Serializable]
public class EquipmentStats
{
    public int count = 1;
}


[System.Serializable]
public class WeaponStats : EquipmentStats, INetworkSerializable
{
    public float weapondamage;
    public float strength;
    public float critChance;
    public float critDamage;
    public float attackSpeed;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref weapondamage);
        serializer.SerializeValue(ref strength);
        serializer.SerializeValue(ref critChance);
        serializer.SerializeValue(ref critDamage);
        serializer.SerializeValue(ref attackSpeed);
    }
}

[System.Serializable]
public class ArmorStats : EquipmentStats, INetworkSerializable
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
public class AccessoryStats : EquipmentStats, INetworkSerializable
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