using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot_UI : MonoBehaviour
{
    [SerializeField] public Image itemSprite;
    [SerializeField] public TextMeshProUGUI itemCount;
    private InventoryItemInstance inventoryItemInstance;
    public int slotnum;
    private int itemcount;
    private Button button;

    private void Awake()
    {
        ClearSlot();

        button = GetComponent<Button>();
        button?.onClick.AddListener(OnUISlotClick);
    }
    public void OnUISlotClick()
    {
//        Debug.Log("UI Slot clicked");
        MouseItemData mouseItemData = FindAnyObjectByType<MouseItemData>();
        mouseItemData.clickedOnInventorySlot(this, slotnum);
    }
    public virtual bool Init(InventorySlot _inventorySlot, int _slotnum)
    {
        if (_inventorySlot.IsEmpty)
        {
            ClearSlot();
        }
        inventoryItemInstance = _inventorySlot.InventoryItemInstance;
        if (inventoryItemInstance != null)
        {
            UpdateUISlot(_inventorySlot);
        }
        slotnum = _slotnum;
        return true;
    }
    public void UpdateUISlot(InventorySlot slot)
    {
        if (slot.IsEmpty || slot.InventoryItemInstance.itemData == null)
        {
            ClearSlot();
            return;
        }
        Debug.Log("Updating UI slot" + slot.StackSize + " slotnum: " + slot.slotnum + " slotnum:"+ slotnum + " " + slot.InventoryItemInstance.itemData.ItemName);
        itemSprite.sprite = slot.InventoryItemInstance.itemData.Icon;
        if (itemSprite.sprite == null)
        {
            Debug.Log("sprite changed to null");
        }
        itemSprite.color = Color.white; 
        if (slot.StackSize > 1)
            itemCount.text = slot.StackSize.ToString();
        else
            itemCount.text = "";
        itemCount.text = slot.StackSize.ToString();
        
    }   
    public void ClearSlot() 
    {
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }
    public void setSlotNum(int _slotnum)
    {
        slotnum = _slotnum;
    }
}
