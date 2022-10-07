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

    public Direction ShootingDirection;

    public bool InitDone = false;
    
    public GameObject User;
    public GameObject ProjectilePrefab;
    public GameObject WeaponModel;
    public Transform WeaponModelParent;
    public Transform ShootPoint;
    public Transform LookPoint;
    [SerializeField] public Vector3 DefaultShootForce;
    [SerializeField] public Vector3 CurrentShootForce;
    [SerializeField] public Vector3 MaxShootForce;
    public CinemachineVirtualCamera FPSCamera;
    [SerializeField] float _chargeSpeed;

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
        WeaponModel = selectedWeapon.Model;
        
        DefaultShootForce = selectedWeapon.ShootingForce;
        
        CurrentShootForce = selectedWeapon.ShootingForce;
        
        MaxShootForce = selectedWeapon.MaxShootingForce;
        
        ProjectilePrefab = selectedWeapon.AmmoPrefab;
        
        ProjectilePrefab.GetComponent<ProjectileBehaviour>().Damage = selectedWeapon.Damage;
        
        ProjectilePrefab.GetComponent<ProjectileBehaviour>().Explosive = selectedWeapon.Explosive;
        
        ProjectilePrefab.GetComponent<ProjectileBehaviour>().ExplosionForce = selectedWeapon.ExplosionForce;
        
        ProjectilePrefab.GetComponent<ProjectileBehaviour>().ExplosionRadius = selectedWeapon.ExplosionRadius;
        
        ProjectilePrefab.GetComponent<ProjectileBehaviour>().UpwardsModifier = selectedWeapon.UpwardsModifier;
        
        ShootingDirection = selectedWeapon.ShootingDirection;
        
    }

    public void ChargeShot()
    {
        if (GameManager.Instance.CurrentPlayer.RoundUnitPicked
            && GameManager.Instance.CurrentPlayer.CurrentUnit == User.GetComponent<UnitBehaviour>()
            && User.GetComponent<UnitBehaviour>().CanShoot)
        {
            var newShootForceX = CurrentShootForce.x += _chargeSpeed;
            var newShootForceY = CurrentShootForce.y += _chargeSpeed;
            
            newShootForceX = Mathf.Clamp(newShootForceX, DefaultShootForce.x, MaxShootForce.x);
            newShootForceY = Mathf.Clamp(newShootForceY, DefaultShootForce.y, MaxShootForce.y);
            
            CurrentShootForce = new Vector3(newShootForceX, newShootForceY, 0);
            
            Debug.Log("charging " + newShootForceX + " | " + newShootForceY + " | " + CurrentShootForce);
            
            var barValue = Mathf.InverseLerp(DefaultShootForce.magnitude, MaxShootForce.magnitude, CurrentShootForce.magnitude);
            GameManager.Instance.UIReferences.ChargeBar.fillAmount = barValue;
        }
    }

    public void Shoot()
    {

        if (GameManager.Instance.CurrentPlayer.RoundUnitPicked
            && GameManager.Instance.CurrentPlayer.CurrentUnit == User.GetComponent<UnitBehaviour>()
            && User.GetComponent<UnitBehaviour>().CanShoot)
        {
            var newProjectile = Instantiate(ProjectilePrefab, ShootPoint.position, Quaternion.identity);
            Physics.IgnoreCollision(newProjectile.GetComponent<Collider>(), User.GetComponent<Collider>());

            newProjectile.GetComponent<ProjectileBehaviour>().ExplosionForce = CurrentShootForce.x;

            switch (ShootingDirection)
            {
                case Direction.Forward:
                    newProjectile.GetComponent<Rigidbody>().AddForce(ShootPoint.forward * CurrentShootForce.x, ForceMode.Impulse);
                    break;
                case Direction.Up:
                    newProjectile.GetComponent<Rigidbody>().AddForce(ShootPoint.up * CurrentShootForce.y, ForceMode.Impulse);
                    break;
                case Direction.Diagonal:
                    newProjectile.GetComponent<Rigidbody>()
                        .AddForce((ShootPoint.forward * CurrentShootForce.x / 2) + (ShootPoint.up * CurrentShootForce.y / 2),
                            ForceMode.Impulse);
                    break;
            }

            User.GetComponent<UnitBehaviour>().ShotsFiredDuringRound++;
            
            //user.GetComponent<UnitBehaviour>().canAct = false;
            
            CurrentShootForce = DefaultShootForce;
            
            GameManager.Instance.UIReferences.ChargeBar.fillAmount = 0;
            
            GameManager.Instance.FirstPersonCamera.Follow = newProjectile.transform;
            GameManager.Instance.FirstPersonCamera.LookAt = newProjectile.transform;

            User.GetComponent<UnitBehaviour>().CanAim = false;

            GameManager.Instance.MainCamera.m_XAxis.m_InputAxisName = "";
            GameManager.Instance.MainCamera.m_YAxis.m_InputAxisName = "";
            
            if (GameManager.Instance.AlivePlayers.Count > 1)
            {
                //Debug.Log("can't act " + GameManager.Instance._currentPlayer.currentUnit);
                
                //TODO: Fix turn starting twice if unit shoots an enemy and kills them/itself
                
                GameManager.Instance.ShotFiredEvent.Invoke();
            }

            User.GetComponent<UnitBehaviour>().CanShoot = false;
            
            Destroy(newProjectile.gameObject, 10f);
        }
    }
}
