using UnityEngine;
using UnityEngine.EventSystems; // include EventSystems namespace so can set initial input for controller support
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    // references to Submenus
    public GameObject _MainMenu;
    public GameObject _OptionsMenu;
    public GameObject _AboutMenu;

    // references to Button GameObjects
    public GameObject PlayButton;
    public GameObject OptionsDefaultButton;
    public GameObject AboutDefaultButton;
    public GameObject QuitButton;

    // reference the menuTitleText so we can change it dynamically
    public TMP_Text menuTitleText;

    // store the initial title so we can set it back
    private string _mainTitle;

    public GameObject UICanvas;

    public AudioClip clickSFX;
    AudioSource _audio;

    // init the menu
    void Awake()
    {
        // store the initial title so we can set it back
        _mainTitle = menuTitleText.text;

        _audio = GetComponent<AudioSource>();

        // determine if Quit button should be shown
        DisplayQuitWhenAppropriate();
    }

    // determine if the QUIT button should be present based on what platform the game is running on
    void DisplayQuitWhenAppropriate()
    {
        switch (Application.platform)
        {
            // platforms that should have quit button
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:

            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
                QuitButton.SetActive(true);
                break;

            // platforms that should not have quit button
            // note: included just for demonstration purposed since
            // default will cover all of these.
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.WebGLPlayer:
                QuitButton.SetActive(false);
                break;

            // all other platforms default to no quit button
            default:
                QuitButton.SetActive(false);
                break;
        }
    }

    void ResetMenus()
    {
        _MainMenu.SetActive(false);
        _OptionsMenu.SetActive(false);
        _AboutMenu.SetActive(false);
    }

    // Public functions below that are available via the UI Event Triggers, such as on Buttons.

    // Show the proper menu
    public void ShowMenu(string name)
    {
        // turn on desired menu and set default selected button for controller input
        switch (name)
        {
            case "Home":
                ResetMenus();
                PlaySound(clickSFX);
                _MainMenu.SetActive(true);
                menuTitleText.text = _mainTitle;
                break;
            case "Options":
                if (_OptionsMenu.activeSelf)
                {
                    ShowMenu("Home");
                }
                else
                {
                    ResetMenus();
                    PlaySound(clickSFX);
                    _OptionsMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(OptionsDefaultButton);
                    menuTitleText.text = "Options";
                }
                break;
            case "About":
                if (_AboutMenu.activeSelf)
                {
                    ShowMenu("Home");
                }
                else
                {
                    ResetMenus();
                    PlaySound(clickSFX);
                    _AboutMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(AboutDefaultButton);
                    menuTitleText.text = "About";
                }
                break;
        }
    }

    // load the specified Unity level
    public void LoadLevel(string levelToLoad)
    {
        // start new game so initialize player state
        PlayerPrefManager.ResetPlayerState(false);

        SceneManager.LoadScene(levelToLoad);
    }

    // quit the game
    public void QuitGame()
    {
        Gamepad.current?.ResetHaptics();
        PlayerPrefManager.ResetPlayerState(false); // reset player state so we start a new game
        Application.Quit();
    }

    void PlaySound(AudioClip clip)
    {
        _audio.PlayOneShot(clip);
    }

    void OnApplicationQuit()
    {
        PlayerPrefManager.ResetPlayerState(false); // reset player state so we start a new game
        Gamepad.current?.ResetHaptics();
    }
}
