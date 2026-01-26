using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [Header("Hauptmenü Buttons")]
    public Button hostMenuBtn;   // Der Knopf "Host Game" im Hauptmenü
    public Button clientMenuBtn; // Der Knopf "Client Game" im Hauptmenü
    public Button backBtn;       // Zurück zum Hauptmenü

    [Header("Lobby Elemente")]
    public GameObject lobbyPanel;     // Das ganze Lobby-Fenster
    public GameObject hostArea;       // Bereich für den Host (Code + Start)
    public GameObject clientArea;     // Bereich für den Client (Input + Join)
    public TMP_InputField playernameInput; // Input Feld für Spielernamen

    [Header("Host Controls")]
    public TextMeshProUGUI joinCodeDisplay;
    public Button copyCodeBtn;
    public Button startGameBtn;

    [Header("Client Controls")]
    public TMP_InputField joinCodeInput;
    public Button submitCodeBtn; 
    public TextMeshProUGUI statusText;

    [Header("Referenzen")]
    public GameObject MainMenuUI; // Dein altes Hauptmenü Panel
    public GameObject NetworkUIGO; // Dieses UI hier
    public string firstlevel = "FirstLevel";

    void Start()
    {
        // Grundeinstellungen: Alles verstecken, was nicht sofort da sein soll
        lobbyPanel.SetActive(false);
        hostArea.SetActive(false);
        clientArea.SetActive(false);

        // --- HOST FLOW ---
        hostMenuBtn.onClick.AddListener(async () => 
        {
            // UI Umschalten
            lobbyPanel.SetActive(true);
            hostArea.SetActive(true);
            clientArea.SetActive(false);
            
            // Verstecke die Haupt-Buttons
            hostMenuBtn.gameObject.SetActive(false);
            clientMenuBtn.gameObject.SetActive(false);

            // Relay erstellen
            string code = await RelayManager.Instance.CreateRelay();
            
            if (code != null)
            {
                joinCodeDisplay.text = code;
                startGameBtn.gameObject.SetActive(true);
                copyCodeBtn.gameObject.SetActive(true);
            }
        });
        // Status Text am Anfang verstecken oder leeren
        if(statusText != null) statusText.text = "";

        // --- CLIENT FLOW
        clientMenuBtn.onClick.AddListener(() => {
            // UI Umschalten
            lobbyPanel.SetActive(true);
            hostArea.SetActive(false);
            clientArea.SetActive(true); // Zeige das Eingabefeld

            // Verstecke die Haupt-Buttons
            hostMenuBtn.gameObject.SetActive(false);
            clientMenuBtn.gameObject.SetActive(false);
        });

        // --- CLIENT FLOW (Schritt 2: Wirklich beitreten) ---
        submitCodeBtn.onClick.AddListener(async () => { 
            string code = joinCodeInput.text;

            if (!string.IsNullOrEmpty(code))
            {
                // 1. Status anzeigen: "Ich arbeite dran..."
                if(statusText != null) 
                {
                    statusText.gameObject.SetActive(true);
                    statusText.text = "Verbinde zu Relay...";
                    statusText.color = Color.yellow; // Gelb für "Warten"
                }

                // Button deaktivieren, damit man nicht spammen kann
                submitCodeBtn.interactable = false;

                // 2. Warten auf das Ergebnis vom RelayManager (Dank Task<bool>)
                bool success = await RelayManager.Instance.JoinRelay(code);

                // 3. Ergebnis anzeigen
                if (success)
                {
                    if(statusText != null)
                    {
                        statusText.text = "Erfolg! Warte auf Host...";
                        statusText.color = Color.green; // Grün für Erfolg
                    }
                    // Wir bleiben hier, bis der Host das Spiel startet
                }
                else
                {
                    if(statusText != null)
                    {
                        statusText.text = "Fehler: Code ungültig oder Timeout.";
                        statusText.color = Color.red; // Rot für Fehler
                    }
                    // Button wieder aktivieren, damit er es nochmal versuchen kann
                    submitCodeBtn.interactable = true;
                }
            }
            else
            {
                 if(statusText != null) statusText.text = "Bitte Code eingeben!";
            }
        });

        // --- START GAME (Nur Host) ---
        startGameBtn.onClick.AddListener(() => {
            if (playernameInput != null)
            {
                GameData.PlayerName = playernameInput.text;
            }
            NetworkManager.Singleton.SceneManager.LoadScene(firstlevel, UnityEngine.SceneManagement.LoadSceneMode.Single);
        });

        // --- COPY FEATURE ---
        copyCodeBtn.onClick.AddListener(() => {
            GUIUtility.systemCopyBuffer = joinCodeDisplay.text;
            copyCodeBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Kopiert!";
        });

        // --- BACK BUTTON ---
        backBtn.onClick.AddListener(() => {
            // Alles zurücksetzen
            lobbyPanel.SetActive(false);
            NetworkUIGO.SetActive(false);
            MainMenuUI.SetActive(true);
            
            // Buttons wieder zeigen für nächstes Mal
            hostMenuBtn.gameObject.SetActive(true);
            clientMenuBtn.gameObject.SetActive(true);
            
            // Verbindung trennen falls man schon im Relay war (optional aber sauber)
            if(NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
        });
    }
}