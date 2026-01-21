using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;

public class OptionsMenuManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;
    public Slider mainVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public Toggle vibrationToggle;
    public Toggle screenShakeToggle;
    public Toggle mouseWindowLockToggle;
    public Toggle postProcessingToggle;

    void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            float refreshRate = Mathf.Round((float)resolutions[i].refreshRateRatio.value * 100f) / 100f; // Round to 2 decimal places
            string option = resolutions[i].width + " x " + resolutions[i].height + ", " + refreshRate + " Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height && resolutions[i].refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        SetResolution(currentResolutionIndex);
        SetMainVolume(PlayerPrefManager.GetVolume());
        SetSFXVolume(PlayerPrefManager.GetSFXVolume());
        SetMusicVolume(PlayerPrefManager.GetMusicVolume());
        SetQuality(PlayerPrefManager.GetQuality());
        SetFullscreen(PlayerPrefManager.GetFullscreen());
        SetVibration(PlayerPrefManager.GetVibration());
        SetScreenShake(PlayerPrefManager.GetScreenShake());
        SetMouseWindowLock(PlayerPrefManager.GetMouseWindowLock());
        SetPostProcessing(PlayerPrefManager.GetPostProcessing());
    }

    public void SetResolution(int resolutionIndex)
    {
        resolutionDropdown.value = resolutionIndex;
        Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].height, Screen.fullScreen);
        PlayerPrefManager.SetResolution(resolutionIndex);
    }

    public void SetMainVolume(float volume)
    {
        mainVolumeSlider.value = volume;
        audioMixer.SetFloat("Main volume", volume);
        PlayerPrefManager.SetVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolumeSlider.value = volume;
        audioMixer.SetFloat("SFX volume", volume);
        PlayerPrefManager.SetSFXVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolumeSlider.value = volume;
        audioMixer.SetFloat("Music volume", volume);
        PlayerPrefManager.SetMusicVolume(volume);
    }

    public void SetQuality(int qualityIndex)
    {
        qualityDropdown.value = qualityIndex;
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefManager.SetQuality(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;
        PlayerPrefManager.SetFullscreen(isFullscreen);
    }

    public void SetVibration(bool isVibration)
    {
        vibrationToggle.isOn = isVibration;
        PlayerPrefManager.SetVibration(isVibration);
    }

    public void SetScreenShake(bool isScreenShake)
    {
        screenShakeToggle.isOn = isScreenShake;
        PlayerPrefManager.SetScreenShake(isScreenShake);
    }

    public void SetMouseWindowLock(bool isMouseWindowLock)
    {
        mouseWindowLockToggle.isOn = isMouseWindowLock;
        Cursor.lockState = isMouseWindowLock ? CursorLockMode.Confined : CursorLockMode.None;
        PlayerPrefManager.SetMouseWindowLock(isMouseWindowLock);
    }

    public void SetPostProcessing(bool isPostProcessing)
    {
        postProcessingToggle.isOn = isPostProcessing;
        PlayerPrefManager.SetPostProcessing(isPostProcessing);
        if (GameManager.gm != null)
        {
            GameManager.gm.postProcessingVolume.SetActive(isPostProcessing);
        }
    }
}
