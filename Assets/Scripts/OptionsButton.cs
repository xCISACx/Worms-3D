using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsButton : MonoBehaviour
{
    [SerializeField] private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        
        _button.onClick.AddListener(ToggleOptions);
    }

    public void ToggleOptions()
    {
        FindObjectOfType<MenuManager>().Options();
    }
}
