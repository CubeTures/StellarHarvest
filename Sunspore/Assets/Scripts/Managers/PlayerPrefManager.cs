using System.Linq;
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
            return -300000000; // default score
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

    public static void SetEventCount(int eventCount)
    {
        PlayerPrefs.SetInt("EventCount", eventCount);
    }

    public static int GetEventCount()
    {
        if (PlayerPrefs.HasKey("EventCount"))
        {
            return PlayerPrefs.GetInt("EventCount");
        }
        else
        {
            return 0;
        }
    }

    public static void SetTutorialStep(int tutorialStep)
    {
        PlayerPrefs.SetInt("TutorialStep", tutorialStep);
    }

    public static int GetTutorialStep()
    {
        if (PlayerPrefs.HasKey("TutorialStep"))
        {
            return PlayerPrefs.GetInt("TutorialStep");
        }
        else
        {
            return 0;
        }
    }

    public static void SetInventoryOpenCount(int eventCount)
    {
        PlayerPrefs.SetInt("InventoryOpenCount", eventCount);
    }

    public static int GetInventoryOpenCount()
    {
        if (PlayerPrefs.HasKey("InventoryOpenCount"))
        {
            return PlayerPrefs.GetInt("InventoryOpenCount");
        }
        else
        {
            return 0;
        }
    }

    public static void SetStoreOpenCount(int eventCount)
    {
        PlayerPrefs.SetInt("StoreOpenCount", eventCount);
    }

    public static int GetStoreOpenCount()
    {
        if (PlayerPrefs.HasKey("StoreOpenCount"))
        {
            return PlayerPrefs.GetInt("StoreOpenCount");
        }
        else
        {
            return 0;
        }
    }

    public static void SetMoveCount(int moveCount)
    {
        PlayerPrefs.SetInt("MoveCount", moveCount);
    }

    public static int GetMoveCount()
    {
        if (PlayerPrefs.HasKey("MoveCount"))
        {
            return PlayerPrefs.GetInt("MoveCount");
        }
        else
        {
            return 0;
        }
    }

    public static void SetCraftingCount(int craftingCount)
    {
        PlayerPrefs.SetInt("CraftingCount", craftingCount);
    }

    public static int GetCraftingCount()
    {
        if (PlayerPrefs.HasKey("CraftingCount"))
        {
            return PlayerPrefs.GetInt("CraftingCount");
        }
        else
        {
            return 0;
        }
    }

    public static void SetResolution(int resolutionIndex)
    {
        PlayerPrefs.SetInt("ResolutionHighFirst", resolutionIndex);
    }

    /// <summary>
    /// Same ordering as the options dropdown: largest pixel count first, then highest refresh.
    /// </summary>
    public static Resolution[] GetResolutionsSortedHighFirst()
    {
        return Screen.resolutions
            .OrderByDescending(r => r.width * r.height)
            .ThenByDescending(r => (float)r.refreshRateRatio.value)
            .ToArray();
    }

#if !UNITY_EDITOR
    /// <summary>
    /// Applies saved resolution before the first scene so gameplay is not stuck at the OS/window default
    /// until the options menu opens (especially relevant on Linux standalone builds).
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void ApplySavedScreenResolutionBootstrap()
    {
        ApplySavedScreenResolution();
    }
#endif

    /// <summary>
    /// Applies width/height/fullscreen from prefs. Safe to call multiple times (e.g. menu + bootstrap).
    /// </summary>
    public static void ApplySavedScreenResolution()
    {
        Resolution[] sorted = GetResolutionsSortedHighFirst();
        if (sorted.Length == 0)
            return;

        int idx = GetResolutionIndex(sorted);
        Resolution res = sorted[idx];
        Screen.SetResolution(res.width, res.height, GetFullscreen());
    }

    /// <summary>
    /// Index into <paramref name="sortedHighFirst"/> (width×height descending, then refresh descending).
    /// Defaults to 0 (highest).
    /// <para>
    /// Older builds initialized the dropdown index to 0 and only overwrote it when
    /// <see cref="Screen.currentResolution"/> matched an entry in <see cref="Screen.resolutions"/>.
    /// On Linux (Wayland/scaling/refresh reporting), that match often failed, leaving index 0 — and Unity lists
    /// modes ascending, so 0 was the smallest resolution.
    /// </para>
    /// Migrates legacy <c>Resolution</c> prefs by matching WxH (and refresh when possible).
    /// </summary>
    public static int GetResolutionIndex(Resolution[] sortedHighFirst)
    {
        int n = sortedHighFirst.Length;
        if (n == 0)
            return 0;

        const string key = "ResolutionHighFirst";
        if (PlayerPrefs.HasKey(key))
            return Mathf.Clamp(PlayerPrefs.GetInt(key), 0, n - 1);

        const string legacyKey = "Resolution";
        if (PlayerPrefs.HasKey(legacyKey))
        {
            var raw = Screen.resolutions;
            if (raw.Length > 0)
            {
                int oldIdx = Mathf.Clamp(PlayerPrefs.GetInt(legacyKey), 0, raw.Length - 1);
                Resolution r = raw[oldIdx];
                for (int i = 0; i < sortedHighFirst.Length; i++)
                {
                    if (sortedHighFirst[i].width != r.width || sortedHighFirst[i].height != r.height)
                        continue;
                    if (Mathf.Approximately(
                            (float)sortedHighFirst[i].refreshRateRatio.value,
                            (float)r.refreshRateRatio.value))
                        return i;
                }

                // Refresh may differ slightly vs. listed modes (common on Linux); keep WxH.
                for (int i = 0; i < sortedHighFirst.Length; i++)
                {
                    if (sortedHighFirst[i].width == r.width && sortedHighFirst[i].height == r.height)
                        return i;
                }
            }
        }

        return 0;
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
        PlayerPrefs.SetInt("Score", -300000000);

        if (resetHighscore)
            PlayerPrefs.SetInt("Highscore", 0);

        PlayerPrefs.SetInt("EventCount", 0);
    }

    // Output the defined Player Prefs to the console
    public static void ShowPlayerPrefs()
    {
        // Store the PlayerPref keys to output to the console
        string[] values =
        {
            "Score",
            "Highscore",
            "EventCount",
            "InventoryOpenCount",
            "StoreOpenCount",
            "MoveCount",
            "CraftingCount",
            "Resolution",
            "ResolutionHighFirst",
            "Main volume",
            "Music volume",
            "SFX volume",
            "Quality",
            "Fullscreen",
            "Vibration",
            "Screenshake",
            "Mouse window lock",
            "Post processing",
            "TutorialStep",
        };

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
