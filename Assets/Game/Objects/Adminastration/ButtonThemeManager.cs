using UnityEngine;
using UnityEngine.UI;

public class ButtonThemeManager : MonoBehaviour
{
    [Header("Design-Einstellungen")]
    public Color normalColor = Color.white;
    public Color highlightedColor = new Color(1f, 0.8f, 0f);
    public Color pressedColor = new Color(0.8f, 0.6f, 0f);
    public Color selectedColor = new Color(1f, 0.8f, 0f);
    
    [Range(1f, 5f)]
    public float colorMultiplier = 2f;

    [ContextMenu("Farben auf ALLE Buttons anwenden!")]
    public void ApplyThemeToAllButtons()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int count = 0;

        foreach (Button btn in allButtons)
        {
            ColorBlock cb = btn.colors;
            
            cb.normalColor = normalColor;
            cb.highlightedColor = highlightedColor;
            cb.pressedColor = pressedColor;
            cb.selectedColor = selectedColor;
            cb.colorMultiplier = colorMultiplier;

            btn.colors = cb;
            count++;
        }

        Debug.Log($"Erfolg! Das Design wurde auf {count} Buttons angewendet.");
    }
}