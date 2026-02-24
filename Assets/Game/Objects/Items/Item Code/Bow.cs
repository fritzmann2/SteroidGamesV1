using UnityEngine;

public class Bow : Weapon
{
    override public void Attack1()
    {
        performattack(AttackTypeBow.normal_shot.ToString());
    }
    override public void Attack2()
    {
        performattack(AttackTypeBow.bow_uppercut.ToString());
    }
    override public void Attack3()
    {
        performattack(AttackTypeBow.Charge.ToString());
    }

    protected override void Awake()
    {
        type = EquipmentType.Bow;
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }
}

public enum AttackTypeBow
{
    normal_shot,
    bow_uppercut,
    Charge
}