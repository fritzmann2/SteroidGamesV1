using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode; // Wichtig für den lokalen Spieler!

public class BossUIController : MonoBehaviour
{
    public static BossUIController Instance;

    [Header("UI Elemente")]
    public GameObject uiContainer;
    public TextMeshProUGUI bossNameText;
    public Image healthFillImage; 

    [Header("Distanz Einstellungen")]
    public float showDistance = 200f; 

    private float currentMaxHealth;
    private float currentHealth;
    private string currentBossName;
    
    private Transform trackedBoss;
    private Transform localPlayer;

    void Awake()
    {
        Instance = this;
        if (uiContainer != null) uiContainer.SetActive(false);
    }

    public void ShowBoss(string bossName, float maxHP, Transform bossTransform)
    {
        if (uiContainer == null) return;
        
        trackedBoss = bossTransform;
        currentBossName = bossName;
        currentMaxHealth = maxHP;
        currentHealth = maxHP;

        UpdateUIText();
        
        healthFillImage.fillAmount = 1f;
        
        CheckDistanceAndToggle(); 
    }

    public void UpdateHealth(float currentHP)
    {
        currentHealth = currentHP;
        
        if (currentMaxHealth > 0 && healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHP / currentMaxHealth;
            UpdateUIText();
        }
    }

    private void UpdateUIText()
    {
        bossNameText.text = currentBossName + " " + currentHealth.ToString() + "/" + currentMaxHealth.ToString();
    }

    public void HideBoss()
    {
        if (uiContainer != null) uiContainer.SetActive(false);
        trackedBoss = null;
    }

    void Update()
    {
        if (trackedBoss != null)
        {
            CheckDistanceAndToggle();
        }
    }

    private void CheckDistanceAndToggle()
    {
        if (localPlayer == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            {
                var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
                if (playerObj != null) localPlayer = playerObj.transform;
            }
            return;
        }

        float distance = Vector2.Distance(localPlayer.position, trackedBoss.position);
        
        if (distance <= showDistance)
        {
            if (!uiContainer.activeSelf) uiContainer.SetActive(true);
        }
        else
        {
            if (uiContainer.activeSelf) uiContainer.SetActive(false);
        }
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
            healthFillImage.color = new Color(1f, 0.5f, 0.5f); 
        }
    }
}