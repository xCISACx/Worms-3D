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
    public int OriginalPlayerIndex = 0;

    enum CurrentPhase
    {
        Waiting,
        UnitSelection,
        UnitAction,
        TurnEnd
    }
    
    public bool CanPlay;
    public bool TurnStarted;
    public bool UnitPickedFlag;
    public bool CanChangeTurn;
    public List<UnitBehaviour> UnitList;
    public UnitBehaviour CurrentUnit;
    public int CurrentUnitIndex = 0;
    public bool RoundUnitPicked;
    private bool _initialisationDone;

    public List<Weapon> WeaponInventory;

    public int GlobalTeamHp = 0;
    public Image TeamHpBar;

    public bool SelfDestructed;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance.MatchStarted)
        {
            CurrentUnit = UnitList[0];
            GameManager.Instance.SetCurrentUnitEvent.Invoke(CurrentUnit);
        }
    }

    private void Update()
    {
        if (!_initialisationDone)
        {
            UpdateBar();
            
            _initialisationDone = true;
        }
        
        if (!CurrentUnit && UnitList.Count > 0)
        {
            CurrentUnit = UnitList[0];
            GameManager.Instance.SetCurrentUnitEvent.Invoke(CurrentUnit);
        }
    }

    private void Awake()
    {
        GameManager.Instance.AlivePlayers.Add(this);
    }
    
    public void SelfDestruct()
    {
        GameManager.Instance.PlayerDiedEvent.Invoke(this);
    }
    
    public void UpdateBar()
    {
        GlobalTeamHp = 0;
        
        //Debug.Log("Updating Health Bar for " + gameObject.name);
        
        for (int j = 0; j < UnitList.Count; j++)
        {
            var unitScript = UnitList[j].GetComponent<UnitBehaviour>();

            GlobalTeamHp += unitScript.CurrentHealth;
        }
        
        GlobalTeamHp /= GameManager.Instance.NumberOfStartingUnits;

        TeamHpBar.fillAmount = Mathf.InverseLerp(0, 100, GlobalTeamHp);
    }

    private void OnDestroy()
    {
        //Debug.Log(gameObject.name + " died");
    }
}
