using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] List<GameObject> currentHitObjects;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 20f, Vector3.zero, Mathf.Infinity);
        foreach (var hit in hits)
        {
            currentHitObjects.Add(hit.transform.gameObject);
        }
    }

    private void Awake()
    {
        
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 20f);
    }
}
