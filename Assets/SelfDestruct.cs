using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public GameObject parent;
    
    public void Destruct()
    {
        Debug.Log("destroy damage pop-up");
        Destroy(parent);
    }
}
