using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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
    public Transform lookPoint;
    [SerializeField] public Vector3 defaultShootForce;
    [SerializeField] public Vector3 currentShootForce;
    [SerializeField] public Vector3 maxShootForce;
    public CinemachineVirtualCamera FPSCamera;
    public LineRenderer lineRenderer;
    [SerializeField] float chargeSpeed;
    public bool charging = false;
    
    public PlayerInput playerInput;
    
    public InputAction fire;
 
    [SerializeField] private InputActionAsset controls;

    private void Awake()
    {
        lineRenderer.enabled = true;
        //FPSCamera = GetComponentInChildren<CinemachineFreeLook>();
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.PlayerControls.Player.Fire.inProgress)
        {
            ChargeShot();
        }
        
        if (InputManager.Instance.PlayerControls.Player.Fire.WasReleasedThisFrame())
        {
            Shoot();
        }
    }

    public void Init(Weapon selectedWeapon)
    {
        weaponModel = selectedWeapon.model;
        
        defaultShootForce = selectedWeapon.shootingForce;
        
        currentShootForce = selectedWeapon.shootingForce;
        
        maxShootForce = selectedWeapon.maxShootingForce;
        
        projectilePrefab = selectedWeapon.ammoPrefab;
        
        projectilePrefab.GetComponent<ProjectileBehaviour>().damage = selectedWeapon.damage;
        
        projectilePrefab.GetComponent<ProjectileBehaviour>().explosive = selectedWeapon.explosive;
        
        projectilePrefab.GetComponent<ProjectileBehaviour>().explosionForce = selectedWeapon.explosionForce;
        
        projectilePrefab.GetComponent<ProjectileBehaviour>().explosionRadius = selectedWeapon.explosionRadius;
        
        projectilePrefab.GetComponent<ProjectileBehaviour>().upwardsModifier = selectedWeapon.upwardsModifier;
        
        shootingDirection = selectedWeapon.shootingDirection;
        
    }

    public void ChargeShot()
    {
        if (GameManager.Instance._currentPlayer.roundUnitPicked
            && GameManager.Instance._currentPlayer.currentUnit == user.GetComponent<UnitBehaviour>()
            && user.GetComponent<UnitBehaviour>().canShoot)
        {
            var newShootForceX = currentShootForce.x += chargeSpeed;
            var newShootForceY = currentShootForce.y += chargeSpeed;
            
            newShootForceX = Mathf.Clamp(newShootForceX, defaultShootForce.x, maxShootForce.x);
            newShootForceY = Mathf.Clamp(newShootForceY, defaultShootForce.y, maxShootForce.y);
            
            currentShootForce = new Vector3(newShootForceX, newShootForceY, 0);
            
            Debug.Log("charging " + newShootForceX + " | " + newShootForceY + " | " + currentShootForce);
            
            var barValue = Mathf.InverseLerp(defaultShootForce.magnitude, maxShootForce.magnitude, currentShootForce.magnitude);
            GameManager.Instance.UIReferences.ChargeBar.fillAmount = barValue;
        }
    }

    public void Shoot()
    {

        if (GameManager.Instance._currentPlayer.roundUnitPicked
            && GameManager.Instance._currentPlayer.currentUnit == user.GetComponent<UnitBehaviour>()
            && user.GetComponent<UnitBehaviour>().canShoot)
        {
            var newProjectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Physics.IgnoreCollision(newProjectile.GetComponent<Collider>(), user.GetComponent<Collider>());

            newProjectile.GetComponent<ProjectileBehaviour>().explosionForce = currentShootForce.x;

            switch (shootingDirection)
            {
                case Direction.Forward:
                    newProjectile.GetComponent<Rigidbody>().AddForce(shootPoint.forward * currentShootForce.x, ForceMode.Impulse);
                    break;
                case Direction.Up:
                    newProjectile.GetComponent<Rigidbody>().AddForce(shootPoint.up * currentShootForce.y, ForceMode.Impulse);
                    break;
                case Direction.Diagonal:
                    newProjectile.GetComponent<Rigidbody>()
                        .AddForce((shootPoint.forward * currentShootForce.x / 2) + (shootPoint.up * currentShootForce.y / 2),
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
            
            user.GetComponent<UnitBehaviour>().shotsFiredDuringRound++;
            
            //user.GetComponent<UnitBehaviour>().canAct = false;
            
            currentShootForce = defaultShootForce;
            
            GameManager.Instance.UIReferences.ChargeBar.fillAmount = 0;
            
            GameManager.Instance.firstPersonCamera.Follow = newProjectile.transform;
            GameManager.Instance.firstPersonCamera.LookAt = newProjectile.transform;

            user.GetComponent<UnitBehaviour>().canAim = false;

            GameManager.Instance.mainCamera.m_XAxis.m_InputAxisName = "";
            GameManager.Instance.mainCamera.m_YAxis.m_InputAxisName = "";
            
            if (GameManager.Instance.AlivePlayers.Count > 1)
            {
                //Debug.Log("can't act " + GameManager.Instance._currentPlayer.currentUnit);
                
                //TODO: Fix turn starting twice if unit shoots an enemy and kills them/itself
                
                GameManager.Instance.ShotFiredEvent.Invoke();
            }

            user.GetComponent<UnitBehaviour>().canShoot = false;
            
            Destroy(newProjectile.gameObject, 10f);
        }
    }
}
