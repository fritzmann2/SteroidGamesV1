using UnityEngine;

public class ZombyScript : BaseEnemy
{
    public override void Reset()
    {
        id = "Zomby";
        maxHealth = 300;
        damage = 20f;
        movementSpeed = 6f;
        base.Reset();
    }

    public override void Awake()
    {
        base.Awake();
    }
    override public void Attack()
    {
        if (!IsServer) return;
        targetPlayer.GetComponent<BaseEntety>().TakeDamage(damage, false);
    }
}
