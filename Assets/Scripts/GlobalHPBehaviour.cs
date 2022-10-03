using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class GlobalHPBehaviour : MonoBehaviour
{
    public List<Image> HPBars;

    private bool initialisedColours = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.AlivePlayers.Count > 0 && !initialisedColours)
        {
            var allBars = GetComponentsInChildren<Image>(true).ToList();

            for (int i = 0; i < GameManager.Instance.playerList.Count; i++)
            {
                HPBars.Add(allBars[i]);

                GameManager.Instance.playerList[i].TeamHPBar = HPBars[i];
                
                HPBars[i].color = GameManager.Instance.playerList[i].unitList[0].PlayerColour;
                HPBars[i].gameObject.SetActive(true);

                initialisedColours = true;
            }
        }
    }

    private void Awake()
    {
        HPBars = GetComponentsInChildren<Image>().ToList();
    }
}
