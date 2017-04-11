using System;
using System.Collections;
using DeveloperConsole;
using ProjectPorcupine.Localization;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// This class is what the settings menu will call to retrieve the classes.
/// </summary>
public static class SettingsMenuFunctions
{
    public static GenericToggle GetGenericToggle()
    {
        return new GenericToggle();
    }

    public static GenericInputField GetGenericInputField()
    {
        return new GenericInputField();
    }

    public static GenericSlider GetGenericSlider()
    {
        return new GenericSlider();
    }

    public static AutosaveIntervalInputField GetAutosaveIntervalInputField()
    {
        return new AutosaveIntervalInputField();
    }

    public static LocalizationComboBox GetLocalizationComboBox()
    {
        return new LocalizationComboBox();
    }

    public static UISkinComboBox GetUISkinComboBox()
    {
        return new UISkinComboBox();
    }

    public static QualityComboBox GetQualityComboBox()
    {
        return new QualityComboBox();
    }

    public static VSyncComboBox GetVSyncComboBox()
    {
        return new VSyncComboBox();
    }

    public static ResolutionComboBox GetResolutionComboBox()
    {
        return new ResolutionComboBox();
    }

    public static PerformanceHUDComboBox GetPerformanceHUDComboBox()
    {
        return new PerformanceHUDComboBox();
    }

    public static AnisotropicFilteringComboBox GetAnisotropicFilteringComboBox()
    {
        return new AnisotropicFilteringComboBox();
    }

    public static ShadowComboBox GetShadowComboBox()
    {
        return new ShadowComboBox();
    }

    public static AAComboBox GetAAComboBox()
    {
        return new AAComboBox();
    }

    public static SoundDeviceComboBox GetSoundDeviceComboBox()
    {
        return new SoundDeviceComboBox();
    }

    public static DeveloperConsoleSlider GetDeveloperConsoleSlider()
    {
        return new DeveloperConsoleSlider();
    }

    public static UIScaleSlider GetUIScaleSlider()
    {
        return new UIScaleSlider();
    }

    public static SoundSlider GetSoundSlider()
    {
        return new SoundSlider();
    }

    public static DeveloperConsoleToggle GetDeveloperConsoleToggle()
    {
        return new DeveloperConsoleToggle();
    }

    public static FullScreenToggle GetFullScreenToggle()
    {
        return new FullScreenToggle();
    }

    public static DeveloperModeToggle GetDeveloperModeToggle()
    {
        return new DeveloperModeToggle();
    }

    public static SoftParticlesToggle GetSoftParticlesToggle()
    {
        return new SoftParticlesToggle();
    }
}

/// <summary>
/// A generic toggle.
/// </summary>
public class GenericToggle : BaseSettingsElement
{
    protected bool isOn;
    protected Toggle toggleElement;

    public override GameObject InitializeElement()
    {
        string type = this.parameterData.ContainsKey("Type") ? this.parameterData["Type"].ToString() : "Toggle";
        GameObject element = GetHorizontalBaseElement(type, 120, 60, TextAnchor.MiddleLeft);

        Text text = CreateText(option.name, true);
        text.transform.SetParent(element.transform);

        toggleElement = CreateToggle(type);
        toggleElement.transform.SetParent(element.transform);

        isOn = getValue();
        toggleElement.isOn = isOn;

        toggleElement.onValueChanged.AddListener(
            (bool v) =>
            {
                if (v != isOn)
                {
                    valueChanged = true;
                    isOn = v;
                }
            });

        LayoutElement layout = toggleElement.gameObject.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        RectTransform rTransform = toggleElement.GetComponent<RectTransform>();
        rTransform.sizeDelta = type == "Switch" ? new Vector2(60, 30) : new Vector2(40, 40);
        rTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rTransform.localPosition = new Vector3(45, 0, 0);

        return element;
    }

    public override void CancelSetting()
    {
    }

    public override void ApplySetting()
    {
        Settings.SetSetting(option.key, isOn);
    }

    public bool getValue()
    {
        bool temp;
        return Settings.GetSetting(option.key, out temp) ? temp : option.defaultValue.ToLower() == "true" ? true : false;
    }
}

/// <summary>
/// A generic input field.
/// </summary>
public class GenericInputField : BaseSettingsElement
{
    protected InputField fieldElement;
    protected string value;

    public override GameObject InitializeElement()
    {
        GameObject element = GetFluidHorizontalBaseElement("InputField", true, false, TextAnchor.MiddleLeft, 10);

        CreateText(option.name, true).transform.SetParent(element.transform);

        value = getValue();

        fieldElement = CreateInputField(value);
        fieldElement.transform.SetParent(element.transform);
        fieldElement.textComponent.alignment = TextAnchor.MiddleCenter;
        fieldElement.onValueChanged.AddListener(
            (string v) =>
            {
                if (v != value)
                {
                    valueChanged = true;
                    value = v;
                }
            });

        LayoutElement layout = fieldElement.gameObject.AddComponent<LayoutElement>();
        layout.minWidth = 60;
        layout.minHeight = 30;

        string verification = this.parameterData.ContainsKey("Verification") ? this.parameterData["Verification"].ToString() : "Standard";

        switch (verification)
        {
            case "PositiveInteger":
                fieldElement.onValidateInput += ValidateInputForPositiveNumber;
                break;
            case "Integer":
                fieldElement.contentType = InputField.ContentType.IntegerNumber;
                break;
            case "Alphanumeric":
                fieldElement.contentType = InputField.ContentType.Alphanumeric;
                break;
            case "Decimal":
                fieldElement.contentType = InputField.ContentType.DecimalNumber;
                break;
            case "Pin":
                fieldElement.contentType = InputField.ContentType.Pin;
                break;
            case "Password":
                fieldElement.contentType = InputField.ContentType.Password;
                break;
            case "Name":
                fieldElement.contentType = InputField.ContentType.Name;
                break;
            case "EmailAddress":
                fieldElement.contentType = InputField.ContentType.EmailAddress;
                break;
            case "Autocorrected":
                fieldElement.contentType = InputField.ContentType.Autocorrected;
                break;
        }

        return element;
    }

    public char ValidateInputForPositiveNumber(string text, int charIndex, char addedChar)
    {
        char output = addedChar;

        if (addedChar != '1'
            && addedChar != '2'
            && addedChar != '3'
            && addedChar != '4'
            && addedChar != '5'
            && addedChar != '6'
            && addedChar != '7'
            && addedChar != '8'
            && addedChar != '9'
            && addedChar != '0')
        {
            //return a null character
            output = '\0';
        }

        return output;
    }

    public override void CancelSetting()
    {
    }

    public override void ApplySetting()
    {
        Settings.SetSetting(option.key, value);
    }

    public string getValue()
    {
        string temp;
        return Settings.GetSetting(option.key, out temp) ? temp : option.defaultValue;
    }
}

public class GenericSlider : BaseSettingsElement
{
    protected Slider sliderElement;
    protected Text textElement;
    protected string format = "({0:0.##}) ";
    protected float value;

    public override GameObject InitializeElement()
    {
        // Note this is just from playing around and finding a nice value
        GameObject element = GetVerticalBaseElement("Slider", 200, 20, TextAnchor.MiddleLeft, 0, 40);

        textElement = CreateText(string.Format(format, getValue()) + LocalizationTable.GetLocalization(option.name), true, TextAnchor.MiddleCenter);
        textElement.transform.SetParent(element.transform);

        float minValue = this.parameterData.ContainsKey("MinimumValue") ? this.parameterData["MinimumValue"].ToFloat() : 0;
        float maxValue = this.parameterData.ContainsKey("MaximumValue") ? this.parameterData["MaximumValue"].ToFloat() : 1;
        bool wholeNumbers = this.parameterData.ContainsKey("WholeNumbers") ? this.parameterData["WholeNumbers"].ToBool() : false;

        sliderElement = CreateSlider(getValue(), new Vector2(minValue, maxValue), wholeNumbers);
        sliderElement.transform.SetParent(element.transform);
        sliderElement.onValueChanged.AddListener(
            (float v) =>
            {
                if (v != value)
                {
                    valueChanged = true;
                    textElement.text = string.Format(format, v) + LocalizationTable.GetLocalization(option.name);
                    value = v;
                }
            });
        sliderElement.onValueChanged.Invoke(sliderElement.value);

        return element;
    }

    public override void CancelSetting()
    {
    }

    public override void ApplySetting()
    {
        Settings.SetSetting(option.key, value);
    }

    public float getValue()
    {
        float v = 0;
        float.TryParse(option.defaultValue, out v);

        float temp;

        return Settings.GetSetting(option.key, out temp) ? temp : v;
    }
}

// This class is just to help you create your own dropdown class
public class GenericComboBox : BaseSettingsElement
{
    protected Dropdown dropdownElement;
    protected int selectedValue;

    public GameObject DropdownHelperFromText(string[] options, int value)
    {
        GameObject element = GetBasicElement();

        dropdownElement = CreateDropdownFromText(options, value);
        dropdownElement.transform.SetParent(element.transform);

        return element;
    }

    public GameObject DropdownHelperFromOptionData(Dropdown.OptionData[] options, int value)
    {
        GameObject element = GetBasicElement();

        dropdownElement = CreateDropdownFromOptionData(options, value);
        dropdownElement.transform.SetParent(element.transform);

        return element;
    }

    public GameObject DropdownHelperEmpty()
    {
        GameObject element = GetBasicElement();

        dropdownElement = CreateEmptyDropdown();
        dropdownElement.transform.SetParent(element.transform);

        return element;
    }

    public GameObject GetBasicElement()
    {
        GameObject element = GetVerticalBaseElement("Dropdown", 220, 30, TextAnchor.MiddleCenter, 0);

        CreateText(option.name, true, TextAnchor.MiddleCenter).transform.SetParent(element.transform);

        return element;
    }

    public override GameObject InitializeElement()
    {
        return GetBasicElement();
    }

    public override void CancelSetting()
    {
    }

    public override void ApplySetting()
    {
        Settings.SetSetting(option.key, selectedValue);
    }

    public int getValue()
    {
        int v = 0;
        int.TryParse(option.defaultValue, out v);
        int temp;

        return Settings.GetSetting(option.key, out temp) ? temp : v;
    }
}

#region CustomLogic

public class LocalizationComboBox : GenericComboBox
{
    public override GameObject InitializeElement()
    {
        GameObject go = DropdownHelperEmpty();
        dropdownElement.gameObject.AddComponent<LanguageDropdownUpdater>();
        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedValue = v;
                }
            });
        dropdownElement.value = getValue();

        return go;
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        if (LocalizationTable.currentLanguage != LocalizationTable.GetLanguages()[getValue()])
        {
            LocalizationTable.SetLocalization(getValue());
        }
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        if (LocalizationTable.currentLanguage != LocalizationTable.GetLanguages()[selectedValue])
        {
            Settings.SetSetting(option.key, LocalizationTable.GetLanguages()[selectedValue]);
            LocalizationTable.SetLocalization(selectedValue);
        }
    }

    public new int getValue()
    {
        // Tbh this never gets called (like legit never) or rather ever gets used
        // But this is here cause if you ever need to call it for some reason it can come as a number or a string...
        // So this handles both
        string lang;
        lang = (Settings.GetSetting<string>(option.key, out lang) ? lang : option.defaultValue).Replace("en_US", "English (US)");
        int value = -1;

        if (int.TryParse(lang, out value) == false)
        {
            value = dropdownElement.options.FindIndex(x => x.text == lang);
        }

        return (value >= 0) ? value : 0;
    }
}

public class QualityComboBox : GenericComboBox
{
    protected int count;

    public override GameObject InitializeElement()
    {
        GameObject go = DropdownHelperFromText(new string[] { "Low", "Med", "High" }, getValue());

        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedValue = v;
                }
            });

        count = dropdownElement.options.Count;

        return go;
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        ApplyQuality(count, getValue());
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        Settings.SetSetting(option.key, selectedValue);
        ApplyQuality(count, selectedValue);
    }

    public void ApplyQuality(int count, int value)
    {
        // Copied from DialogBoxSettings
        // MasterTextureLimit should get 0 for High quality and higher values for lower qualities.
        // For example count is 3 (0:Low, 1:Med, 2:High).
        // For High: count - 1 - value  =  3 - 1 - 2  =  0  (therefore no limit = high quality).
        // For Med:  count - 1 - value  =  3 - 1 - 1  =  1  (therefore a slight limit = medium quality).
        // For Low:  count - 1 - value  =  3 - 1 - 0  =  1  (therefore more limit = low quality).
        QualitySettings.masterTextureLimit = count - 1 - value;
    }
}

public class UISkinComboBox : GenericComboBox
{
    public override GameObject InitializeElement()
    {
        GameObject go = DropdownHelperFromText(new string[] { "Light" }, getValue());

        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedValue = v;
                }
            });

        return go;
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        // Apply Skin
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        // Undo Skin
    }
}

public class AnisotropicFilteringComboBox : GenericComboBox
{
    public override GameObject InitializeElement()
    {
        GameObject go = DropdownHelperFromText(new string[] { "Disable", "Enable", "Force Enable" }, getValue());

        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedValue = v;
                }
            });

        return go;
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        ApplyAFSeting(selectedValue);
    }

    public void ApplyAFSeting(int value)
    {
        switch (value)
        {
            case 0:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                break;
            case 1:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                break;
            case 2:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                break;
        }
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        ApplyAFSeting(getValue());
    }
}

public class AAComboBox : GenericComboBox
{
    public override GameObject InitializeElement()
    {
        GameObject go = DropdownHelperFromText(new string[] { "Disabled", "2x Multi-Sampling", "4x Multi-Sampling", "8x Multi-Sampling" }, getValue());

        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedValue = v;
                }
            });

        return go;
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        ApplyAA(selectedValue);
    }

    // I was going to do a power trick (2^x, with edge case 0), but C# reflection doesn't allow imported types (for some reason)
    // So this is a good comprimise
    public void ApplyAA(int value)
    {
        switch (value)
        {
            case 0:
                QualitySettings.antiAliasing = 0;
                break;
            case 1:
                QualitySettings.antiAliasing = 2;
                break;
            case 2:
                QualitySettings.antiAliasing = 4;
                break;
            case 3:
                QualitySettings.antiAliasing = 8;
                break;
        }
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        ApplyAA(getValue());
    }
}

public class ShadowComboBox : GenericComboBox
{
    public override GameObject InitializeElement()
    {
        GameObject go = DropdownHelperFromText(new string[] { "Very High", "High", "Medium", "Low", "Disabled" }, getValue());

        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedValue = v;
                }
            });

        return go;
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        ApplyShadowSetting(selectedValue);
    }

    // When update to 5.5+ add shadows = ShadowQuality (with 0, 1 being all, 2 and 3 being hardOnly, and 4 being disabled)
    // That's why there is repetition
    public void ApplyShadowSetting(int value)
    {
        switch (value)
        {
            case 0:
                QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                break;
            case 1:
                QualitySettings.shadowResolution = ShadowResolution.High;
                break;
            case 2:
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                break;
            case 3:
                QualitySettings.shadowResolution = ShadowResolution.Low;
                break;
            case 4:
                QualitySettings.shadowResolution = ShadowResolution.Low;
                break;
        }
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        ApplyShadowSetting(getValue());
    }
}

public class SoundDeviceComboBox : GenericComboBox
{
    private DriverDropdownOption selectedOption;

    private class DriverDropdownOption : Dropdown.OptionData
    {
        public string driverInfo { get; set; }
    }

    public override GameObject InitializeElement()
    {
        GameObject go = DropdownHelperFromOptionData(CreateDeviceDropdown(), WorldController.Instance.soundController.GetCurrentAudioDriver());

        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedOption = (DriverDropdownOption)dropdownElement.options[v];
                    selectedValue = v;
                }
            });

        return go;
    }

    private Dropdown.OptionData[] CreateDeviceDropdown()
    {
        Dropdown.OptionData[] options = new Dropdown.OptionData[WorldController.Instance.soundController.GetDriverCount()];

        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            DriverInfo info = WorldController.Instance.soundController.GetDriverInfo(i);

            options[i] = new DriverDropdownOption
            {
                text = info.name.ToString(),
                driverInfo = info.guid.ToString()
            };
        }

        return options;
    }

    public override void ApplySetting()
    {
        base.ApplySetting();

        if (selectedOption != null)
        {
            WorldController.Instance.soundController.SetAudioDriver(selectedOption.driverInfo);
        }
    }

    public override void CancelSetting()
    {
        base.CancelSetting();

        if (selectedOption != null)
        {
            WorldController.Instance.soundController.SetAudioDriver(getValue());
        }
    }

    public new string getValue()
    {
        string GUID;

        if (Settings.GetSetting(option.key, out GUID))
        {
            return GUID;
        }
        else
        {
            return WorldController.Instance.soundController.GetCurrentAudioDriverInfo().guid.ToString();
        }
    }
}

public class VSyncComboBox : GenericComboBox
{
    public override GameObject InitializeElement()
    {
        GameObject go = DropdownHelperFromText(new string[] { "Disabled", "Every frame", "Every second frame" }, getValue());

        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedValue = v;
                }
            });

        return go;
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        QualitySettings.vSyncCount = selectedValue;
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        QualitySettings.vSyncCount = getValue();
    }
}

public class PerformanceHUDComboBox : GenericComboBox
{
    string[] groupNames;

    public override GameObject InitializeElement()
    {
        groupNames = PerformanceHUDManager.GetNames();

        int locationOfVariable = 0;
        string value = getValue();

        for (int i = 0; i < groupNames.Length; i++)
        {
            if (groupNames[i] == value)
            {
                locationOfVariable = i;
                break;
            }
        }

        GameObject go = DropdownHelperFromText(groupNames, locationOfVariable);

        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedValue = v;
                }
            });

        return go;
    }

    public string getValue()
    {
        string temp;

        return Settings.GetSetting(option.key, out temp) ? temp : option.defaultValue;
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        Settings.SetSetting(option.key, groupNames[selectedValue]);
        PerformanceHUDManager.DirtyUI();
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        PerformanceHUDManager.DirtyUI();
    }
}

public class ResolutionComboBox : GenericComboBox
{
    private ResolutionOption selectedOption;

    private class ResolutionOption : Dropdown.OptionData
    {
        public Resolution Resolution { get; set; }
    }

    public override GameObject InitializeElement()
    {
        selectedValue = getValue();
        GameObject go = DropdownHelperFromOptionData(CreateResolutionDropdown(), selectedValue);
        dropdownElement.onValueChanged.AddListener(
            (int v) =>
            {
                if (v != selectedValue)
                {
                    valueChanged = true;
                    selectedOption = (ResolutionOption)dropdownElement.options[v];
                    selectedValue = v;
                }
            });

        return go;
    }

    /// <summary>
    /// Create the differents option for the resolution dropdown.
    /// </summary>
    private Dropdown.OptionData[] CreateResolutionDropdown()
    {
        Dropdown.OptionData[] options = new Dropdown.OptionData[Screen.resolutions.Length + 1];
        options[0] = new ResolutionOption
        {
            text = string.Format(
                    "{0} x {1} @ {2}",
                    Screen.currentResolution.width,
                    Screen.currentResolution.height,
                    Screen.currentResolution.refreshRate),
            Resolution = Screen.currentResolution
        };

        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            options[i + 1] = new ResolutionOption
            {
                text = string.Format(
                        "{0} x {1} @ {2}",
                        Screen.resolutions[i].width,
                        Screen.resolutions[i].height,
                        Screen.resolutions[i].refreshRate),
                Resolution = Screen.resolutions[i]
            };
        }

        return options;
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        Resolution resolution = ((ResolutionOption)dropdownElement.options[getValue()]).Resolution;
        Screen.SetResolution(resolution.width, resolution.height, SettingsKeyHolder.Fullscreen, resolution.refreshRate);
    }

    public override void ApplySetting()
    {
        base.ApplySetting();

        Settings.SetSetting(option.key, selectedValue);

        Resolution resolution = selectedOption.Resolution;
        Screen.SetResolution(resolution.width, resolution.height, SettingsKeyHolder.Fullscreen, resolution.refreshRate);
    }
}

public class DeveloperConsoleSlider : GenericSlider
{
    public override void ApplySetting()
    {
        base.ApplySetting();
        DeveloperConsole.DevConsole.DirtySettings();
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        DeveloperConsole.DevConsole.DirtySettings();
    }
}

public class UIScaleSlider : GenericSlider
{
    public override void ApplySetting()
    {
        base.ApplySetting();
        sliderElement.GetComponentInParent<UIRescaler>().AdjustScale();
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        sliderElement.GetComponentInParent<UIRescaler>().AdjustScale();
    }
}

public class SoundSlider : GenericSlider
{
    public override GameObject InitializeElement()
    {
        GameObject go = base.InitializeElement();
        sliderElement.value = getValue() * 100.0f;
        return go;
    }

    public override void ApplySetting()
    {
        base.ApplySetting();
        if (this.parameterData.ContainsKey("SoundChannel"))
        {
            Settings.SetSetting(option.key, sliderElement.normalizedValue);
            WorldController.Instance.soundController.SetVolume(this.parameterData["SoundChannel"].ToString(), sliderElement.normalizedValue);
        }
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        if (this.parameterData.ContainsKey("SoundChannel"))
        {
            WorldController.Instance.soundController.SetVolume(this.parameterData["SoundChannel"].ToString(), getValue());
        }
    }
}

/// <summary>
/// Custom Logic For Autosave
/// </summary>
public class AutosaveIntervalInputField : GenericInputField
{
    public override void ApplySetting()
    {
        base.ApplySetting();
        if (WorldController.Instance != null)
        {
            WorldController.Instance.autosaveManager.SetAutosaveInterval(int.Parse(value));
        }
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        if (WorldController.Instance != null)
        {
            WorldController.Instance.autosaveManager.SetAutosaveInterval(int.Parse(getValue()));
        }
    }
}

public class FullScreenToggle : GenericToggle
{
    public override void ApplySetting()
    {
        base.ApplySetting();
        Screen.fullScreen = isOn;
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        Screen.fullScreen = getValue();
    }
}

// Enable at 5.5+, since the option doesn't exist earlier
public class SoftParticlesToggle : GenericToggle
{
    public override void ApplySetting()
    {
        base.ApplySetting();
        // QualitySettings.softParticles = isOn;
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        // QualitySettings.softParticles = getValue();
    }
}

public class DeveloperConsoleToggle : GenericToggle
{
    public override void ApplySetting()
    {
        base.ApplySetting();
        DeveloperConsole.DevConsole.DirtySettings();
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        DeveloperConsole.DevConsole.DirtySettings();
    }
}

#endregion