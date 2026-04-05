public class ZombyScript : BaseEnemy
{
    public override void Reset()
    {
        id = "Zomby";
        maxHealth = 300;
        damage = 30f;
        movementSpeed = 4f;
        attackDistance = 1f;
        attackCooldown = 2f;
        base.Reset();
    }

    override public void Attack()
    {
        if (!IsServer) return;
        targetPlayer.GetComponent<BaseEntety>().TakeDamage(damage, false);
    }
}
