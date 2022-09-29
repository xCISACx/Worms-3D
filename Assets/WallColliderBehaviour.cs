using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallColliderBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject Unit;

    private void OnDisable()
    {
        Unit.GetComponent<CapsuleCollider>().material.dynamicFriction = 0.4f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Unit.GetComponent<CapsuleCollider>().material.dynamicFriction = 0;
        }
    }
}
