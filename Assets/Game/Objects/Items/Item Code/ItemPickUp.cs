using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class ItemPickUp : NetworkBehaviour
{
    public int itemRarity = 0;
    public int amount = 1;
    public string id;
    private BoxCollider2D bx;

    public void Awake()
    {
        bx = GetComponent<BoxCollider2D>();
        bx.isTrigger = true;
    }
    
    
    public void setitem(int itemRarity, int amount, string id)
    {
        this.itemRarity  = itemRarity;
        this.amount = amount;
        this.id = id;
    }


    public ItemPickUpData getItemData()
    {
        return new ItemPickUpData
        {
            itemRarity = itemRarity,
            amount = amount,
            id = id
        };
    }  
}

public class ItemPickUpData
{
    public int itemRarity;
    public int amount;
    public string id;
}


