using UnityEngine;

public class dummyscript : BaseEntety
{
    public GameObject hpbarfiller;
    public WorldGenerator worldgen;
    private string id = "Testsubject";

    override public void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        maxHealth = 20;
        health.Value = maxHealth;
    }
    public override void OnHealthChanged(float previousValue, float newValue)
    {
        if (newValue <= 0)
        {
           ItemManager.Instance.SpawnPickUpItem(id, transform.position);
        }
        base.OnHealthChanged(previousValue, newValue);
        if (hpbarfiller != null)
        {
            hpbarfiller.transform.localScale = new Vector3 (newValue / maxHealth, 1f, 1f);
        }
    }
    public void Setparrent(WorldGenerator parrentworldgen)
    {
        this.worldgen = parrentworldgen;
    }
}
