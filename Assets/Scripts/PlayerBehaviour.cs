using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public bool canPlay;
    public bool turnStarted;
    public bool unitPickedFlag;
    public List<UnitBehaviour> unitList;
    public UnitBehaviour currentUnit;
    public int currentUnitIndex = 0;
    public bool roundUnitPicked;
    
    // Start is called before the first frame update
    void Start()
    {
        currentUnit = unitList[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
