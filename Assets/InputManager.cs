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
    private bool controlsInitialised;
    
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
        PlayerControls?.Disable();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.SetCurrentPlayerEvent.AddListener(SetCurrentPlayer);
        GameManager.Instance.SetCurrentUnitEvent.AddListener(SetCurrentUnit);
        //PlayerControls.Player.Fire.started 
        //PlayerControls.Player.PickUnit.started += GameManager.Instance.PickUnit;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.matchStarted)
        {
            if (!controlsInitialised)
            {
                InitControls();
            }

            /*if (_currentPlayer)
            {
                if (PlayerControls.Player.ChangeTurn.triggered && _currentPlayer.canChangeTurn)
                {
                    GameManager.Instance.NextTurn();
                } 
            }  */

            if (_currentPlayer && _currentPlayer.roundUnitPicked)
            {
                _currentUnit.movementValue = PlayerControls.Player.Move.ReadValue<Vector2>();
                _currentUnit.Move();
                _currentUnit.LimitTotalMovement();
                _currentUnit.RotateWithMovement();

                if (_currentPlayer.turnStarted)
                {
                    _currentUnit.InitTurn();
                }
            
                if (_currentPlayer.unitPickedFlag)
                {
                    _currentUnit.InitUnit();
                }
            }

            /*if (canMove)
            {
                Move();
                LimitTotalMovement();
            }
            else
            {
                movementVector = new Vector3(movementValue.x, 0, movementValue.y);
                rigidbody.velocity = new Vector3(0f, rigidbody.velocity.y, 0f);
            }
            //We can't move but can still rotate for aiming purposes
                    
            RotateWithMovement();
                    
            //only aim if the unit has a weapon equipped*/
                    
            if (PlayerControls.Player.AimWeaponLock.inProgress && _currentUnit.currentWeaponObject)
            {
                GameManager.Instance.firstPersonCamera = _currentUnit.currentWeaponObject.GetComponent<WeaponBehaviour>().FPSCamera;
                MakeUnitAim();
            }
            else
            {
                MakeUnitStopAiming();
            }
            
            if (PlayerControls.Player.EquipWeapon.triggered && _currentUnit.currentWeaponSelected && !_currentUnit.currentWeaponObject)
            {
                _currentUnit.EquipWeapon();
            }

            if (PlayerControls.Player.ChangeWeapon.triggered && _currentUnit.canSwitchWeapon)
            {
                _currentUnit.selectedWeaponIndex++;
                _currentUnit.selectedWeaponIndex = _currentUnit.selectedWeaponIndex % _currentPlayer.WeaponInventory.Count;
                _currentUnit.selectedWeapon = _currentPlayer.WeaponInventory[_currentUnit.selectedWeaponIndex];
                _currentUnit.EquipWeapon();
            }
        }
    }

    void InitControls()
    {
        PlayerControls.Player.Jump.canceled += MakeUnitJump;
        PlayerControls.Player.DoubleJump.performed += MakeUnitDoubleJump;
        PlayerControls.Player.HighJump.performed += MakeUnitHighJump;
        PlayerControls.Player.HighJump.canceled -= MakeUnitJump;
        //PlayerControls.Player.EquipWeapon.started += MakeUnitEquipWeapon;
        //PlayerControls.Player.ChangeWeapon.started += MakeUnitChangeWeapon;
        PlayerControls.Player.PickUnit.started += PickUnit;
        PlayerControls.Player.ChangeUnit.started += ChangeUnit;
        PlayerControls.Player.ChangeTurn.started += ChangeTurn;

        PlayerControls.Player.Fire.performed += MakeUnitStartCharging;
        PlayerControls.Player.Fire.canceled += MakeUnitShoot;

        controlsInitialised = true;
    }

    void MakeUnitJump(InputAction.CallbackContext ctx)
    {
        _currentUnit.Jump(0);
        Debug.Log("jumping");
    }
    
    void MakeUnitDoubleJump(InputAction.CallbackContext ctx)
    {
        _currentUnit.Jump(1);
        Debug.Log("double jumping");
    }
    
    void MakeUnitHighJump(InputAction.CallbackContext ctx)
    {
        _currentUnit.Jump(2);
        Debug.Log("high jumping");
    }

    void MakeUnitAim()
    {
        _currentUnit.HandleWeaponAiming();
    }
    
    void MakeUnitStopAiming()
    {
        _currentUnit.StopWeaponAiming();
    }

    void MakeUnitStartCharging(InputAction.CallbackContext ctx)
    {
        if (_currentUnit.currentWeaponObject)
        {
            _currentUnit.currentWeaponObject.GetComponent<WeaponBehaviour>().ChargeShot();
        }
    }
    
    void MakeUnitShoot(InputAction.CallbackContext ctx)
    {
        if (_currentUnit.currentWeaponObject)
        {
            _currentUnit.currentWeaponObject.GetComponent<WeaponBehaviour>().Shoot();   
        }
    }

    void MakeUnitChangeWeapon(InputAction.CallbackContext ctx)
    {
        _currentUnit.selectedWeaponIndex++;
        _currentUnit.selectedWeaponIndex = _currentUnit.selectedWeaponIndex % _currentPlayer.WeaponInventory.Count;
        _currentUnit.selectedWeapon = _currentPlayer.WeaponInventory[_currentUnit.selectedWeaponIndex];
        _currentUnit.EquipWeapon();
    }
    
    void MakeUnitEquipWeapon(InputAction.CallbackContext ctx)
    {
        _currentUnit.EquipWeapon();
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
        if (_currentPlayer.canChangeTurn)
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
