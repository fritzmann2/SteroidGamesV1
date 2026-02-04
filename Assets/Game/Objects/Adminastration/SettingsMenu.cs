using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class SettingsMenu : MonoBehaviour
{
    public TMP_InputField GravityScale;
    

    private PlayerMovement localMovementScript; 

    void OnEnable()
    {
        Getplayersettings();
    }
    
    public void Getplayersettings()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null || NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            Debug.LogWarning("Kein lokaler Spieler gefunden! (Spiel läuft vielleicht noch nicht?)");
            return;
        }

        GameObject myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        localMovementScript = myPlayer.GetComponent<PlayerMovement>();
        if (localMovementScript != null)
        {            
            Rigidbody2D rb = myPlayer.GetComponent<Rigidbody2D>();
            if (rb != null && GravityScale != null)
            {
                GravityScale.text = rb.gravityScale.ToString();
            }
        }
    }
    
    public void ApplySettings()
    {
        if (localMovementScript == null) 
        {
            if (localMovementScript == null) return;
        }

        if (GravityScale != null && float.TryParse(GravityScale.text, out float gravVal))
        {
            Rigidbody2D rb = localMovementScript.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = gravVal;
            }
        }
        Backtomenu();
    }
    public void Backtomenu()
    {
        if (transform.parent != null)
        {
            transform.parent.gameObject.GetComponent<PauseManager>().OpenMenu();
            gameObject.SetActive(false);
        }
    }
}
