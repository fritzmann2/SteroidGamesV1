using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string ID;
    public string ItemName;
    public Sprite Icon;
    public Itemtype Type;
    public GameObject itemObject;

    [SerializeField] private int _maxStackSize = 64;

    public virtual int MaxStackSize => _maxStackSize; 
}



