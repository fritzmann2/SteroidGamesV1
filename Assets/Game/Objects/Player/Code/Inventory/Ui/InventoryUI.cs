using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Inventory playerinventory;
    public List<InventorySlot_UI> inventorySlot_UI;
    public List<EquipmentSlot_UI> equipmentSlot_UI;
    private Dictionary<InventorySlot_UI, InventorySlot> slotDictionary; 
    private bool hasInitializedOnce = false;
    [Header("Multiplayer Info")]
    public TextMeshProUGUI joinCodeText;
    
    
    
    private void OnEnable()
    {
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        yield return null; 

        if (playerinventory != null && slotDictionary != null)
        {
   
            if (!hasInitializedOnce)
            {
                RefreshAllSlots();
                hasInitializedOnce = true;
            }
            RefreshAllSlots();
        }
    }

    public void RefreshAllSlots()
    {
        if (playerinventory == null || playerinventory.itemInventory == null) return;

        for (int i = 0; i < playerinventory.itemInventory.inventorySlots.Count; i++)
        {
            updateSlot(i, false);
        }
        
        for (int i = 0; i < playerinventory.itemInventory.equipmentSlots.Count; i++)
        {
            updateSlot(i, true);
        }
        
//        Debug.Log("UI Refreshed via OnEnable");
    }

    public void initplayer(Inventory _playerinventory)
    {
        playerinventory = _playerinventory;
        initUI();
        FindAnyObjectByType<MouseItemData>().initMouse(_playerinventory);
    }

    private void initUI()
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();
        if (inventorySlot_UI.Count != playerinventory.itemInventory.inventorySlots.Count)
        {
            Debug.LogWarning("slotcount is not equal to uislot count");
            return;
        }
        if (equipmentSlot_UI.Count != playerinventory.itemInventory.equipmentSlots.Count)
        {
            Debug.LogWarning($"UI Mismatch: Equipment UI Slots ({equipmentSlot_UI.Count}) != Data Slots ({playerinventory.itemInventory.equipmentSlots.Count})");
            return;
        }
        for (int i = 0; i < playerinventory.itemInventory.inventorySlots.Count; i++)
        {
            slotDictionary.Add(inventorySlot_UI[i], playerinventory.itemInventory.getInventorySlot(i));
            playerinventory.itemInventory.getInventorySlot(i).initInventoryUI(this, i);
            inventorySlot_UI[i].setSlotNum(i);
            updateSlot(i, false);

        }
        for (int i = 0; i < playerinventory.itemInventory.equipmentSlots.Count; i++)
        {
            slotDictionary.Add(equipmentSlot_UI[i], playerinventory.itemInventory.getEquipmentSlot(i));
            playerinventory.itemInventory.getEquipmentSlot(i).initInventoryUI(this, i);
            equipmentSlot_UI[i].setSlotNum(i);
            updateSlot(i, true);
        }
        if (RelayManager.Instance != null && !string.IsNullOrEmpty(RelayManager.Instance.CurrentJoinCode))
        {
            UpdateJoinCodeDisplay(RelayManager.Instance.CurrentJoinCode);
        }
        else
        {
            UpdateJoinCodeDisplay("Offline / Kein Code");
        }
    }

    public void updateSlot(int _slotnum, bool isequipment)
    {
        InventorySlot_UI slotUI = isequipment ? equipmentSlot_UI[_slotnum] : inventorySlot_UI[_slotnum];

        if (slotDictionary.TryGetValue(slotUI, out InventorySlot dataSlot))
        {
            slotUI.UpdateUISlot(dataSlot);
        }
        else
        {
            Debug.LogWarning("Kein Daten-Slot für dieses UI-Element im Dictionary gefunden.");
        }
    }
    public void UpdateJoinCodeDisplay(string code)
    {
        if (joinCodeText != null)
        {
            joinCodeText.text = "Join Code: " + code;
        }
        else
        {
            Debug.LogWarning("JoinCodeText ist im Inspector nicht zugewiesen!");
        }
    }

    public void CopyJoinCodeToClipboard()
    {
        if (RelayManager.Instance != null && !string.IsNullOrEmpty(RelayManager.Instance.CurrentJoinCode))
        {
            GUIUtility.systemCopyBuffer = RelayManager.Instance.CurrentJoinCode;
            Debug.Log("Code in die Zwischenablage kopiert: " + RelayManager.Instance.CurrentJoinCode);
        }
        else
        {
            Debug.LogWarning("Kein Code zum Kopieren vorhanden!");
        }
    }
}
