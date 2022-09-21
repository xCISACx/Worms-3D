using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourDropdownBehaviour : MonoBehaviour
{
    public MainMenuManager MainMenuManager;
    public int playerIndex;
    // Start is called before the first frame update
    private void Awake()
    {
        MainMenuManager = FindObjectOfType<MainMenuManager>();
    }

    public void SetPlayerColour(int colour)
    {
        MainMenuManager.PlayerColours[playerIndex] = colour.ToString();
        
        //Debug.Log("index: " + playerIndex + "colour " + colour);

        switch (MainMenuManager.PlayerColours[playerIndex])
        {
            case "0":
                //Red
                MainMenuManager.PlayerColours[playerIndex] = "#FF0000";
                break;
            case "1":
                //Green
                MainMenuManager.PlayerColours[playerIndex] = "#00FF00";
                break;
            case "2":
                MainMenuManager.PlayerColours[playerIndex] = "#0000FF";
                //Blue
                break;
            case "3":
                //Yellow
                MainMenuManager.PlayerColours[playerIndex] = "#FFFF00";
                break;
        }
    }
}
