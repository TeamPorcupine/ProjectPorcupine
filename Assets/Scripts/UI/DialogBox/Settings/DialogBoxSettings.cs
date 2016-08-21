using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    public GameSettings gameSettings;

    public Button closeButton;
    public Button saveButton;


    void OnEnable()
    {
        // Create an instance of this class.
        gameSettings = new GameSettings();

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
        gameSettings.isVisibleLangage = langToggle.isOn;
        langDropDown.SetActive(langToggle.isOn);
    }

    public void OnFPSToggle()
    {
        gameSettings.isVisibleFPS = fpsToggle.isOn;
        fpsObject.SetActive(fpsToggle.isOn);
    }

    public void OnFullScreenToggle()
    {
        gameSettings.isFullscreen = Screen.fullScreen = fullScreenToggle.isOn;
    }

    public void OnQualityChange()
    {
        gameSettings.qualityIndex = QualitySettings.masterTextureLimit = qualityDropdown.value;
    }

    public void OnVSyncChange()
    {
        gameSettings.vSyncIndex = QualitySettings.vSyncCount = vSyncDropdown.value;
    }

    public void OnResolutionChange()
    {
        gameSettings.resolutionIndex = resolutionDropdown.value;
        // Need to make work.
    }

    public void OnAliasingChange()
    {
        gameSettings.antiAliasingIndex = QualitySettings.antiAliasing = aliasingDropdown.value;
    }


    public void OnMusicChange()
    {

    }

    public void SaveSetting()
    {

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

    void LoadSetting()
    {
        aliasingDropdown.value = QualitySettings.antiAliasing;
        vSyncDropdown.value = QualitySettings.vSyncCount;
        qualityDropdown.value = QualitySettings.masterTextureLimit;

        fullScreenToggle.isOn = Screen.fullScreen;
        fpsToggle.isOn = fpsObject.activeSelf;
        langToggle.isOn = langDropDown.activeSelf;
    }

    void CreateResDropDown()
    {

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            this.CloseDialog();
        }
    }
}