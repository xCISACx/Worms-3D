using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    
    [SerializeField] private PlayerBehaviour _currentPlayer;
    
    [SerializeField] private UnitBehaviour _currentUnit;
    
    [SerializeField] public Worms3D PlayerControls;
    
    private bool _controlsInitialised;
    
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
    }
    
    private void OnEnable()
    {
        PlayerControls = new Worms3D();
        PlayerControls.Enable();
    }

    private void OnDisable()
    {
        //PlayerControls.Disable();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.SetCurrentPlayerEvent.AddListener(SetCurrentPlayer);
        GameManager.Instance.SetCurrentUnitEvent.AddListener(SetCurrentUnit);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.MatchStarted)
        {
            if (!_controlsInitialised)
            {
                InitControls();
            }

            if (_currentPlayer && _currentUnit &&  _currentPlayer.RoundUnitPicked)
            {
                _currentUnit.MovementValue = PlayerControls.Player.Move.ReadValue<Vector2>();
                
                _currentUnit.Move();
                
                _currentUnit.LimitTotalMovement();
                
                _currentUnit.RotateWithMovement();

                if (_currentPlayer.TurnStarted)
                {
                    _currentUnit.InitTurn();
                }
            
                if (_currentPlayer.UnitPickedFlag)
                {
                    _currentUnit.InitUnit();
                }
            }

            // Had to use Update for inputs that require holding since they can't be hooked up on Awake()
                    
            if (PlayerControls.Player.AimWeaponLock.inProgress && _currentUnit.CurrentWeaponObject)
            {
                GameManager.Instance.FirstPersonCamera = _currentUnit.CurrentWeaponObject.GetComponent<WeaponBehaviour>().FPSCamera;
                
                MakeUnitAim();
            }
            else
            {
                MakeUnitStopAiming();
            }
            
            if (PlayerControls.Player.EquipWeapon.triggered && _currentUnit.CurrentWeaponSelected && !_currentUnit.CurrentWeaponObject)
            {
                _currentUnit.EquipWeapon();
            }

            if (PlayerControls.Player.ChangeWeapon.triggered && _currentUnit.CanSwitchWeapon)
            {
                _currentUnit.SelectedWeaponIndex++;
                
                _currentUnit.SelectedWeaponIndex = _currentUnit.SelectedWeaponIndex % _currentPlayer.WeaponInventory.Count;
                
                _currentUnit.SelectedWeapon = _currentPlayer.WeaponInventory[_currentUnit.SelectedWeaponIndex];
                
                _currentUnit.EquipWeapon();
            }
        }
    }

    void InitControls()
    {
        PlayerControls.Player.Jump.performed += MakeUnitJump;
        
        PlayerControls.Player.DoubleJump.performed += MakeUnitDoubleJump;
        
        PlayerControls.Player.HighJump.performed += MakeUnitHighJump;

        PlayerControls.Player.PickUnit.started += PickUnit;
        
        PlayerControls.Player.ChangeUnit.started += ChangeUnit;
        
        PlayerControls.Player.ChangeTurn.started += ChangeTurn;

        _controlsInitialised = true;
    }

    void MakeUnitJump(InputAction.CallbackContext ctx)
    {
        _currentUnit.Jump(0);
    }
    
    void MakeUnitDoubleJump(InputAction.CallbackContext ctx)
    {
        _currentUnit.Jump(1);
    }
    
    void MakeUnitHighJump(InputAction.CallbackContext ctx)
    {
        _currentUnit.Jump(2);
    }

    void MakeUnitAim()
    {
        _currentUnit.HandleWeaponAiming();
    }
    
    void MakeUnitStopAiming()
    {
        _currentUnit.StopWeaponAiming();
    }

    void PickUnit(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.PickUnit();
    }
    
    void ChangeUnit(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.ChangeUnit();
    }

    void ChangeTurn(InputAction.CallbackContext ctx)
    {
        if (_currentPlayer.CanChangeTurn)
        {
            GameManager.Instance.NextTurn();   
        }
    }

    void SetCurrentPlayer(PlayerBehaviour newPlayer)
    {
        _currentPlayer = newPlayer;
    }

    void SetCurrentUnit(UnitBehaviour newUnit)
    {
        _currentUnit = newUnit;
    }
}
