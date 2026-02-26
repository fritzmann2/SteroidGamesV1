using UnityEngine;
using UnityEngine.EventSystems; // WICHTIG: Das brauchen wir für die Controller-Navigation!

public class Mainmenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject MainMenuUI;
    public GameObject HelpMenuUI;
    public GameObject NetworkUIGO;

    [Header("First Selected Buttons")]
    // Hier ziehst du im Inspector jeweils den obersten Button des jeweiligen Menüs rein
    public GameObject firstMainMenuButton;
    public GameObject firstHelpMenuButton;
    public GameObject firstNetworkMenuButton;

    public void Start()
    {
        MainMenuUI.SetActive(true);
        HelpMenuUI.SetActive(false);
        NetworkUIGO.SetActive(false);

        SetFirstSelected(firstMainMenuButton);
    }

    public void OpenHelpMenu()
    {
        MainMenuUI.SetActive(false);
        HelpMenuUI.SetActive(true);

        SetFirstSelected(firstHelpMenuButton);
    }
    
    public void CloseHelpMenu()
    {
        HelpMenuUI.SetActive(false);
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