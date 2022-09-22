using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject SettingsPopup;
    private bool showSettings;
    public AudioClip ButtonHoverSFX;
    public AudioClip ButtonClickSFX;
    public FullScreenMode FullScreenMode;

    private void Awake()
    {
        Screen.fullScreen = true;
        FullScreenMode = FullScreenMode.FullScreenWindow;
        Cursor.lockState = CursorLockMode.None;
        SettingsPopup.SetActive(false);
    }

    public void Options()
    {
        //SettingsPopup.enabled = true;
        SettingsPopup.gameObject.SetActive(true);
    }
    
    public void CloseOptions() //TODO: MAKE TOGGLE?
    {
        //SettingsPopup.enabled = false;
        SettingsPopup.gameObject.SetActive(false);
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
        GameManager.Instance.AudioMixer.SetFloat("masterVolume", Mathf.Log10(value) * 20f);
    }
    
    public void SetMusicVolume(float value)
    {
        GameManager.Instance.AudioMixer.SetFloat("musicVolume", Mathf.Log10(value) * 20f);
    }
    
    public void SetSFXVolume(float value)
    {
        GameManager.Instance.AudioMixer.SetFloat("sfxVolume", Mathf.Log10(value) * 20f);
    }

    public void ToggleFullscreen(bool value)
    {
        if (value)
        {
            Screen.fullScreen = true;
            FullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreen = false;
            FullScreenMode = FullScreenMode.Windowed;
        }

        Screen.fullScreenMode = FullScreenMode;
    }

    public void SetResolution(int index)
    {
        switch (index)
        {
            case 0:
                Screen.SetResolution(1920, 1080, FullScreenMode);
                Debug.LogError(Screen.width + "x" + Screen.height + " " + Screen.fullScreen);
                Debug.Log("1920x1080");
                break;
            case 1:
                Screen.SetResolution(1366, 768, FullScreenMode);
                Debug.LogError(Screen.width + "x" + Screen.height + " " + Screen.fullScreen);
                Debug.Log("1366x768");
                break;
            case 2:
                Screen.SetResolution(1280, 720, FullScreenMode);
                Debug.LogError(Screen.width + "x" + Screen.height + " " + Screen.fullScreen);
                Debug.Log("1280x720");
                break;
            case 3:
                Screen.SetResolution(1024, 768, FullScreenMode);
                Debug.LogError(Screen.width + "x" + Screen.height + " " + Screen.fullScreen);
                Debug.Log("1024x768");
                break;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
