using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public bool explosive;
    public int damage;
    public GameObject ExplosionPrefab;
    public LayerMask layerMask;
    private Vector3 origin;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float upwardsModifier;
    [SerializeField] private TerrainDamageConfig TerrainDamageConfig;
    [SerializeField] private bool spawnedExplosion = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Environment"))
        {
            origin = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            
            if (explosive && !spawnedExplosion)
            {
                Instantiate(ExplosionPrefab, 
                    origin,
                    Quaternion.identity);
                ApplyKnockback(origin);
                ApplySplashDamage(origin);
                //other.GetComponent<TerrainDamager>().ApplyDamage(origin, TerrainDamageConfig, 1.0f);
                spawnedExplosion = true;
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
                //TODO: Destroy environment?
            }
        }
        
        else if (other.gameObject.CompareTag("Unit"))
        {
            origin = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            
            if (explosive && !spawnedExplosion)
            {
                Instantiate(ExplosionPrefab, 
                    origin,
                    Quaternion.identity);
                ApplyKnockback(origin);
                ApplySplashDamage(origin);
                //TODO: Deal more damage closer to explosion center.
                spawnedExplosion = true;
                Destroy(gameObject);
            }
            else
            {
                other.gameObject.GetComponent<UnitBehaviour>().TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    private void ApplySplashDamage(Vector3 pos)
    {
        Debug.Log("Applying Splash Damage");
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, layerMask);

        foreach (Collider collider in colliders)
        {
            Debug.Log(collider.gameObject.name);
            UnitBehaviour unitBehaviour = collider.GetComponent<UnitBehaviour>();
            
            if (unitBehaviour != null)
            {
                unitBehaviour.TakeDamage(damage);
            }
        }
    }

    private void ApplyKnockback(Vector3 pos)
    {
        Debug.Log("Applying Knockback");
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, layerMask);

        foreach (Collider collider in colliders)
        {
            Debug.Log(collider.gameObject.name);
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier);
                Debug.Log("adding explosion force");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
