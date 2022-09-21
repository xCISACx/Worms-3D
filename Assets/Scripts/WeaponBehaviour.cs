using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using Math = System.Math;

public class WeaponBehaviour : MonoBehaviour
{
    public enum Direction
    {
        Forward,
        Up,
        Diagonal
    }

    public Direction shootingDirection;

    public bool initDone = false;
    
    public GameObject user;
    public GameObject projectilePrefab;
    public GameObject weaponModel;
    public Transform weaponModelParent;
    public Transform shootPoint;
    [SerializeField] public Vector3 shootForce;
    public LineRenderer lineRenderer;
    
    public PlayerInput playerInput;
    
    public InputAction fire;
 
    [SerializeField] private InputActionAsset controls;

    private void Awake()
    {
        lineRenderer.enabled = true;
        GameManager.Instance.PlayerControls = new Worms3D();
    }

    private void OnEnable()
    {
        GameManager.Instance.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        GameManager.Instance.PlayerControls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (user.GetComponent<UnitBehaviour>().canAct)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Shoot();
            }

            if (GameManager.Instance.PlayerControls.Player.Fire.triggered)
            {
                Shoot();
            }
        }
    }
    private void Shoot()
    {
        var newProjectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        Physics.IgnoreCollision(newProjectile.GetComponent<Collider>(), user.GetComponent<Collider>());

        switch (shootingDirection)
        {
            case Direction.Forward:
                newProjectile.GetComponent<Rigidbody>().AddForce(shootPoint.forward * shootForce.x, ForceMode.Impulse);
                break;
            case Direction.Up:
                newProjectile.GetComponent<Rigidbody>().AddForce(shootPoint.up * shootForce.y, ForceMode.Impulse);
                break;
            case Direction.Diagonal:
                newProjectile.GetComponent<Rigidbody>()
                    .AddForce((shootPoint.forward * shootForce.x / 2) + (shootPoint.up * shootForce.y / 2),
                        ForceMode.Impulse);
                break;
        }
        
        /*switch (shootingDirection)
        {
            case Direction.Forward:
                GetComponent<TrajectoryLine>().DrawStraightTrajectory(shootForce, new Vector3(0, 0), shootPoint); //bugged, won't rotate properly
                break;
            case Direction.Diagonal:
                GetComponent<TrajectoryLine>().DrawCurvedTrajectory(shootForce, new Vector3(0, 0)); //bugged, doesn't work
                break;
        }*/

        lineRenderer.transform.rotation = transform.localRotation;

        //Debug.Log("shoot");
        user.GetComponent<UnitBehaviour>().canAct = false;
        user.GetComponent<UnitBehaviour>().shotsFiredDuringRound++;
        Destroy(newProjectile.gameObject, 10f);
    }
}
