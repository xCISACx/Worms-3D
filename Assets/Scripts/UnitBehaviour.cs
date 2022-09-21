using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class UnitBehaviour : MonoBehaviour
{
    public enum PlayerNumber
    {
        Player1,
        Player2,
        Player3,
        Player4
    }

    public PlayerNumber Owner;
    public Color PlayerColour;
    public PlayerBehaviour Player;
    
    public bool canMove;
    public bool canAct;
    public bool canSwitchWeapon;
    public bool highlighted;
    public bool matchInitDone = false;
    
    public int MaxHealth = 100;
    public int CurrentHealth = 100;
    public TMP_Text HealthText;
    
    public int shotsFiredDuringRound;
    
    [SerializeField] private Transform camera;
    [SerializeField] private Vector3 movementVector;
    [SerializeField] private Vector3 movementDirection;
    [SerializeField] private Vector3 lastPosition;

    private float distanceMovedX;
    private float distanceMovedZ;
    [SerializeField] private float distanceMoved;
    private Rigidbody rigidbody;
    [SerializeField] float movementSpeed;
    [SerializeField] private float maxMoveDistance = 30f;
    private Vector3 turnStartPosition;
    [SerializeField] private float jumpForce;
    private float horizontalInput;
    private float verticalInput;
    [SerializeField] private GameObject SelectionArrow;
    [SerializeField] private bool currentWeaponSelected;
    [SerializeField] private Weapon selectedWeapon;
    [SerializeField] private int selectedWeaponIndex;
    [SerializeField] private GameObject currentWeaponObject;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private GameObject WeaponParentPrefab;
    [SerializeField] private Vector2 movementValue;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.matchStarted)
        {
            Player = GameManager.Instance.playerList[(int) Owner];
        }
    }

    private void Reset()
    {
        Player = GameManager.Instance.playerList[(int) Owner];
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        camera = Camera.main.transform;
        rigidbody = GetComponent<Rigidbody>();

        GameManager.Instance.PlayerControls.Player.Move.performed += ctx => movementValue = ctx.ReadValue<Vector2>();
        GameManager.Instance.PlayerControls.Player.Move.canceled += ctx => movementValue = ctx.ReadValue<Vector2>();

        //TODO: Make weapon selection function
    }

    /*private void OnValidate()
    {
        Player = GameManager.Instance.playerList[(int) Owner];

        gameObject.name = Owner.ToString() + " Unit " + (transform.GetSiblingIndex() + 1);
    }*/

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.matchStarted)
        {
            if (!matchInitDone)
            {
                var meshRenderer = GetComponentInChildren<MeshRenderer>();
                meshRenderer.material.color = PlayerColour;
                //Debug.Log("changed colour");
                matchInitDone = true;
            }

            if (!Player)
            {
                Player = transform.parent.GetComponent<PlayerBehaviour>();
            }
            
            if (Player.turnStarted)
            {
                InitTurn();
            }

            if (Player.unitPickedFlag && Player.currentUnit == this)
            {
                InitUnit();
            }

            if (Player.canPlay)
            {
                if (canAct)
                {
                    if (GameManager.Instance.PlayerControls.Player.Jump.triggered)
                    {
                        Jump();
                    }

                    if (GameManager.Instance.PlayerControls.Player.EquipWeapon.triggered && currentWeaponSelected && !currentWeaponObject)
                    {
                        this.EquipWeapon();
                    }

                    if (GameManager.Instance.PlayerControls.Player.ChangeWeapon.triggered && canSwitchWeapon)
                    {
                        selectedWeaponIndex++;
                        selectedWeaponIndex = selectedWeaponIndex % Player.WeaponInventory.Count;
                        selectedWeapon = Player.WeaponInventory[selectedWeaponIndex];
                        EquipWeapon();
                    }
                }
            }

            SelectionArrow.SetActive(highlighted);   
        }
    }

    private void InitUnit()
    {
        //Debug.Log("initialising unit " + this.gameObject.name);
        turnStartPosition = transform.position;
        lastPosition = transform.position;
        distanceMovedX = 0f;
        distanceMovedZ = 0f;
        distanceMoved = 0f;
        Player.unitPickedFlag = false;
    }

    private void InitTurn()
    {
        //SpawnedSphere = Instantiate(SpherePrefab, turnStartPosition, Quaternion.identity);
        //SpawnedSphere.transform.localScale = new Vector3(maxMoveDistance * 2, maxMoveDistance * 2, maxMoveDistance * 2);
        //Destroy(SpawnedSphere.gameObject, GameManager.Instance.defaultTurnTime);
        Player.canChangeTurn = true;
        Player.turnStarted = false;
        canMove = true;
        canSwitchWeapon = true;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.matchStarted)
        {
            if (Player.canPlay)
            {
                if (canAct)
                {
                    if (canMove)
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
                    
                    if (GameManager.Instance.PlayerControls.Player.AimWeaponLock.inProgress)
                    {
                        HandleWeaponAiming();
                    }
                    else
                    {
                        GameManager.Instance.mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
                        GameManager.Instance.mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";
                    }
                }
            }

            if (shotsFiredDuringRound >= 1)
            {
                canSwitchWeapon = false;
                Player.canChangeTurn = false;
                
                if (GameManager.Instance.AlivePlayers.Count > 1)
                {
                    StartCoroutine(GameManager.Instance.WaitForTurnToEnd());
                }
            }

            if (rigidbody.velocity.y <= 0f)
            {
                //Debug.Log("going down");
                rigidbody.velocity += Vector3.up * Physics.gravity.y * (5f - 1) * Time.deltaTime;
            }
        }
    }

    private void HandleWeaponAiming()
    {
        //remove Cinemachine mouse input strings so we can't move the camera
        GameManager.Instance.mainCamera.m_XAxis.m_InputAxisName = string.Empty;
        GameManager.Instance.mainCamera.m_YAxis.m_InputAxisName = string.Empty;
        
        var aimSpeed = 2f;
        float vertical = Input.GetAxis ("Mouse Y") * aimSpeed;

        var target = weaponSlot.transform;
        
        target.transform.Rotate(-vertical, 0, 0);

        /*Vector3 rot = target.localRotation.eulerAngles; TODO: FIX CLAMPING NOT WORKING

        rot.x = Mathf.Clamp(rot.x, a, b);
        //rot.x = Mathf.Clamp(rot.x, 90, 180);

        target.transform.localRotation = Quaternion.Euler(rot);*/
        
        //Debug.Log("local: " + target.localEulerAngles);
        //Debug.Log("global: " + target.eulerAngles);

        //target.transform.localEulerAngles = new Vector3(Mathf.Clamp(target.localEulerAngles.x, -85f, 85f), target.localEulerAngles.y, target.localEulerAngles.z);
        //Debug.Log("clamping");
    }
    
    float ClampAngle(float angle, float from, float to)
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360+from);
        return Mathf.Min(angle, to);
    }

    private void LimitTotalMovement()
    {
        if (movementVector != Vector3.zero)
        {
            //We separate the distance moved on the X and Z axes so we don't count distance moved in the Y axis.
            
            distanceMovedX += Mathf.Abs((transform.position.x - lastPosition.x));
            distanceMovedZ += Mathf.Abs((transform.position.z - lastPosition.z));
            distanceMoved = distanceMovedX + distanceMovedZ;
            lastPosition = transform.position;
            //Debug.Log(distanceMovedX);
            //Debug.Log(distanceMovedZ);
        }

        //If we go over the move distance limit, stop movement
        
        if (movementVector != Vector3.zero && distanceMoved > maxMoveDistance)
        {
            canMove = false;
            //Debug.Log("Can't move any more");
        }
    }

    private void RotateWithMovement()
    {
        if (movementVector != Vector3.zero)
        {
            Vector3 targetDirection = Vector3.zero;

            targetDirection = camera.forward * movementValue.y;
            targetDirection = targetDirection + camera.right * movementValue.x;
            targetDirection.Normalize();
            targetDirection.y = 0f;
            
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            
        }
    }

    private void LimitMovement()
    {
        if (movementVector != Vector3.zero)
        {
            distanceMoved = Vector3.Distance(transform.position, turnStartPosition);
        
            if (distanceMoved > maxMoveDistance)
            {
                //Debug.Log("Can't move farther.");
                Vector3 spawnToPlayer = transform.position - turnStartPosition;
                spawnToPlayer *= maxMoveDistance / distanceMoved;
                //transform.position = turnStartPosition + fromOrigintoObject;
                transform.position = new Vector3(turnStartPosition.x + spawnToPlayer.x, transform.position.y,
                    turnStartPosition.z + spawnToPlayer.z);
            }
        }
        
    }

    public void ReadMovementValues(InputAction.CallbackContext context)
    {
        movementValue = context.ReadValue<Vector2>();
    }

    public void Move()
    {
        movementVector = new Vector3(movementValue.x, 0, movementValue.y);
        
        movementDirection = camera.forward * movementValue.y;
        movementDirection = movementDirection + camera.right * movementValue.x;
        movementDirection.Normalize();
        movementDirection.y = 0f;
        movementDirection = movementDirection * movementSpeed;
        rigidbody.velocity = new Vector3(movementDirection.x, rigidbody.velocity.y, movementDirection.z);
        //Debug.Log(rigidbody.velocity);
    }

    private void Jump()
    {
        //rigidbody.velocity += new Vector3(rigidbody.velocity.x, jumpForce, rigidbody.velocity.z);
        rigidbody.AddForce(Vector3.up * jumpForce);
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        HealthText.text = CurrentHealth.ToString();
        GameManager.Instance.UIReferences.GlobalHPBarParent.UpdateBar((int) Owner);
        if (CurrentHealth <= 0)
        {
            StartCoroutine(GameManager.Instance.WaitForTurnToEnd());
            Die();
        }
    }

    public void Die()
    {
        if (Player.currentUnit == this && Player.unitList.Count > 0)
        {
            Player.currentUnit = Player.unitList[0];
        }

        if (Player.unitList.Count == 1) //If this was the player's last unit
        {
            GameManager.Instance.AlivePlayers.Remove(Player);
        }
        
        Player.unitList.Remove(this);
        GameManager.Instance.unitList.Remove(this);

        if (Player.currentUnit == this)
        {
            GameManager.Instance.NextTurn();
        }
        
        Destroy(gameObject);
    }

    private void EquipWeapon()
    {
        if (weaponSlot.childCount > 0) //destroy weapon if one already exists
        {
            Destroy(weaponSlot.GetChild(0).gameObject);
        }
        
        var newWeaponParentObject = Instantiate(WeaponParentPrefab, weaponSlot.transform.position, weaponSlot.transform.rotation);
        
        currentWeaponObject = newWeaponParentObject;
        
        var weaponScript = currentWeaponObject.GetComponent<WeaponBehaviour>();
        
        newWeaponParentObject.transform.SetParent(weaponSlot);
        
        weaponScript.user = this.gameObject;

        weaponScript.weaponModel = selectedWeapon.model;
        weaponScript.shootForce = selectedWeapon.shootingForce;
        weaponScript.projectilePrefab = selectedWeapon.ammoPrefab;
        weaponScript.projectilePrefab.GetComponent<ProjectileBehaviour>().damage = selectedWeapon.damage;
        weaponScript.shootingDirection = selectedWeapon.shootingDirection;

        var newWeapon = Instantiate(weaponScript.weaponModel, newWeaponParentObject.transform.position,
            newWeaponParentObject.transform.rotation);

        weaponScript.shootPoint = newWeapon.GetComponent<ModelProperties>().ShootPoint;
        weaponScript.lineRenderer.transform.position = weaponScript.shootPoint.position;

        newWeapon.transform.SetParent(weaponScript.weaponModelParent);
    }
}
