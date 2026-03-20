using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; 
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems; 

public class ControlsUI : MonoBehaviour
{
    [Header("UI Setup")]
    public RebindUIItem rebindPrefab; 
    public Transform contentContainer;  
    public TMP_Dropdown deviceDropdown; 

    [Header("Menu Navigation (Von links nach rechts)")]
    public Selectable[] topButtons; 
    public Selectable[] bottomButtons;

    [Header("Input System Setup")]
    public string keyboardSchemeName = "KeyboardMouse"; 
    public string gamepadSchemeName = "Gamepad";

    [Header("Welche Tasten sollen ins Menü?")]
    public InputActionReference[] actionsToRebind; 

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation; 
    private InputAction currentRebindAction;
    private TextMeshProUGUI currentRebindText;
    private List<RebindUIItem> generatedUIItems = new List<RebindUIItem>();
    
    private string currentScheme; 

    private void Start()
    {
        string savedRebinds = PlayerPrefs.GetString("CustomControls", string.Empty);
        if (!string.IsNullOrEmpty(savedRebinds) && actionsToRebind.Length > 0)
        {
            actionsToRebind[0].action.actionMap.LoadBindingOverridesFromJson(savedRebinds);
        }

        currentScheme = keyboardSchemeName; 
        if (deviceDropdown != null)
        {
            deviceDropdown.onValueChanged.AddListener(OnDeviceDropdownChanged);
        }

        GenerateUI();
    }

    private void GenerateUI()
    {
        foreach (InputActionReference actionRef in actionsToRebind)
        {
            if (actionRef == null) continue;
            
            RebindUIItem uiItem = Instantiate(rebindPrefab, contentContainer);
            uiItem.actionNameLabel.text = actionRef.action.name; 
            uiItem.rebindButton1.onClick.AddListener(() => StartRebinding(actionRef.action, uiItem.bindButtonText1, 0));
            uiItem.rebindButton2.onClick.AddListener(() => StartRebinding(actionRef.action, uiItem.bindButtonText2, 1));
            generatedUIItems.Add(uiItem);
        }

        UpdateUI(); 
    }

    private void OnDeviceDropdownChanged(int index)
    {
        currentScheme = (index == 1) ? gamepadSchemeName : keyboardSchemeName;
        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < actionsToRebind.Length; i++)
        {
            if (actionsToRebind[i] != null && i < generatedUIItems.Count)
            {
                var action = actionsToRebind[i].action;
                var uiItem = generatedUIItems[i];
                List<int> validIndices = GetBindingIndicesForCurrentScheme(action);
                
                if (validIndices.Count > 0)
                {
                    uiItem.rebindButton1.gameObject.SetActive(true);
                    uiItem.bindButtonText1.text = action.GetBindingDisplayString(validIndices[0]);
                }
                else
                {
                    uiItem.rebindButton1.gameObject.SetActive(false);
                }

                if (validIndices.Count > 1)
                {
                    uiItem.rebindButton2.gameObject.SetActive(true);
                    uiItem.bindButtonText2.text = action.GetBindingDisplayString(validIndices[1]);
                }
                else
                {
                    uiItem.rebindButton2.gameObject.SetActive(false);
                }
            }
        }

        if (generatedUIItems.Count > 0 && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(generatedUIItems[0].rebindButton1.gameObject);
        }
        
        SetupControllerNavigation();
    }

    private List<int> GetBindingIndicesForCurrentScheme(InputAction action)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].groups.Contains(currentScheme) && !action.bindings[i].isComposite)
            {
                indices.Add(i);
            }
        }
        return indices; 
    }

    private void StartRebinding(InputAction actionToRebind, TextMeshProUGUI buttonText, int slotIndex)
    {
        if (rebindingOperation != null) return; 

        List<int> validIndices = GetBindingIndicesForCurrentScheme(actionToRebind);
        if (slotIndex >= validIndices.Count) return; 
        int actualBindingIndex = validIndices[slotIndex];
        currentRebindAction = actionToRebind;
        currentRebindText = buttonText;
        currentRebindText.text = "Warten...";
        currentRebindAction.Disable();
        rebindingOperation = currentRebindAction.PerformInteractiveRebinding(actualBindingIndex)
            .OnMatchWaitForAnother(0.1f) 
            .OnComplete(operation => RebindComplete(actualBindingIndex));
        
        if (currentScheme == gamepadSchemeName)
        {
            rebindingOperation.WithControlsExcluding("<Keyboard>");
            rebindingOperation.WithControlsExcluding("<Mouse>");
        }
        else
        {
            rebindingOperation.WithControlsExcluding("<Gamepad>");
        }

        rebindingOperation.Start();
    }

    private void RebindComplete(int bindingIndex)
    {
        rebindingOperation.Dispose();
        rebindingOperation = null;
        currentRebindAction.Enable();
        string rebinds = currentRebindAction.actionMap.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("CustomControls", rebinds);
        PlayerPrefs.Save();
        currentRebindText.text = currentRebindAction.GetBindingDisplayString(bindingIndex);
    }

    public void ResetToDefault()
    {
        if (rebindingOperation != null)
        {
            rebindingOperation.Cancel();
            rebindingOperation.Dispose();
            rebindingOperation = null;
            if (currentRebindAction != null) currentRebindAction.Enable();
        }

        if (actionsToRebind.Length > 0 && actionsToRebind[0] != null)
        {
            actionsToRebind[0].action.actionMap.RemoveAllBindingOverrides();
        }
        PlayerPrefs.DeleteKey("CustomControls");
        PlayerPrefs.Save();

        UpdateUI();
    }

    private void SetupControllerNavigation()
    {
        if (generatedUIItems.Count == 0) return;

        for (int i = 0; i < topButtons.Length; i++)
        {
            if (topButtons[i] == null) continue;
            Navigation nav = topButtons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i > 0) nav.selectOnLeft = topButtons[i - 1];
            if (i < topButtons.Length - 1) nav.selectOnRight = topButtons[i + 1];
            
            if (deviceDropdown != null)
                nav.selectOnDown = deviceDropdown;
            else
                nav.selectOnDown = generatedUIItems[0].rebindButton1;
                
            topButtons[i].navigation = nav;
        }

        if (deviceDropdown != null)
        {
            Navigation dropNav = deviceDropdown.navigation;
            dropNav.mode = Navigation.Mode.Explicit; 
            dropNav.selectOnDown = generatedUIItems[0].rebindButton1;
            
            if (topButtons.Length > 0 && topButtons[0] != null)
            {
                dropNav.selectOnUp = topButtons[0];
            }
            
            deviceDropdown.navigation = dropNav;
        }

        for (int i = 0; i < generatedUIItems.Count; i++)
        {
            Button btn1 = generatedUIItems[i].rebindButton1;
            Button btn2 = generatedUIItems[i].rebindButton2;

            Navigation nav1 = btn1.navigation;
            Navigation nav2 = btn2.navigation;

            nav1.mode = Navigation.Mode.Explicit;
            nav2.mode = Navigation.Mode.Explicit;

            if (i == 0)
            {
                nav1.selectOnUp = deviceDropdown;
                nav2.selectOnUp = deviceDropdown;
            }
            else
            {
                nav1.selectOnUp = generatedUIItems[i - 1].rebindButton1;
                if (generatedUIItems[i - 1].rebindButton2.gameObject.activeSelf)
                    nav2.selectOnUp = generatedUIItems[i - 1].rebindButton2;
                else
                    nav2.selectOnUp = generatedUIItems[i - 1].rebindButton1; 
            }

            if (i < generatedUIItems.Count - 1)
            {
                nav1.selectOnDown = generatedUIItems[i + 1].rebindButton1;
                if (generatedUIItems[i + 1].rebindButton2.gameObject.activeSelf)
                    nav2.selectOnDown = generatedUIItems[i + 1].rebindButton2;
                else
                    nav2.selectOnDown = generatedUIItems[i + 1].rebindButton1;
            }
            else
            {
                Selectable bottomTarget = (bottomButtons.Length > 0) ? bottomButtons[0] : null;
                nav1.selectOnDown = bottomTarget;
                nav2.selectOnDown = bottomTarget;
            }

            if (btn2.gameObject.activeSelf)
            {
                nav1.selectOnRight = btn2;
                nav2.selectOnLeft = btn1;
            }
            else
            {
                nav1.selectOnRight = null;
            }

            btn1.navigation = nav1;
            btn2.navigation = nav2;
        }

        Button lastItemBtn = generatedUIItems[generatedUIItems.Count - 1].rebindButton1;

        for (int i = 0; i < bottomButtons.Length; i++)
        {
            if (bottomButtons[i] == null) continue;
            Navigation nav = bottomButtons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i > 0) nav.selectOnLeft = bottomButtons[i - 1];
            if (i < bottomButtons.Length - 1) nav.selectOnRight = bottomButtons[i + 1];
            nav.selectOnUp = lastItemBtn; 
            bottomButtons[i].navigation = nav;
        }
    }
}