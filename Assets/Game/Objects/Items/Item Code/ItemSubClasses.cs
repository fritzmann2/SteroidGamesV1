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
    Equipment,
    None

}


[System.Serializable]
public class WeaponStats : INetworkSerializable
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