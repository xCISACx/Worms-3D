using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(ToggleOptions);
    }

    public void ToggleOptions()
    {
        FindObjectOfType<MenuManager>().Options();
        Debug.Log("a");
    }
}
