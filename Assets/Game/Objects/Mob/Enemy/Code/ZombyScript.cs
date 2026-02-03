using UnityEngine;

public class ZombyScript : BaseEnemy
{
    public override void Awake()
    {
        base.Awake();
        maxHealth = 30;
        health.Value = maxHealth;
        id = "Zomby"; 
    }
}
