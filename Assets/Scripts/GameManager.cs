using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using Cinemachine;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public int NumberOfStartingUnits;
    public List<PlayerBehaviour> AlivePlayers;
    public static GameManager Instance;
    public UIReferences UIReferences;
    public bool matchStarted = false;
    public bool initDone = false;
    public bool changeTurnFlag;
    public CinemachineFreeLook mainCamera;
    public int currentPlayerIndex;
    [SerializeField] private Dictionary<PlayerBehaviour, UnitBehaviour[]> playerUnitDictionary;
    public PlayerBehaviour _currentPlayer;
    public List<PlayerBehaviour> playerList;
    public List<UnitBehaviour> unitList;
    public float defaultTurnTime = 60f;
    public float turnTimer = 60f;
    public bool startTurnTimer;
    public bool gameOver = false;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            gameOver = false;
            initDone = false;
        }

        //Cursor.lockState = CursorLockMode.Locked;
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
            if (Input.GetKeyDown(KeyCode.Return) && _currentPlayer.canChangeTurn)
            {
                NextTurn();
            }
        
            if (!_currentPlayer.roundUnitPicked && Input.GetKeyDown(KeyCode.Tab))
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
        }
    }
    
    public IEnumerator WaitForTurnToEnd()
    {
        _currentPlayer.currentUnit.canAct = false;
        changeTurnFlag = true;
        
        if (changeTurnFlag)
        {
            _currentPlayer.currentUnit.shotsFiredDuringRound = 0;
        }
        
        yield return new WaitForSeconds(3);
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

    private void ChangeUnit()
    {
        _currentPlayer.currentUnit.highlighted = false;
        //Debug.Log("previous unit: " + _currentPlayer.currentUnit);
        _currentPlayer.currentUnit.canMove = false;
        _currentPlayer.currentUnit.canAct = false;
        
        _currentPlayer = playerList[currentPlayerIndex];
        _currentPlayer.currentUnitIndex++;
        _currentPlayer.currentUnitIndex %= _currentPlayer.unitList.Count;
        _currentPlayer.currentUnit = _currentPlayer.unitList[_currentPlayer.currentUnitIndex];
        
        //Debug.Log("new unit: " + _currentPlayer.currentUnit);
        
        mainCamera.Follow = _currentPlayer.currentUnit.transform;
        mainCamera.LookAt = _currentPlayer.currentUnit.transform;

        UIReferences.currentUnitText.text = "Current Unit: " + (_currentPlayer.currentUnitIndex + 1);
        _currentPlayer.currentUnit.highlighted = true;
    }

    private void PickUnit()
    {
        _currentPlayer.unitPickedFlag = true;
        _currentPlayer.roundUnitPicked = true;
        _currentPlayer.currentUnit.canMove = true;
        _currentPlayer.currentUnit.highlighted = false;
        _currentPlayer.currentUnit.canAct = true;

        turnTimer = defaultTurnTime;
        startTurnTimer = true;
    }

    void Init()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _currentPlayer = playerList[currentPlayerIndex];
        
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
        
        //Debug.Log(_currentPlayer);
        initDone = true;
    }

    public void NextTurn()
    {
        changeTurnFlag = false;
        _currentPlayer.currentUnit.highlighted = false; // disable the highlight of the previous player's unit before switching to the next player
        
        startTurnTimer = false;
        
        currentPlayerIndex++;
        currentPlayerIndex %= GameManager.Instance.playerList.Count;
        
        _currentPlayer = playerList[currentPlayerIndex];
        
        if (_currentPlayer.unitList.Count > 0)
        {
            mainCamera.Follow = _currentPlayer.currentUnit.transform;
            mainCamera.LookAt = _currentPlayer.currentUnit.transform;   
            _currentPlayer.currentUnit.highlighted = true;
        }
        
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
