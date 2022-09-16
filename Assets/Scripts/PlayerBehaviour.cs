using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public string PlayerName;
    public int playerIndex = 0;
    public bool canPlay;
    public bool turnStarted;
    public bool unitPickedFlag;
    public List<UnitBehaviour> unitList;
    public UnitBehaviour currentUnit;
    public int currentUnitIndex = 0;
    public bool roundUnitPicked;

    public List<Weapon> WeaponInventory;

    // Start is called before the first frame update
    void Start()
    {
        currentUnit = unitList[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        unitList = GetComponentsInChildren<UnitBehaviour>().ToList();
        name = "Player " + (playerIndex + 1);
    }
}
