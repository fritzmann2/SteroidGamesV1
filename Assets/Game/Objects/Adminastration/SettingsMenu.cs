using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class SettingsMenu : MonoBehaviour
{
    public TMP_InputField inputDashforce;
    public TMP_InputField inputMoveSpeed;
    public TMP_InputField Jumpforce;
    public TMP_InputField GravityScale;
    

    private movement localMovementScript; 

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
        localMovementScript = myPlayer.GetComponent<movement>();
        if (localMovementScript != null)
        {
            if(inputDashforce != null) inputDashforce.text = localMovementScript.dashforce.ToString();
            
            if(inputMoveSpeed != null) inputMoveSpeed.text = localMovementScript.basemoveSpeed.ToString(); 
            
            if(Jumpforce != null) Jumpforce.text = localMovementScript.jumpForce.ToString();
            
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

        if (inputDashforce != null && float.TryParse(inputDashforce.text, out float dashVal))
        {
             localMovementScript.dashforce = dashVal;
        }

        if (inputMoveSpeed != null && float.TryParse(inputMoveSpeed.text, out float speedVal))
        {
            localMovementScript.basemoveSpeed = (int)speedVal; 
        }

        if (Jumpforce != null && float.TryParse(Jumpforce.text, out float jumpVal))
        {
            localMovementScript.jumpForce = jumpVal;
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
