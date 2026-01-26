using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;

public class PauseManager : MonoBehaviour
{
    [Header("UI Referenz")]
    public GameObject settingsUI;
    public GameObject pauseMenuUI;
    private GameControls controls;
    private GameObject WhilePlayingObj;
    private GameObject InventoryObj;
    private bool escapepressed = false;

    private bool isMenuOpen = false;

    
    void Awake()
    {
        InventoryObj = GameObject.FindGameObjectWithTag("Inventory");
        WhilePlayingObj = GameObject.FindGameObjectWithTag("WhilePlaying");
        controls = new GameControls();
        pauseMenuUI.SetActive(false);
        settingsUI.SetActive(false);
        InventoryObj.SetActive(false);
        WhilePlayingObj.SetActive(true);        
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
//            Debug.Log("Try open inventory");
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
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
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
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        settingsUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        isMenuOpen = true;
    }
    public void CloseMenu()
    {
        isMenuOpen = false;
        settingsUI.SetActive(false);
        pauseMenuUI.SetActive(false);
    }
    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsUI.SetActive(true);
    }
    public void QuitGame()
    {
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
    }
}