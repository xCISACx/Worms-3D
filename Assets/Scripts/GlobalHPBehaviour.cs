using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class GlobalHPBehaviour : MonoBehaviour
{
    public List<Image> HPBars;
    public List<Transform> HPBarsTransform;

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
            HPBars.Clear();
            
            var allBars = new List<Image>();
            
            HPBarsTransform = GetTopLevelChildren(transform).ToList();
        
            foreach (var tr in HPBarsTransform)
            {
                var image = tr.GetComponent<Image>();
            
                allBars.Add(image);
            }

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
        /*HPBars.Clear();

        HPBarsTransform = GetTopLevelChildren(transform).ToList();
        
        foreach (var tr in HPBarsTransform)
        {
            var image = tr.GetComponent<Image>();
            
            HPBars.Add(image);
        }*/
    }
    
    public static Transform[] GetTopLevelChildren(Transform parent)
    {
        Transform[] Children = new Transform[parent.childCount];
        
        for (int ID = 0; ID < parent.childCount; ID++)
        {
            Children[ID] = parent.GetChild(ID);
        }
        
        return Children;
    }
}
