using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; 

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    public AudioMixer mainMixer;

    [Header("UI Sliders (Optional hier reinziehen)")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    
    [Header("Audio")]
    public AudioSource backgroundAudioSource;
    public AudioClip backgroundClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (backgroundAudioSource != null && backgroundClip != null)
        {
            backgroundAudioSource.clip = backgroundClip;
            backgroundAudioSource.loop = true;
            backgroundAudioSource.Play(); 
        }
    }

    private void Start()
    {
        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (masterSlider != null) masterSlider.value = savedMaster;
        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        SetMasterVolume(savedMaster);
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);
    }

    public void SetMasterVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        mainMixer.SetFloat("MasterVolume", dbValue);
        PlayerPrefs.SetFloat("MasterVolume", sliderValue); 
    }

    public void SetMusicVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        mainMixer.SetFloat("MusicVolume", dbValue);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        mainMixer.SetFloat("SFXVolume", dbValue);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }
}