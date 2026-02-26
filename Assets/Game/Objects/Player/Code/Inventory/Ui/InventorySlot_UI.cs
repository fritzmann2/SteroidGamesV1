using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot_UI : MonoBehaviour
{
    [SerializeField] public Image itemSprite;
    [SerializeField] public Image raritySprite;

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
        if (raritySprite != null)
        {
            raritySprite.sprite = null;
            raritySprite.color = Color.clear;
        }
        else
        {
            Debug.LogWarning($"Achtung: Auf Slot {slotnum} fehlt das RaritySprite im Inspector!");
        }
        itemCount.text = "";
        itemRarity = 0;
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
                raritySprite.color = Color.grey;
            }
            else if (itemRarity > 10 && itemRarity <= 15)
            {
                raritySprite.color = Color.lightBlue;
            }
            else if (itemRarity > 15 && itemRarity <= 20)
            {
                raritySprite.color = Color.yellow;
            }
            else if (itemRarity > 20 && itemRarity <= 25)
            {
                raritySprite.color = Color.orange;
            }
            else if (itemRarity > 25)
            {
                raritySprite.color = Color.black;
            }
        }
        else
        {
            raritySprite.color = Color.grey;
        }
    }
    
}
