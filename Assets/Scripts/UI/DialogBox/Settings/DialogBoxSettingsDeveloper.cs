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

public class DialogBoxSettingsDeveloper : DialogBox
{
    // FPS Option.
    public Dropdown performanceDropdown;

    public Toggle timeStampToggle;
    public Toggle developerConsoleToggle;
    public Toggle developerModeToggle;

    public Slider fontSize;
    public Slider scrollingSensitivity;

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
        if (WorldController.Instance != null)
        {
            WorldController.Instance.spawnInventoryController.SetUIVisibility(developerModeToggle.isOn);
        }

        // One to many but we want an applying feature;
        PerformanceHUDManager.DirtyUI();
    }

    /// <summary>
    /// Saves settings to Settings.xml via the Settings class.
    /// </summary>
    public void SaveSetting()
    {
        Settings.SetSetting("DialogBoxSettingsDevConsole_devConsoleToggle", developerConsoleToggle.isOn);
        Settings.SetSetting("DialogBoxSettingsDevConsole_developerModeToggle", developerModeToggle.isOn);
        Settings.SetSetting("DialogBoxSettingsDevConsole_timeStampToggle", timeStampToggle.isOn);

        Settings.SetSetting("DialogBoxSettingsDevConsole_performanceGroup", performanceDropdown.value);

        Settings.SetSetting("DialogBoxSettingsDevConsole_scrollSensitivity", scrollingSensitivity.value);
        Settings.SetSetting("DialogBoxSettingsDevConsole_consoleFontSize", (int)fontSize.value);

        Settings.SaveSettings();

        PerformanceHUDManager.DirtyUI();
        DeveloperConsole.DevConsole.DirtySettings();
    }

    public void OnEnable()
    {
        // Add our listeners.
        closeButton.onClick.AddListener(CloseDialog);
        saveButton.onClick.AddListener(OnSave);
        applyButton.onClick.AddListener(OnApply);

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
        fontSize.value = Settings.GetSetting("DialogBoxSettingsDevConsole_consoleFontSize", 15);
        fontSize.maxValue = 20;
        fontSize.minValue = 10;

        scrollingSensitivity.value = Settings.GetSetting("DialogBoxSettingsDevConsole_scrollSensitivity", 6);
        scrollingSensitivity.maxValue = 15;
        scrollingSensitivity.minValue = 5;

        developerConsoleToggle.isOn = Settings.GetSetting("DialogBoxSettingsDevConsole_devConsoleToggle", true);
        developerModeToggle.isOn = Settings.GetSetting("DialogBoxSettingsDevConsole_developerModeToggle", false);
        timeStampToggle.isOn = Settings.GetSetting("DialogBoxSettingsDevConsole_timeStampToggle", true);

        performanceDropdown.value = Settings.GetSetting("DialogBoxSettingsDevConsole_performanceGroup", 1);
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
}