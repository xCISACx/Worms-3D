using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundcheckBehaviour : MonoBehaviour
{
    private UnitBehaviour unit;
    // Start is called before the first frame update
    private void Awake()
    {
        unit = transform.parent.GetComponent<UnitBehaviour>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            unit.grounded = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            unit.grounded = false;
        }
    }
}
