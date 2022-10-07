using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBehaviour : MonoBehaviour
{
    public Mesh Model;
    public Pickup Pickup;
    public int healthToRestore = 0;
    public int timeToAdd = 0;
    private bool used = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(Pickup pickup)
    {
        var meshFilter = GetComponent<MeshFilter>();
        
        meshFilter.mesh = pickup.Model;
        
        healthToRestore = pickup.HealthAmount;

        timeToAdd = pickup.TimeAmount;
    }
    
    void Activate(UnitBehaviour unit, GameManager gameManager)
    {
        var newHealth = unit.CurrentHealth + healthToRestore;
        
        unit.SetHealth(newHealth);

        if (unit == gameManager._currentPlayer.currentUnit)
        {
            gameManager.SetTurnTimer(timeToAdd);
        
            gameManager._turnTimer += timeToAdd;   
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Unit"))
        {
            Debug.Log("unit trigger");
            
            var unit = other.GetComponent<UnitBehaviour>();
            
            Debug.Log(unit);

            if (!used)
            {
                Activate(unit, GameManager.Instance);
                
                used = true;
                
                Destroy(gameObject);
            }
        }
    }
}
