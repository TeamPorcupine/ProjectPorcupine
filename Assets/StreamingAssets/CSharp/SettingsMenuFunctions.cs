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

    public static AutosaveIntervalNumberField GetAutosaveIntervalNumberField()
    {
        return new AutosaveIntervalNumberField();
    }

    public static AutosaveNumberField GetAutosaveNumberField()
    {
        return new AutosaveNumberField();
    }

    public static LocalizationComboBox GetLocalizationComboBox()
    {
        return new LocalizationComboBox();
    }

    public static SoundSlider GetSoundSlider()
    {
        return new SoundSlider();
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

    public static FullScreenToggle GetFullScreenToggle()
    {
        return new FullScreenToggle();
    }

    public static ResolutionComboBox GetResolutionComboBox()
    {
        return new ResolutionComboBox();
    }

    public static ScrollSensitivitySlider GetScrollSensitivitySlider()
    {
        return new ScrollSensitivitySlider();
    }

    public static TimeStampToggle GetTimeStampToggle()
    {
        return new TimeStampToggle();
    }

    public static DeveloperConsoleToggle GetDeveloperConsoleToggle()
    {
        return new DeveloperConsoleToggle();
    }

    public static FontSizeSlider GetFontSizeSlider()
    {
        return new FontSizeSlider();
    }

    public static PerformanceHUDComboBox GetPerformanceHUDComboBox()
    {
        return new PerformanceHUDComboBox();
    }

    public static DeveloperModeToggle GetDeveloperModeToggle()
    {
        return new DeveloperModeToggle();
    }
}

public class GenericSwitch : BaseSettingsElement
{
    protected bool isOn;
    protected Toggle toggleElement;

    public override GameObject InitializeElement()
    {
        GameObject element = GetHorizontalBaseElement("Switch", 120, 60, TextAnchor.MiddleLeft);

        Text text = CreateText(option.name + ": ", true);
        text.transform.SetParent(element.transform);

        toggleElement = CreateSwitch();
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
        rTransform.sizeDelta = new Vector2(60, 30);
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
        return Settings.GetSetting(option.key, option.defaultValue.ToLower() == "true" ? true : false);
    }
}

public class GenericCircleRadio : BaseSettingsElement
{
    protected bool isOn;
    protected Toggle toggleElement;

    public override GameObject InitializeElement()
    {
        GameObject element = GetHorizontalBaseElement("Circle-Radio", 120, 60, TextAnchor.MiddleLeft);

        Text text = CreateText(option.name + ": ", true);
        text.transform.SetParent(element.transform);

        toggleElement = CreateCircleRadio();
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
        rTransform.sizeDelta = new Vector2(40, 40);
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
        return Settings.GetSetting(option.key, option.defaultValue.ToLower() == "true" ? true : false);
    }
}

public class GenericSquareRadio : BaseSettingsElement
{
    protected bool isOn;
    protected Toggle toggleElement;

    public override GameObject InitializeElement()
    {
        GameObject element = GetHorizontalBaseElement("Square-Radio", 120, 60, TextAnchor.MiddleLeft);

        Text text = CreateText(option.name + ": ", true);
        text.transform.SetParent(element.transform);

        toggleElement = CreateSquareRadio();
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
        rTransform.sizeDelta = new Vector2(40, 40);
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
        return Settings.GetSetting(option.key, option.defaultValue.ToLower() == "true" ? true : false);
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
        GameObject element = GetHorizontalBaseElement("Toggle", 120, 60, TextAnchor.MiddleLeft);

        Text text = CreateText(option.name + ": ", true);
        text.transform.SetParent(element.transform);

        toggleElement = CreateToggle();
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
        rTransform.sizeDelta = new Vector2(40, 40);
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
        return Settings.GetSetting(option.key, option.defaultValue.ToLower() == "true" ? true : false);
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

        CreateText(option.name + ": ", true).transform.SetParent(element.transform);

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

        return element;
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
        return Settings.GetSetting(option.key, option.defaultValue);
    }
}

public class GenericSlider : BaseSettingsElement
{
    protected Slider sliderElement;
    protected Text textElement;
    protected string format;
    protected float value;

    public override GameObject InitializeElement()
    {
        // Note this is just from playing around and finding a nice value
        GameObject element = GetVerticalBaseElement("Slider", 200, 30, TextAnchor.MiddleLeft, 0);

        format = option.name + " ({0:00}): ";

        textElement = CreateText(string.Format(format, getValue()), true, TextAnchor.MiddleCenter);
        textElement.transform.SetParent(element.transform);

        sliderElement = CreateSlider(getValue(), new Vector2(0, 1), false);
        sliderElement.transform.SetParent(element.transform);
        sliderElement.onValueChanged.AddListener(
            (float v) =>
            {
                if (v != value)
                {
                    valueChanged = true;
                    textElement.text = string.Format(format, v);
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

        return Settings.GetSetting(option.key, v);
    }
}

// This class is just to help you create your own dropdown class
private class GenericComboBox : BaseSettingsElement
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

        CreateText(option.name + ": ", true, TextAnchor.MiddleCenter).transform.SetParent(element.transform);

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

        return Settings.GetSetting(option.key, v);
    }
}

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
        LocalizationTable.SetLocalization(getValue());
    }

    public override void ApplySetting()
    {
        Settings.SetSetting(option.key, LocalizationTable.GetLanguages()[selectedValue]);
        LocalizationTable.SetLocalization(selectedValue);
    }

    public int getValue()
    {
        // Tbh this never gets called (like legit never) or rather ever gets used
        // But this is here cause if you ever need to call it for some reason it can come as a number or a string...
        // So this handles both
        string lang = Settings.GetSetting<string>(option.key, option.defaultValue).Replace("en_US", "English (US)");
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
        ApplyQuality(count, getValue());
    }

    public override void ApplySetting()
    {
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
        Resolution resolution = ((ResolutionOption)dropdownElement.options[selectedValue]).Resolution;
        Screen.SetResolution(resolution.width, resolution.height, Settings.GetSetting("fullScreenToggle", true), resolution.refreshRate);
    }

    public override void ApplySetting()
    {
        Settings.SetSetting(option.key, selectedValue);

        Resolution resolution = selectedOption.Resolution;
        Screen.SetResolution(resolution.width, resolution.height, Settings.GetSetting("fullScreenToggle", true), resolution.refreshRate);
    }
}

public class SoundSlider : GenericSlider
{
    public override GameObject InitializeElement()
    {
        GameObject go = base.InitializeElement();

        // Set it from 0 - 100 (still reflective of 0-1, but shows from 0 - 100)

        // We want to apply our own listener
        sliderElement.onValueChanged.RemoveAllListeners();
        sliderElement.onValueChanged.AddListener(
            (float v) =>
            {
                if (v != value)
                {
                    valueChanged = true;
                    value = v;
                    textElement.text = string.Format(format, (int)(value * 100));
                }
            });
        sliderElement.onValueChanged.Invoke(sliderElement.value);
        textElement.text = string.Format(format, (int)(value * 100));

        return go;
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

public class ScrollSensitivitySlider : GenericSlider
{
    public override GameObject InitializeElement()
    {
        GameObject go = base.InitializeElement();

        sliderElement.maxValue = 15;
        sliderElement.minValue = 5;
        sliderElement.wholeNumbers = true;
        sliderElement.value = getValue();
        sliderElement.onValueChanged.Invoke(sliderElement.value);

        return go;
    }

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

public class FontSizeSlider : GenericSlider
{
    public override GameObject InitializeElement()
    {
        GameObject go = base.InitializeElement();

        sliderElement.maxValue = 20;
        sliderElement.minValue = 12;
        sliderElement.wholeNumbers = true;
        sliderElement.value = getValue();
        sliderElement.onValueChanged.Invoke(sliderElement.value);

        return go;
    }

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

/// <summary>
/// Will only accept numbers that are positive.
/// </summary>
public class AutosaveNumberField : GenericInputField
{
    public override GameObject InitializeElement()
    {
        GameObject go = base.InitializeElement();

        fieldElement.onValidateInput += ValidateInput;

        return go;
    }

    public char ValidateInput(string text, int charIndex, char addedChar)
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
}

public class AutosaveIntervalNumberField : AutosaveNumberField
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

// This seems to be a placeholder (so I'll just make it a place holder)
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

public class DeveloperConsoleToggle : GenericSwitch
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

public class DeveloperModeToggle : GenericSwitch
{
    public override void ApplySetting()
    {
        base.ApplySetting();
        if (WorldController.Instance != null)
        {
            WorldController.Instance.spawnInventoryController.SetUIVisibility(isOn);
        }
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        if (WorldController.Instance != null)
        {
            WorldController.Instance.spawnInventoryController.SetUIVisibility(isOn);
        }
    }
}

public class TimeStampToggle : GenericToggle
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

public class PerformanceHUDComboBox : GenericComboBox
{
    public override GameObject InitializeElement()
    {
        string[] groupNames = new string[PerformanceComponentGroups.groups.Length];

        for (int i = 0; i < PerformanceComponentGroups.groups.Length; i++)
        {
            groupNames[i] = PerformanceComponentGroups.groups[i].groupName;
        }

        GameObject go = DropdownHelperFromText(groupNames, getValue());

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
        PerformanceHUDManager.DirtyUI();
    }

    public override void CancelSetting()
    {
        base.CancelSetting();
        PerformanceHUDManager.DirtyUI();
    }
}