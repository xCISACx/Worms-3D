using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Player Stats")] 
    
    public PlayerBehaviour Player;
    public PlayerNumber Owner;
    public Color PlayerColour;
    
    [Header("Unit Conditions")] 
    
    public bool matchInitDone = false;
    
    public bool grounded = false;
    public bool canMove;
    public bool canAct;
    
    public bool highlighted = false;

    [Header("Unit Stats")]
    
    public int MaxHealth = 100;
    public int CurrentHealth = 100;

    [SerializeField] private float jumpForce;

    public int shotsFiredDuringRound;

    [Header("Unit References")]
    
    [SerializeField] private Transform camera;
    
    [SerializeField] private TMP_Text HealthText;
    
    [SerializeField] private GameObject SelectionArrow;
    
    private Rigidbody rigidbody;

    [Header("Movement Values")]

    private Vector2 movementValue;
    private Vector3 movementVector;
    private Vector3 movementDirection;
    
    [SerializeField] float movementSpeed;
    [SerializeField] float airMovementSpeed;
    
    [Header("Ground Check")]
    
    [SerializeField] private float groundCheckDistance = 1.6f;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask GroundLayerMask;
    
    [Header("Movement Limits")]

    private Vector3 turnStartPosition;
    private Vector3 lastPosition;
    
    private float distanceMovedX;
    private float distanceMovedZ;
                                          
    [SerializeField] private float distanceMoved;
    [SerializeField] private float maxMoveDistance = 30f;
    
    [Header("Weapon")]
    
    public bool canSwitchWeapon;
    private bool currentWeaponSelected;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private GameObject WeaponParentPrefab;
    [SerializeField] private Weapon selectedWeapon;
    [SerializeField] private int selectedWeaponIndex;
    [SerializeField] public GameObject currentWeaponObject;

    [Header("Fall Damage")]
    
    [SerializeField] public bool canTakeDamage;
    [SerializeField] private float fallDamageToTake;
    [SerializeField] private bool countFallDamage = false;

    [Header("Slope Handling")]
    
    [SerializeField] float maxSlopeAngle;

    [SerializeField] private float slopeCheckDistance;
    private RaycastHit slopeHit;
    [SerializeField] private LayerMask SlopeMask;

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

        //TODO: Make weapon selection function
    }

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
                
                GameManager.Instance.PlayerControls.Player.Move.performed += ctx => movementValue = ctx.ReadValue<Vector2>();
                GameManager.Instance.PlayerControls.Player.Move.canceled += ctx => movementValue = ctx.ReadValue<Vector2>();

                GameManager.Instance.PlayerControls.Player.Jump.started += Jump;
                
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
                    /*if (grounded && GameManager.Instance.PlayerControls.Player.Jump.triggered)
                    {
                        Jump();
                    }*/

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

        grounded = Physics.CheckSphere(transform.position - new Vector3(0, groundCheckDistance, 0), groundCheckRadius, GroundLayerMask);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position - new Vector3(0, groundCheckDistance, 0), groundCheckRadius, GroundLayerMask);
        //if (hitColliders[0].
        // check grounded normal to check if the touched object is not a wall
        

        //Debug.DrawLine(transform.localPosition, transform.position - Vector3.down * 10f, Color.red);
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
                    
                    //only aim if the unit has a weapon equipped
                    
                    if (GameManager.Instance.PlayerControls.Player.AimWeaponLock.inProgress && currentWeaponObject)
                    {
                        GameManager.Instance.firstPersonCamera = currentWeaponObject.GetComponent<WeaponBehaviour>().FPSCamera;
                        HandleWeaponAiming();
                    }
                    else
                    {
                        StopWeaponAiming();
                    }
                }
            }

            if (shotsFiredDuringRound >= 1)
            {
                canSwitchWeapon = false;
                Player.canChangeTurn = false;
                
                if (GameManager.Instance.AlivePlayers.Count > 1)
                {
                    //Debug.Log("can't act " + GameManager.Instance._currentPlayer.currentUnit);
                    StartCoroutine(GameManager.Instance.WaitForTurnToEnd());
                }
            }

            if (!grounded && rigidbody.velocity.y <= 0f)
            {
                //Debug.Log("going down");
                rigidbody.velocity += Vector3.up * Physics.gravity.y * (5f - 1) * Time.deltaTime;
            }

            if (!grounded && rigidbody.velocity.y < 0f)
            {
                countFallDamage = true;
                
                //Debug.Log(rigidbody.velocity);
                
                if (countFallDamage)
                {
                    fallDamageToTake += 0.3f;
                }
            }
        }

        rigidbody.useGravity = !OnSlope();
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

    public void Move()
    {
        movementVector = new Vector3(movementValue.x, 0, movementValue.y);
        
        movementDirection = camera.forward * movementValue.y + camera.right  * movementValue.x;

        //Debug.Log(OnSlope());

        if (grounded && !OnSlope())
        {
            movementDirection = movementDirection * movementSpeed;
            movementDirection.y = 0f;

            rigidbody.velocity = new Vector3(movementDirection.x, rigidbody.velocity.y, movementDirection.z);
        }
        else if (grounded && OnSlope())
        {
            movementDirection = GetSlopeMoveDirection() * movementSpeed;
            movementDirection.y = 0f;

            rigidbody.velocity = new Vector3(movementDirection.x, rigidbody.velocity.y, movementDirection.z);
        }
        else if (!grounded && !OnSlope())
        {
            movementDirection = movementDirection * airMovementSpeed;
            movementDirection.y = 0f;
            
            rigidbody.velocity = new Vector3(movementDirection.x, rigidbody.velocity.y, movementDirection.z);
        }
        //Debug.Log(rigidbody.velocity);
    }

    private void HandleWeaponAiming()
    {
        GameManager.Instance.firstPersonCamera.Priority = 100;

        GameManager.Instance.Reticle.GetComponent<Image>().enabled = true;
        
        //remove Cinemachine mouse input strings so we can't move the camera
        GameManager.Instance.mainCamera.m_XAxis.m_InputAxisName = string.Empty;
        GameManager.Instance.mainCamera.m_YAxis.m_InputAxisName = string.Empty;

        var aimSpeed = 2f;
        float vertical = Input.GetAxis ("Mouse Y") * aimSpeed;
        float horizontal = Input.GetAxis ("Mouse X") * aimSpeed;

        var target = weaponSlot.transform;
        var target2 = transform;
        
        target.transform.Rotate(-vertical, 0, 0);
        target2.transform.Rotate(0, horizontal, 0);
    }

    private void StopWeaponAiming()
    {
        if (GameManager.Instance.firstPersonCamera)
        {
            GameManager.Instance.firstPersonCamera.Priority = 0;   
        }

        GameManager.Instance.Reticle.GetComponent<Image>().enabled = false;
        
        GameManager.Instance.mainCamera.m_XAxis.m_InputAxisName = "Mouse X";
        GameManager.Instance.mainCamera.m_YAxis.m_InputAxisName = "Mouse Y";
    }

    private void LimitTotalMovement()
    {
        if (movementVector != Vector3.zero)
        {
            //We separate the distance moved on the X and Z axes so we don't count distance moved in the Y axis.

            var position = transform.position;
            
            distanceMovedX += Mathf.Abs((position.x - lastPosition.x));
            distanceMovedZ += Mathf.Abs((position.z - lastPosition.z));
            
            distanceMoved = distanceMovedX + distanceMovedZ;
            
            lastPosition = position;
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
            /*Vector3 targetDirection = Vector3.zero;

            targetDirection = camera.forward * movementValue.y;
            targetDirection = targetDirection + camera.right * movementValue.x;
            targetDirection.Normalize();
            targetDirection.y = 0f;*/
            
            float targetAngle = Mathf.Atan2(movementValue.x, movementValue.y) * Mathf.Rad2Deg + camera.eulerAngles.y;
            
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, targetAngle, transform.rotation.z);
            
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            
            /*Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);*/
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, slopeCheckDistance, SlopeMask))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            //Debug.Log(slopeHit.normal);
            //Debug.DrawRay(transform.position, Vector3.down * slopeCheckDistance, Color.red);
            //Debug.Log(angle);
            rigidbody.AddForce(slopeHit.normal * -Physics.gravity.magnitude);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(movementDirection, slopeHit.normal).normalized;
    }

    private void LimitMovementWithinRange()
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

    private void Jump(InputAction.CallbackContext ctx)
    {
        if (Player.canPlay && canAct && grounded)
        {
            rigidbody.AddForce(Vector3.up * jumpForce);
        }
        //rigidbody.velocity += new Vector3(rigidbody.velocity.x, jumpForce, rigidbody.velocity.z);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("take damage " + damage);
        CurrentHealth -= damage;
        canTakeDamage = false;
        HealthText.text = CurrentHealth.ToString();
        GameManager.Instance.UIReferences.GlobalHPBarParent.UpdateBar((int) Owner);
        GameManager.Instance.SpawnDamagePopUp(transform, new Vector3(0, 2f, 0), damage);

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
            //Debug.Log("unit killed itself");
            //Debug.Log("suicide turn end");
            Player.currentUnit = Player.unitList[(Player.currentUnitIndex + 1) % Player.unitList.Count];
            GameManager.Instance.NextTurn();
        }

        if (Player.unitList.Count == 1) //If this was the player's last unit
        {
            GameManager.Instance.AlivePlayers.Remove(Player);
        }
        
        Player.unitList.Remove(this);
        GameManager.Instance.unitList.Remove(this);

        Destroy(gameObject);
    }

    private void EquipWeapon()
    {
        if (weaponSlot.childCount > 0) //destroy weapon if one already exists in the weapon slot
        {
            Destroy(weaponSlot.GetChild(0).gameObject);
        }
        
        // Instantiate a new weapon parent
        
        var newWeaponParentObject = Instantiate(WeaponParentPrefab, weaponSlot.transform.position, weaponSlot.transform.rotation);
        
        // Set the currentWeaponObject variable to the instantiated weapon parent
        
        currentWeaponObject = newWeaponParentObject;
        
        var weaponScript = currentWeaponObject.GetComponent<WeaponBehaviour>();
        
        // Parent the newWeaponParentObject to the weaponSlot transform
        
        newWeaponParentObject.transform.SetParent(weaponSlot);
        
        weaponScript.user = gameObject;

        weaponScript.weaponModel = selectedWeapon.model;
        weaponScript.defaultShootForce = selectedWeapon.shootingForce;
        weaponScript.currentShootForce = selectedWeapon.shootingForce;
        weaponScript.maxShootForce = selectedWeapon.maxShootingForce;
        weaponScript.projectilePrefab = selectedWeapon.ammoPrefab;
        weaponScript.projectilePrefab.GetComponent<ProjectileBehaviour>().damage = selectedWeapon.damage;
        weaponScript.shootingDirection = selectedWeapon.shootingDirection;

        var newWeapon = Instantiate(weaponScript.weaponModel, newWeaponParentObject.transform.position,
            newWeaponParentObject.transform.rotation);

        weaponScript.shootPoint.localPosition = newWeapon.GetComponent<ModelProperties>().ShootPoint.localPosition;
        weaponScript.lineRenderer.transform.position = weaponScript.shootPoint.position;
        weaponScript.FPSCamera.transform.position = weaponScript.shootPoint.position;
        
        //weaponScript.user.GetComponent<UnitBehaviour>().FPSTarget = weaponScript.shootPoint;

        GameManager.Instance.firstPersonCamera = weaponScript.FPSCamera;

        //GameManager.Instance.firstPersonCamera.Follow = weaponScript.shootPoint;
        //GameManager.Instance.firstPersonCamera.LookAt = weaponScript.shootPoint;

        newWeapon.transform.SetParent(weaponScript.weaponModelParent);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            countFallDamage = false;

            if (canTakeDamage && fallDamageToTake >= GameManager.Instance.fallDamageTreshold)
            {
                TakeDamage((int) fallDamageToTake);
                GameManager.Instance.SpawnDamagePopUp(transform, new Vector3(0, 2f, 0),  (int) fallDamageToTake);
            }
            fallDamageToTake = 0;
        }
    }
    
    private void OnCollisionStay (Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            countFallDamage = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position - new Vector3(0, groundCheckDistance, 0), groundCheckRadius);
    }
}
