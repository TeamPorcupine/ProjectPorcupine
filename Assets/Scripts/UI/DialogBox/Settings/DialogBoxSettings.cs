#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxSettings : DialogBox
{
    // Language Option.
    public Toggle langToggle;
    public GameObject langDropDown;

    // FPS Option.
    public Toggle fpsToggle;
    public GameObject fpsObject;

    public Toggle fullScreenToggle;

    public Toggle developerModeToggle;

    public Slider musicVolume;

    public Resolution[] resolutions;
    public Dropdown resolutionDropdown;

    public Dropdown aliasingDropdown;
    public Dropdown vsyncDropdown;
    public Dropdown qualityDropdown;

    public Button closeButton;
    public Button saveButton;

    public void OnLangageToggle()
    {
        langDropDown.SetActive(langToggle.isOn);
    }

    public void OnFPSToggle()
    {
        fpsObject.SetActive(fpsToggle.isOn);
    }

    public void OnFullScreenToggle()
    {
        /// TODO : impliment full screen toggle.
    }

    public void OnQualityChange()
    {
        // MasterTextureLimit should get 0 for High quality and higher values for lower qualities.
        // For example count is 3 (0:Low, 1:Med, 2:High).
        // For High: count - 1 - value  =  3 - 1 - 2  =  0  (therefore no limit = high quality).
        // For Med:  count - 1 - value  =  3 - 1 - 1  =  1  (therefore a slight limit = medium quality).
        // For Low:  count - 1 - value  =  3 - 1 - 0  =  1  (therefore more limit = low quality).
        QualitySettings.masterTextureLimit = qualityDropdown.options.Count - 1 - qualityDropdown.value;
    }

    public void OnVSyncChange()
    {
        /// TODO : Implement VSync changes.
    }

    public void OnResolutionChange()
    {
        /// TODO : Implement Resolution changes.
    }

    public void OnAliasingChange()
    {
        /// TODO : Implement AA changes.
    }

    public void OnMusicChange()
    {
        /// TODO : Implement Music changes.
    }

    public void OnClickClose()
    {
        this.CloseDialog();
    }

    public void OnClickSave()
    {
        this.CloseDialog();
        WorldController.Instance.spawnInventoryController.SetUIVisibility(developerModeToggle.isOn);
        SaveSetting();
    }

    /// <summary>
    /// Saves settings to Settings.xml via the Settings class.
    /// </summary>
    public void SaveSetting()
    {
        Settings.SetSetting("DialogBoxSettings_musicVolume", musicVolume.normalizedValue);

        Settings.SetSetting("DialogBoxSettings_langToggle", langToggle.isOn);
        Settings.SetSetting("DialogBoxSettings_fpsToggle", fpsToggle.isOn);
        Settings.SetSetting("DialogBoxSettings_fullScreenToggle", fullScreenToggle.isOn);
        Settings.SetSetting("DialogBoxSettings_developerModeToggle", developerModeToggle.isOn);

        Settings.SetSetting("DialogBoxSettings_qualityDropdown", qualityDropdown.value);
        Settings.SetSetting("DialogBoxSettings_vSyncDropdown", vsyncDropdown.value);
        Settings.SetSetting("DialogBoxSettings_resolutionDropdown", resolutionDropdown.value);
        Settings.SetSetting("DialogBoxSettings_aliasingDropdown", aliasingDropdown.value);
    }

    private void OnEnable()
    {
        // Get all avalible resolution for the display.
        resolutions = Screen.resolutions;

        // Add our listeners.
        closeButton.onClick.AddListener(delegate
        {
            OnClickClose();
        });
        saveButton.onClick.AddListener(delegate
        {
            OnClickSave();
        });

        fpsToggle.onValueChanged.AddListener(delegate
        {
            OnFPSToggle();
        });
        langToggle.onValueChanged.AddListener(delegate
        {
            OnLangageToggle();
        });
        fullScreenToggle.onValueChanged.AddListener(delegate
        {
            OnFullScreenToggle();
        });
        resolutionDropdown.onValueChanged.AddListener(delegate
        {
            OnResolutionChange();
        });
        aliasingDropdown.onValueChanged.AddListener(delegate
        {
            OnAliasingChange();
        });
        vsyncDropdown.onValueChanged.AddListener(delegate
        {
            OnVSyncChange();
        });
        qualityDropdown.onValueChanged.AddListener(delegate
        {
            OnQualityChange();
        });

        musicVolume.onValueChanged.AddListener(delegate
        {
            OnMusicChange();
        });

        // Create the drop down for resolution.
        CreateResDropDown();

        // Load the setting.
        LoadSetting();
    }

    /// <summary>
    /// Loads settings from Settings.xml via the Settings class.
    /// </summary>
    private void LoadSetting()
    {
        musicVolume.normalizedValue = Settings.GetSettingAsFloat("DialogBoxSettings_musicVolume", 0.5f);

        langToggle.isOn = Settings.GetSettingAsBool("DialogBoxSettings_langToggle", true);
        fpsToggle.isOn = Settings.GetSettingAsBool("DialogBoxSettings_fpsToggle", true);
        fullScreenToggle.isOn = Settings.GetSettingAsBool("DialogBoxSettings_fullScreenToggle", true);
        developerModeToggle.isOn = Settings.GetSettingAsBool("DialogBoxSettings_developerModeToggle", false);

        qualityDropdown.value = Settings.GetSettingAsInt("DialogBoxSettings_qualityDropdown", 0);
        vsyncDropdown.value = Settings.GetSettingAsInt("DialogBoxSettings_vSyncDropdown", 0);
        resolutionDropdown.value = Settings.GetSettingAsInt("DialogBoxSettings_resolutionDropdown", 0);
        aliasingDropdown.value = Settings.GetSettingAsInt("DialogBoxSettings_aliasingDropdown", 0);
    }

    private void CreateResDropDown()
    {
        List<string> resolutionStrings = new List<string>();
        foreach (Resolution r in resolutions)
        {
            resolutionStrings.Add(r.ToString());
        }

        resolutionDropdown.AddOptions(resolutionStrings);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            this.CloseDialog();
        }
    }
}