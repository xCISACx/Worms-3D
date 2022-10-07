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
    public Prefs Prefs;
    public int NumberOfStartingUnits = 1;
    
    [Header("Player Lists")]
    
    public List<PlayerBehaviour> PlayerList;
    public List<PlayerBehaviour> PlayerQueue = new List<PlayerBehaviour>();
    public List<PlayerBehaviour> AlivePlayers;
    public List<UnitBehaviour> UnitList;
    
    [Header("References")]
    
    public MenuManager MenuManager;
    
    public UIReferences UIReferences;
    
    public CinemachineFreeLook MainCamera;
    public CinemachineVirtualCamera FirstPersonCamera;
    
    public PlayerBehaviour CurrentPlayer;

    public AudioMixer AudioMixer;
    
    public AudioSource MusicSource;
    public AudioSource SfxSource;

    [SerializeField] public WaterBehaviour Water;
    
    private Coroutine _turnEndCoroutine = null;
    private Coroutine _waterCoroutine = null;
    
    [Header("Conditions")]
    
    public bool InitDone = false;
    
    public bool MatchStarted = false;
    public bool GameOver = false;
    
    public bool StartTurnTimer;
    public bool StartEndTurnTimer;
    public bool IsTurnStarting;
    
    [Header("Values")]
    
    public int CurrentPlayerIndex;
    
    [SerializeField] private int _turnNumber = 0;
    private float _defaultTurnTime = 60f;
    private float _defaultEndTurnTime = 3f;
    public float TurnTimer = 60f;
    private float _endTurnTimer = 3f;
    
    public int PlayersEliminatedDuringTurn = 0;
    
    public int FallDamageTreshold;
    
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

        //Debug.Log("gm awake");

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
        if (!InitDone && PlayerList.Count > 0)
        {
            Init();
        }
        
        if (MatchStarted)
        {
            if (StartTurnTimer)
            {
                TurnTimer -= Time.deltaTime;
            }

            if (TurnTimer <= 0f && !IsTurnStarting)
            {
                StartNextTurn();
            }
            
            if (StartEndTurnTimer)
            {
                _endTurnTimer -= Time.deltaTime;
            }
            
            if (UIReferences)
            {
                UIReferences.TimerText.text = TurnTimer.ToString("F0");
                UIReferences.EndTurnTimerText.text = _endTurnTimer.ToString("F0");
            }

            if (AlivePlayers.Count <= 1)
            {
                GameOver = true;
            }

            if (GameOver)
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
        
        Prefs = Resources.Load<Prefs>("Prefs");
        
        AudioMixer.SetFloat("masterVolume", Prefs.MasterVolume);
        AudioMixer.SetFloat("musicVolume", Prefs.MusicVolume);
        AudioMixer.SetFloat("sfxVolume", Prefs.SfxVolume);
        
        Screen.SetResolution(Prefs.ResolutionW, Prefs.ResolutionH, Prefs.FullScreenMode);
        Screen.fullScreen = Prefs.Fullscreen;
    }

    public IEnumerator WaitForTurnToEnd()
    {
        CurrentPlayer.CurrentUnit.CanMove = false;
        CurrentPlayer.CurrentUnit.CanShoot = false;
        
        StartTurnTimer = false;
        
        StartEndTurnTimer = true;
        
        UIReferences.EndTurnTimerText.gameObject.SetActive(true);

        //Debug.Log("waiting for turn to end coroutine 5");

        //yield return new WaitWhile(() => !CurrentPlayer.CurrentUnit.Grounded);
        yield return new WaitForSeconds(_defaultEndTurnTime);

        //Debug.LogWarning("3 seconds passed 6");

        NextTurn();
    }

    private void Win()
    {
        FollowCurrentUnit(AlivePlayers[0].CurrentUnit);
        CurrentPlayer.CanPlay = false;
        UIReferences.WinCanvas.SetActive(true);
        UIReferences.WinCanvasText.text = AlivePlayers[0].name + " WINS!";

        if (InputManager.Instance.PlayerControls.Player.ResetGame.triggered)
        {
            ResetGame();
            
            //Debug.Log("setting game over to false");
            SceneManager.LoadScene(0);
        }
    }

    private void Tie()
    {
        CurrentPlayer.CanPlay = false;
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
        GameOver = false;
        PlayersEliminatedDuringTurn = 0;
        
        StopAllCoroutines();
            
        PlayerList.Clear();
        UnitList.Clear();
        AlivePlayers.Clear();
        PlayerQueue.Clear();
            
        InitDone = false;
    }

    public void ChangeUnit()
    {
        if (!CurrentPlayer.RoundUnitPicked)
        {
            InitMainCamera();

            ResetUnit();

            //Debug.Log("previous unit: " + _currentPlayer.currentUnit);

            NextUnit();

            //SetUnitValues();

            SetCurrentPlayerEvent.Invoke(CurrentPlayer);

            //Debug.Log("new unit: " + _currentPlayer.currentUnit);
            
            InitMainCamera();

            InitUI();
        }
    }

    private void ResetUnit()
    {
        CurrentPlayer.CurrentUnit.Highlighted = false;
        
        CurrentPlayer.CurrentUnit.CanMove = false;
        
        CurrentPlayer.CurrentUnit.CanSwitchWeapon = false;
    }

    public void PickUnit()
    {
        if (!CurrentPlayer.RoundUnitPicked && !GameOver)
        {
            CurrentPlayer.CanChangeTurn = true;
            
            CurrentPlayer.CurrentUnit.Highlighted = false;
            
            CurrentPlayer.UnitPickedFlag = true;
            
            CurrentPlayer.RoundUnitPicked = true;
            
            CurrentPlayer.CurrentUnit.CanMove = true;
            
            CurrentPlayer.CurrentUnit.CanTakeDamage = true;
            
            CurrentPlayer.CurrentUnit.CanTakeFallDamage = true;
            
            //_currentPlayer.currentUnit.canAct = true;
            
            CurrentPlayer.CurrentUnit.CanSwitchWeapon = true;
            
            SetCurrentUnitEvent.Invoke(CurrentPlayer.CurrentUnit);
            
            CurrentPlayer.CurrentUnit.GetComponent<Rigidbody>().isKinematic = false;

            //Debug.Log("setting kinematic to false pick");

            TurnTimer = _defaultTurnTime;
            
            StartTurnTimer = true;
        }
    }

    // This function runs once after all the units spawn

    void Init()
    {
        IsTurnStarting = false;

        StopAllCoroutines();

        //_turnEndCoroutine = null;

        GameOver = false;

        Cursor.lockState = CursorLockMode.Locked;
        
        UIReferences = FindObjectOfType<UIReferences>();
        
        UIReferences.WinCanvas.SetActive(false);
        
        MainCamera = FindObjectOfType<CinemachineFreeLook>();

        Water = FindObjectOfType<WaterBehaviour>();

        InitCurrentPlayer();
        
        SetCurrentPlayerValues();
        
        InitUI();

        PlayerDiedEvent.AddListener(OnPlayerDied);

        ShotFiredEvent.AddListener(StartNextTurn);

        InitMainCamera();

        InitUI();

        //Debug.Log(_currentPlayer);
        
        InitDone = true;
    }

    private void InitUI()
    {
        UIReferences.CurrentPlayerText.text = "Current Player: " + (CurrentPlayer.OriginalPlayerIndex + 1);
        
        UIReferences.CurrentUnitText.text = "Current Unit: " + (CurrentPlayer.CurrentUnit.OriginalIndex + 1);
    }

    private void InitMainCamera()
    {
        MainCamera.Follow = CurrentPlayer.CurrentUnit.transform;
        MainCamera.LookAt = CurrentPlayer.CurrentUnit.transform;
        
        MainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        MainCamera.m_YAxis.m_InputAxisName = "Mouse Y";
    }

    private void OnPlayerDied(PlayerBehaviour player)
    {
        PlayersEliminatedDuringTurn++;
        
        //Debug.LogWarning(player, player);
        
        var barToDestroy = player.TeamHpBar.gameObject;

        var playerToDestroy = player;
        
        //Debug.Log(barToDestroy.name);

        UIReferences.GlobalHpBarParent.HpBars.Remove(player.TeamHpBar);
        
        Destroy(barToDestroy.gameObject);
        
        //Debug.Log("Destroyed bar " + barToDestroy.name + " belonging to player " + (currentPlayerIndex + 1));
        
        AlivePlayers.Remove(playerToDestroy);
            
        //Debug.Log("removing from alive players " + playerToDestroy);
        
        PlayerList.Remove(playerToDestroy);

        //Debug.Log("Removed from player list " + playerToDestroy);

        PlayerQueue.Remove(playerToDestroy);
        
        //Debug.LogWarning("Removed from player queue " + playerToDestroy);
        
        Destroy(playerToDestroy.gameObject);

        //SetCurrentUnitEvent.Invoke(_currentPlayer.currentUnit);
    }

    public void StartNextTurn()
    {
        //Debug.Log("starting next turn flag 1 " + IsTurnStarting);
        
        if (!IsTurnStarting)
        {
            //Debug.Log("start next turn if 2 " + IsTurnStarting);
            
            _turnEndCoroutine = StartCoroutine(WaitForTurnToEnd());
            
            //Debug.Log("coroutine called if 3 " + IsTurnStarting);
            
            IsTurnStarting = true;
        }
        else
        {
            //Debug.Log("can't start next turn else 4 " + IsTurnStarting);
            
            //StopCoroutine(_turnEndCoroutine);
        }
    }

    public void NextTurn()
    {
        if (!GameOver)
        {
            StartEndTurnTimer = false;
            
            _endTurnTimer = _defaultEndTurnTime;
            
            UIReferences.EndTurnTimerText.gameObject.SetActive(false);
            
            //Debug.Log("starting next turn");
        
            InitTurn();
        
            // Add the previous player to the end of the queue as they just finished their turn, if they are still alive

            if (!CurrentPlayer.SelfDestructed)
            {
                PlayerQueue.Add(CurrentPlayer);
            }

            var previousPlayer = CurrentPlayer;
        
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

            CurrentPlayer = PlayerQueue[0];
        
            CurrentPlayer.CurrentUnitIndex = 0;
        
            CurrentPlayer.CurrentUnit = CurrentPlayer.UnitList[CurrentPlayer.CurrentUnitIndex];
        
            SetCurrentPlayerEvent.Invoke(CurrentPlayer);

            SetCurrentPlayerValues();
        
            //Debug.Log("switching from " + previousPlayer + " to " + CurrentPlayer);

            //MakeAllUnitsKinematic();

            if (CurrentPlayer.UnitList.Count > 0)
            {
                FollowCurrentUnit(CurrentPlayer.CurrentUnit);
            
                // If the current unit has a weapon equipped

                if (CurrentPlayer.CurrentUnit.CurrentWeaponObject)
                {
                    SetFirstPersonCamera();
                    
                    CurrentPlayer.CurrentUnit.CanShoot = true;
                    CurrentPlayer.CurrentUnit.CanAim = true;
                }
                else
                {
                    if (FirstPersonCamera)
                    {
                        FirstPersonCamera.Follow = null;
                        FirstPersonCamera.LookAt = null;   
                    }
                }

                foreach (var unit in UnitList)
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

            if (_turnNumber % 4 == 1)
            {
                _waterCoroutine = StartCoroutine(Water.RaiseWaterLevel());
                
                PickupManager.Instance.SpawnRandomPickup();
            }
        }
    }

    public void SetSelfDestructed(bool selfDestructed)
    {
        CurrentPlayer.SelfDestructed = selfDestructed;
    }

    public void SetCurrentPlayerValues()
    {
        CurrentPlayer.RoundUnitPicked = false;

        foreach (var player in PlayerList)
        {
            if (player == CurrentPlayer)
            {
                player.CanPlay = true;
                player.CurrentUnit.CanMove = true;
                player.TurnStarted = true;
                
                //Debug.Log("setting " + player + "'s values");
            }
            else
            {
                player.CanPlay = false;
                player.CurrentUnit.CanMove = false;
                player.TurnStarted = false;
                //Debug.Log("setting everyone else's values");
            }
            
            player.CurrentUnit.CanTakeDamage = true;
            player.CurrentUnit.CanTakeFallDamage = true;
        }
    }

    private void SetFirstPersonCamera()
    {
        var currentWeaponScript = CurrentPlayer.CurrentUnit.CurrentWeaponObject.GetComponent<WeaponBehaviour>();
                
        FirstPersonCamera = currentWeaponScript.FPSCamera;
        FirstPersonCamera = currentWeaponScript.FPSCamera;
                
        FirstPersonCamera.Follow = null;
                
        FirstPersonCamera.transform.position = currentWeaponScript.ShootPoint.transform.position;
                
        FirstPersonCamera.LookAt = currentWeaponScript.LookPoint.transform;
    }

    private void FollowCurrentUnit(UnitBehaviour unit)
    {
        MainCamera.Priority = 100;
        MainCamera.Follow = unit.transform;
        MainCamera.LookAt = unit.transform;
            
        //Debug.Log("setting camera follow to: " + unit.transform);
    }

    public void InitTurn()
    {
        PlayersEliminatedDuringTurn = 0;
        
        //Debug.LogWarning("next turn init turn");

        CurrentPlayer.CanChangeTurn = false;
        
        IsTurnStarting = false;
        
        // Set the FPS Camera's priority to 0 so the unit's camera becomes the default
        
        if (FirstPersonCamera)
        {
            FirstPersonCamera.Priority = 0;
        }
        
        // Re-enable the camera movement by setting the mouse axes string properties

        MainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        MainCamera.m_YAxis.m_InputAxisName = "Mouse Y";

        // Disable the highlight of the previous player's unit before switching to the next player
        
        CurrentPlayer.CurrentUnit.Highlighted = false;
        
        // Reset the shots of the previous player's unit before switching to the next player   
        
        CurrentPlayer.CurrentUnit.ShotsFiredDuringRound = 0;

        StartTurnTimer = false;
        
        TurnTimer = _defaultTurnTime;
    }

    public void NextPlayer()
    {
        _turnNumber++;

        if (CurrentPlayer.CurrentUnit)
        {
            CurrentPlayer.CurrentUnit.GetComponent<Rigidbody>().isKinematic = true;
        }

        //Debug.LogWarning("switching from player " + (currentPlayerIndex + 1) + " index: " + currentPlayerIndex);
        
        // if the current player did not self-destruct and they still have more than 1 unit, remove them from the queue
        // we also check if more than 1 player was eliminated during the round to make sure we don't remove the first element twice

        if (!CurrentPlayer.SelfDestructed && PlayerList.Count > 1 && PlayersEliminatedDuringTurn < 2)
        {
            //Debug.Log("removed " + PlayerQueue[0] + "from queue");
            PlayerQueue.RemoveAt(0);
        }
        else if (CurrentPlayer.SelfDestructed && PlayerList.Count > 1)
        {
            //Debug.Log("did not remove " + PlayerQueue[1] + "from queue as " + PlayerQueue[0] + " self destructed");
        }
        
        // set the current player to the next one on the queue, now the first element

        if (!GameOver && MatchStarted)
        {
            CurrentPlayer = PlayerQueue[0];
        }

        SetCurrentPlayerEvent.Invoke(CurrentPlayer);
        SetCurrentUnitEvent.Invoke(CurrentPlayer.CurrentUnit);
        
        //Debug.LogWarning("to player " + (currentPlayerIndex + 1) + " index: " + currentPlayerIndex);

        CurrentPlayer.CurrentUnit.Highlighted = true;

        InitUI();

        //Debug.LogWarning("UI Switching from player " + (_currentPlayer.OriginalPlayerIndex - 1) + " to player" + (_currentPlayer.OriginalPlayerIndex + 1));
    }

    public void InitCurrentPlayer()
    {
        CurrentPlayer = PlayerList[0];
        
        CurrentPlayer.CurrentUnit = CurrentPlayer.UnitList[CurrentPlayer.CurrentUnitIndex];
        
        CurrentPlayer.CanChangeTurn = true;
        
        CurrentPlayer.CanPlay = true;
        
        CurrentPlayer.TurnStarted = true;

        CurrentPlayer.CurrentUnit.Highlighted = true;
        
        SetCurrentPlayerEvent.Invoke(CurrentPlayer);
        
        InitUI();
        
        //Debug.Log("Switching to player" + (CurrentPlayerIndex + 1));
    }

    public void NextUnit()
    {
        CurrentPlayer.CurrentUnit.MovementValue = Vector2.zero;
        
        CurrentPlayer.CurrentUnitIndex++;
        CurrentPlayer.CurrentUnitIndex %= CurrentPlayer.UnitList.Count;
        
        CurrentPlayer.CurrentUnit = CurrentPlayer.UnitList[CurrentPlayer.CurrentUnitIndex];
        
        CurrentPlayer.CurrentUnit.Highlighted = true;
            
        SetCurrentUnitEvent.Invoke(CurrentPlayer.CurrentUnit);
        
        UIReferences.CurrentUnitText.text = "Current Unit: " + (CurrentPlayer.CurrentUnit.OriginalIndex + 1);
        
        //Debug.Log("Switching to player " + (CurrentPlayerIndex + 1) + "'s unit " + (CurrentPlayer.CurrentUnitIndex + 1));
    }

    public void SpawnDamagePopUp(Transform parent, Vector3 offset, int damage)
    {
        var damagePrefab = Instantiate(UIReferences.DamagePopUpPrefab, Vector3.zero, Quaternion.identity);
        
        damagePrefab.transform.SetParent(parent);
        
        damagePrefab.transform.localPosition = Vector3.zero + offset;
        
        damagePrefab.GetComponent<DamagePopUpBehaviour>().InitialPosition = damagePrefab.transform.localPosition;
        
        damagePrefab.GetComponent<DamagePopUpBehaviour>().DamageText.text = "- " + damage;
    }

    public void SetTurnTimer(int timeToAdd)
    {
        var newTurnTimer = TurnTimer + timeToAdd;

        TurnTimer = newTurnTimer;
        
        Debug.Log("set turn timer to " + TurnTimer);

        //UIReferences.timerText.text = _turnTimer.ToString();
    }
}
