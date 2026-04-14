using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI")]
    public Image healthBarFill;
    public TextMeshProUGUI HPText;

    public Image manaBarFill;
    public TextMeshProUGUI manaText;

    public Image xpBarFill;
    public TextMeshProUGUI xpText;
    public static PlayerStatsUI LocalInstance;

    private void Awake()
    {
        LocalInstance = this;
        HPText.color = Color.black;
        manaText.color = Color.black;
        xpText.color = Color.black;
    }
    public void UpdateHealthUI(int health, int maxHealth)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)health / (float)maxHealth; 
            HPText.text = ("HP:" + health + "/" + maxHealth);
        }
    }
    public void UpdateManaUI(int mana, int maxMana)
    {
        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = (float)mana / (float)maxMana;
            manaText.text = ("Mana:" + mana + "/" + maxMana);
        }
    }

    public void UpdateXPUI(int xp, int maxXP, int playerLevel)
    {
        if (xpBarFill != null)
        { 
            xpBarFill.fillAmount = (float)xp / (float)maxXP;
            xpText.text = ("XP:" + xp + "/" + maxXP + " Level:" + playerLevel);
        }
    }
}
