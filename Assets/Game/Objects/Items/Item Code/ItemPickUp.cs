using Unity.Netcode;
using UnityEngine;

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
    
    
    public void setitem(int _itemRarity, int _amount, string _id)
    {
        itemRarity  = _itemRarity;
        amount = _amount;
        id = _id;
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


