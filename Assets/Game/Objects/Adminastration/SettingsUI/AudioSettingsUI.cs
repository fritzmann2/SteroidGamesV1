using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Menu Navigation (Von links nach rechts)")]
    public Button[] topButtons;   
    public Button[] bottomButtons; 

    [Header("Audio Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        if (masterSlider != null) 
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicSlider != null) 
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxSlider != null) 
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        SetupControllerNavigation();
    }

    private void OnEnable()
    {
        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private void SetupControllerNavigation()
    {
        for (int i = 0; i < topButtons.Length; i++)
        {
            if (topButtons[i] == null) continue;
            Navigation nav = topButtons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i > 0) nav.selectOnLeft = topButtons[i - 1];
            if (i < topButtons.Length - 1) nav.selectOnRight = topButtons[i + 1];
            nav.selectOnDown = masterSlider; 
            topButtons[i].navigation = nav;
        }

        if (masterSlider != null)
        {
            Navigation nav = masterSlider.navigation;
            nav.mode = Navigation.Mode.Explicit;
            if (topButtons.Length > 0) nav.selectOnUp = topButtons[0];
            if (musicSlider != null) nav.selectOnDown = musicSlider; 
            masterSlider.navigation = nav;
        }

        if (musicSlider != null)
        {
            Navigation nav = musicSlider.navigation;
            nav.mode = Navigation.Mode.Explicit;
            if (masterSlider != null) nav.selectOnUp = masterSlider;
            if (sfxSlider != null) nav.selectOnDown = sfxSlider;
            musicSlider.navigation = nav;
        }

        if (sfxSlider != null)
        {
            Navigation nav = sfxSlider.navigation;
            nav.mode = Navigation.Mode.Explicit;
            if (musicSlider != null) nav.selectOnUp = musicSlider;
            if (bottomButtons.Length > 0) nav.selectOnDown = bottomButtons[0];
            sfxSlider.navigation = nav;
        }

        for (int i = 0; i < bottomButtons.Length; i++)
        {
            if (bottomButtons[i] == null) continue;
            Navigation nav = bottomButtons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i > 0) nav.selectOnLeft = bottomButtons[i - 1];
            if (i < bottomButtons.Length - 1) nav.selectOnRight = bottomButtons[i + 1];
            
            nav.selectOnUp = sfxSlider; 
            
            bottomButtons[i].navigation = nav;
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}