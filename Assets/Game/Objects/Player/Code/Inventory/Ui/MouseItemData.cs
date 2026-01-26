using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.InputSystem;

public class MouseItemData : MonoBehaviour
{
    public Image ItemSprite;
    public TextMeshProUGUI itemCount;
    public InventorySlot_UI inventorySlot;
    public Inventory inventory;
    private int indexslot;
    private bool hasitem = false;
    private bool eqfirst = false;
    private bool eqsecond = false;
    private bool invoke = false;
    public event UnityAction<int, int, bool, bool> ItemChange;


    void Awake() 
    {
        ItemSprite.color = Color.clear;
        itemCount.text = null;
    }
    public void initMouse(Inventory _inventory)
    {
        inventory = _inventory;
        inventory.changesuccess += ClearSlot;
    }
    void OnDestroy()
    {
        inventory.changesuccess -= ClearSlot;
    }

    public void clickedOnInventorySlot(InventorySlot_UI clickedSlot, int index)
    {
        if (hasitem)
        {
            invoke = true;
        }
        if (clickedSlot is EquipmentSlot_UI && hasitem)
        {
            eqsecond = true;
        }
        else if (!(clickedSlot is EquipmentSlot_UI) && hasitem)
        {
            eqsecond = false;
        }
        else if (clickedSlot is EquipmentSlot_UI && !hasitem)
        {
            Setitem(clickedSlot, index);
            eqfirst = true;
            hasitem = true;
        }
        else if (!(clickedSlot is EquipmentSlot_UI) && !hasitem)
        {
            Setitem(clickedSlot, index);
            eqfirst = false;
            hasitem = true;
        }
        if (invoke)
        {
//            Debug.Log("Invoking with indexes:" + indexslot + " and " + index);
            ItemChange?.Invoke(indexslot, index, eqfirst, eqsecond);
            return;
        }
    }


    private void ClearSlot()
    {
        eqfirst = false;
        eqsecond = false;
        invoke = false;
        inventorySlot = null;
        ItemSprite.sprite = null;
        ItemSprite.color = Color.clear;
        itemCount.text = null;
        hasitem = false;
    }

    private void Setitem(InventorySlot_UI clickedSlot, int index)
    {
        inventorySlot = clickedSlot;
        indexslot = index;
//        Debug.Log("set index to:" + indexslot);
        ItemSprite.sprite = inventorySlot.itemSprite.sprite;
        itemCount.text = inventorySlot.itemCount.text;
        ItemSprite.color = Color.white;
        hasitem = true;
    }
    
    void Update()
    {
        if (inventorySlot != null)
        {
            followmouse();
        }
    }

    private void followmouse()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            mousePos, 
            null, 
            out pos);
        
        transform.localPosition = pos;
    }
}
