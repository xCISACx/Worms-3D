using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourDropdownBehaviour : MonoBehaviour
{
    public MainMenuManager MainMenuManager;
    public int PlayerIndex;
    // Start is called before the first frame update
    private void Awake()
    {
        MainMenuManager = FindObjectOfType<MainMenuManager>();
    }

    public void SetPlayerColour(int colour)
    {
        MainMenuManager.PlayerColours[PlayerIndex] = colour.ToString();
        
        //Debug.Log("index: " + playerIndex + "colour " + colour);

        switch (MainMenuManager.PlayerColours[PlayerIndex])
        {
            case "0":
                //Red
                MainMenuManager.PlayerColours[PlayerIndex] = "#FF0000";
                break;
            case "1":
                //Green
                MainMenuManager.PlayerColours[PlayerIndex] = "#00FF00";
                break;
            case "2":
                MainMenuManager.PlayerColours[PlayerIndex] = "#0000FF";
                //Blue
                break;
            case "3":
                //Yellow
                MainMenuManager.PlayerColours[PlayerIndex] = "#FFFF00";
                break;
        }
    }
}
