using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PauseManager : MonoBehaviour
{
    public bool GamePaused = false;
    public Canvas PauseCanvas;
    public MenuManager MenuManager;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.PlayerControls.Player.Pause.triggered)
        {
            TogglePause();
        }

        if (GamePaused)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1;
            MenuManager.CloseOptions();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void TogglePause()
    {
        GamePaused = !GamePaused;
        PauseCanvas.enabled = GamePaused;
    }

    public void Options()
    {
        
    }

    public void QuitToMenu()
    {
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(0);
    }
}
