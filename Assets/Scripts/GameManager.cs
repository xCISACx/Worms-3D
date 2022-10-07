using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Prefs prefs;
    public int NumberOfStartingUnits = 1;
    
    [Header("Player Lists")]
    
    public List<PlayerBehaviour> playerList;
    public List<PlayerBehaviour> PlayerQueue = new List<PlayerBehaviour>();
    public List<PlayerBehaviour> AlivePlayers;
    public List<UnitBehaviour> unitList;
    
    [Header("References")]
    
    public MenuManager MenuManager;
    
    public UIReferences UIReferences;
    
    public CinemachineFreeLook mainCamera;
    public CinemachineVirtualCamera firstPersonCamera;
    
    public PlayerBehaviour _currentPlayer;

    public AudioMixer AudioMixer;
    
    public AudioSource MusicSource;
    public AudioSource SFXSource;

    [SerializeField] public WaterBehaviour Water;
    
    private Coroutine _turnEndCoroutine = null;
    private Coroutine _waterCoroutine = null;
    
    [Header("Conditions")]
    
    public bool initDone = false;
    
    public bool matchStarted = false;
    public bool gameOver = false;
    
    public bool startTurnTimer;
    public bool startEndTurnTimer;
    public bool isTurnStarting;
    
    [Header("Values")]
    
    public int currentPlayerIndex;
    
    [SerializeField] private int turnNumber = 0;
    private float _defaultTurnTime = 60f;
    private float _defaultEndTurnTime = 3f;
    public float _turnTimer = 60f;
    private float _endTurnTimer = 3f;
    
    public int playersEliminatedDuringTurn = 0;
    
    public int fallDamageTreshold;
    
    [Header("Events")]
    
    public UnityEvent<PlayerBehaviour> SetCurrentPlayerEvent = new UnityEvent<PlayerBehaviour>();
    public UnityEvent<UnitBehaviour> SetCurrentUnitEvent = new UnityEvent<UnitBehaviour>();
    public UnityEvent<PlayerBehaviour> PlayerDiedEvent = new UnityEvent<PlayerBehaviour>();
    public UnityEvent ShotFiredEvent = new UnityEvent();
    
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

        Debug.Log("gm awake");

        //LoadPlayerPrefs();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        LoadPrefs();
        MenuManager.LoadMenuUIValues();
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
            if (startTurnTimer)
            {
                _turnTimer -= Time.deltaTime;
            }

            if (_turnTimer <= 0f && !isTurnStarting)
            {
                StartNextTurn();
            }
            
            if (startEndTurnTimer)
            {
                _endTurnTimer -= Time.deltaTime;
            }
            
            if (UIReferences)
            {
                UIReferences.timerText.text = _turnTimer.ToString("F0");
                UIReferences.EndTurnTimerText.text = _endTurnTimer.ToString("F0");
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
        }
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

    public IEnumerator WaitForTurnToEnd()
    {
        _currentPlayer.currentUnit.canMove = false;
        _currentPlayer.currentUnit.canShoot = false;
        
        startTurnTimer = false;
        
        startEndTurnTimer = true;
        
        UIReferences.EndTurnTimerText.gameObject.SetActive(true);

        //Debug.Log("waiting for turn to end coroutine 5");

        yield return new WaitForSeconds(_defaultEndTurnTime);

        //Debug.LogWarning("3 seconds passed 6");

        NextTurn();
    }

    private void Win()
    {
        FollowCurrentUnit(AlivePlayers[0].currentUnit);
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
        playersEliminatedDuringTurn = 0;
        
        StopAllCoroutines();
            
        playerList.Clear();
        unitList.Clear();
        AlivePlayers.Clear();
        PlayerQueue.Clear();
            
        initDone = false;
    }

    public void ChangeUnit()
    {
        if (!_currentPlayer.roundUnitPicked)
        {
            InitMainCamera();

            ResetUnit();

            //Debug.Log("previous unit: " + _currentPlayer.currentUnit);

            NextUnit();

            //SetUnitValues();

            SetCurrentPlayerEvent.Invoke(_currentPlayer);

            //Debug.Log("new unit: " + _currentPlayer.currentUnit);
            
            InitMainCamera();

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
        if (!_currentPlayer.roundUnitPicked && !gameOver)
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

            _turnTimer = _defaultTurnTime;
            
            startTurnTimer = true;
        }
    }

    // This function runs once after all the units spawn

    void Init()
    {
        isTurnStarting = false;

        StopAllCoroutines();

        //_turnEndCoroutine = null;

        gameOver = false;

        Cursor.lockState = CursorLockMode.Locked;
        
        UIReferences = FindObjectOfType<UIReferences>();
        
        UIReferences.WinCanvas.SetActive(false);
        
        mainCamera = FindObjectOfType<CinemachineFreeLook>();

        Water = FindObjectOfType<WaterBehaviour>();

        InitCurrentPlayer();
        
        SetCurrentPlayerValues();
        
        InitUI();

        PlayerDiedEvent.AddListener(OnPlayerDied);

        ShotFiredEvent.AddListener(StartNextTurn);

        InitMainCamera();

        InitUI();

        //Debug.Log(_currentPlayer);
        
        initDone = true;
    }

    private void InitUI()
    {
        UIReferences.currentPlayerText.text = "Current Player: " + (_currentPlayer.OriginalPlayerIndex + 1);
        
        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnit.originalIndex + 1);
    }

    private void InitMainCamera()
    {
        mainCamera.Follow = _currentPlayer.currentUnit.transform;
        mainCamera.LookAt = _currentPlayer.currentUnit.transform;
        
        mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";
    }

    private void OnPlayerDied(PlayerBehaviour player)
    {
        playersEliminatedDuringTurn++;
        
        Debug.LogWarning(player, player);
        
        var barToDestroy = player.TeamHPBar.gameObject;

        var playerToDestroy = player;
        
        //Debug.Log(barToDestroy.name);

        UIReferences.GlobalHPBarParent.HPBars.Remove(player.TeamHPBar);
        
        Destroy(barToDestroy.gameObject);
        
        //Debug.Log("Destroyed bar " + barToDestroy.name + " belonging to player " + (currentPlayerIndex + 1));
        
        AlivePlayers.Remove(playerToDestroy);
            
        Debug.Log("removing from alive players " + playerToDestroy);
        
        playerList.Remove(playerToDestroy);

        Debug.Log("Removed from player list " + playerToDestroy);

        PlayerQueue.Remove(playerToDestroy);
        
        Debug.LogWarning("Removed from player queue " + playerToDestroy);
        
        Destroy(playerToDestroy.gameObject);

        //SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);
    }

    public void StartNextTurn()
    {
        Debug.LogWarning("starting next turn flag 1 " + isTurnStarting);
        
        if (!isTurnStarting)
        {
            Debug.LogWarning("start next turn if 2 " + isTurnStarting);
            
            _turnEndCoroutine = StartCoroutine(WaitForTurnToEnd());
            
            Debug.LogWarning("coroutine called if 3 " + isTurnStarting);
            
            isTurnStarting = true;
        }
        else
        {
            Debug.LogWarning("can't start next turn else 4 " + isTurnStarting);
            
            //StopCoroutine(_turnEndCoroutine);
        }
    }

    public void NextTurn()
    {
        if (!gameOver)
        {
            startEndTurnTimer = false;
            
            _endTurnTimer = _defaultEndTurnTime;
            
            UIReferences.EndTurnTimerText.gameObject.SetActive(false);
            
            Debug.Log("starting next turn");
        
            InitTurn();
        
            // Add the previous player to the end of the queue as they just finished their turn, if they are still alive

            if (!_currentPlayer.SelfDestructed)
            {
                PlayerQueue.Add(_currentPlayer);
            }

            var previousPlayer = _currentPlayer;
        
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

            //CheckForDeadPlayers();

            _currentPlayer = PlayerQueue[0];
        
            _currentPlayer.currentUnitIndex = 0;
        
            _currentPlayer.currentUnit = _currentPlayer.unitList[_currentPlayer.currentUnitIndex];
        
            SetCurrentPlayerEvent.Invoke(_currentPlayer);

            SetCurrentPlayerValues();
        
            Debug.Log("switching from " + previousPlayer + " to " + _currentPlayer);

            //MakeAllUnitsKinematic();

            if (_currentPlayer.unitList.Count > 0)
            {
                FollowCurrentUnit(_currentPlayer.currentUnit);
            
                // If the current unit has a weapon equipped

                if (_currentPlayer.currentUnit.currentWeaponObject)
                {
                    SetFirstPersonCamera();
                    
                    _currentPlayer.currentUnit.canShoot = true;
                    _currentPlayer.currentUnit.canAim = true;
                }
                else
                {
                    if (firstPersonCamera)
                    {
                        firstPersonCamera.Follow = null;
                        firstPersonCamera.LookAt = null;   
                    }
                }

                foreach (var unit in unitList)
                {
                    if (unit) // the units fail to remove themselves from the GameManager's unit list so we do this to prevent errors
                    {
                        unit.SetHighlight();
                    }
                }

                //_currentPlayer.currentUnit.SetHighlight();
            }
        
            InitUI();

            //Debug.Log(_currentPlayer);

            if (turnNumber % 4 == 1)
            {
                _waterCoroutine = StartCoroutine(Water.RaiseWaterLevel());
                
                PickupManager.Instance.SpawnRandomPickup();
            }
        }
    }

    public void SetSelfDestructed(bool selfDestructed)
    {
        _currentPlayer.SelfDestructed = selfDestructed;
    }

    public void SetCurrentPlayerValues()
    {
        _currentPlayer.roundUnitPicked = false;

        foreach (var player in playerList)
        {
            if (player == _currentPlayer)
            {
                player.canPlay = true;
                player.currentUnit.canMove = true;
                player.turnStarted = true;
                
                Debug.Log("setting " + player + "'s values");
            }
            else
            {
                player.canPlay = false;
                player.currentUnit.canMove = false;
                player.turnStarted = false;
                Debug.Log("setting everyone else's values");
            }
            
            player.currentUnit.canTakeDamage = true;
            player.currentUnit.canTakeFallDamage = true;
        }

        /*for (int i = 0; i < playerList.Count; i++)
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
        }*/
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

    private void FollowCurrentUnit(UnitBehaviour unit)
    {
        mainCamera.Priority = 100;
        mainCamera.Follow = unit.transform;
        mainCamera.LookAt = unit.transform;
            
        Debug.Log("setting camera follow to: " + unit.transform);
    }

    public void InitTurn()
    {
        playersEliminatedDuringTurn = 0;
        
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
        
        _turnTimer = _defaultTurnTime;
    }

    public void NextPlayer()
    {
        turnNumber++;

        if (_currentPlayer.currentUnit)
        {
            _currentPlayer.currentUnit.GetComponent<Rigidbody>().isKinematic = true;
        }

        //Debug.LogWarning("switching from player " + (currentPlayerIndex + 1) + " index: " + currentPlayerIndex);
        
        // if the current player did not self-destruct and they still have more than 1 unit, remove them from the queue
        // we also check if more than 1 player was eliminated during the round to make sure we don't remove the first element twice

        if (!_currentPlayer.SelfDestructed && playerList.Count > 1 && playersEliminatedDuringTurn < 2)
        {
            Debug.Log("removed " + PlayerQueue[0] + "from queue");
            PlayerQueue.RemoveAt(0);
        }
        else if (_currentPlayer.SelfDestructed && playerList.Count > 1)
        {
            Debug.Log("did not remove " + PlayerQueue[1] + "from queue as " + PlayerQueue[0] + " self destructed");
        }
        
        // set the current player to the next one on the queue, now the first element

        if (!gameOver && matchStarted)
        {
            _currentPlayer = PlayerQueue[0];
        }

        SetCurrentPlayerEvent.Invoke(_currentPlayer);
        SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);
        
        //Debug.LogWarning("to player " + (currentPlayerIndex + 1) + " index: " + currentPlayerIndex);

        _currentPlayer.currentUnit.highlighted = true;

        InitUI();

        //Debug.LogWarning("UI Switching from player " + (_currentPlayer.OriginalPlayerIndex - 1) + " to player" + (_currentPlayer.OriginalPlayerIndex + 1));
    }

    public void InitCurrentPlayer()
    {
        _currentPlayer = playerList[0];
        
        _currentPlayer.currentUnit = _currentPlayer.unitList[_currentPlayer.currentUnitIndex];
        
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
        _currentPlayer.currentUnit.movementValue = Vector2.zero;
        
        _currentPlayer.currentUnitIndex++;
        _currentPlayer.currentUnitIndex %= _currentPlayer.unitList.Count;
        
        _currentPlayer.currentUnit = _currentPlayer.unitList[_currentPlayer.currentUnitIndex];
        
        _currentPlayer.currentUnit.highlighted = true;
            
        SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);
        
        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnit.originalIndex + 1);
        
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

    public void SetTurnTimer(int timeToAdd)
    {
        var newTurnTimer = _turnTimer + timeToAdd;

        _turnTimer = newTurnTimer;
        
        Debug.Log("set turn timer to " + _turnTimer);

        //UIReferences.timerText.text = _turnTimer.ToString();
    }
}
