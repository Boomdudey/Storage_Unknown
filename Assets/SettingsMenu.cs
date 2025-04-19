using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    #region Variables
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Slider volumeSlider;
    public AudioMixer audioMixer;

    Resolution[] screenrResolutions;
    #endregion

    public void GraphicsQualityChange()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }

    public void ResolutionChange(int resolutionIndex)
    {
        Resolution resolution = screenrResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void FullScreenToggle(bool isFullScreenButtonClicked)
    {
        Screen.fullScreen = isFullScreenButtonClicked;
    }

    public void VolumeChange()
    {
        audioMixer.SetFloat("MasterVolume", volumeSlider.value);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // sets graphic settings to ultra on start
        QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1);
        graphicsDropdown.value = QualitySettings.names.Length - 1;
        graphicsDropdown.RefreshShownValue();

        // Set screenResolutions array to the screen resolutions available on the device
        screenrResolutions = Screen.resolutions;
        // clears any and all options on the dropdown
        resolutionDropdown.ClearOptions();
        // makes a new list of strings to hold the screen resolutions
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        // adds the current screen resolution to the list of options
        for (int i = 0; i < screenrResolutions.Length; i++)
        {
            string option = screenrResolutions[i].width + " x " + screenrResolutions[i].height;
            options.Add(option);

            if (screenrResolutions[i].width == Screen.currentResolution.width && screenrResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        // finally adds the options to the dropdown 
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
