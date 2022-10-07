using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallColliderBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _unit;

    private void OnDisable()
    {
        if (_unit)
        {
            _unit.GetComponent<CapsuleCollider>().material.dynamicFriction = 0.4f;   
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            _unit.GetComponent<CapsuleCollider>().material.dynamicFriction = 0;
        }
    }
}
