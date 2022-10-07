using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class GlobalHpBehaviour : MonoBehaviour
{
    public List<Image> HpBars;
    public List<Transform> HpBarsTransform;

    private bool _initialisedColours = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.AlivePlayers.Count > 0 && !_initialisedColours)
        {
            HpBars.Clear();
            
            var allBars = new List<Image>();
            
            HpBarsTransform = GetTopLevelChildren(transform).ToList();
        
            foreach (var tr in HpBarsTransform)
            {
                var image = tr.GetComponent<Image>();
            
                allBars.Add(image);
            }

            for (int i = 0; i < GameManager.Instance.PlayerList.Count; i++)
            {
                HpBars.Add(allBars[i]);

                GameManager.Instance.PlayerList[i].TeamHpBar = HpBars[i];
                
                HpBars[i].color = GameManager.Instance.PlayerList[i].UnitList[0].PlayerColour;
                HpBars[i].gameObject.SetActive(true);

                _initialisedColours = true;
            }
        }
    }

    public static Transform[] GetTopLevelChildren(Transform parent)
    {
        Transform[] children = new Transform[parent.childCount];
        
        for (int id = 0; id < parent.childCount; id++)
        {
            children[id] = parent.GetChild(id);
        }
        
        return children;
    }
}
