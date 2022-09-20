using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    [Header ("Name")]
    
    public string name;
    
    [Header("Model")] 
    
    public GameObject model;

    [Header("Stats")]
    
    public GameObject ammoPrefab;
    public int damage;
    public Vector3 shootingForce;
    public float accuracy;

    public WeaponBehaviour.Direction shootingDirection;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
