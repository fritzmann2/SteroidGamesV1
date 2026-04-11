using UnityEngine;
using TMPro;

public class SkillTooltip : MonoBehaviour
{
    public static SkillTooltip Instance { get; private set; }

    [Header("UI Referenzen")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    
    [Header("Einstellungen")]
    public Vector3 offset = new Vector3(50f, -50f, 0f);

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
        gameObject.SetActive(false); 
    }

    public void ShowTooltip(string title, string description, Vector3 nodePosition)
    {
        titleText.text = title;
        descriptionText.text = description;
        transform.position = nodePosition + offset;

        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}