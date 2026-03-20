using UnityEngine;
using UnityEngine.EventSystems;

public class Mainmenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject MainMenuUI;
    public GameObject SettingsMenuUI;
    public GameObject NetworkUIGO;

    [Header("First Selected Buttons")]
    public GameObject firstMainMenuButton;
    public GameObject firstNetworkMenuButton;

    public void Start()
    {
        MainMenuUI.SetActive(true);
        SettingsMenuUI.SetActive(false);
        NetworkUIGO.SetActive(false);

        SetFirstSelected(firstMainMenuButton);
    }

    public void OpenHelpMenu()
    {
        MainMenuUI.SetActive(false);
        SettingsMenuUI.SetActive(true);
    }
    
    public void CloseHelpMenu()
    {
        SettingsMenuUI.SetActive(false);
        MainMenuUI.SetActive(true);

        SetFirstSelected(firstMainMenuButton);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
    
    public void OpenNetworkMenu()
    {
        MainMenuUI.SetActive(false);
        NetworkUIGO.SetActive(true);

        SetFirstSelected(firstNetworkMenuButton);
    }

    private void SetFirstSelected(GameObject firstButton)
    {
        if (EventSystem.current != null && firstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
        else
        {
            Debug.LogWarning("EventSystem fehlt oder 'First Button' wurde im Inspector nicht zugewiesen!");
        }
    }
}