using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; 

public class VirtualKeyboardTrigger : MonoBehaviour, ISubmitHandler
{
    public NetworkUI uiManager;
    
    [Tooltip("Haken rein, wenn das für den Spielernamen ist. Haken raus für den Join Code.")]
    public bool isPlayerNameField;

    public void OnSubmit(BaseEventData eventData)
    {
        if (Gamepad.current != null)
        {
            if (isPlayerNameField)
            {
                uiManager.OpenKeyboardForPlayerName();
            }
            else
            {
                uiManager.OpenKeyboardForJoinCode();
            }
        }
    }
}