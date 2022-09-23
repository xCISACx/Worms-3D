using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject SettingsPopup;
    private bool showSettings;
    public AudioClip ButtonHoverSFX;
    public AudioClip ButtonClickSFX;
    public FullScreenMode FullScreenMode;
    [SerializeField] private Prefs prefs;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        SettingsPopup.SetActive(false);
        Debug.Log("awake: " + prefs);
    }

    public void Options()
    {
        //SettingsPopup.enabled = true;
        SettingsPopup.gameObject.SetActive(true);
        LoadMenuUIValues();
    }
    
    public void CloseOptions() //TODO: MAKE TOGGLE?
    {
        //SettingsPopup.enabled = false;
        SettingsPopup.gameObject.SetActive(false);
    }

    public void LoadMenuUIValues()
    {
        masterSlider.value = prefs.masterValue;
        musicSlider.value = prefs.musicValue;
        sfxSlider.value = prefs.sfxValue;
        resolutionDropdown.value = prefs.resolutionIndex;
        fullscreenToggle.isOn = prefs.fullscreen;
    }

    public void PlayButtonHoverSound()
    {
        var audioSource = GameManager.Instance.SFXSource;
        audioSource.PlayOneShot(ButtonHoverSFX);
    }
    
    public void PlayButtonClickSound()
    {
        var audioSource = GameManager.Instance.SFXSource;
        audioSource.PlayOneShot(ButtonClickSFX);
    }

    public void SetMasterVolume(float value)
    {
        var newValue = value;
        var newVolume = Mathf.Log10(value) * 20f;
        Debug.Log("new master volume: " + newVolume);
        
        //for some reason Unity runs this method when the scene updates and GameManager's instance is not set by then so this is a hacky fix
        if (GameManager.Instance)
        {
            GameManager.Instance.AudioMixer.SetFloat("masterVolume", newVolume);
        }
        
        // for some reason Unity sets prefs to null after Awake so we need to get prefs from the Resources folder
        prefs = Resources.Load<Prefs>("Prefs");
        prefs.masterValue = newValue;
        prefs.masterVolume = newVolume;
    }
    
    public void SetMusicVolume(float value)
    {
        var newValue = value;
        var newVolume = Mathf.Log10(value) * 20f;
        GameManager.Instance.AudioMixer.SetFloat("musicVolume", newVolume);
        
        // for some reason Unity sets prefs to null after Awake so we need to get prefs from the Resources folder
        prefs = Resources.Load<Prefs>("Prefs");
        prefs.musicValue = newValue;
        prefs.musicVolume = newVolume;
    }
    
    public void SetSFXVolume(float value)
    {
        var newValue = value;
        var newVolume = Mathf.Log10(value) * 20f;
        GameManager.Instance.AudioMixer.SetFloat("sfxVolume", newVolume);
        
        // for some reason Unity sets prefs to null after Awake so we need to get prefs from the Resources folder
        prefs = Resources.Load<Prefs>("Prefs");
        prefs.sfxValue = newValue;
        prefs.sfxVolume = newVolume;
    }

    public void ToggleFullscreen(bool value)
    {
        if (value)
        {
            Screen.fullScreen = true;
            prefs.fullscreen = true;
            FullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreen = false;
            prefs.fullscreen = false;
            FullScreenMode = FullScreenMode.Windowed;
        }

        Screen.fullScreenMode = FullScreenMode;
        prefs.fullScreenMode = FullScreenMode;
    }

    public void SetResolution(int index)
    {
        switch (index)
        {
            case 0:
                Screen.SetResolution(1920, 1080, FullScreenMode);
                prefs.resolutionW = 1920;
                prefs.resolutionH = 1080;
                Debug.LogError(Screen.width + "x" + Screen.height + " " + Screen.fullScreen);
                Debug.Log("1920x1080");
                break;
            case 1:
                Screen.SetResolution(1366, 768, FullScreenMode);
                prefs.resolutionW = 1366;
                prefs.resolutionH = 768;
                Debug.LogError(Screen.width + "x" + Screen.height + " " + Screen.fullScreen);
                Debug.Log("1366x768");
                break;
            case 2:
                Screen.SetResolution(1280, 720, FullScreenMode);
                prefs.resolutionW = 1280;
                prefs.resolutionH = 720;
                Debug.LogError(Screen.width + "x" + Screen.height + " " + Screen.fullScreen);
                Debug.Log("1280x720");
                break;
            case 3:
                Screen.SetResolution(1024, 768, FullScreenMode);
                prefs.resolutionW = 1024;
                prefs.resolutionH = 768;
                Debug.LogError(Screen.width + "x" + Screen.height + " " + Screen.fullScreen);
                Debug.Log("1024x768");
                break;
        }
        
        prefs.resolutionIndex = index;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
