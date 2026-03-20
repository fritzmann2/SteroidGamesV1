using UnityEngine;
using UnityEngine.EventSystems;

public class KeepUIFocused : MonoBehaviour
{
    private GameObject lastSelected;

    void Update()
    {
        if (EventSystem.current == null) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != null)
        {
            lastSelected = current;
        }
        else if (current == null && lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }
}