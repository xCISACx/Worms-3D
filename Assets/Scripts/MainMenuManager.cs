using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _matchSettingsPopup;
    [SerializeField] private GameObject _settingsPopup;
    [SerializeField] private GameObject _colourHParent;
    [SerializeField] private GameObject _colourHPrefab;
    private bool _showMatchSettings;

    public GameObject Map;
    
    public string GameScene;
    
    public int NumberOfPlayers;
    
    [SerializeField] private int _numberOfPlayerUnits;
    
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _unitPrefab;
    
    public string[] PlayerColours;
    
    public AudioClip ButtonHoverSfx;
    public AudioClip ButtonClickSfx;

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

        for (int i = 0; i < NumberOfPlayers; i++)
        {
            var newPlayer = Instantiate(_playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            newPlayer.name = "Player " + (i + 1);
            var playerScript = newPlayer.GetComponent<PlayerBehaviour>();
            
            playerScript.OriginalPlayerIndex = i;
            
            GameManager.Instance.PlayerList.Add(newPlayer.GetComponent<PlayerBehaviour>());
            GameManager.Instance.PlayerQueue.Add(newPlayer.GetComponent<PlayerBehaviour>());

            for (int j = 0; j < _numberOfPlayerUnits; j++)
            {
                var closestPoint = GenerateSpawnPoint();
                
                Debug.Log(closestPoint);

                var newUnit = SpawnUnitAtLocation(closestPoint);
                
                //Collider[] collisionWithUnit = Physics.OverlapSphere(closestPoint, 5f, LayerMask.GetMask("Unit"));

                //GameObject newUnit = null;
                
                //closestPoint = GenerateSpawnPoint();
                
                

                /*while (collisionWithUnit.Length != 0)
                {
                    closestPoint = GenerateSpawnPoint();
                    
                    collisionWithUnit = Physics.OverlapSphere(closestPoint, 5f, LayerMask.GetMask("Unit"));
                }

                SpawnUnitAtLocation(closestPoint);*/

                newUnit.transform.SetParent(newPlayer.transform);
                
                playerScript.UnitList.Add(newUnit.GetComponent<UnitBehaviour>());
                
                
                var unitScript = newUnit.GetComponent<UnitBehaviour>();
                
                unitScript.enabled = true;
                
                unitScript.Owner = (UnitBehaviour.PlayerNumber) i;
                
                unitScript.OriginalIndex = j;
                
                var color = Color.black;
                ColorUtility.TryParseHtmlString(PlayerColours[i], out color);
                
                unitScript.PlayerColour = color;
                
                newUnit.name = unitScript.Owner + " Unit " + (j + 1);
                
                GameManager.Instance.UnitList.Add(newUnit.GetComponent<UnitBehaviour>());
            }
            
            SceneManager.MoveGameObjectToScene(Map, SceneManager.GetSceneByName(GameScene));
            
            SceneManager.MoveGameObjectToScene(newPlayer, SceneManager.GetSceneByName(GameScene));
        }
        
        // Unload the previous Scene
        GameManager.Instance.MatchStarted = true;
        
        SceneManager.UnloadSceneAsync(currentScene);
    }

    private GameObject SpawnUnitAtLocation(Vector3 closestPoint)
    {
        var newUnit = Instantiate(_unitPrefab, closestPoint, Quaternion.identity);

        return newUnit;
    }

    private Vector3 GenerateSpawnPoint()
    {
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        
        var xPos = UnityEngine.Random.Range(-27f, 27f);
        var zPos = UnityEngine.Random.Range(-27f, 27f);
        var yPos = UnityEngine.Random.Range(0f, 50f);

        Vector3 spawnPoint = new Vector3(xPos, yPos, zPos);

        float unitRadius = _unitPrefab.GetComponent<Collider>().bounds.extents.x;

        var minDistance = float.PositiveInfinity;
        
        var closestPointToCollider = Vector3.zero;

        foreach (var collider in Map.GetComponentsInChildren<Collider>())
        {
            if (collider.isTrigger)
            {
                var closestPointToPosition = collider.ClosestPoint(spawnPoint);
            
                if (Vector3.Distance(spawnPoint, closestPointToPosition) < minDistance)
                {
                    minDistance = Vector3.Distance(spawnPoint, closestPointToPosition);
                
                    closestPointToCollider = closestPointToPosition;
                    
                    closestPointToCollider.y += 1f;
                    
                    Debug.Log(closestPointToCollider);
                }   
            }
        }

        return closestPointToCollider;
    }

    private void Awake()
    {
        _colourHParent = GameObject.Find("ColourHParent");
        
        Cursor.lockState = CursorLockMode.None;
        
        _matchSettingsPopup.SetActive(false);
        
        GameManager.Instance.GameOver = false;
        
        GameManager.Instance.MatchStarted = false;
        
        PlayerColours = new string[2];
        
        for (int i = 0; i < NumberOfPlayers; i++)
        {
            PlayerColours[i] = "#FF0000";
        }
    }

    public void Play()
    {
        _showMatchSettings = true;
        _matchSettingsPopup.SetActive(true);
        //SceneManager.LoadScene(sceneName: "Game");
    }
    
    public void CloseMatchSettings() //TODO: MAKE TOGGLE?
    {
        _showMatchSettings = false;
        _matchSettingsPopup.SetActive(false);
    }

    public void Options()
    {
        _settingsPopup.SetActive(true);
    }
    
    public void CloseOptions() //TODO: MAKE TOGGLE?
    {
        _settingsPopup.SetActive(false);
    }
    public void SetNumberOfPlayers(int num)
    {
        NumberOfPlayers = num + 2;
        PlayerColours = new string[num + 2];
        
        //Add or remove colour selection containers according to number of players
        
        _colourHParent = GameObject.Find("ColourHParent");

        if (_colourHParent.transform.childCount > 0)
        {
            for (int j = 0; j < _colourHParent.transform.childCount; j++)
            {
                Destroy(_colourHParent.transform.GetChild(j).gameObject);
            }   
        }

        for (int i = 0; i < NumberOfPlayers; i++)
        {
            var newContainer = Instantiate(_colourHPrefab, Vector3.zero, Quaternion.identity);
            
            newContainer.transform.SetParent(_colourHParent.transform);
            
            newContainer.GetComponentInChildren<TMP_Text>().text = "Player " + (i + 1) + " Colour";
            
            newContainer.GetComponent<ColourDropdownBehaviour>().PlayerIndex = i;
        }

        for (int i = 0; i < NumberOfPlayers; i++)
        {
            PlayerColours[i] = "#FF0000";
        }
    }
    
    public void SetNumberOfPlayerUnits(int num)
    {
        _numberOfPlayerUnits = num + 1;
        
        GameManager.Instance.NumberOfStartingUnits = num + 1;
    }

    public void StartMatch()
    {
        StartCoroutine(LoadAsyncScene());
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

    public void Quit()
    {
        Application.Quit();
    }
}
