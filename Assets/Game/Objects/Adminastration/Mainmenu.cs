using UnityEngine;

public class Mainmenu : MonoBehaviour
{
    public GameObject MainMenuUI;
    public GameObject HelpMenuUI;
    public GameObject NetworkUIGO;
    
    public void Start()
    {
        MainMenuUI.SetActive(true);
        HelpMenuUI.SetActive(false);
        NetworkUIGO.SetActive(false);
    }

    public void OpenHelpMenu()
    {
        MainMenuUI.SetActive(false);
        HelpMenuUI.SetActive(true);
    }
    public void CloseHelpMenu()
    {
        HelpMenuUI.SetActive(false);
        MainMenuUI.SetActive(true);
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
    }
}
