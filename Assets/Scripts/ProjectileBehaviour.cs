using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public bool Explosive;
    public int Damage;
    public GameObject ExplosionPrefab;
    public LayerMask LayerMask;
    private Vector3 _origin;
    [SerializeField] public float ExplosionForce;
    [SerializeField] public float ExplosionRadius;
    [SerializeField] public float UpwardsModifier;
    [SerializeField] private bool _spawnedExplosion = false;

    private void OnTriggerEnter(Collider other)
    {
        var unit = other.GetComponent<UnitBehaviour>();
                
        if (unit)
        {
            unit.BeingKnockedBack = true;
            //unit.grounded = false;
            unit.GetComponent<Rigidbody>().isKinematic = false;
            //Debug.Log("set being knocked back to true");
        }
        
        if (other.gameObject.CompareTag("Environment") || other.gameObject.CompareTag("Wall"))
        {
            _origin = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            if (Explosive && !_spawnedExplosion)
            {
                Instantiate(ExplosionPrefab, _origin, Quaternion.identity);
                ApplyKnockback(_origin);
                ApplySplashDamage(_origin);
                
                _spawnedExplosion = true;
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        if (other.gameObject.CompareTag("Ground"))
        {
            _origin = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            if (Explosive && !_spawnedExplosion)
            {
                Instantiate(ExplosionPrefab, _origin, Quaternion.identity);
                ApplyKnockback(_origin);
                ApplySplashDamage(_origin);

                var origin = transform.position;

                if (other.gameObject.GetComponent<TerrainBehaviour>())
                {
                    other.gameObject.GetComponent<TerrainBehaviour>().DestroyTerrain(origin, ExplosionRadius);
                }

                _spawnedExplosion = true;
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        else if (other.gameObject.CompareTag("Unit"))
        {
            GameManager.Instance.FirstPersonCamera.Follow = other.transform;
            GameManager.Instance.FirstPersonCamera.LookAt = other.transform;
            
            _origin = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            if (Explosive && !_spawnedExplosion)
            {
                Instantiate(ExplosionPrefab, _origin, Quaternion.identity);
                ApplyKnockback(_origin);
                ApplySplashDamage(_origin);

                //TODO: Deal more damage closer to explosion center.
                _spawnedExplosion = true;
                Destroy(gameObject);
            }
            else
            {
                other.gameObject.GetComponent<UnitBehaviour>().TakeDamage(Damage);
                Destroy(gameObject);
            }
        }
    }

    private void ApplySplashDamage(Vector3 pos)
    {
        //Debug.Log("Applying Splash Damage");
        
        Collider[] colliders = Physics.OverlapSphere(pos, ExplosionRadius, LayerMask);

        foreach (Collider collider in colliders)
        {
            //Debug.Log(collider.gameObject.name);
            UnitBehaviour unitBehaviour = collider.GetComponent<UnitBehaviour>();
            
            if (unitBehaviour != null)
            {
                unitBehaviour.TakeDamage(Damage);
            }
        }
    }

    private void ApplyKnockback(Vector3 pos)
    {
        //Debug.Log("Applying Knockback with force " + ExplosionForce);
        
        Collider[] colliders = Physics.OverlapSphere(pos, ExplosionRadius, LayerMask);

        foreach (Collider collider in colliders)
        {
            //Debug.Log(collider.gameObject.name);
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            UnitBehaviour unit = collider.GetComponent<UnitBehaviour>();

            if (rb != null)
            {
                rb.isKinematic = false;
                //Debug.Log("setting kinematic to false knockback");
                rb.AddExplosionForce(ExplosionForce / 10f, transform.position, ExplosionRadius, UpwardsModifier, ForceMode.Impulse);
                if (unit)
                {
                    unit.TimeSpentGrounded = 0;
                    unit.Grounded = false;
                }
                //Debug.Log("adding explosion force");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
    }
}
