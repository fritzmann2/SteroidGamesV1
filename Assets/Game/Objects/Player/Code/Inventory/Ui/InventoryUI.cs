using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    public Inventory playerinventory;
    public List<InventorySlot_UI> inventorySlot_UI;
    public List<EquipmentSlot_UI> equipmentSlot_UI;
    private Dictionary<InventorySlot_UI, InventorySlot> slotDictionary; 
    private bool hasInitializedOnce = false;

    [Header("Multiplayer Info")]
    public TextMeshProUGUI joinCodeText;
    private GameObject firstSelectedSlot;
    private PlayerSaveHandler saveHandler;
    private GameControls controls;

    [Header("Stats UI")]
    public GameObject statsPanel;
    public TextMeshProUGUI statsContentText;

    
    void Update()
    {
        if (controls.Gameplay.Teleport.triggered) 
        {
            if (!statsPanel.activeSelf)
            {
                ShowItemStats();
            }
            else
            {
                statsPanel.SetActive(false);
            }
        }
        if (statsPanel.activeSelf)
        {
            ShowItemStats();
        }
    }
    
    private void OnEnable()
    {
        controls = new GameControls();
        controls.Enable();
        StartCoroutine(DelayedRefresh());
        SetFirstSelectedSlot();
    }
    private void OnDisable()
    {
        controls.Disable();
    }

    private void ShowItemStats()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;

        InventorySlot_UI slotUI = selected.GetComponent<InventorySlot_UI>();
        if (slotUI == null) return;

        if (slotDictionary.TryGetValue(slotUI, out InventorySlot dataSlot))
        {
            InventoryItemInstance item = dataSlot.inventoryItemInstance;

            if (item is EquipmentInstance eqitem)
            {
                statsPanel.SetActive(true);
                UpdateStatDisplay(eqitem);
            }
            else
            {
                statsPanel.SetActive(false); 
            }
        }
    }

    private void UpdateStatDisplay(EquipmentInstance eqInstance)
    {
        EquipmentStats stats = eqInstance.GetEquipmentStats();
        string displayString = $"<size=120%><color=yellow>{eqInstance.itemData.name}</color></size>\n\n";

        if (stats is WeaponStats w)
        {
            displayString += $"Angriff: {w.weapondamage:F0}\n";
            displayString += $"Stärke: {w.strength:F0}\n";
            displayString += $"Crit Chance: {w.critChance:F1}%\n";
            displayString += $"Crit Schaden: {w.critDamage:F0}%\n";
            displayString += $"Attack Speed: {w.attackSpeed:F2}";
        }
        else if (stats is ArmorStats a)
        {
            displayString += $"Verteidigung: {a.defense:F0}\n";
            displayString += $"Magieresistenz: {a.spellresistance:F0}";
        }
        else if (stats is AccessoryStats acc)
        {
            displayString += $"Leben: {acc.health:F0} | Mana: {acc.mana:F0}\n";
            displayString += $"Regeneration: L:{acc.healthRegen:F1} M:{acc.manaRegen:F1}\n";
            displayString += $"Bewegung: {acc.movementSpeed:F1}\n";
            displayString += $"Stärke: {acc.strength:F0} | Def: {acc.defence:F0}";
        }

        statsContentText.text = displayString;
    }

    private void SetFirstSelectedSlot()
    {
        if (firstSelectedSlot == null)
        {
            if (inventorySlot_UI != null && inventorySlot_UI.Count > 0)
            {
                firstSelectedSlot = inventorySlot_UI[0].gameObject;
            }
        }

        if (firstSelectedSlot != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedSlot);
        }
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

        saveHandler = playerinventory.GetComponent<PlayerSaveHandler>();
        if (saveHandler != null)
        {
            saveHandler.dataLoaded -= RefreshAllSlots; 
            
            saveHandler.dataLoaded += RefreshAllSlots;
        }
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

    public void RequestPlayerReset()
    {
        if (playerinventory != null)
        {
            saveHandler.RequestPlayerReset();
        }
    }

    public void DropItem()
    {
        playerinventory.dropItem();
    }
}
