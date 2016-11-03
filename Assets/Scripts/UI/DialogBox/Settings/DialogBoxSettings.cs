#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxSettings : DialogBox
{
    // FPS Option.
    public Dropdown performanceDropdown;

    public Toggle fullScreenToggle;

    public Toggle developerModeToggle;

    public Slider musicVolume;

    public Resolution[] resolutions;
    public Dropdown resolutionDropdown;

    public Dropdown languageDropdown;
    public Dropdown vsyncDropdown;
    public Dropdown qualityDropdown;

    public InputField autosaveInterval;
    public InputField autosaveFiles;

    public Button closeButton;
    public Button saveButton;
    public Button applyButton;

    public void OnSave()
    {
        if (WorldController.Instance != null)
        {
            WorldController.Instance.spawnInventoryController.SetUIVisibility(developerModeToggle.isOn);
        }

        OnApply();
        SaveSetting();

        CloseDialog();
    }

    public void OnApply()
    {
        LocalizationTable.SetLocalization(languageDropdown.value);

        if (WorldController.Instance != null)
        {
            WorldController.Instance.spawnInventoryController.SetUIVisibility(developerModeToggle.isOn);
        }

        // MasterTextureLimit should get 0 for High quality and higher values for lower qualities.
        // For example count is 3 (0:Low, 1:Med, 2:High).
        // For High: count - 1 - value  =  3 - 1 - 2  =  0  (therefore no limit = high quality).
        // For Med:  count - 1 - value  =  3 - 1 - 1  =  1  (therefore a slight limit = medium quality).
        // For Low:  count - 1 - value  =  3 - 1 - 0  =  1  (therefore more limit = low quality).
        QualitySettings.masterTextureLimit = qualityDropdown.options.Count - 1 - qualityDropdown.value;

        Screen.fullScreen = fullScreenToggle.isOn;

        ResolutionOption selectedOption = (ResolutionOption)resolutionDropdown.options[resolutionDropdown.value];
        Resolution resolution = selectedOption.Resolution;
        Screen.SetResolution(resolution.width, resolution.height, fullScreenToggle.isOn, resolution.refreshRate);

        if (WorldController.Instance != null)
        {
            WorldController.Instance.autosaveManager.SetAutosaveInterval(int.Parse(autosaveInterval.text));
        }

        // One to many but we want an applying feature;
        PerformanceHUDManager.DirtyUI();
    }

    /// <summary>
    /// Saves settings to Settings.xml via the Settings class.
    /// </summary>
    public void SaveSetting()
    {
        Settings.SetSetting("DialogBoxSettings_musicVolume", musicVolume.normalizedValue);

        Settings.SetSetting("DialogBoxSettings_fullScreenToggle", fullScreenToggle.isOn);
        Settings.SetSetting("DialogBoxSettings_developerModeToggle", developerModeToggle.isOn);

        Settings.SetSetting("DialogBoxSettings_performanceGroup", performanceDropdown.value);
        Settings.SetSetting("DialogBoxSettings_qualityDropdown", qualityDropdown.value);
        Settings.SetSetting("DialogBoxSettings_vSyncDropdown", vsyncDropdown.value);
        Settings.SetSetting("DialogBoxSettings_resolutionDropdown", resolutionDropdown.value);
        Settings.SetSetting("AutosaveInterval", int.Parse(autosaveInterval.text));
        Settings.SetSetting("AutosaveFiles", int.Parse(autosaveFiles.text));
        Settings.SaveSettings();

        PerformanceHUDManager.DirtyUI();
    }

    public void OnEnable()
    {
        // Get all avalible resolution for the display.
        resolutions = Screen.resolutions;

        // Add our listeners.
        closeButton.onClick.AddListener(CloseDialog);
        saveButton.onClick.AddListener(OnSave);
        applyButton.onClick.AddListener(OnApply);

        fullScreenToggle.isOn = Screen.fullScreen;

        CreateResolutionDropdown();
        CreatePerformanceHUDDropdown();

        // Load the setting.
        LoadSetting();
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            CloseDialog();
        }
    }

    /// <summary>
    /// Loads settings from Settings.xml via the Settings class.
    /// </summary>
    private void LoadSetting()
    {
        musicVolume.normalizedValue = Settings.GetSetting("DialogBoxSettings_musicVolume", 0.5f);

        fullScreenToggle.isOn = Settings.GetSetting("DialogBoxSettings_fullScreenToggle", true);
        developerModeToggle.isOn = Settings.GetSetting("DialogBoxSettings_developerModeToggle", false);

        performanceDropdown.value = Settings.GetSetting("DialogBoxSettings_performanceGroup", 1);
        qualityDropdown.value = Settings.GetSetting("DialogBoxSettings_qualityDropdown", 0);
        vsyncDropdown.value = Settings.GetSetting("DialogBoxSettings_vSyncDropdown", 0);
        resolutionDropdown.value = Settings.GetSetting("DialogBoxSettings_resolutionDropdown", 0);

        autosaveInterval.text = Settings.GetSetting("AutosaveInterval", 2).ToString();
        autosaveFiles.text = Settings.GetSetting("AutosaveFiles", 5).ToString();
    }

    /// <summary>
    /// Create the differents option for the resolution dropdown.
    /// </summary>
    private void CreateResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        options.Add(new ResolutionOption
        {
            text = string.Format(
                "{0} x {1} @ {2}",
                Screen.currentResolution.width,
                Screen.currentResolution.height,
                Screen.currentResolution.refreshRate),
            Resolution = Screen.currentResolution
        });

        foreach (Resolution resolution in Screen.resolutions)
        {
            options.Add(new ResolutionOption
            {
                text = string.Format(
                    "{0} x {1} @ {2}",
                    resolution.width,
                    resolution.height,
                    resolution.refreshRate),
                Resolution = resolution
            });
        }

        resolutionDropdown.AddOptions(options);
    }

    /// <summary>
    /// Create the differents option for the performance HUD dropdown.
    /// </summary>
    private void CreatePerformanceHUDDropdown()
    {
        performanceDropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach (PerformanceComponentGroup option in PerformanceComponentGroups.groups)
        {
            options.Add(new Dropdown.OptionData(option.groupName));
        }

        performanceDropdown.AddOptions(options);
    }

    private class ResolutionOption : Dropdown.OptionData
    {
        public Resolution Resolution { get; set; }
    }
}