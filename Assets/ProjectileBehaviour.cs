using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    public GameObject ExplosionPrefab;
    public LayerMask layerMask;
    private Vector3 origin;
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float upwardsModifier;
    


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
            Instantiate(ExplosionPrefab, 
                origin,
                Quaternion.identity);
            ApplyKnockback(origin);
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
}
