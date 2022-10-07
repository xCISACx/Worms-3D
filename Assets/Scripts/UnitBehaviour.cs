using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
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

    public int OriginalIndex;

    [Header("Player Stats")] 
    
    public PlayerBehaviour Player;
    public PlayerNumber Owner;
    public Color PlayerColour;

    [Header("Unit Conditions")]
    
    public bool CanMove = false;

    public bool CanJump = true;

    public bool CanAim = false;
    
    public bool CanShoot = false;

    public bool MatchInitDone = false;
    
    public bool Grounded = false;
    
    public bool OnUnit = false;
    
    public bool Falling = false;
    
    public bool Jumping = false;
    
    public bool HighJumping = false;

    public bool Highlighted = false;

    [Header("Unit Stats")]
    
    public int MaxHealth = 100;
    public int CurrentHealth = 100;

    [SerializeField] private float _jumpForce = 8;
    [SerializeField] private float _doubleJumpForce = 12;
    [SerializeField] private float _highJumpForce = 12;
    [SerializeField] private float _forwardJumpForce = 10;

    public int ShotsFiredDuringRound;

    [Header("Unit References")]
    
    [SerializeField] private Transform _camera;
    
    [SerializeField] private TMP_Text _healthText;
    
    [SerializeField] private GameObject _selectionArrow;

    [SerializeField] private GameObject _wallCollider;

    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    
    private Rigidbody _rigidbody;

    [Header("Movement Values")]

    public Vector2 MovementValue;
    private Vector3 _movementVector;
    private Vector3 _movementDirection;
    
    [SerializeField] float _movementSpeed;
    [SerializeField] float _airMovementSpeed;

    [SerializeField] private float _turnRate;
    
    [Header("Ground Check")]
    
    [SerializeField] private float _groundCheckDistance = 1.6f;
    [SerializeField] private float _groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private LayerMask _unitLayerMask;
    
    [Header("Movement Limits")]

    private Vector3 _turnStartPosition;
    private Vector3 _lastPosition;
    
    private float _distanceMovedX;
    private float _distanceMovedZ;
                                          
    [SerializeField] private float _distanceMoved;
    [SerializeField] private float _maxMoveDistance = 30f;
    
    [Header("Weapon")]
    
    public bool CanSwitchWeapon;
    public bool CurrentWeaponSelected;
    [SerializeField] private Transform _weaponSlot;
    [SerializeField] private GameObject _weaponParentPrefab;
    [SerializeField] public Weapon SelectedWeapon;
    [SerializeField] public int SelectedWeaponIndex;
    [SerializeField] public GameObject CurrentWeaponObject;

    [Header("Fall Damage")]
    
    [SerializeField] public bool CanTakeDamage;
    [SerializeField] public bool CanTakeFallDamage;
    [SerializeField] private int _fallDamageToTake;
    [SerializeField] private float _fallHeight;
    [SerializeField] private bool _countFallDamage = false;
    [SerializeField] public float TimeSpentGrounded;

    public bool BeingKnockedBack = false;

    [Header("Slope Handling")]
    
    [SerializeField] float _maxSlopeAngle;

    [SerializeField] private float _slopeCheckDistance;
    private RaycastHit _slopeHit;
    [SerializeField] private LayerMask _slopeMask;
    [SerializeField] private float _fallMultiplier = 4f;

    [Header("Animation")] 
    
    public Animator Animator;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.MatchStarted)
        {
            Player = GameManager.Instance.PlayerList[(int) Owner];
        }
    }

    private void Reset()
    {
        Player = GameManager.Instance.PlayerList[(int) Owner];
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _camera = Camera.main.transform;
        Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.MatchStarted)
        {
            if (!MatchInitDone)
            {
                _meshRenderer.material.color = PlayerColour;
                
                //Player.GlobalTeamHP += MaxHealth;

                //Debug.Log("changed colour");

                MatchInitDone = true;
            }

            if (!Player)
            {
                Player = transform.parent.GetComponent<PlayerBehaviour>();
            }

            _selectionArrow.SetActive(Highlighted);   
        }

        /*if (movementValue.sqrMagnitude > 0)
        {
            animator.SetTrigger("Move");
        }
        else
        {
            animator.SetTrigger("Idle");
        }*/
        
        Animator.SetInteger("Input", (int) MovementValue.sqrMagnitude);
        
        //Debug.Log("magnitude: " + movementVector.sqrMagnitude);
        
        //Collider[] hitColliders = Physics.OverlapSphere(transform.position - new Vector3(0, groundCheckDistance, 0), groundCheckRadius, GroundLayerMask);
        //if (hitColliders[0].
        // check grounded normal to check if the touched object is not a wall
        

        //Debug.DrawLine(transform.localPosition, transform.position - Vector3.down * 10f, Color.red);
    }
    
    private void FixedUpdate()
    {
        if (GameManager.Instance.MatchStarted)
        {
            /*int oldLayer = gameObject.layer; // This variable now stored our original layer
            
            gameObject.layer = 2; // The game object will now ignore all forms of raycasting
            
            var hitColliders = Physics.OverlapSphere(transform.position - new Vector3(0, groundCheckDistance, 0),
                groundCheckRadius, GroundLayerMask);

            if (hitColliders.Length > 0)
            {
                grounded = true;
            }
            else
            {
                grounded = false;
            }*/
 
            Grounded = Physics.CheckSphere(transform.position - new Vector3(0, _groundCheckDistance, 0), _groundCheckRadius, _groundLayerMask);
            
            OnUnit = Physics.CheckSphere(transform.position - new Vector3(0, _groundCheckDistance, 0), _groundCheckRadius, _unitLayerMask);
 
            //gameObject.layer = oldLayer;

            //grounded = Physics.CheckSphere(transform.position - new Vector3(0, groundCheckDistance, 0), groundCheckRadius, GroundLayerMask);

            //grounded = true;

            if (ShotsFiredDuringRound >= 1)
            {
                CanSwitchWeapon = false;
                Player.CanChangeTurn = false;
            }

            if (!Grounded && _rigidbody.velocity.y <= 0f)
            {
                //Debug.Log("going down");
                _rigidbody.velocity += Vector3.up * Physics.gravity.y * _fallMultiplier * Time.deltaTime;
            }
            
            // TODO: Add the forward force to regular and double jumping, but not high jumping

            if (!Grounded && Jumping || HighJumping)
            {
                _rigidbody.AddForce(transform.forward * _forwardJumpForce);
            }

            if (!Grounded && Mathf.Abs(_rigidbody.velocity.y) <= 0.5f)
            {
                CanShoot = false;

                Falling = true;
                
                TimeSpentGrounded = 0;
                
                //countFallDamage = true;
                
                _fallHeight = transform.position.y;
                
                Debug.Log("fall height: " + _fallHeight);

                //Debug.Log(rigidbody.velocity);
                
                /*if (countFallDamage)
                {
                    fallDamageToTake += 0.3f;
                }*/
            }
        }

        _rigidbody.useGravity = !OnSlope();
    }

    public void InitUnit()
    {
        //Debug.Log("initialising unit " + this.gameObject.name);
        CanTakeDamage = true;
        _wallCollider.SetActive(false);
        _turnStartPosition = transform.position;
        _lastPosition = transform.position;
        _distanceMovedX = 0f;
        _distanceMovedZ = 0f;
        _distanceMoved = 0f;
        Player.UnitPickedFlag = false;
    }

    public void InitTurn()
    {
        Player.CanChangeTurn = true;
        Player.TurnStarted = false;
        CanMove = true;
        CanSwitchWeapon = true;
    }

    public void Move()
    {
        if (CanMove)
        {
            _movementVector = new Vector3(MovementValue.x, 0, MovementValue.y);
        
            _movementDirection = _camera.forward * MovementValue.y + _camera.right  * MovementValue.x;

            //Debug.Log(OnSlope());

            if (Grounded || OnUnit && !OnSlope())
            {
                _movementDirection = _movementDirection * _movementSpeed;
                _movementDirection.y = 0f;

                _rigidbody.velocity = new Vector3(_movementDirection.x, _rigidbody.velocity.y, _movementDirection.z);
            }
            else if (Grounded && OnSlope())
            {
                _movementDirection = GetSlopeMoveDirection() * _movementSpeed;
                _movementDirection.y = 0f;

                _rigidbody.velocity = new Vector3(_movementDirection.x, _rigidbody.velocity.y, _movementDirection.z);
            }
            else if (!Grounded && !OnSlope())
            {
                _movementDirection = Vector3.zero;
                _movementDirection.y = 0f;
            
                _rigidbody.velocity = new Vector3(_movementDirection.x, _rigidbody.velocity.y, _movementDirection.z);
            }
            //Debug.Log(rigidbody.velocity);
            
            Animator.SetInteger("Input", (int) MovementValue.sqrMagnitude);
        }
        else
        {
            _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
            
            MovementValue = Vector2.zero;

            Animator.SetInteger("Input", 0);
        }
        
    }

    public void HandleWeaponAiming()
    {
        if (CanAim)
        {
            GameManager.Instance.FirstPersonCamera.Priority = 100;

            GameManager.Instance.UIReferences.Reticle.GetComponent<Image>().enabled = true;
        
            //remove Cinemachine mouse input strings so we can't move the camera
            GameManager.Instance.MainCamera.m_XAxis.m_InputAxisName = string.Empty;
            GameManager.Instance.MainCamera.m_YAxis.m_InputAxisName = string.Empty;

            var aimSpeed = 2f;
            float vertical = Input.GetAxis ("Mouse Y") * aimSpeed;
            float horizontal = Input.GetAxis ("Mouse X") * aimSpeed;

            var target = _weaponSlot.transform;
            var target2 = transform;
        
            target.transform.Rotate(-vertical, 0, 0);
            target2.transform.Rotate(0, horizontal, 0);   
        }
    }

    public void StopWeaponAiming()
    {
        if (CanAim)
        {
            if (GameManager.Instance.FirstPersonCamera)
            {
                GameManager.Instance.FirstPersonCamera.Priority = 0;   
            }

            GameManager.Instance.UIReferences.Reticle.GetComponent<Image>().enabled = false;
            
            GameManager.Instance.MainCamera.m_XAxis.m_InputAxisName = "Mouse X";
            GameManager.Instance.MainCamera.m_YAxis.m_InputAxisName = "Mouse Y";
        }
    }

    public void LimitTotalMovement()
    {
        if (_movementVector != Vector3.zero)
        {
            //We separate the distance moved on the X and Z axes so we don't count distance moved in the Y axis.

            var position = transform.position;
            
            _distanceMovedX += Mathf.Abs((position.x - _lastPosition.x));
            _distanceMovedZ += Mathf.Abs((position.z - _lastPosition.z));
            
            _distanceMoved = _distanceMovedX + _distanceMovedZ;
            
            _lastPosition = position;
        }

        //If we go over the move distance limit, stop movement
        
        if (_movementVector != Vector3.zero && _distanceMoved > _maxMoveDistance)
        {
            CanMove = false;
            //Debug.Log("Can't move any more");
        }
    }

    public void RotateWithMovement()
    {
        if (_movementVector != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(MovementValue.x, MovementValue.y) * Mathf.Rad2Deg + _camera.eulerAngles.y;
            
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.x, targetAngle, transform.rotation.z);
            
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _turnRate);
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, _slopeCheckDistance, _slopeMask))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            //Debug.Log(slopeHit.normal);
            //Debug.DrawRay(transform.position, Vector3.down * slopeCheckDistance, Color.red);
            //Debug.Log(angle);
            _rigidbody.AddForce(_slopeHit.normal * -Physics.gravity.magnitude);
            return angle < _maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_movementDirection, _slopeHit.normal).normalized;
    }

    private void LimitMovementWithinRange()
    {
        if (_movementVector != Vector3.zero)
        {
            _distanceMoved = Vector3.Distance(transform.position, _turnStartPosition);
        
            if (_distanceMoved > _maxMoveDistance)
            {
                //Debug.Log("Can't move farther.");
                Vector3 spawnToPlayer = transform.position - _turnStartPosition;
                spawnToPlayer *= _maxMoveDistance / _distanceMoved;
                //transform.position = turnStartPosition + fromOrigintoObject;
                transform.position = new Vector3(_turnStartPosition.x + spawnToPlayer.x, transform.position.y,
                    _turnStartPosition.z + spawnToPlayer.z);
            }
        }
        
    }

    public void Jump(int jumpType)
    {
        if (Grounded && CanMove && CanJump && !Jumping)
        {
            switch (jumpType)
            {
                case 0:
                    //rigidbody.AddForce(Vector3.up * jumpForce + transform.forward * (forwardJumpForce), ForceMode.Impulse);
                    //rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _jumpForce, _rigidbody.velocity.z);
                    Jumping = true;
                    CanMove = false;
                    Debug.Log("jumping");
                    break;
                case 1:
                    //rigidbody.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _doubleJumpForce, _rigidbody.velocity.z);
                    Jumping = true;
                    CanMove = false;
                    Debug.Log("double jumping");
                    break;
                case 2:
                    //rigidbody.AddForce(Vector3.up * highJumpForce, ForceMode.Impulse);
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _highJumpForce, _rigidbody.velocity.z);
                    HighJumping = true;
                    CanMove = false;
                    Debug.Log("high jumping");
                    break;
            }

            //rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpForce, rigidbody.velocity.z);
            
            _wallCollider.SetActive(true);
            Grounded = false;
            CanJump = false;
        }
        //rigidbody.velocity += new Vector3(rigidbody.velocity.x, jumpForce, rigidbody.velocity.z);
    }

    public void TakeDamage(int damage)
    {
        if (CanTakeDamage)
        {
            CanTakeDamage = false;
            
            //Debug.Log("take damage " + damage);
            CurrentHealth -= damage;
        
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, 100);
        
            CanTakeDamage = false;
        
            _healthText.text = CurrentHealth.ToString();
        
            if (Player.TeamHpBar)
            {
                Player.UpdateBar();
            }
        
            GameManager.Instance.SpawnDamagePopUp(transform, new Vector3(0, 2f, 0), damage);

            CanTakeDamage = true;

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        // If the unit self-destructs but there are more units, switch to the next player
        
        if (GameManager.Instance.CurrentPlayer == Player && Player.CurrentUnit == this && Player.UnitList.Count > 0)
        {
            Debug.LogWarning("unit killed itself");
            Debug.LogWarning("suicide turn end");

            if (!GameManager.Instance.GameOver)
            {
                GameManager.Instance.StartNextTurn();
                
                GameManager.Instance.NextUnit();

                //Player.currentUnit = Player.unitList[(Player.currentUnitIndex + 1) % Player.unitList.Count];

                GameManager.Instance.SetCurrentPlayerValues();

                GameManager.Instance.SetCurrentUnitEvent.Invoke(Player.CurrentUnit);
            }
        }
        
        // If the unit self-destructs and it's the player's last unit, eliminate the player

        if (GameManager.Instance.CurrentPlayer == Player && Player.CurrentUnit == this && Player.UnitList.Count == 1)
        {
            GameManager.Instance.SetSelfDestructed(true);
            Player.SelfDestruct(); //OnPlayerDied
            Debug.Log("unit self destructed");
        }

        else if (Player.UnitList.Count == 1)
        {
            GameManager.Instance.SetSelfDestructed(false);
            Player.SelfDestruct();
        }
        
        // Remove the unit from the unit lists and destroy it

        Player.UnitList.Remove(this);
        
        Debug.Log("removed " + this + " from " + Player + "'s unitList");
        
        GameManager.Instance.UnitList.Remove(this);
        
        Debug.Log("removed " + this + " from GM unitList");

        Destroy(gameObject);
    }

    //todo: SEPARATE BLOCKS INTO FUNCTIONS
    
    public void EquipWeapon()
    {
        /*if (!currentWeaponSelected || currentWeaponObject || !canSwitchWeapon)
        {
            return;
        }*/

        if (_weaponSlot.childCount > 0) //destroy weapon if one already exists in the weapon slot
        {
            Destroy(_weaponSlot.GetChild(0).gameObject);
        }
        
        // Instantiate a new weapon parent
        
        var newWeaponParentObject = Instantiate(_weaponParentPrefab, _weaponSlot.transform.position, _weaponSlot.transform.rotation);
        
        // Set the currentWeaponObject variable to the instantiated weapon parent
        
        CurrentWeaponObject = newWeaponParentObject;
        
        var weaponScript = CurrentWeaponObject.GetComponent<WeaponBehaviour>();
        
        // Parent the newWeaponParentObject to the weaponSlot transform
        
        newWeaponParentObject.transform.SetParent(_weaponSlot);
        
        weaponScript.User = gameObject;
        
        weaponScript.Init(SelectedWeapon);

        var newWeapon = Instantiate(weaponScript.WeaponModel, newWeaponParentObject.transform.position,
            newWeaponParentObject.transform.rotation);

        weaponScript.ShootPoint.position = newWeapon.GetComponent<ModelProperties>().ShootPoint.position;

        weaponScript.FPSCamera.transform.position = weaponScript.ShootPoint.position;
        
        //weaponScript.user.GetComponent<UnitBehaviour>().FPSTarget = weaponScript.shootPoint;

        GameManager.Instance.FirstPersonCamera = weaponScript.FPSCamera;

        //GameManager.Instance.firstPersonCamera.Follow = weaponScript.shootPoint;
        //GameManager.Instance.firstPersonCamera.LookAt = weaponScript.shootPoint;

        newWeapon.transform.SetParent(weaponScript.WeaponModelParent);

        CanAim = true;
        CanShoot = true;
    }

    public void SetHighlight()
    {
        if (GameManager.Instance.CurrentPlayer.CurrentUnit == this)
        {
            Highlighted = true;
        }
        else
        {
            Highlighted = false;
        }
        
        _selectionArrow.SetActive(Highlighted);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            CanJump = true;
            CanMove = true;
            Jumping = false;
            HighJumping = false;
            _wallCollider.SetActive(false);
            _countFallDamage = false;
            TimeSpentGrounded++;

            //Debug.Log("setting kinematic to true ground");

            var landHeight = transform.position.y;
            
            var heightFallen = MathF.Abs(_fallHeight - landHeight);

            if (Falling && CanTakeFallDamage)
            {
                Debug.Log("Fell from " + _fallHeight + " to " + landHeight);

                var heightFallenInt = Mathf.FloorToInt(heightFallen);

                if (heightFallenInt >= GameManager.Instance.FallDamageTreshold)
                {
                    Debug.Log("Taking fall damage from height " + heightFallenInt);

                    _fallDamageToTake = heightFallenInt - GameManager.Instance.FallDamageTreshold;

                    if (_fallDamageToTake > 0)
                    {
                        TakeDamage(_fallDamageToTake);  
                        CanShoot = false;
                        GameManager.Instance.StartNextTurn();
                    }
                }
                else
                {
                    CanShoot = true;
                }
                
                _fallHeight = 0;

                Falling = false;
            }

            _fallDamageToTake = 0;

            if (BeingKnockedBack && TimeSpentGrounded > 0.5f)
            {
                _rigidbody.isKinematic = true;
                BeingKnockedBack = false;
                Debug.LogWarning("set being knocked back to false");
            }
        }
        
        if (collision.gameObject.CompareTag("KillPlane"))
        {
            Die();
        }
    }
    
    private void OnCollisionStay (Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _countFallDamage = false;
        }
        
        if (collision.gameObject.CompareTag("KillPlane"))
        {
            Die();
        }
        
        // This was added so the units don't get stuck on each other if they jump towards one another
        
        if (collision.gameObject.CompareTag("Unit"))
        {
            GetComponent<CapsuleCollider>().material.dynamicFriction = 0;
        }
    }

    public void SetHealth(int health)
    {
        CurrentHealth = health;
        
        _healthText.text = health.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("KillPlane"))
        {
            Die();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position - new Vector3(0, _groundCheckDistance, 0), _groundCheckRadius);
    }
}
