using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class VirtualKeyboard : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField targetInputField;

    [Header("Generator Settings")]
    public GameObject keyboardButtonPrefab;
    public Transform keyboardGridParent;
    
    [Tooltip("Gib hier den Buchstaben oder die Zahl ein, auf der das Keyboard starten soll (z.B. G)")]
    public string startCharacter = "Q";

    [Header("Manuelle Tasten (Zuweisung im Inspector)")]
    public Button backspaceButton;
    public Button enterButton;   
    public Button spaceButton;  

    private string[] layoutRows = new string[] 
    {
        "1234567890", 
        "QWERTZUIOP", 
        "ASDFGHJKLY",
        "XCVBNM"       
    };

    private List<List<Button>> keyboardGrid = new List<List<Button>>();
    
    private GameObject initialSelectedButton;
    private GameObject firstGeneratedButton; 

    private void Awake()
    {
        GenerateKeys();
    }

    private void Update()
    {
        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            Backspace();
        }

        if (gameObject.activeSelf && EventSystem.current != null && spaceButton != null)
        {
            GameObject currentGO = EventSystem.current.currentSelectedGameObject;
            
            if (currentGO != null && currentGO != spaceButton.gameObject && currentGO.transform.IsChildOf(this.transform))
            {
                Button currentBtn = currentGO.GetComponent<Button>();
                if (currentBtn != null)
                {
                    Navigation spaceNav = spaceButton.navigation;
                    spaceNav.selectOnUp = currentBtn;
                    spaceButton.navigation = spaceNav;
                }
            }
        }
    }

    private void GenerateKeys()
    {
        keyboardGrid.Clear();
        firstGeneratedButton = null;
        initialSelectedButton = null;

        if (backspaceButton != null)
        {
            backspaceButton.onClick.RemoveAllListeners();
            backspaceButton.onClick.AddListener(() => Backspace());
        }
        
        if (enterButton != null)
        {
            enterButton.onClick.RemoveAllListeners();
            enterButton.onClick.AddListener(() => CloseKeyboard());
        }

        if (spaceButton != null)
        {
            spaceButton.onClick.RemoveAllListeners();
            spaceButton.onClick.AddListener(() => TypeCharacter(" "));
        }

        for (int r = 0; r < layoutRows.Length; r++)
        {
            List<Button> currentRow = new List<Button>();
            string charsInRow = layoutRows[r];

            for (int c = 0; c < charsInRow.Length; c++)
            {
                char character = charsInRow[c];

                GameObject newButtonGO = Instantiate(keyboardButtonPrefab, keyboardGridParent);
                newButtonGO.GetComponentInChildren<TextMeshProUGUI>().text = character.ToString();
                
                Button btn = newButtonGO.GetComponent<Button>();
                string charToType = character.ToString();
                btn.onClick.AddListener(() => TypeCharacter(charToType));
                currentRow.Add(btn);
                if (firstGeneratedButton == null) firstGeneratedButton = newButtonGO;
                if (charToType.Equals(startCharacter, System.StringComparison.OrdinalIgnoreCase))
                {
                    initialSelectedButton = newButtonGO;
                }
            }

            if (r == 0 && backspaceButton != null) currentRow.Add(backspaceButton);
            if (r == 1 && enterButton != null) currentRow.Add(enterButton);

            keyboardGrid.Add(currentRow);
        }

        if (spaceButton != null)
        {
            List<Button> spaceRow = new List<Button>();
            spaceRow.Add(spaceButton);
            keyboardGrid.Add(spaceRow);
        }

        if (initialSelectedButton == null)
        {
            initialSelectedButton = firstGeneratedButton;
        }

        SetupSmartNavigation();
    }

    private void SetupSmartNavigation()
    {
        for (int r = 0; r < keyboardGrid.Count; r++)
        {
            List<Button> row = keyboardGrid[r];
            if (row.Count == 0) continue;

            for (int c = 0; c < row.Count; c++)
            {
                Button btn = row[c];
                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.Explicit; 

                nav.selectOnLeft = row[(c - 1 + row.Count) % row.Count];
                nav.selectOnRight = row[(c + 1) % row.Count];

                int upRowIndex = (r - 1 + keyboardGrid.Count) % keyboardGrid.Count;
                int downRowIndex = (r + 1) % keyboardGrid.Count;

                List<Button> upRow = keyboardGrid[upRowIndex];
                List<Button> downRow = keyboardGrid[downRowIndex];

                nav.selectOnUp = upRow[Mathf.Min(c, upRow.Count - 1)];
                nav.selectOnDown = downRow[Mathf.Min(c, downRow.Count - 1)];

                btn.navigation = nav;
            }
        }

        if (keyboardGrid.Count > 2)
        {
            Button btnY = keyboardGrid[2][keyboardGrid[2].Count - 1]; 
            if (enterButton != null && btnY != null)
            {
                Navigation navY = btnY.navigation;
                navY.selectOnRight = enterButton; 
                btnY.navigation = navY;
            }

            if (enterButton != null)
            {
                Navigation navEnter = enterButton.navigation;
                if (backspaceButton != null) navEnter.selectOnUp = backspaceButton; 
                if (spaceButton != null) navEnter.selectOnDown = spaceButton;    
                enterButton.navigation = navEnter;
            }
            
            if (backspaceButton != null && enterButton != null)
            {
                Navigation navBS = backspaceButton.navigation;
                navBS.selectOnDown = enterButton; 
                backspaceButton.navigation = navBS;
            }
        }
    }

    public void OpenKeyboard()
    {
        gameObject.SetActive(true);
        if (EventSystem.current != null && initialSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(initialSelectedButton);
        }
    }

    public void TypeCharacter(string character)
    {
        if (targetInputField != null && targetInputField.text.Length < 15)
            targetInputField.text += character;
    }

    public void Backspace()
    {
        if (targetInputField != null && targetInputField.text.Length > 0)
            targetInputField.text = targetInputField.text.Substring(0, targetInputField.text.Length - 1);
    }

    public void CloseKeyboard()
    {
        if (EventSystem.current != null && targetInputField != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(targetInputField.gameObject);
        }
        gameObject.SetActive(false);
    }
}