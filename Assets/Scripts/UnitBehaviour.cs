using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public PlayerBehaviour Player;
    public bool canMove;
    public bool canPlay;
    public bool highlighted;
    
    [SerializeField] private Transform shootPoint;
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
    [SerializeField] private GameObject currentWeapon;
    [SerializeField] private Transform weaponSlot;

    // Start is called before the first frame update
    void Start()
    {
        Player = GetComponentInParent<PlayerBehaviour>();
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        //TODO: Make weapon selection function
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.turnStarted)
        {
            InitTurn();
        }

        if (Player.unitPickedFlag && Player.currentUnit == this)
        {
            InitUnit();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetMouseButtonDown(2) && currentWeaponSelected && !currentWeapon)
        {
            EquipWeapon();
        }

        SelectionArrow.SetActive(highlighted);
    }

    private void InitUnit()
    {
        Debug.Log("initialising unit " + this.gameObject.name);
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
        Player.turnStarted = false;
        canMove = true;
    }

    private void FixedUpdate()
    {
        if (Player.canPlay)
        {
            if (canPlay)
            {
                if (canMove)
                {
                    Move();
                    //LimitMovement();
                    LimitTotalMovement();

                    /*transform.position = new Vector3(Mathf.Clamp(transform.position.x, SpawnedSphere.GetComponent<Collider>().bounds.min.x, SpawnedSphere.GetComponent<Collider>().bounds.max.x), 
                        transform.position.y, 
                        Mathf.Clamp(transform.position.z, SpawnedSphere.GetComponent<Collider>().bounds.min.z, SpawnedSphere.GetComponent<Collider>().bounds.max.z));
                    
                    Debug.Log(SpawnedSphere.GetComponent<Collider>().bounds.min.x + " | " + SpawnedSphere.GetComponent<Collider>().bounds.max.x);*/

                    /*if (transform.position.x > Sphere.GetComponent<MeshRenderer>().bounds.x)
                    {
                        if (transform.position.z >= turnStartPosition.z + maxMoveDistance)
                        {
                            transform.position = new Vector3(transform.position.x, transform.position.y, turnStartPosition.z + maxMoveDistance);
                            Debug.Log("can't move any farther forward");
                        }
                        transform.position = new Vector3(turnStartPosition.x + maxMoveDistance, transform.position.y, turnStartPosition.z + maxMoveDistance);
                        Debug.Log("can't move any farther forward");
                    }
                    else if (transform.position.x <= turnStartPosition.x - maxMoveDistance)
                    {
                        if (transform.position.z <= turnStartPosition.z - maxMoveDistance)
                        {
                            transform.position = new Vector3(transform.position.x, transform.position.y, turnStartPosition.z - maxMoveDistance);
                            Debug.Log("can't move any farther left");
                        }
                        transform.position = new Vector3(turnStartPosition.x - maxMoveDistance, transform.position.y, transform.position.z);
                        Debug.Log("can't move any farther left");
                    }*/
                    
                    //Debug.Log(rigidbody.velocity);
                }
                else
                {
                    //We can't move but can still rotate for aiming purposes
                    
                    horizontalInput = Input.GetAxisRaw("Horizontal");
                    verticalInput = Input.GetAxisRaw("Vertical");
                    movementVector = new Vector3(horizontalInput, 0, verticalInput);
                    rigidbody.velocity = new Vector3(0f, rigidbody.velocity.y, 0f);
                    
                    RotateWithMovement();
                }
                RotateWithMovement();
            }
            
        }

        if (rigidbody.velocity.y < 0f)
        {
            //Debug.Log("going down");
            rigidbody.velocity += Vector3.up * Physics.gravity.y * (5f - 1) * Time.deltaTime;
        }
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
            Debug.Log(distanceMovedX);
            Debug.Log(distanceMovedZ);   
        }

        if (movementVector != Vector3.zero && distanceMoved > maxMoveDistance)
        {
            canMove = false;
            Debug.Log("Can't move any more");
        }
    }

    private void RotateWithMovement()
    {
        if (movementVector != Vector3.zero)
        {
            Vector3 targetDirection = Vector3.zero;

            targetDirection = camera.forward * verticalInput;
            targetDirection = targetDirection + camera.right * horizontalInput;
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
                Debug.Log("Can't move farther.");
                Vector3 spawnToPlayer = transform.position - turnStartPosition;
                spawnToPlayer *= maxMoveDistance / distanceMoved;
                //transform.position = turnStartPosition + fromOrigintoObject;
                transform.position = new Vector3(turnStartPosition.x + spawnToPlayer.x, transform.position.y,
                    turnStartPosition.z + spawnToPlayer.z);
            }
        }
        
    }

    private void Move()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        movementVector = new Vector3(horizontalInput, 0, verticalInput);
        
        var movementDirection = camera.forward * Input.GetAxisRaw("Vertical");
        movementDirection = movementDirection + camera.right * horizontalInput;
        movementDirection.Normalize();
        movementDirection.y = 0f;
        movementDirection = movementDirection * movementSpeed;
        rigidbody.velocity = new Vector3(movementDirection.x, rigidbody.velocity.y, movementDirection.z);
    }

    private void Jump()
    {
        //rigidbody.velocity += new Vector3(rigidbody.velocity.x, jumpForce, rigidbody.velocity.z);
        rigidbody.AddForce(Vector3.up * jumpForce);
    }

    private void EquipWeapon()
    {
        var newWeapon = Instantiate(selectedWeapon.model, weaponSlot.transform.position, weaponSlot.transform.rotation);
        newWeapon.transform.SetParent(weaponSlot);
        newWeapon.GetComponent<WeaponBehaviour>().user = this.gameObject;
        currentWeapon = newWeapon;
    }
}
