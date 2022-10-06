using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MatchSettingsPopup;
    [SerializeField] private GameObject SettingsPopup;
    [SerializeField] private GameObject ColourHParent;
    [SerializeField] private GameObject ColourHPrefab;
    private bool showMatchSettings;

    public GameObject map;
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
            playerScript.OriginalPlayerIndex = i;
            
            GameManager.Instance.playerList.Add(newPlayer.GetComponent<PlayerBehaviour>());
            GameManager.Instance.PlayerQueue.Add(newPlayer.GetComponent<PlayerBehaviour>());

            for (int j = 0; j < numberOfPlayerUnits; j++)
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
                playerScript.unitList.Add(newUnit.GetComponent<UnitBehaviour>());

                /*var newUnit = Instantiate(UnitPrefab, 
                    new Vector3(xPos, yPos + 10f, zPos),
                    Quaternion.identity);*/

                var unitScript = newUnit.GetComponent<UnitBehaviour>();
                unitScript.enabled = true;
                unitScript.Owner = (UnitBehaviour.PlayerNumber) i;
                
                var color = Color.black;
                ColorUtility.TryParseHtmlString(PlayerColours[i], out color);
                unitScript.PlayerColour = color;
                
                newUnit.name = unitScript.Owner + " Unit " + (j + 1);
                
                GameManager.Instance.unitList.Add(newUnit.GetComponent<UnitBehaviour>());
            }
            
            SceneManager.MoveGameObjectToScene(map, SceneManager.GetSceneByName(GameScene));
            SceneManager.MoveGameObjectToScene(newPlayer, SceneManager.GetSceneByName(GameScene));
        }
        
        // Unload the previous Scene
        GameManager.Instance.matchStarted = true;
        SceneManager.UnloadSceneAsync(currentScene);
    }

    private GameObject SpawnUnitAtLocation(Vector3 closestPoint)
    {
        var newUnit = Instantiate(UnitPrefab, closestPoint, Quaternion.identity);

        return newUnit;
    }

    private Vector3 GenerateSpawnPoint()
    {
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        
        var xPos = UnityEngine.Random.Range(-27f, 27f);
        var zPos = UnityEngine.Random.Range(-27f, 27f);
        var yPos = UnityEngine.Random.Range(0f, 50f);

        Vector3 spawnPoint = new Vector3(xPos, yPos, zPos);

        float unitRadius = UnitPrefab.GetComponent<Collider>().bounds.extents.x;

        var minDistance = float.PositiveInfinity;
        
        var closestPointToCollider = Vector3.zero;

        foreach (var collider in map.GetComponentsInChildren<Collider>())
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
        ColourHParent = GameObject.Find("ColourHParent");
        Cursor.lockState = CursorLockMode.None;
        MatchSettingsPopup.SetActive(false);
        GameManager.Instance.gameOver = false;
        GameManager.Instance.matchStarted = false;
        
        PlayerColours = new string[2];
        
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
    
    public void CloseMatchSettings() //TODO: MAKE TOGGLE?
    {
        showMatchSettings = false;
        MatchSettingsPopup.SetActive(false);
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
        numberOfPlayers = num + 2;
        PlayerColours = new string[num + 2];
        
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

    public void Quit()
    {
        Application.Quit();
    }
}
