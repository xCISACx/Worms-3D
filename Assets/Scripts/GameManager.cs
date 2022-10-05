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
    public List<PlayerBehaviour> DeadPlayers;
    public UIReferences UIReferences;
    public bool matchStarted = false;
    public bool initDone = false;
    public bool isTurnStarting;
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
    public UnityEvent NextTurnEvent = new UnityEvent();
    public UnityEvent ShotFiredEvent = new UnityEvent();

    public List<PlayerBehaviour> PlayerQueue = new List<PlayerBehaviour>();

    private Coroutine _turnEndCoroutine = null;

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
        //Debug.Log("LOADING PREFERENCES...");
        
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

            if (turnTimer <= 0f && !isTurnStarting)
            {
                StartNextTurn();
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
            
            /*if (!gameOver && (!mainCamera.Follow || !mainCamera.LookAt) && (_currentPlayer.currentUnit.transform))
            {
                mainCamera.Follow = _currentPlayer.currentUnit.transform;
                mainCamera.LookAt = _currentPlayer.currentUnit.transform;
            }*/
        }
    }
    
    public IEnumerator WaitForTurnToEnd()
    {
        _currentPlayer.currentUnit.canMove = false;
        
        startTurnTimer = false;
        
        //Debug.LogWarning("waiting for turn to end coroutine 5");
        
        //_currentPlayer.currentUnit.canAct = false;

        yield return new WaitForSeconds(3);

        //Debug.LogWarning("3 seconds passed 6");
        CheckForDeadPlayers();
        NextTurn();
    }

    private void Win()
    {
        _currentPlayer.canPlay = false;
        UIReferences.WinCanvas.SetActive(true);
        UIReferences.WinCanvasText.text = AlivePlayers[0].name + " WINS!";

        if (Input.GetKeyDown(KeyCode.F))
        {
            ResetGame();
            
            //Debug.Log("setting game over to false");
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
            InitMainCamera();

            ResetUnit();
        
            //_currentPlayer.currentUnit.highlighted = false;
            
            //Debug.Log("previous unit: " + _currentPlayer.currentUnit);
            
            //_currentPlayer.currentUnit.canMove = false;
            
            //_currentPlayer.currentUnit.canAct = false;
            
            //_currentPlayer.currentUnit.canSwitchWeapon = false;
        
            //_currentPlayer = playerList[currentPlayerIndex];
            
            NextUnit();

            //SetUnitValues();

            SetCurrentPlayerEvent.Invoke(_currentPlayer);

            //Debug.Log("new unit: " + _currentPlayer.currentUnit);
            
            InitMainCamera();
        
            /*mainCamera.Follow = _currentPlayer.currentUnit.transform;
            mainCamera.LookAt = _currentPlayer.currentUnit.transform;*/
            
            InitUI();
        }
    }

    private void ResetUnit()
    {
        _currentPlayer.currentUnit.highlighted = false;
        
        _currentPlayer.currentUnit.canMove = false;
        
        _currentPlayer.currentUnit.canSwitchWeapon = false;
    }

    public void PickUnit()
    {
        if (!_currentPlayer.roundUnitPicked)
        {
            _currentPlayer.canChangeTurn = true;
            
            _currentPlayer.currentUnit.highlighted = false;
            
            _currentPlayer.unitPickedFlag = true;
            
            _currentPlayer.roundUnitPicked = true;
            
            _currentPlayer.currentUnit.canMove = true;
            
            _currentPlayer.currentUnit.canTakeDamage = true;
            
            _currentPlayer.currentUnit.canTakeFallDamage = true;
            
            //_currentPlayer.currentUnit.canAct = true;
            
            _currentPlayer.currentUnit.canSwitchWeapon = true;
            
            SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);
            
            _currentPlayer.currentUnit.GetComponent<Rigidbody>().isKinematic = false;

            //Debug.Log("setting kinematic to false pick");

            turnTimer = defaultTurnTime;
            
            startTurnTimer = true;
        }
    }
    
    // This function runs once after all the units spawn

    void Init()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        UIReferences = FindObjectOfType<UIReferences>();
        
        UIReferences.WinCanvas.SetActive(false);
        
        mainCamera = FindObjectOfType<CinemachineFreeLook>();

        InitCurrentPlayer();
        
        InitUI();

        /*currentPlayerIndex = 0;
        
        _currentPlayer = playerList[currentPlayerIndex];
        
        _currentPlayer.currentUnit = _currentPlayer.unitList[0];*/

        /*SetCurrentPlayerEvent.Invoke(_currentPlayer);
        SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);*/
        
        PlayerDiedEvent.AddListener(OnPlayerDied);
        //NextTurnEvent.AddListener(NextTurn);
        ShotFiredEvent.AddListener(StartNextTurn);

        InitMainCamera();

        InitUI();

        //PlayerControls.Player.ChangeUnit.started += ChangeUnit;

        /*PlayerControls.Player.Jump.started += _currentPlayer.currentUnit.Jump;
        PlayerControls.Player.PickUnit.started += PickUnit;
        PlayerControls.Player.Fire.started += _currentPlayer.currentUnit.EquipWeapon;*/
        
        //Debug.Log(_currentPlayer);
        
        initDone = true;
    }

    private void InitUI()
    {
        /*UIReferences = FindObjectOfType<UIReferences>();
        UIReferences.WinCanvas.SetActive(false);*/
        
        UIReferences.currentPlayerText.text = "Current Player: " + (_currentPlayer.OriginalPlayerIndex + 1);
        
        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
    }

    private void InitMainCamera()
    {
        //mainCamera = FindObjectOfType<CinemachineFreeLook>();

        mainCamera.Follow = _currentPlayer.currentUnit.transform;
        mainCamera.LookAt = _currentPlayer.currentUnit.transform;
        
        mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";
    }

    private void OnPlayerDied(PlayerBehaviour player)
    {
        Debug.LogWarning(player, player);
        
        var barToDestroy = player.TeamHPBar.gameObject;
        
        //Debug.Log(barToDestroy.name);

        UIReferences.GlobalHPBarParent.HPBars.Remove(player.TeamHPBar);
        
        Destroy(barToDestroy.gameObject);
        
        //Debug.Log("Destroyed bar " + barToDestroy.name + " belonging to player " + (currentPlayerIndex + 1));
        
        AlivePlayers.Remove(player);
            
        Debug.Log("removing from alive players " + player);
        
        playerList.Remove(player);

        Debug.Log("Removed from player list " + player);
        
        PlayerQueue.Remove(player);
        
        Debug.LogWarning("Removed from player queue " + player);
        
        Destroy(player.gameObject);

        /*if (playerList.Count > 0)
        {
            NextPlayer();
        }*/
        
        //_currentPlayer.currentUnit = _currentPlayer.unitList[0];
        
        //SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);
    }

    public void StartNextTurn()
    {
        //Debug.LogWarning("starting next turn flag 1 " + isTurnStarting);
        
        if (!isTurnStarting)
        {
            //Debug.LogWarning("start next turn if 2 " + isTurnStarting);
            
            _turnEndCoroutine = StartCoroutine(WaitForTurnToEnd());
            
            //Debug.LogWarning("coroutine called if 3 " + isTurnStarting);
            
            isTurnStarting = true;
        }
        else
        {
            //Debug.LogWarning("start next turn else 4 " + isTurnStarting);
            
            //StopCoroutine(_turnEndCoroutine);
        }
    }

    public void CheckForDeadPlayers()
    {
        foreach (var player in playerList)
        {
            if (player.unitList.Count == 0) //If this was the current player's last unit
            { 
                DeadPlayers.Add(player);
            }
        }

        foreach (var player in DeadPlayers)
        {
            if (player)
            {
                player.SelfDestruct(); //OnPlayerDied
            }
        }
        
        DeadPlayers.Clear();
    }

    public void NextTurn()
    {
        Debug.Log("starting next turn");
        
        InitTurn();
        
        // Add the previous player to the end of the queue as they just finished their turn
        
        PlayerQueue.Add(_currentPlayer);
        
        // This shouldn't be necessary but the SelfDestruct fails to remove from the PlayerQueue

        for (int i = 0; i < PlayerQueue.Count; i++)
        {
            var player = PlayerQueue[i];

            if (player == null)
            {
                PlayerQueue.RemoveAt(i);
            }
        }

        NextPlayer();
        
        CheckForDeadPlayers();

        SetCurrentPlayerValues();

        //MakeAllUnitsKinematic();

        if (_currentPlayer.unitList.Count > 0)
        {
            FollowCurrentPlayer();
            
            // If the current unit has a weapon equipped

            if (_currentPlayer.currentUnit.currentWeaponObject)
            {
                SetFirstPersonCamera();
            }

            foreach (var unit in unitList)
            {
                unit.SetHighlight();
            }

            //_currentPlayer.currentUnit.SetHighlight();
        }
        
        InitUI();

        //Debug.Log(_currentPlayer);
    }

    public void MakeAllUnitsKinematic()
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            unitList[i].GetComponent<Rigidbody>().isKinematic = true;
        }
        
        //Debug.Log("setting all to kinematic");
    }

    public void SetSelfDestructed(bool selfDestructed)
    {
        _currentPlayer.SelfDestructed = selfDestructed;
    }

    public void SetCurrentPlayerValues()
    {
        _currentPlayer.roundUnitPicked = false;

        for (int i = 0; i < playerList.Count; i++)
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
            
            playerList[i].currentUnit.canTakeDamage = true;
            playerList[i].currentUnit.canTakeFallDamage = true;
        }
    }

    private void SetFirstPersonCamera()
    {
        var currentWeaponScript = _currentPlayer.currentUnit.currentWeaponObject.GetComponent<WeaponBehaviour>();
                
        firstPersonCamera = currentWeaponScript.FPSCamera;
        firstPersonCamera = currentWeaponScript.FPSCamera;
                
        firstPersonCamera.Follow = null;
                
        firstPersonCamera.transform.position = currentWeaponScript.shootPoint.transform.position;
                
        firstPersonCamera.LookAt = currentWeaponScript.lookPoint.transform;
    }

    private void FollowCurrentPlayer()
    {
        mainCamera.Follow = _currentPlayer.currentUnit.transform;
        mainCamera.LookAt = _currentPlayer.currentUnit.transform;
            
        Debug.Log("setting camera follow to: " + _currentPlayer.currentUnit.transform);
    }

    public void InitTurn()
    {
        //Debug.LogWarning("next turn init turn");

        _currentPlayer.canChangeTurn = false;
        
        isTurnStarting = false;
        
        // Set the FPS Camera's priority to 0 so the unit's camera becomes the default
        
        if (firstPersonCamera)
        {
            firstPersonCamera.Priority = 0;
        }
        
        // Re-enable the camera movement by setting the mouse axes string properties

        mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";

        // Disable the highlight of the previous player's unit before switching to the next player
        
        _currentPlayer.currentUnit.highlighted = false;
        
        // Reset the shots of the previous player's unit before switching to the next player   
        
        _currentPlayer.currentUnit.shotsFiredDuringRound = 0;

        startTurnTimer = false;
        
        turnTimer = defaultTurnTime;
    }

    public void NextPlayer()
    {
        Debug.LogWarning("switching from player " + (currentPlayerIndex + 1) + " index: " + currentPlayerIndex);

        if (!_currentPlayer.SelfDestructed)
        {
            PlayerQueue.RemoveAt(0);
        }

        _currentPlayer = PlayerQueue[0];
        
        /*currentPlayerIndex++;

        currentPlayerIndex %= playerList.Count;

        _currentPlayer = playerList[currentPlayerIndex];*/
        
        SetCurrentPlayerEvent.Invoke(_currentPlayer);
        
        Debug.LogWarning("to player " + (currentPlayerIndex + 1) + " index: " + currentPlayerIndex);

        _currentPlayer.currentUnit.highlighted = true;

        InitUI();
        
        Debug.LogWarning("UI Switching from player " + (_currentPlayer.OriginalPlayerIndex - 1) + " to player" + (_currentPlayer.OriginalPlayerIndex + 1));
    }

    public void InitCurrentPlayer()
    {
        _currentPlayer = playerList[0];
        
        _currentPlayer.currentUnit = _currentPlayer.unitList[0];
        
        _currentPlayer.canChangeTurn = true;
        
        _currentPlayer.canPlay = true;
        
        _currentPlayer.turnStarted = true;

        _currentPlayer.currentUnit.highlighted = true;
        
        SetCurrentPlayerEvent.Invoke(_currentPlayer);
        
        InitUI();
        
        Debug.Log("Switching to player" + (currentPlayerIndex + 1));
    }

    public void NextUnit()
    {
        _currentPlayer.currentUnitIndex++;
        _currentPlayer.currentUnitIndex %= _currentPlayer.unitList.Count;
        
        _currentPlayer.currentUnit = _currentPlayer.unitList[_currentPlayer.currentUnitIndex];
        
        _currentPlayer.currentUnit.highlighted = true;
            
        SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);
        
        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
        
        Debug.Log("Switching to player " + (currentPlayerIndex + 1) + "'s unit " + (_currentPlayer.currentUnitIndex + 1));
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
