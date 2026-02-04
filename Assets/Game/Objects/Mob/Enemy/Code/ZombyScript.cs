using UnityEngine;

public class ZombyScript : BaseEnemy
{
    public override void Reset()
    {
        id = "Zomby";
        maxHealth = 30;
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
