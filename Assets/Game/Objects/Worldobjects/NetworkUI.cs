using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.EventSystems; 

public class NetworkUI : MonoBehaviour
{
    [Header("Hauptmenü Buttons")]
    public Button hostMenuBtn;
    public Button clientMenuBtn; 
    public Button backBtn; 

    [Header("Lobby Elemente")]
    public GameObject lobbyPanel;  
    public GameObject hostArea;   
    public GameObject clientArea;   
    public TMP_InputField playernameInput; 

    [Header("Host Controls")]
    public TextMeshProUGUI joinCodeDisplay;
    public Button copyCodeBtn;
    public Button startGameBtn;

    [Header("Client Controls")]
    public TMP_InputField joinCodeInput;
    public Button submitCodeBtn; 
    public TextMeshProUGUI statusText;

    [Header("Referenzen")]
    public GameObject MainMenuUI;
    public GameObject NetworkUIGO;
    public string firstlevel = "FirstLevel";

    [Header("Controller Navigation")]
    public GameObject firstNetworkMenuButton; 
    public GameObject firstClientButton; 

    void Start()
    {
        lobbyPanel.SetActive(false);
        hostArea.SetActive(false);
        clientArea.SetActive(false);

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playernameInput.text = PlayerPrefs.GetString("PlayerName");
        }

        hostMenuBtn.onClick.AddListener(async () => 
        {
            
            
            lobbyPanel.SetActive(true);
            hostArea.SetActive(true);
            clientArea.SetActive(false);
            
            hostMenuBtn.gameObject.SetActive(false);
            clientMenuBtn.gameObject.SetActive(false);

            SetFirstSelected(backBtn.gameObject);

            string code = await RelayManager.Instance.CreateRelay();
            
            if (code != null)
            {
                joinCodeDisplay.text = code;
                startGameBtn.gameObject.SetActive(true);
                copyCodeBtn.gameObject.SetActive(true);

                SetFirstSelected(startGameBtn.gameObject);
            }
        });

        if(statusText != null) statusText.text = "";

        clientMenuBtn.onClick.AddListener(() => {
            lobbyPanel.SetActive(true);
            hostArea.SetActive(false);
            clientArea.SetActive(true); 

            hostMenuBtn.gameObject.SetActive(false);
            clientMenuBtn.gameObject.SetActive(false);

            SetFirstSelected(firstClientButton); 
        });

        submitCodeBtn.onClick.AddListener(async () => {

            string pName = playernameInput.text;
            if (string.IsNullOrEmpty(pName)) pName = "Spieler" + Random.Range(1000, 9999);
            PlayerPrefs.SetString("PlayerName", pName); 
            
            string code = joinCodeInput.text.Trim();

            if (!string.IsNullOrEmpty(code))
            {
                if(statusText != null) 
                {
                    statusText.gameObject.SetActive(true);
                    statusText.text = "Verbinde zu Relay...";
                    statusText.color = Color.yellow; 
                }

                submitCodeBtn.interactable = false;

                bool success = await RelayManager.Instance.JoinRelay(code);

                if (success)
                {
                    if(statusText != null)
                    {
                        statusText.text = "Erfolg! Warte auf Host...";
                        statusText.color = Color.green; 
                    }
                    SetFirstSelected(backBtn.gameObject);
                }
                else
                {
                    if(statusText != null)
                    {
                        statusText.text = "Fehler: Code ungültig oder Timeout.";
                        statusText.color = Color.red; 
                    }
                    submitCodeBtn.interactable = true;
                    SetFirstSelected(submitCodeBtn.gameObject);
                }
            }
            else
            {
                 if(statusText != null) statusText.text = "Bitte Code eingeben!";
            }
        });

        startGameBtn.onClick.AddListener(() => {
            string pName = playernameInput.text;
            if (string.IsNullOrEmpty(pName)) pName = "Spieler" + Random.Range(1000, 9999);
            PlayerPrefs.SetString("PlayerName", pName);
            NetworkManager.Singleton.SceneManager.LoadScene(firstlevel, UnityEngine.SceneManagement.LoadSceneMode.Single);
        });

        copyCodeBtn.onClick.AddListener(() => {
            GUIUtility.systemCopyBuffer = joinCodeDisplay.text;
            copyCodeBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Kopiert!";
        });

        backBtn.onClick.AddListener(() => {
            lobbyPanel.SetActive(false);
            NetworkUIGO.SetActive(false);
            MainMenuUI.SetActive(true);
            
            hostMenuBtn.gameObject.SetActive(true);
            clientMenuBtn.gameObject.SetActive(true);
            
            SetFirstSelected(firstNetworkMenuButton);

            if(NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
        });
    }

    private void SetFirstSelected(GameObject firstButton)
    {
        if (EventSystem.current != null && firstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
    }
}