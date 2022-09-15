using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pickup", menuName = "Pickup")]
public class Pickup : ScriptableObject
{
    [Header("Model")] 
    
    public GameObject model;
    
    private float ammoAmount;
    private float healthAmount;
    private float armourAmount;
    private Weapon weapon;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
