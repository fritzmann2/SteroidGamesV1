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
    public GameObject shopUI;
    public GameObject skillTree;
    private PlayerSaveHandler playerSaveHandler;
    private bool escapepressed = false;

    private GameObject firstSelectedSlot;
    public Button firstButton;

    public static PauseManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        controls = InputManager.Instance.Controls;
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
                ResetAllUI(); 
            }
            else
            {
                CloseAllOverlays();
                InventoryObj.SetActive(true); 
                WhilePlayingObj.SetActive(false);
            }
        }
    }

    void OnEnable()
    {
        SetFirstSelectedSlot();
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

    private void CloseAllOverlays()
    {
        pauseMenuUI.SetActive(false);
        settingsUI.SetActive(false);
        InventoryObj.SetActive(false);
        shopUI.SetActive(false);
        skillTree.SetActive(false);
    }

    private void ResetAllUI()
    {
        CloseAllOverlays();
        WhilePlayingObj.SetActive(true); 
    }

    private void ToggleMenu()
    {
        if (!WhilePlayingObj.activeInHierarchy)
        {
            ResetAllUI();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        CloseAllOverlays();
        pauseMenuUI.SetActive(true);
        WhilePlayingObj.SetActive(false);
        SetFirstSelectedSlot();
    }

    public void CloseMenu()
    {
        ResetAllUI();
    }

    public void OpenSettings()
    {
        CloseAllOverlays();
        settingsUI.SetActive(true);
        WhilePlayingObj.SetActive(false);
    }

    public void OpenShopUI()
    {
        CloseAllOverlays();
        shopUI.SetActive(true);
        WhilePlayingObj.SetActive(false);
    }

    public void OpenSkillTree()
    {
        CloseAllOverlays();
        skillTree.SetActive(true);
        WhilePlayingObj.SetActive(false);
    }

    public void OpenInventoryUI()
    {
        CloseAllOverlays();
        InventoryObj.SetActive(true);
        WhilePlayingObj.SetActive(false);
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