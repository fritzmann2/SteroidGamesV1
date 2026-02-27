using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossUIController : MonoBehaviour
{
    public static BossUIController Instance;

    [Header("UI Elemente")]
    public GameObject uiContainer;
    public TextMeshProUGUI bossNameText;
    public Image healthFillImage; 
    private float currentMaxHealth;

    void Awake()
    {
        Instance = this;
        if (uiContainer != null) uiContainer.SetActive(false);
    }

    public void ShowBoss(string name, float maxHP)
    {
        if (uiContainer == null) return;

        bossNameText.text = name + " " + currentMaxHealth.ToString() + "/" + currentMaxHealth.ToString();
        currentMaxHealth = maxHP;
        healthFillImage.fillAmount = 1f;
        uiContainer.SetActive(true);
    }

    public void UpdateHealth(float currentHP)
    {
        if (currentMaxHealth > 0 && healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHP / currentMaxHealth;
            bossNameText.text = name + " " + currentHP.ToString() + "/" + currentMaxHealth.ToString();
        }
    }

    public void HideBoss()
    {
        if (uiContainer != null) uiContainer.SetActive(false);
    }

    public void changeHPcolor(int color)
    {
        if (color == 1)
        {
            healthFillImage.color = Color.green;
        }
        else if (color == 2)
        {
            healthFillImage.color = Color.yellow;
        }
        else if (color == 3)
        {
            healthFillImage.color = Color.red;
        }
        else if (color == 0)
        {
            healthFillImage.color = Color.softRed;
        }
    }
}