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
    public int compleatValue;
    protected int bonuspoint = 0;
    public virtual void generateStats(float rarity)
    {
        if (rarity == 3.5f)
        {
            bonuspoint = 1;
        }
    }
    protected int calcRandomNum()
    {
        int randomnum = UnityEngine.Random.Range(1, 5) + bonuspoint;
        compleatValue += randomnum;
        return randomnum;
    }
    
}


[System.Serializable]
public class WeaponStats : EquipmentStats, INetworkSerializable
{
    public float weapondamage;
    public float strength;
    public float critChance;
    public float critDamage;
    public float attackSpeed;
    private int multiplier = 5;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref weapondamage);
        serializer.SerializeValue(ref strength);
        serializer.SerializeValue(ref critChance);
        serializer.SerializeValue(ref critDamage);
        serializer.SerializeValue(ref attackSpeed);
    }
    public override void generateStats(float rarity)
    {
        base.generateStats(rarity);
        compleatValue = 0;   
        weapondamage = (calcRandomNum() + 5) * rarity * multiplier;
        strength = (calcRandomNum() + 5) * rarity;
        critChance = calcRandomNum() * rarity;
        critDamage = calcRandomNum() * 5 * rarity;
        attackSpeed = calcRandomNum() * 0.1f * rarity;
    }
    public WeaponStats Clone()
    {
        return (WeaponStats)this.MemberwiseClone();
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

    public override void generateStats(float rarity)
    {
        base.generateStats(rarity);
        compleatValue = 0;
        defense = (calcRandomNum() + 5) * rarity;
        spellresistance = (calcRandomNum() + 5) * rarity;
    }
    public ArmorStats Clone()
    {
        return (ArmorStats)this.MemberwiseClone();
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
    public float defence;
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
        serializer.SerializeValue(ref defence);
        serializer.SerializeValue(ref spellresistance);
    }

    public override void generateStats(float rarity)
    {
        base.generateStats(rarity);
        compleatValue = 0;

        health = (calcRandomNum() + 5) * rarity;
        mana = (calcRandomNum() + 5) * rarity;
        healthRegen = (calcRandomNum() * 0.1f + 0.5f) * rarity;
        manaRegen = (calcRandomNum() * 0.1f + 0.5f) * rarity;
        movementSpeed = (calcRandomNum() * 0.1f + 0.5f) * rarity;
        attackSpeed = (calcRandomNum() * 0.1f + 0.5f) * rarity;
        critChance = calcRandomNum() * rarity;
        critDamage = calcRandomNum() * 5 * rarity;
        strength = (calcRandomNum() + 5) * rarity;
        defence = (calcRandomNum() + 5) * rarity;
        spellresistance = (calcRandomNum() + 5) * rarity;
    }
    public AccessoryStats Clone()
    {
        return (AccessoryStats)this.MemberwiseClone();
    }
}