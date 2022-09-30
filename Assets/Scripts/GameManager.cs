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
using UnityEngine.Events;
using UnityEngine.InputSystem;
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
    //public Worms3D PlayerControls;
    public int fallDamageTreshold;
    public UnityEvent<PlayerBehaviour> SetCurrentPlayerEvent = new UnityEvent<PlayerBehaviour>();
    public UnityEvent<UnitBehaviour> SetCurrentUnitEvent = new UnityEvent<UnitBehaviour>();
    public UnityEvent<PlayerBehaviour> PlayerDiedEvent = new UnityEvent<PlayerBehaviour>();

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

        NumberOfStartingUnits = 1;

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

    // Update is called once per frame
    void Update()
    {
        if (!initDone && playerList.Count > 0)
        {
            Init();
        }
        
        if (matchStarted)
        {
            //s_currentPlayer.currentUnit.movementValue = PlayerControls.Player.Move.ReadValue<Vector2>();

            /*if (PlayerControls.Player.ChangeTurn.triggered && _currentPlayer.canChangeTurn)
            {
                NextTurn();
            }

            if (_currentPlayer.turnStarted)
            {
                _currentPlayer.currentUnit.InitTurn();
            }
            
            if (_currentPlayer.unitPickedFlag)
            {
                _currentPlayer.currentUnit.InitUnit();
            }*/
        
            /*if (!_currentPlayer.roundUnitPicked && PlayerControls.Player.ChangeUnit.triggered)
            {
                ChangeUnit();
            }*/

            /*if (PlayerControls.Player.PickUnit.triggered)
            {
                PickUnit();
            }*/

            if (startTurnTimer)
            {
                turnTimer -= Time.deltaTime;
            }

            if (turnTimer <= 0f)
            {
                StartCoroutine(WaitForTurnToEnd());
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
            
            if (!gameOver && (!mainCamera.Follow || !mainCamera.LookAt) && (_currentPlayer.currentUnit.transform))
            {
                mainCamera.Follow = _currentPlayer.currentUnit.transform;
                mainCamera.LookAt = _currentPlayer.currentUnit.transform;
            }
        }
    }
    
    public IEnumerator WaitForTurnToEnd()
    {
        _currentPlayer.currentUnit.canMove = false;
        startTurnTimer = false;
        Debug.Log("waiting for turn to end");
        //_currentPlayer.currentUnit.canAct = false;
        changeTurnFlag = true;

        yield return new WaitForSeconds(3);
        if (changeTurnFlag)
        {
            Debug.Log("coroutine turn end");
            NextTurn();
        }
    }

    private void Win()
    {
        _currentPlayer.canPlay = false;
        UIReferences.WinCanvas.SetActive(true);
        UIReferences.WinCanvasText.text = AlivePlayers[0].name + " WINS!";

        if (Input.GetKeyDown(KeyCode.F))
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

        if (Input.GetKeyDown(KeyCode.F))
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

    public void ChangeUnit()
    {
        if (!_currentPlayer.roundUnitPicked)
        {
            mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
            mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";
        
            _currentPlayer.currentUnit.highlighted = false;
            //Debug.Log("previous unit: " + _currentPlayer.currentUnit);
            _currentPlayer.currentUnit.canMove = false;
            //_currentPlayer.currentUnit.canAct = false;
            _currentPlayer.currentUnit.canSwitchWeapon = false;
        
            _currentPlayer = playerList[currentPlayerIndex];
            _currentPlayer.currentUnitIndex++;
            _currentPlayer.currentUnitIndex %= _currentPlayer.unitList.Count;
            _currentPlayer.currentUnit = _currentPlayer.unitList[_currentPlayer.currentUnitIndex];
            
            SetCurrentPlayerEvent.Invoke(_currentPlayer);
            SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);
        
            //Debug.Log("new unit: " + _currentPlayer.currentUnit);
        
            mainCamera.Follow = _currentPlayer.currentUnit.transform;
            mainCamera.LookAt = _currentPlayer.currentUnit.transform;

            UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
            _currentPlayer.currentUnit.highlighted = true;
        }
    }

    public void PickUnit()
    {
        if (!_currentPlayer.roundUnitPicked)
        {
            _currentPlayer.currentUnit.highlighted = false;
            _currentPlayer.unitPickedFlag = true;
            _currentPlayer.roundUnitPicked = true;
            _currentPlayer.currentUnit.canMove = true;
            _currentPlayer.currentUnit.canTakeDamage = true;
            //_currentPlayer.currentUnit.canAct = true;
            _currentPlayer.currentUnit.canSwitchWeapon = true;
            
            SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);

            turnTimer = defaultTurnTime;
            startTurnTimer = true;    
        }
    }

    void Init()
    {
        Cursor.lockState = CursorLockMode.Locked;

        currentPlayerIndex = 0;
        _currentPlayer = playerList[currentPlayerIndex];
        _currentPlayer.currentUnit = _currentPlayer.unitList[0];
        _currentPlayer.canChangeTurn = true;
        
        PlayerDiedEvent.AddListener(OnPlayerDied);

        SetCurrentPlayerEvent.Invoke(_currentPlayer);
        SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);

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

        //PlayerControls.Player.ChangeUnit.started += ChangeUnit;

        /*PlayerControls.Player.Jump.started += _currentPlayer.currentUnit.Jump;
        PlayerControls.Player.PickUnit.started += PickUnit;
        PlayerControls.Player.Fire.started += _currentPlayer.currentUnit.EquipWeapon;*/
        
        //Debug.Log(_currentPlayer);
        initDone = true;
    }

    private void OnPlayerDied(PlayerBehaviour player)
    {
        playerList.Remove(player);

        var barToDestroy = UIReferences.GlobalHPBarParent.HPBars[currentPlayerIndex];
        
        UIReferences.GlobalHPBarParent.HPBars.RemoveAt(currentPlayerIndex);
        
        Destroy(barToDestroy.gameObject);
        
        if (playerList.Count > 0)
        {
            currentPlayerIndex++;
            currentPlayerIndex %= playerList.Count;   
        }
        
        _currentPlayer = playerList[currentPlayerIndex];
    }

    public void NextTurn()
    {
        Debug.Log("next turn");
        changeTurnFlag = false;
        
        if (firstPersonCamera)
        {
            firstPersonCamera.Priority = 0;
        }

        mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";

        _currentPlayer.currentUnit.highlighted = false; // disable the highlight of the previous player's unit before switching to the next player
        _currentPlayer.currentUnit.shotsFiredDuringRound = 0; // reset the shots of the previous player's unit before switching to the next player   

        startTurnTimer = false;
        turnTimer = defaultTurnTime;
        
        currentPlayerIndex++;
        currentPlayerIndex %= playerList.Count;

        _currentPlayer = playerList[currentPlayerIndex];
        
        SetCurrentPlayerEvent.Invoke(_currentPlayer);

        _currentPlayer.roundUnitPicked = false;
        
        UIReferences.currentPlayerText.text = "Current Player: " + (currentPlayerIndex + 1);
        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);

        for (int i = 0; i < playerList.Count; i++)
        {
            if (i == currentPlayerIndex)
            {
                playerList[i].canPlay = true;
                playerList[i].currentUnit.canMove = true;
                playerList[i].turnStarted = true;
                playerList[i].canChangeTurn = true;
            }
            else
            {
                playerList[i].canPlay = false;
                playerList[i].currentUnit.canMove = false;
                playerList[i].turnStarted = false;
                playerList[i].canChangeTurn = false;
            }
            
            playerList[i].currentUnit.canTakeDamage = true;
        }
        
        if (_currentPlayer.unitList.Count > 0)
        {
            mainCamera.Follow = _currentPlayer.currentUnit.transform;
            mainCamera.LookAt = _currentPlayer.currentUnit.transform;
            Debug.Log("setting camera follow to: " + _currentPlayer.currentUnit.transform);

            if (_currentPlayer.currentUnit.currentWeaponObject)
            {
                var currentWeaponScript = _currentPlayer.currentUnit.currentWeaponObject.GetComponent<WeaponBehaviour>();
                
                firstPersonCamera = currentWeaponScript.FPSCamera;
                firstPersonCamera = currentWeaponScript.FPSCamera;
                
                firstPersonCamera.Follow = null;
                
                firstPersonCamera.transform.position = currentWeaponScript.shootPoint.transform.position;
                
                firstPersonCamera.LookAt = currentWeaponScript.lookPoint.transform;
            }

            _currentPlayer.currentUnit.highlighted = true;
        }

        //Debug.Log(_currentPlayer);
    }
    
    public void SpawnDamagePopUp(Transform parent, Vector3 offset, int damage)
    {
        var damagePrefab = Instantiate(UIReferences.DamagePopUpPrefab, Vector3.zero, Quaternion.identity);
        damagePrefab.transform.SetParent(parent);
        damagePrefab.transform.localPosition = Vector3.zero + offset;
        damagePrefab.GetComponent<DamagePopUpBehaviour>().initialPosition = damagePrefab.transform.localPosition;
        damagePrefab.GetComponent<DamagePopUpBehaviour>().DamageText.text = "- " + damage;
    }
}
