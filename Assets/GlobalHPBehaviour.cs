using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class GlobalHPBehaviour : MonoBehaviour
{
    public Image[] HPBars;
    public int GlobalTeamHP = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.AlivePlayers.Count > 0)
        {
            for (int i = 0; i < GameManager.Instance.AlivePlayers.Count; i++)
            {
                HPBars[i].color = GameManager.Instance.playerList[i].unitList[0].PlayerColour;
            }
        }
    }

    private void Awake()
    {
        HPBars = GetComponentsInChildren<Image>();
    }

    public void UpdateBar(int playerIndex)
    {
        GlobalTeamHP = 0;
        
        var unitList = GameManager.Instance.playerList[playerIndex].unitList;
        
        for (int j = 0; j < unitList.Count; j++)
        {
            var unitScript = unitList[j].GetComponent<UnitBehaviour>();

            GlobalTeamHP += unitScript.CurrentHealth;
            Debug.Log(GlobalTeamHP);
        }
        
        GlobalTeamHP /= GameManager.Instance.NumberOfStartingUnits;
        
        Debug.Log(GlobalTeamHP);
        
        /*for (int i = 0; i < GameManager.Instance.playerList.Count; i++)
        {
            for (int j = 0; j < GameManager.Instance.playerList[i].unitList.Count; j++)
            {
                var unitScript = GameManager.Instance.playerList[i].unitList[j].GetComponent<UnitBehaviour>();

                GlobalTeamHP += unitScript.CurrentHealth;
                GlobalTeamHP /= GameManager.Instance.playerList[i].unitList.Count;
            }
        }*/

        HPBars[playerIndex].fillAmount = GlobalTeamHP / 100f;

    }
}
