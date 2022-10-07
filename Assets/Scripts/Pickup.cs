using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pickup", menuName = "Pickup")]

public class Pickup : ScriptableObject
{
    [Header("Model")] 
    
    public Mesh Model;
    
    [SerializeField] private int ammoAmount;
    [SerializeField] public int HealthAmount;
    [SerializeField] private int armourAmount;
    [SerializeField] public int TimeAmount;
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
