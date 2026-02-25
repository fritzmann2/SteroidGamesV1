using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot_UI : MonoBehaviour
{
    [SerializeField] public Image itemSprite;
    [SerializeField] public TextMeshProUGUI itemCount;
    [SerializeField] private InventoryItemInstance inventoryItemInstance;
    public int slotnum;
    private Button button;
    private int itemRarity = 0;

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
    
    public void UpdateUISlot(InventorySlot slot)
    {
        if (slot.IsEmpty || slot.InventoryItemInstance.itemData == null)
        {
            ClearSlot();
            return;
        }
        inventoryItemInstance = slot.InventoryItemInstance;

        itemSprite.sprite = inventoryItemInstance.itemData.Icon;

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
        checkRarity();
    }   
    public void ClearSlot() 
    {
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
        itemRarity = 0;
        GetComponent<Image>().color = Color.lightPink;
    }
    public void setSlotNum(int _slotnum)
    {
        slotnum = _slotnum;
    }

    private void checkRarity()
    {
        if (inventoryItemInstance is EquipmentInstance equipmentInstance)
        {
            itemRarity = equipmentInstance.GetEquipmentStats().compleatValue;
            if (itemRarity > 0 && itemRarity <= 10)
            {
                GetComponent<Image>().color = Color.grey;
            }
            else if (itemRarity > 10 && itemRarity <= 15)
            {
                GetComponent<Image>().color = Color.lightBlue;
            }
            else if (itemRarity > 15 && itemRarity <= 20)
            {
                GetComponent<Image>().color = Color.yellow;
            }
            else if (itemRarity > 20 && itemRarity <= 25)
            {
                GetComponent<Image>().color = Color.orange;
            }
            else if (itemRarity > 25)
            {
                GetComponent<Image>().color = Color.black;
            }
        }
        else
        {
            GetComponent<Image>().color = Color.grey;
        }
    }
    
}
