using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerBehaviour : MonoBehaviour
{
    public string PlayerName;
    public int playerIndex = 0;

    enum CurrentPhase
    {
        Waiting,
        UnitSelection,
        UnitAction,
        TurnEnd
    }
    
    public bool canPlay;
    public bool turnStarted;
    public bool unitPickedFlag;
    public bool canChangeTurn;
    public List<UnitBehaviour> unitList;
    public UnitBehaviour currentUnit;
    public int currentUnitIndex = 0;
    public bool roundUnitPicked;
    private bool initialisationDone;

    public List<Weapon> WeaponInventory;

    public int GlobalTeamHP = 0;
    public Image TeamHPBar;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.matchStarted)
        {
            currentUnit = unitList[0];
            GameManager.Instance.SetCurrentUnitEvent.Invoke(currentUnit);
        }
    }

    private void Update()
    {
        if (!initialisationDone)
        {
            UpdateBar();
            
            initialisationDone = true;
        }
        
        if (!currentUnit && unitList.Count > 0)
        {
            currentUnit = unitList[0];
            GameManager.Instance.SetCurrentUnitEvent.Invoke(currentUnit);
        }
    }

    private void Awake()
    {
        GameManager.Instance.AlivePlayers.Add(this);
    }
    
    public void SelfDestruct()
    {
        GameManager.Instance.PlayerDiedEvent.Invoke(this);
        Destroy(gameObject, 2f);
    }
    
    public void UpdateBar()
    {
        GlobalTeamHP = 0;
        
        Debug.Log("Updating Health Bar for " + gameObject.name);
        
        for (int j = 0; j < unitList.Count; j++)
        {
            var unitScript = unitList[j].GetComponent<UnitBehaviour>();

            GlobalTeamHP += unitScript.CurrentHealth;
        }
        
        GlobalTeamHP /= GameManager.Instance.NumberOfStartingUnits;

        TeamHPBar.fillAmount = Mathf.InverseLerp(0, 100, GlobalTeamHP);

    }
}
