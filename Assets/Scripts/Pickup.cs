using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pickup", menuName = "Pickup")]
public class Pickup : ScriptableObject
{
    [Header("Model")] 
    
    public GameObject model;
    
    [SerializeField] private float ammoAmount;
    [SerializeField] private float healthAmount;
    [SerializeField] private float armourAmount;
    [SerializeField] private Weapon weapon;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
