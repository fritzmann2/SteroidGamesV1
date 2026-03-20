using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RebindUIItem : MonoBehaviour
{
    public TextMeshProUGUI actionNameLabel; 
    
    [Header("Primäre Taste (Spalte 1)")]
    public TextMeshProUGUI bindButtonText1;  
    public Button rebindButton1;             

    [Header("Sekundäre Taste (Spalte 2)")]
    public TextMeshProUGUI bindButtonText2;  
    public Button rebindButton2;             
}