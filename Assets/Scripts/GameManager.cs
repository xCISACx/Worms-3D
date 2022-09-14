using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private CameraBehaviour mainCamera;
    public int currentPlayerIndex;
    private int _playerCount = 4;
    [SerializeField] private Dictionary<PlayerBehaviour, UnitBehaviour[]> playerUnitDictionary;
    private PlayerBehaviour _currentPlayer;
    [SerializeField] private List<PlayerBehaviour> playerList;
    [SerializeField] private List<UnitBehaviour> unitList;
    [SerializeField] private TMP_Text currentPlayerText;
    [SerializeField] private TMP_Text currentUnitText;
    [SerializeField] private TMP_Text timerText;
    public float defaultTurnTime = 60f;
    public float turnTimer = 60f;
    public bool startTurnTimer;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        for (int i = 0; i < _playerCount; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                unitList.Add(playerList[i].unitList[j]);
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            NextTurn();
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeUnit();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            PickUnit();
        }

        if (startTurnTimer)
        {
            turnTimer -= Time.deltaTime;
        }

        if (turnTimer <= 0f)
        {
            NextTurn();
        }

        timerText.text = turnTimer.ToString("F0");
    }

    private void ChangeUnit()
    {
        _currentPlayer.currentUnit.canMove = false;
        
        _currentPlayer = playerList[currentPlayerIndex];
        _currentPlayer.currentUnitIndex++;
        _currentPlayer.currentUnitIndex %= _currentPlayer.unitList.Count;
        
        mainCamera.followTarget = _currentPlayer.currentUnit.transform;

        currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
    }

    private void PickUnit()
    {
        _currentPlayer.currentUnit = _currentPlayer.unitList[_currentPlayer.currentUnitIndex];

        _currentPlayer.unitPicked = true;
        
        turnTimer = defaultTurnTime;
        startTurnTimer = true;
    }

    void Init()
    {
        _currentPlayer = playerList[currentPlayerIndex];
        mainCamera.followTarget = _currentPlayer.currentUnit.transform;
        currentPlayerText.text = "Current Player: " + (currentPlayerIndex + 1);
        currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
        playerList[currentPlayerIndex].canPlay = true;
        playerList[currentPlayerIndex].currentUnit.canMove = true;
        playerList[currentPlayerIndex].turnStarted = true;
        Debug.Log(_currentPlayer);
    }

    void NextTurn()
    {
        startTurnTimer = false;
        currentPlayerIndex++;
        currentPlayerIndex %= _playerCount;
        _currentPlayer = playerList[currentPlayerIndex];
        mainCamera.followTarget = _currentPlayer.currentUnit.transform;
        currentPlayerText.text = "Current Player: " + (currentPlayerIndex + 1);
        currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
        turnTimer = defaultTurnTime;
        startTurnTimer = true;

        for (int i = 0; i < _playerCount; i++)
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
            
        Debug.Log(_currentPlayer);
    }
}
