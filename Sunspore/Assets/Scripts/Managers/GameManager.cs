using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    // static reference to game manager so can be called from other scripts directly (not just through gameobject component)
    public static GameManager gm;

    public GameObject postProcessingVolume;

    // UI elements to control
    [Header("UI Elements")]
    public GameObject pauseMenu;
    [SerializeField] GameObject loseMenu;
    public GameObject _MainMenu;
    public TMP_Text menuTitleText;
    private string _mainTitle;

    [Header("SFX")]
    public AudioClip clickSFX;
    GameObject _player;
    PlayerInput _playerInput;
    AudioSource _audio;

    void Awake()
    {
        // setup reference to game manager
        if (gm == null)
            gm = GetComponent<GameManager>();

        SetUpDefaults();

        // Turn post processing volume on/off based on settings
        postProcessingVolume.SetActive(PlayerPrefManager.GetPostProcessing());
    }

    void SetUpDefaults()
    {
        // setup reference to player
        if (_player == null)
            _player = GameObject.FindGameObjectWithTag("Player");

        // setup reference to player input
        if (_playerInput == null)
            _playerInput = _player.GetComponent<PlayerInput>();

        _audio = GetComponent<AudioSource>();

        EnableUIActionMap();
    }

    public void GameLose(int amount)
    {
        if (Time.timeScale > 0f)
        {
            Gamepad.current?.ResetHaptics();
            Time.timeScale = 0f; // this pauses the game action  

            TMP_Text menuTitleText = loseMenu.transform.Find("Menu Title")?.GetComponent<TMP_Text>();
            menuTitleText.text = "You Are Broke!!! \nYou Finished With:\n" + amount + "Credits";

            // Show pause menu  
            loseMenu.SetActive(true);

            // Disable camera movement  
            if (_player != null)
            {
                Player playerScript = _player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.SetPauseState(true);
                }
            }

            Cursor.lockState = CursorLockMode.None;
        }
        Cursor.visible = true;
    }

    public void GoToHome()
    {
        // If home button pressed, go to home screen
        Time.timeScale = 1f;
        loseMenu.SetActive(false);
        PlayerPrefManager.ResetPlayerState(false);
        LoadLevel("Home");
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0f)
        {
            Gamepad.current?.ResetHaptics();
            Time.timeScale = 0f; // this pauses the game action

            // Show pause menu
            pauseMenu.SetActive(true);

            // Disable camera movement
            if (_player != null)
            {
                Player playerScript = _player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.SetPauseState(true);
                }
            }

            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Gamepad.current?.ResetHaptics();
            pauseMenu.SetActive(false);
            Time.timeScale = 1f; // this unpauses the game action (ie. back to normal)

            // Re-enable camera movement
            if (_player != null)
            {
                Player playerScript = _player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.SetPauseState(false);
                }
            }

            Cursor.lockState = CursorLockMode.Confined;
        }
        Cursor.visible = true;
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            TogglePause();
        }
    }

    void ResetMenus()
    {
        _MainMenu.SetActive(false);
    }

    // Show the proper menu
    public void ShowMenu(string name)
    {
        // turn on desired menu and set default selected button for controller input
        switch (name)
        {
            case "MainMenu":
                ResetMenus();
                PlaySound(clickSFX);
                _MainMenu.SetActive(true);
                menuTitleText.text = _mainTitle;
                break;
        }
    }

    // load the specified Unity level
    public void LoadLevel(string levelToLoad)
    {
        SceneManager.LoadScene(levelToLoad);
    }

    void EnableUIActionMap()
    {
        if (_playerInput != null && _playerInput.actions != null)
        {
            // Enable the UI action map so pause input works
            var uiActionMap = _playerInput.actions.FindActionMap("UI");
            if (uiActionMap != null)
            {
                uiActionMap.Enable();
            }
            else
            {
                Debug.LogWarning("UI Action Map not found in input actions");
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (_audio != null && clip != null)
        {
            _audio.PlayOneShot(clip);
        }
    }

    // If editor is quit, reset player state
    void OnApplicationQuit()
    {
        PlayerPrefManager.ResetPlayerState(false);
    }
}
