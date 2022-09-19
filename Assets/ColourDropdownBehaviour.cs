using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourDropdownBehaviour : MonoBehaviour
{
    public MenuManager MenuManager;
    public int playerIndex;
    // Start is called before the first frame update
    private void Awake()
    {
        MenuManager = FindObjectOfType<MenuManager>();
    }

    public void SetPlayerColour(int colour)
    {
        MenuManager.PlayerColours[playerIndex] = colour.ToString();
        
        Debug.Log("index: " + playerIndex + "colour " + colour);

        switch (MenuManager.PlayerColours[playerIndex])
        {
            case "0":
                //Red
                MenuManager.PlayerColours[playerIndex] = "#FF0000";
                break;
            case "1":
                //Green
                MenuManager.PlayerColours[playerIndex] = "#00FF00";
                break;
            case "2":
                MenuManager.PlayerColours[playerIndex] = "#0000FF";
                //Blue
                break;
            case "3":
                //Yellow
                MenuManager.PlayerColours[playerIndex] = "#FFFF00";
                break;
        }
    }
}
