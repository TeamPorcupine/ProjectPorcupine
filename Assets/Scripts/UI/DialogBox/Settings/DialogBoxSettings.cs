using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogBoxSettings : DialogBox
{

    // langage Option.
    public Toggle langToggle;
    public GameObject langDropDown;

    // FPS Option.
    public Toggle fpsToggle;
    public GameObject fpsObject;

    public Toggle fullScreenToggle;

    public Slider musicVolume;

    public Resolution[] myResolutions;
    public Dropdown resolutionDropdown;

    public Dropdown aliasingDropdown;
    public Dropdown vSyncDropdown;
    public Dropdown qualityDropdown;

    public Button closeButton;
    public Button saveButton;


    void OnEnable()
    {

        // Get all avalible resolution for the display.
        myResolutions = Screen.resolutions;

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
        vSyncDropdown.onValueChanged.AddListener(delegate
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
        /// TODO : impliment full screen toggle
    }

    public void OnQualityChange()
    {
        /// TODO : impliment Quality changes
    }

    public void OnVSyncChange()
    {
        /// TODO : impliment VSync changes
    }

    public void OnResolutionChange()
    {
        /// TODO : impliment Resolution changes
    }

    public void OnAliasingChange()
    {
        /// TODO : impliment AA changes
    }


    public void OnMusicChange()
    {
        /// TODO : impliment Music changes
    }

    public void OnClickClose()
    {
        this.CloseDialog();
    }

    public void OnClickSave()
    {
        this.CloseDialog();
        SaveSetting();
    }

    /// <summary>
    /// saves settings to Settings.xml via the Settings class
    /// </summary>
    public void SaveSetting()
    {
        Settings.setSetting("DialogBoxSettings_musicVolume", musicVolume.normalizedValue);

        Settings.setSetting("DialogBoxSettings_langToggle", langToggle.isOn);
        Settings.setSetting("DialogBoxSettings_fpsToggle", fpsToggle.isOn);
        Settings.setSetting("DialogBoxSettings_fullScreenToggle", fullScreenToggle.isOn);

        Settings.setSetting("DialogBoxSettings_qualityDropdown", qualityDropdown.value);
        Settings.setSetting("DialogBoxSettings_vSyncDropdown", vSyncDropdown.value);
        Settings.setSetting("DialogBoxSettings_resolutionDropdown", resolutionDropdown.value);
        Settings.setSetting("DialogBoxSettings_aliasingDropdown", aliasingDropdown.value);
    }

    /// <summary>
    /// Loads settings from Settings.xml via the Settings class
    /// </summary>
    void LoadSetting()
    {
        musicVolume.normalizedValue = Settings.getSettingAsFloat("DialogBoxSettings_musicVolume", 0.5f);

        langToggle.isOn = Settings.getSettingAsBool("DialogBoxSettings_langToggle", true);
        fpsToggle.isOn = Settings.getSettingAsBool("DialogBoxSettings_fpsToggle", true);
        fullScreenToggle.isOn = Settings.getSettingAsBool("DialogBoxSettings_fullScreenToggle", true);

        qualityDropdown.value = Settings.getSettingAsInt("DialogBoxSettings_qualityDropdown", 0);
        vSyncDropdown.value = Settings.getSettingAsInt("DialogBoxSettings_vSyncDropdown", 0);
        resolutionDropdown.value = Settings.getSettingAsInt("DialogBoxSettings_resolutionDropdown", 0);
        aliasingDropdown.value = Settings.getSettingAsInt("DialogBoxSettings_aliasingDropdown", 0);

    }

    void CreateResDropDown()
    {
        List<string> myResolutionStrings = new List<string>();
        foreach (Resolution r in myResolutions)
        {
            myResolutionStrings.Add(r.ToString());
        }
        resolutionDropdown.AddOptions(myResolutionStrings);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            this.CloseDialog();
        }
    }
}