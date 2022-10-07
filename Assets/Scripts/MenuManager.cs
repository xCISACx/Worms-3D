using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _settingsPopup;
    private bool _showSettings;
    public AudioClip ButtonHoverSfx;
    public AudioClip ButtonClickSfx;
    public FullScreenMode FullScreenMode;
    [SerializeField] private Prefs _prefs;
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        _settingsPopup.SetActive(false);
        //Debug.Log("awake: " + prefs);
    }

    public void Options()
    {
        //SettingsPopup.enabled = true;
        _settingsPopup.gameObject.SetActive(true);
        LoadMenuUIValues();
    }
    
    public void CloseOptions() //TODO: MAKE TOGGLE?
    {
        //SettingsPopup.enabled = false;
        _settingsPopup.gameObject.SetActive(false);
    }

    public void LoadMenuUIValues()
    {
        _masterSlider.value = _prefs.MasterValue;
        _musicSlider.value = _prefs.MusicValue;
        _sfxSlider.value = _prefs.SfxValue;
        _resolutionDropdown.value = _prefs.ResolutionIndex;
        _fullscreenToggle.isOn = _prefs.Fullscreen;
    }

    public void PlayButtonHoverSound()
    {
        var audioSource = GameManager.Instance.SfxSource;
        audioSource.PlayOneShot(ButtonHoverSfx);
    }
    
    public void PlayButtonClickSound()
    {
        var audioSource = GameManager.Instance.SfxSource;
        audioSource.PlayOneShot(ButtonClickSfx);
    }

    public void SetMasterVolume(float value)
    {
        var newValue = value;
        var newVolume = Mathf.Log10(value) * 20f;
        //Debug.Log("new master volume: " + newVolume);
        
        // for some reason Unity runs this method when the scene updates and GameManager's instance is not set by then so this is a hacky fix
        if (GameManager.Instance)
        {
            GameManager.Instance.AudioMixer.SetFloat("masterVolume", newVolume);
        }
        
        // for some reason Unity sets prefs to null after Awake so we need to get prefs from the Resources folder
        _prefs = Resources.Load<Prefs>("Prefs");
        _prefs.MasterValue = newValue;
        _prefs.MasterVolume = newVolume;
    }
    
    public void SetMusicVolume(float value)
    {
        var newValue = value;
        var newVolume = Mathf.Log10(value) * 20f;
        GameManager.Instance.AudioMixer.SetFloat("musicVolume", newVolume);
        
        // for some reason Unity sets prefs to null after Awake so we need to get prefs from the Resources folder
        _prefs = Resources.Load<Prefs>("Prefs");
        _prefs.MusicValue = newValue;
        _prefs.MusicVolume = newVolume;
    }
    
    public void SetSfxVolume(float value)
    {
        var newValue = value;
        var newVolume = Mathf.Log10(value) * 20f;
        GameManager.Instance.AudioMixer.SetFloat("sfxVolume", newVolume);
        
        // for some reason Unity sets prefs to null after Awake so we need to get prefs from the Resources folder
        _prefs = Resources.Load<Prefs>("Prefs");
        _prefs.SfxValue = newValue;
        _prefs.SfxVolume = newVolume;
    }

    public void ToggleFullscreen(bool value)
    {
        if (value)
        {
            Screen.fullScreen = true;
            _prefs.Fullscreen = true;
            FullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreen = false;
            _prefs.Fullscreen = false;
            FullScreenMode = FullScreenMode.Windowed;
        }

        Screen.fullScreenMode = FullScreenMode;
        _prefs.FullScreenMode = FullScreenMode;
    }

    public void SetResolution(int index)
    {
        switch (index)
        {
            case 0:
                Screen.SetResolution(1920, 1080, FullScreenMode);
                _prefs.ResolutionW = 1920;
                _prefs.ResolutionH = 1080;
                break;
            case 1:
                Screen.SetResolution(1366, 768, FullScreenMode);
                _prefs.ResolutionW = 1366;
                _prefs.ResolutionH = 768;
                break;
            case 2:
                Screen.SetResolution(1280, 720, FullScreenMode);
                _prefs.ResolutionW = 1280;
                _prefs.ResolutionH = 720;
                break;
            case 3:
                Screen.SetResolution(1024, 768, FullScreenMode);
                _prefs.ResolutionW = 1024;
                _prefs.ResolutionH = 768;
                break;
        }
        
        _prefs.ResolutionIndex = index;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
