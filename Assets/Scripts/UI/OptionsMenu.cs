using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    private bool fullScreen;
    private int overallQuallity;
    private int antialiasing;
    private int resolutionIndex;
    private bool vSync;
    public Toggle fullScreenToggle;
    public TMP_Dropdown overallQuallityDropdown;
    public TMP_Dropdown antialiasingDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle vSyncToggle;
    public GameObject menuRef;
    private string fullScreenKey = "FullScreenKey";
    private string overallQuallityKey = "OverallQuallityKey";
    private string antialiasingKey = "AntialasingKey";
    private string resolutionIndexKey = "ResolutionIndexKey";
    private string vSyncKey = "VSyncKey";
    private void OnEnable()
    {
        if (PlayerPrefs.HasKey(fullScreenKey))
        {
            fullScreen = PlayerPrefs.GetInt(fullScreenKey) == 1;
            Screen.fullScreen = fullScreen;
        }
        else
        {
            fullScreen = Screen.fullScreen;
            PlayerPrefs.SetInt(fullScreenKey, Convert.ToInt32(fullScreen));
        }
        fullScreenToggle.isOn = fullScreen;
        if (PlayerPrefs.HasKey(overallQuallityKey))
        {
            overallQuallity = PlayerPrefs.GetInt(overallQuallityKey);
            QualitySettings.SetQualityLevel(overallQuallity);
        }
        else
        {
            overallQuallity = QualitySettings.GetQualityLevel();
            PlayerPrefs.SetInt(overallQuallityKey, overallQuallity);
        }
        overallQuallityDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = overallQuallityDropdown.options;
        options.Add(new TMP_Dropdown.OptionData("Fastest"));
        options.Add(new TMP_Dropdown.OptionData("Fast"));
        options.Add(new TMP_Dropdown.OptionData("Simple"));
        options.Add(new TMP_Dropdown.OptionData("Good"));
        options.Add(new TMP_Dropdown.OptionData("Beautiful"));
        options.Add(new TMP_Dropdown.OptionData("Fantastic"));
        overallQuallityDropdown.value = overallQuallity;
        overallQuallityDropdown.RefreshShownValue();
        if (PlayerPrefs.HasKey(antialiasingKey))
        {
            antialiasing = PlayerPrefs.GetInt(antialiasingKey);
            QualitySettings.antiAliasing = antialiasing;
        }
        else
        {
            antialiasing = QualitySettings.antiAliasing;
            PlayerPrefs.SetInt(antialiasingKey, antialiasing);
        }
        antialiasingDropdown.ClearOptions();
        options = antialiasingDropdown.options;
        options.Add(new TMP_Dropdown.OptionData("Off"));
        options.Add(new TMP_Dropdown.OptionData("x2"));
        options.Add(new TMP_Dropdown.OptionData("x4"));
        options.Add(new TMP_Dropdown.OptionData("x8"));
        antialiasingDropdown.value = antialiasing;
        antialiasingDropdown.RefreshShownValue();

        Resolution[] resolutions = Screen.resolutions;
        if (PlayerPrefs.HasKey(resolutionIndexKey))
        {
            resolutionIndex = PlayerPrefs.GetInt(resolutionIndexKey);
            Resolution res = Screen.resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, fullScreen, res.refreshRate);
        }
        else
        {
            int i;
            for (i = 0; i < resolutions.Length; i++)
            {
                if (Screen.currentResolution.Equals(resolutions[i]))
                {
                    break;
                }
            }
            resolutionIndex = i;
            PlayerPrefs.SetInt(resolutionIndexKey, resolutionIndex);
        }
        resolutionDropdown.ClearOptions();
        options = resolutionDropdown.options;
        foreach (Resolution res in Screen.resolutions)
        {
            options.Add(new TMP_Dropdown.OptionData(res.width + "x" + res.height + " Hz:" + res.refreshRate));
        }
        resolutionDropdown.value = resolutionIndex;
        resolutionDropdown.RefreshShownValue();
        if (PlayerPrefs.HasKey(vSyncKey))
        {
            vSync = PlayerPrefs.GetInt(vSyncKey) == 1;
            QualitySettings.vSyncCount = Convert.ToInt32(vSync);
        }
        else
        {
            vSync = QualitySettings.vSyncCount == 1;
            PlayerPrefs.SetInt(vSyncKey, Convert.ToInt32(vSync));
        }
        vSyncToggle.isOn = vSync;
    }
    public void Apply()
    {
        fullScreen = fullScreenToggle.isOn;
        Screen.fullScreen = fullScreen;
        PlayerPrefs.SetInt(fullScreenKey, Convert.ToInt32(fullScreen));
        overallQuallity = overallQuallityDropdown.value;
        QualitySettings.SetQualityLevel(overallQuallity);
        PlayerPrefs.SetInt(overallQuallityKey, overallQuallity);
        antialiasing = antialiasingDropdown.value;
        QualitySettings.antiAliasing = antialiasing;
        PlayerPrefs.SetInt(antialiasingKey, antialiasing);
        resolutionIndex = resolutionDropdown.value;
        Resolution res = Screen.resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, fullScreen, res.refreshRate);
        PlayerPrefs.SetInt(resolutionIndexKey, resolutionIndex);
        vSync = vSyncToggle.isOn;
        QualitySettings.vSyncCount = Convert.ToInt32(vSync);
        PlayerPrefs.SetInt(vSyncKey, Convert.ToInt32(vSync));
        gameObject.SetActive(false);
        menuRef.SetActive(true);
    }
    public void Cancel()
    {
        gameObject.SetActive(false);
        menuRef.SetActive(true);
    }
}
