using UnityEngine;
using UnityEngine.EventSystems; 

public class SettingsMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject audioPanel;
    public GameObject controlsPanel;

    [Header("Controller Setup")]
    public GameObject firstSelectedOption;

    private void OnEnable()
    {
        if (EventSystem.current != null && firstSelectedOption != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedOption);
        }
        
        ShowAudioPanel();
    }

    public void ShowAudioPanel()
    {
        audioPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowControlsPanel()
    {
        audioPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }
}