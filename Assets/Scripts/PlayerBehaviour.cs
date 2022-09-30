using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            //add each unit's HP and divide by 4
        }
    }

    private void Update()
    {
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
        for (int j = 0; j < unitList.Count; j++)
        {
            var unitScript = unitList[j].GetComponent<UnitBehaviour>();

            GlobalTeamHP += unitScript.CurrentHealth;
            
            //Debug.Log(GlobalTeamHP);
        }
        
        GlobalTeamHP /= GameManager.Instance.NumberOfStartingUnits;
        
        //Debug.Log(GlobalTeamHP);
        
        /*for (int i = 0; i < GameManager.Instance.playerList.Count; i++)
        {
            for (int j = 0; j < GameManager.Instance.playerList[i].unitList.Count; j++)
            {
                var unitScript = GameManager.Instance.playerList[i].unitList[j].GetComponent<UnitBehaviour>();

                GlobalTeamHP += unitScript.CurrentHealth;
                GlobalTeamHP /= GameManager.Instance.playerList[i].unitList.Count;
            }
        }*/

        TeamHPBar.fillAmount = GlobalTeamHP / 100f;

    }
}
