using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}
