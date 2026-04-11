using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

public class SkillNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillData skillData; 
    
    [Header("UI Referenzen")]
    public Image iconImage;
    public TextMeshProUGUI levelText;
    public Button skillButton;
    public Image backgroundFrame;

    [Header("Farben für States")]
    public Color lockedColor = Color.gray;
    public Color availableColor = Color.white;
    public Color unlockedColor = Color.yellow;

    private SkillTreeManager manager;
    private int currentLevel = 0;

    private float hoverDelay = 1.0f; 
    private float hoverTimer = 0f;
    private bool isHovering = false;
    private bool isTooltipShown = false;

    public void Initialize(SkillTreeManager treeManager)
    {
        manager = treeManager;
        
        if (skillData != null)
        {
            iconImage.sprite = skillData.skillIcon;
        }

        skillButton.onClick.AddListener(OnSkillClicked);
        
        UpdateUI();
    }

    private void Update()
    {
        if (isHovering && !isTooltipShown)
        {
            hoverTimer += Time.deltaTime;
            
            if (hoverTimer >= hoverDelay)
            {
                ShowTooltip();
            }
        }
    }

    public void UpdateUI()
    {
        currentLevel = manager.GetSkillLevel(skillData.skillID);
        levelText.text = $"{currentLevel}/{skillData.maxLevel}";

        if (currentLevel > 0)
        {
            backgroundFrame.color = unlockedColor;
            skillButton.interactable = currentLevel < skillData.maxLevel; 
        }
        else if (manager.ArePrerequisitesMet(skillData))
        {
            backgroundFrame.color = availableColor;
            skillButton.interactable = true;
        }
        else
        {
            backgroundFrame.color = lockedColor;
            skillButton.interactable = false;
        }
    }

    private void OnSkillClicked()
    {
        manager.TryUnlockSkill(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        hoverTimer = 0f; 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        hoverTimer = 0f;
        
        if (isTooltipShown)
        {
            HideTooltip();
        }
    }

    private void ShowTooltip()
    {
        isTooltipShown = true;
        if (skillData != null && SkillTooltip.Instance != null)
        {
            SkillTooltip.Instance.ShowTooltip(skillData.skillName, skillData.description, transform.position);
        }
    }

    private void HideTooltip()
    {
        isTooltipShown = false;
        if (SkillTooltip.Instance != null)
        {
            SkillTooltip.Instance.HideTooltip();
        }
    }
    
    private void OnDisable()
    {
        if (isTooltipShown) HideTooltip();
        isHovering = false;
    }
}