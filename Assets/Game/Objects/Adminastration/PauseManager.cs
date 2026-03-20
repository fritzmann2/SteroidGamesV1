using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class PauseManager : MonoBehaviour
{
    [Header("UI Referenz")]
    public GameObject settingsUI;
    public GameObject pauseMenuUI;
    private GameControls controls;
    public GameObject WhilePlayingObj;
    public GameObject InventoryObj;
    private PlayerSaveHandler playerSaveHandler;
    private bool escapepressed = false;

    private bool isMenuOpen = false;
    private GameObject firstSelectedSlot;
    public Button firstButton;


    
    void Awake()
    {
        controls = new GameControls();
        controls.Enable();
        ResetAllUI();     
    }

    void Update()
    {
        if (controls.Gameplay.escape.IsPressed() && !escapepressed)
        {
            ToggleMenu();
            escapepressed = true;
        }
        if (!controls.Gameplay.escape.IsPressed() && escapepressed)
        {
            escapepressed = false;
        }
        if (controls.Gameplay.OpenInventory.WasPressedThisFrame())
        {
            if (InventoryObj.activeSelf)
            {
                InventoryObj.SetActive(false);
                WhilePlayingObj.SetActive(true);
            }
            else
            {
                InventoryObj.SetActive(true); 
                WhilePlayingObj.SetActive(false);
            }
        }
    }
    void OnEnable()
    {
        SetFirstSelectedSlot();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    private void SetFirstSelectedSlot()
    {
        if (firstSelectedSlot == null)
        {
            if (firstButton != null)
            {
                firstSelectedSlot = firstButton.gameObject;
            }
        }

        if (firstSelectedSlot != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedSlot);
        }
    }

    private void ToggleMenu()
    {
        Debug.Log("Toggle Pause Menu");
        if (isMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            if (WhilePlayingObj.activeInHierarchy)
            {
                OpenMenu();
            }
            else
            {
                ResetAllUI();
            }
        }
    }

    private void ResetAllUI()
    {
        pauseMenuUI.SetActive(false);
        settingsUI.SetActive(false);
        InventoryObj.SetActive(false);
        WhilePlayingObj.SetActive(true);   
    }

    public void OpenMenu()
    {
        settingsUI.SetActive(false);
        WhilePlayingObj.SetActive(false);
        pauseMenuUI.SetActive(true);
        isMenuOpen = true;
        SetFirstSelectedSlot();
    }
    public void CloseMenu()
    {
        isMenuOpen = false;
        settingsUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        WhilePlayingObj.SetActive(true);
    }
    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        WhilePlayingObj.SetActive(false);
        settingsUI.SetActive(true);
    }

    public void RegisterPlayerSaveHandler(PlayerSaveHandler _playerSaveHandler)
    {
        playerSaveHandler = _playerSaveHandler;
    }
    public void QuitGame()
    {
        playerSaveHandler.RequestLogoutAndSave();
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
    }
}