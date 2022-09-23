using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using Cinemachine;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Prefs prefs;
    public int NumberOfStartingUnits = 1;
    public List<PlayerBehaviour> AlivePlayers;
    public UIReferences UIReferences;
    public bool matchStarted = false;
    public bool initDone = false;
    public bool changeTurnFlag;
    public CinemachineFreeLook mainCamera;
    public CinemachineVirtualCamera firstPersonCamera;
    public int currentPlayerIndex;
    public PlayerBehaviour _currentPlayer;
    public List<PlayerBehaviour> playerList;
    public List<UnitBehaviour> unitList;
    public float defaultTurnTime = 60f;
    public float turnTimer = 60f;
    public bool startTurnTimer;
    public bool gameOver = false;
    public AudioMixer AudioMixer;
    public AudioSource MusicSource;
    public AudioSource SFXSource;
    public MenuManager MenuManager;
    public Canvas SettingsPopup;
    public Worms3D PlayerControls;
    public GameObject Reticle;

    private void Awake()
    {
        if (!Instance)
        { 
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        MenuManager = FindObjectOfType<MenuManager>();

        //LoadPlayerPrefs();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        LoadPrefs();
        MenuManager.LoadMenuUIValues();
    }

    private void LoadPrefs()
    {
        Debug.Log("LOADING PREFERENCES...");
        
        prefs = Resources.Load<Prefs>("Prefs");
        
        AudioMixer.SetFloat("masterVolume", prefs.masterVolume);
        AudioMixer.SetFloat("musicVolume", prefs.musicVolume);
        AudioMixer.SetFloat("sfxVolume", prefs.sfxVolume);
        
        Screen.SetResolution(prefs.resolutionW, prefs.resolutionH, prefs.fullScreenMode);
        Screen.fullScreen = prefs.fullscreen;
    }

    private void OnEnable()
    {
        PlayerControls = new Worms3D();
        PlayerControls.Enable();
    }

    private void OnDisable()
    {
        PlayerControls?.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!initDone && playerList.Count > 0)
        {
            Init();
        }
        
        if (matchStarted)
        {
            if (PlayerControls.Player.ChangeTurn.triggered && _currentPlayer.canChangeTurn)
            {
                NextTurn(false);
            }
        
            if (!_currentPlayer.roundUnitPicked && PlayerControls.Player.ChangeUnit.triggered)
            {
                ChangeUnit();
            }

            if (PlayerControls.Player.PickUnit.triggered)
            {
                PickUnit();
            }

            if (startTurnTimer)
            {
                turnTimer -= Time.deltaTime;
            }

            if (turnTimer <= 0f)
            {
                NextTurn(false);
            }
            
            if (UIReferences)
            {
                UIReferences.timerText.text = turnTimer.ToString("F0");   
            }

            if (AlivePlayers.Count <= 1)
            {
                gameOver = true;
            }

            if (gameOver)
            {
                if (AlivePlayers.Count == 1)
                {
                    Win();
                }
                else if (AlivePlayers.Count == 0)
                {
                    Tie();
                }   
            }
            
            if ((!mainCamera.Follow || !mainCamera.LookAt) && (_currentPlayer.currentUnit.transform && !gameOver))
            {
                mainCamera.Follow = _currentPlayer.currentUnit.transform;
                mainCamera.LookAt = _currentPlayer.currentUnit.transform;
            }
        }
    }
    
    public IEnumerator WaitForTurnToEnd()
    {
        Debug.Log("waiting for turn to end");
        _currentPlayer.currentUnit.canAct = false;
        //_currentPlayer.currentUnit = _currentPlayer.unitList[0];
        changeTurnFlag = true;

        yield return new WaitForSeconds(3);
        if (changeTurnFlag)
        {
            Debug.Log("coroutine turn end");
            NextTurn(false);
        }
    }

    private void Win()
    {
        _currentPlayer.canPlay = false;
        UIReferences.WinCanvas.SetActive(true);
        UIReferences.WinCanvasText.text = AlivePlayers[0].name + " WINS!";

        if (PlayerControls.Player.PickUnit.triggered)
        {
            ResetGame();
            
            Debug.Log("setting game over to false");
            SceneManager.LoadScene(0);
        }
    }
    
    private void Tie()
    {
        _currentPlayer.canPlay = false;
        UIReferences.WinCanvas.SetActive(true);
        UIReferences.WinCanvasText.text = "It's a tie!";

        if (PlayerControls.Player.PickUnit.triggered)
        {
            ResetGame();
            SceneManager.LoadScene(0);
        }
    }

    public void ResetGame()
    {
        gameOver = false;
            
        playerList.Clear();
        unitList.Clear();
        AlivePlayers.Clear();
            
        initDone = false;
    }

    private void ChangeUnit()
    {
        mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";
        
        _currentPlayer.currentUnit.highlighted = false;
        //Debug.Log("previous unit: " + _currentPlayer.currentUnit);
        _currentPlayer.currentUnit.canMove = false;
        _currentPlayer.currentUnit.canAct = false;
        _currentPlayer.currentUnit.canSwitchWeapon = false;
        
        _currentPlayer = playerList[currentPlayerIndex];
        _currentPlayer.currentUnitIndex++;
        _currentPlayer.currentUnitIndex %= _currentPlayer.unitList.Count;
        _currentPlayer.currentUnit = _currentPlayer.unitList[_currentPlayer.currentUnitIndex];
        
        //Debug.Log("new unit: " + _currentPlayer.currentUnit);
        
        mainCamera.Follow = _currentPlayer.currentUnit.transform;
        mainCamera.LookAt = _currentPlayer.currentUnit.transform;

        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
        _currentPlayer.currentUnit.highlighted = true;
        _currentPlayer.currentUnit.canAim = false;
    }

    private void PickUnit()
    {
        _currentPlayer.unitPickedFlag = true;
        _currentPlayer.roundUnitPicked = true;
        _currentPlayer.currentUnit.canMove = true;
        _currentPlayer.currentUnit.highlighted = false;
        _currentPlayer.currentUnit.canAct = true;
        _currentPlayer.currentUnit.canSwitchWeapon = true;

        turnTimer = defaultTurnTime;
        startTurnTimer = true;
    }

    void Init()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _currentPlayer = playerList[currentPlayerIndex];
        
        Reticle = GameObject.FindWithTag("Reticle");
        
        mainCamera = FindObjectOfType<CinemachineFreeLook>();

        mainCamera.Follow = _currentPlayer.currentUnit.transform;
        mainCamera.LookAt = _currentPlayer.currentUnit.transform;

        UIReferences = FindObjectOfType<UIReferences>();
        UIReferences.WinCanvas.SetActive(false);
        UIReferences.currentPlayerText.text = "Current Player: " + (currentPlayerIndex + 1);
        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
        
        playerList[currentPlayerIndex].canPlay = true;
        playerList[currentPlayerIndex].turnStarted = true;
        _currentPlayer.currentUnit.highlighted = true;
        _currentPlayer.currentUnit.canAim = false;
        
        //Debug.Log(_currentPlayer);
        initDone = true;
    }

    public void NextTurn(bool selfdeath)
    {
        Debug.Log("next turn");
        changeTurnFlag = false;
        firstPersonCamera.Priority = 0;
        
        mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";

        _currentPlayer.currentUnit.highlighted = false; // disable the highlight of the previous player's unit before switching to the next player
        _currentPlayer.currentUnit.shotsFiredDuringRound = 0; // disable the highlight of the previous player's unit before switching to the next player   

        startTurnTimer = false;
        
        currentPlayerIndex++;
        currentPlayerIndex %= playerList.Count;
        
        _currentPlayer = playerList[currentPlayerIndex];

        if (_currentPlayer.unitList.Count > 0)
        {
            mainCamera.Follow = _currentPlayer.currentUnit.transform;
            mainCamera.LookAt = _currentPlayer.currentUnit.transform;

            //firstPersonCamera.Follow = _currentPlayer.currentUnit.FPSTarget.transform;
            //firstPersonCamera.LookAt = _currentPlayer.currentUnit.FPSTarget.transform;
            
            _currentPlayer.currentUnit.highlighted = true;
        }
        
        _currentPlayer.currentUnit.canAim = false;

        _currentPlayer.roundUnitPicked = false;
        
        UIReferences.currentPlayerText.text = "Current Player: " + (currentPlayerIndex + 1);
        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
        
        turnTimer = defaultTurnTime;
        startTurnTimer = true;

        for (int i = 0; i < GameManager.Instance.playerList.Count; i++)
        {
            if (i == currentPlayerIndex)
            {
                playerList[i].canPlay = true;
                playerList[i].currentUnit.canMove = true;
                playerList[i].turnStarted = true;
            }
            else
            {
                playerList[i].canPlay = false;
                playerList[i].currentUnit.canMove = false;
                playerList[i].turnStarted = false;
            }
        }
            
        //Debug.Log(_currentPlayer);
    }
}
