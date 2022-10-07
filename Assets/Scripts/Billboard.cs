using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {
    public Transform Cam;
    private void Awake() 
    {
        Cam = Camera.main.transform;
    }
    void LateUpdate() 
    {
        transform.LookAt(transform.position + Cam.forward);
    }
}
