using UnityEngine;

public static class PlayerPrefManager
{
    public static void SetScore(int score)
    {
        PlayerPrefs.SetInt("Score", score);
    }

    public static int GetScore()
    {
        if (PlayerPrefs.HasKey("Score"))
        {
            return PlayerPrefs.GetInt("Score");
        }
        else
        {
            return 1000; // default score
        }
    }
    public static void SetHighscore(int highscore)
    {
        PlayerPrefs.SetInt("Highscore", highscore);
    }

    public static int GetHighscore()
    {
        if (PlayerPrefs.HasKey("Highscore"))
        {
            return PlayerPrefs.GetInt("Highscore");
        }
        else
        {
            return 0;
        }
    }

    public static void SetResolution(int resolutionIndex)
    {
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }

    public static int GetResolution()
    {
        if (PlayerPrefs.HasKey("Resolution"))
        {
            return PlayerPrefs.GetInt("Resolution");
        }
        else
        {
            return Screen.resolutions.Length - 1;
        }
    }

    public static void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("Main volume", volume);
    }

    public static float GetVolume()
    {
        if (PlayerPrefs.HasKey("Main volume"))
        {
            return PlayerPrefs.GetFloat("Main volume");
        }
        else
        {
            return 1f;
        }
    }

    public static void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("Music volume", volume);
    }

    public static float GetMusicVolume()
    {
        if (PlayerPrefs.HasKey("Music volume"))
        {
            return PlayerPrefs.GetFloat("Music volume");
        }
        else
        {
            return 1f;
        }
    }

    public static void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFX volume", volume);
    }

    public static float GetSFXVolume()
    {
        if (PlayerPrefs.HasKey("SFX volume"))
        {
            return PlayerPrefs.GetFloat("SFX volume");
        }
        else
        {
            return 1f;
        }
    }

    public static void SetQuality(int qualityIndex)
    {
        PlayerPrefs.SetInt("Quality", qualityIndex);
    }

    public static int GetQuality()
    {
        if (PlayerPrefs.HasKey("Quality"))
        {
            return PlayerPrefs.GetInt("Quality");
        }
        else
        {
            return 2; // 0 = low, 1 = medium, 2 = high
        }
    }

    public static void SetFullscreen(bool isFullscreen)
    {
        if (isFullscreen)
        {
            PlayerPrefs.SetInt("Fullscreen", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Fullscreen", 0);
        }
    }

    public static bool GetFullscreen()
    {
        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            return PlayerPrefs.GetInt("Fullscreen") == 1;
        }
        else
        {
            return true;
        }
    }

    public static void SetVibration(bool isVibration)
    {
        if (isVibration)
        {
            PlayerPrefs.SetInt("Vibration", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Vibration", 0);
        }
    }

    public static bool GetVibration()
    {
        if (PlayerPrefs.HasKey("Vibration"))
        {
            return PlayerPrefs.GetInt("Vibration") == 1;
        }
        else
        {
            return true;
        }
    }

    public static void SetScreenShake(bool isScreenShake)
    {
        if (isScreenShake)
        {
            PlayerPrefs.SetInt("Screenshake", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Screenshake", 0);
        }
    }

    public static bool GetScreenShake()
    {
        if (PlayerPrefs.HasKey("Screenshake"))
        {
            return PlayerPrefs.GetInt("Screenshake") == 1;
        }
        else
        {
            return true;
        }
    }

    public static void SetMouseWindowLock(bool isMouseWindowLock)
    {
        if (isMouseWindowLock)
        {
            PlayerPrefs.SetInt("Mouse window lock", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Mouse window lock", 0);
        }
    }

    public static bool GetMouseWindowLock()
    {
        if (PlayerPrefs.HasKey("Mouse window lock"))
        {
            return PlayerPrefs.GetInt("Mouse window lock") == 1;
        }
        else
        {
            return true;
        }
    }

    public static void SetPostProcessing(bool isMouseWindowLock)
    {
        if (isMouseWindowLock)
        {
            PlayerPrefs.SetInt("Post processing", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Post processing", 0);
        }
    }

    public static bool GetPostProcessing()
    {
        if (PlayerPrefs.HasKey("Post processing"))
        {
            return PlayerPrefs.GetInt("Post processing") == 1;
        }
        else
        {
            return true;
        }
    }

    // Store the current player state info into PlayerPrefs
    public static void SavePlayerState(int score, int highScore)
    {
        // Save currentscore and lives to PlayerPrefs for moving to next level
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("Highscore", highScore);
    }

    // Reset stored player state and variables back to defaults
    public static void ResetPlayerState(bool resetHighscore = false)
    {
        PlayerPrefs.SetInt("Score", 1000);

        if (resetHighscore)
            PlayerPrefs.SetInt("Highscore", 0);
    }

    // Output the defined Player Prefs to the console
    public static void ShowPlayerPrefs()
    {
        // Store the PlayerPref keys to output to the console
        string[] values = { "Score", "Highscore", "Resolution", "Main volume", "Music volume", "SFX volume", "Quality", "Fullscreen", "Vibration", "Screenshake", "Mouse window lock", "Post processing" };

        // Loop over the values and output to the console
        foreach (string value in values)
        {
            if (PlayerPrefs.HasKey(value))
            {
                if (value == "Main volume" || value == "Music volume" || value == "SFX volume")
                {
                    Debug.Log(value + " = " + PlayerPrefs.GetFloat(value));
                }
                else
                {
                    Debug.Log(value + " = " + PlayerPrefs.GetInt(value));
                }

            }
            else
            {
                Debug.Log(value + " is not set.");
            }
        }
    }
}
