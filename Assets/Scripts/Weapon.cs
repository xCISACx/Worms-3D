using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    [Header ("Name")]
    
    public string Name;
    
    [Header("Model")] 
    
    public GameObject Model;

    [Header("Stats")]
    
    public GameObject AmmoPrefab;
    public int Damage;
    public Vector3 ShootingForce;
    public Vector3 MaxShootingForce;
    public float Accuracy;

    public WeaponBehaviour.Direction ShootingDirection;
    
    public bool Explosive;
    
    [SerializeField] public float ExplosionForce;
    [SerializeField] public float ExplosionRadius;
    [SerializeField] public float UpwardsModifier;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
