using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MatchSettingsPopup;
    [SerializeField] private GameObject SettingsPopup;
    [SerializeField] private GameObject ColourHParent;
    [SerializeField] private GameObject ColourHPrefab;
    private bool showMatchSettings;

    public string GameScene;
    public int numberOfPlayers;
    [SerializeField] private int numberOfPlayerUnits;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject UnitPrefab;
    public string[] PlayerColours;
    public AudioClip ButtonHoverSFX;
    public AudioClip ButtonClickSFX;

    IEnumerator LoadAsyncScene()
    {
        // Set the current Scene to be able to unload it later
        Scene currentScene = SceneManager.GetActiveScene();

        // The Application loads the Scene in the background at the same time as the current Scene.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Additive);

        // Wait until the last operation fully loads to return anything
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        for (int i = 0; i < numberOfPlayers; i++)
        {
            var newPlayer = Instantiate(PlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            newPlayer.name = "Player " + (i + 1);
            var playerScript = newPlayer.GetComponent<PlayerBehaviour>();
            
            GameManager.Instance.playerList.Add(newPlayer.GetComponent<PlayerBehaviour>());

            for (int j = 0; j < numberOfPlayerUnits; j++)
            {
                var xPos = UnityEngine.Random.Range(0, 100f);
                var zPos = UnityEngine.Random.Range(0, 100f);
                
                var newUnit = Instantiate(UnitPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity);

                newUnit.transform.SetParent(newPlayer.transform);
                playerScript.unitList.Add(newUnit.GetComponent<UnitBehaviour>());
                
                var unitScript = newUnit.GetComponent<UnitBehaviour>();
                unitScript.enabled = true;
                unitScript.Owner = (UnitBehaviour.PlayerNumber) i;
                
                var color = Color.black;
                ColorUtility.TryParseHtmlString(PlayerColours[i], out color);
                unitScript.PlayerColour = color;
                
                newUnit.name = unitScript.Owner.ToString() + " Unit " + (j + 1);
                
                GameManager.Instance.unitList.Add(newUnit.GetComponent<UnitBehaviour>());
            }
            
            SceneManager.MoveGameObjectToScene(newPlayer, SceneManager.GetSceneByName(GameScene));
        }
        
        // Unload the previous Scene
        GameManager.Instance.matchStarted = true;
        SceneManager.UnloadSceneAsync(currentScene);
    }
    
    private void Awake()
    {
        ColourHParent = GameObject.Find("ColourHParent");
        Cursor.lockState = CursorLockMode.None;
        MatchSettingsPopup.SetActive(false);
        GameManager.Instance.gameOver = false;
        GameManager.Instance.matchStarted = false;
        
        PlayerColours = new string[1];
        
        for (int i = 0; i < numberOfPlayers; i++)
        {
            PlayerColours[i] = "#FF0000";
        }
    }

    public void Play()
    {
        showMatchSettings = true;
        MatchSettingsPopup.SetActive(true);
        //SceneManager.LoadScene(sceneName: "Game");
    }

    public void Options()
    {
        SettingsPopup.SetActive(true);
    }
    
    public void CloseOptions() //TODO: MAKE TOGGLE?
    {
        SettingsPopup.SetActive(false);
    }
    public void SetNumberOfPlayers(int num)
    {
        numberOfPlayers = num + 1;
        PlayerColours = new string[num + 1];
        
        //Add or remove colour selection containers according to number of players
        
        ColourHParent = GameObject.Find("ColourHParent");

        if (ColourHParent.transform.childCount > 0)
        {
            for (int j = 0; j < ColourHParent.transform.childCount; j++)
            {
                Destroy(ColourHParent.transform.GetChild(j).gameObject);
            }   
        }

        for (int i = 0; i < numberOfPlayers; i++)
        {
            var newContainer = Instantiate(ColourHPrefab, Vector3.zero, Quaternion.identity);
            newContainer.transform.SetParent(ColourHParent.transform);
            newContainer.GetComponentInChildren<TMP_Text>().text = "Player " + (i + 1) + " Colour";
            newContainer.GetComponent<ColourDropdownBehaviour>().playerIndex = i;
        }

        for (int i = 0; i < numberOfPlayers; i++)
        {
            PlayerColours[i] = "#FF0000";
        }
    }
    
    public void SetNumberOfPlayerUnits(int num)
    {
        numberOfPlayerUnits = num + 1;
        GameManager.Instance.NumberOfStartingUnits = num + 1;
    }

    public void StartMatch()
    {
        StartCoroutine(LoadAsyncScene());
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
        Screen.fullScreen = value;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
