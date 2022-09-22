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
    public bool canChangeTurn;
    public List<UnitBehaviour> unitList;
    public UnitBehaviour currentUnit;
    public int currentUnitIndex = 0;
    public bool roundUnitPicked;

    public List<Weapon> WeaponInventory;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.matchStarted)
        {
            currentUnit = unitList[0];
        }
    }

    private void Update()
    {
        if (!currentUnit && unitList.Count > 0)
        {
            currentUnit = unitList[0];
        }
    }

    private void Awake()
    {
        GameManager.Instance.AlivePlayers.Add(this);
    }
}
