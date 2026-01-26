using UnityEngine;

public class Scythe : Weapon
{
    override public void Attack1()
    {
        attackmulti = 1f;
        performattack(AttackTypeScythe.Slash.ToString());
    }
    override public void Attack2()
    {
        attackmulti = 0.9f;
        performattack(AttackTypeScythe.Round.ToString());
    }
    override public void Attack3()
    {
        attackmulti = 1.5f;
        performattack(AttackTypeScythe.Charge.ToString());
    }
    override public void Attack4()
    {
        attackmulti = 0.7f;
        performattack(AttackTypeScythe.Throw.ToString());
    }

    protected override void Awake()
    {
        type = EquipmentType.Scythe;
        base.Awake();
    }
}

public enum AttackTypeScythe
{
    Round,   
    Slash,
    Charge,
    Throw
}