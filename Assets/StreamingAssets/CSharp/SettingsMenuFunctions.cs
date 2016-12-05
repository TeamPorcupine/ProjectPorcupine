using System.Collections;
using System.Linq;
using DeveloperConsole;
using ProjectPorcupine.Localization;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// This class is what the settings menu will call to retrieve the classes.
/// </summary>
public static class SettingsMenuFunctions
{
    public static AutosaveNumberField GetAutosaveNumberField()
    {
        return new AutosaveNumberField();
    }

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

    public static FontSizeSlider GetFontSizeSlider()
    {
        return new FontSizeSlider();
    }

    public static ScrollSensitivitySlider GetScrollSensitivitySlider()
    {
        return new ScrollSensitivitySlider();
    }

    public static SoundSlider GetSoundSlider()
    {
        return new SoundSlider();
    }

    public static LocalizationComboBox GetLocalizationComboBox()
    {
        return new LocalizationComboBox();
    }

    public static QualityComboBox GetQualityComboBox()
    {
        return new QualityComboBox();
    }

    public static ResolutionComboBox GetResolutionComboBox()
    {
        return new ResolutionComboBox();
    }

    public static FullScreenToggle GetFullScreenToggle()
    {
        return new FullScreenToggle();
    }

    public static AutosaveIntervalNumberField GetAutosaveIntervalNumberField()
    {
        return new AutosaveIntervalNumberField();
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
        GameObject element = GetHorizontalBaseElement("Toggle", 200, 40, TextAnchor.MiddleLeft);

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
        rTransform.sizeDelta = new Vector2(30, 30);
        rTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rTransform.localPosition = new Vector3(110, 0, 0);

        return element;
    }

    public override void SaveElement()
    {
        Settings.SetSetting(option.key, toggleElement.isOn);
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

    public override void SaveElement()
    {
        Settings.SetSetting(option.key, fieldElement.text);
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
        GameObject element = GetHorizontalBaseElement("Slider", 175, 45, TextAnchor.MiddleLeft, 10);

        format = option.name + " ({0:00}): ";

        textElement = CreateText(string.Format(format, getValue(), false, 18 / GameObject.FindObjectOfType<Canvas>().scaleFactor));
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

        LayoutElement layout = sliderElement.gameObject.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        RectTransform rTransform = sliderElement.GetComponent<RectTransform>();
        rTransform.sizeDelta = new Vector2(140, 20);
        rTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rTransform.localPosition = new Vector3(110, 0, 0);

        return element;
    }

    public override void SaveElement()
    {
        Settings.SetSetting(option.key, sliderElement.value);
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
        // Note this is just from playing around and finding a nice value
        GameObject element = GetHorizontalBaseElement("Dropdown", 200, 40, TextAnchor.MiddleCenter, 0);

        dropdownElement = CreateDropdownFromText(options, value);
        dropdownElement.transform.SetParent(element.transform);

        return element;
    }

    public GameObject DropdownHelperFromOptionData(Dropdown.OptionData[] options, int value)
    {
        // Note this is just from playing around and finding a nice value
        GameObject element = GetHorizontalBaseElement("Dropdown", 200, 40, TextAnchor.MiddleCenter, 0);

        dropdownElement = CreateDropdownFromOptionData(options, value);
        dropdownElement.transform.SetParent(element.transform);

        return element;
    }

    public GameObject DropdownHelperEmpty()
    {
        // Note this is just from playing around and finding a nice value
        GameObject element = GetHorizontalBaseElement("Dropdown", 200, 40, TextAnchor.MiddleCenter, 0);

        dropdownElement = CreateEmptyDropdown();
        dropdownElement.transform.SetParent(element.transform);

        return element;
    }

    public override GameObject InitializeElement()
    {
        return GetHorizontalBaseElement("Dropdown", 200, 40, TextAnchor.MiddleCenter, 0);
    }

    public override void SaveElement()
    {
        Settings.SetSetting(option.key, dropdownElement.value);
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

    public override void ApplySave()
    {
        LocalizationTable.SetLocalization(selectedValue);
    }

    public override void SaveElement()
    {
        Settings.SetSetting(option.key, LocalizationTable.GetLanguages()[dropdownElement.value]);
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

    public override void ApplySave()
    {
        // Copied from DialogBoxSettings
        // MasterTextureLimit should get 0 for High quality and higher values for lower qualities.
        // For example count is 3 (0:Low, 1:Med, 2:High).
        // For High: count - 1 - value  =  3 - 1 - 2  =  0  (therefore no limit = high quality).
        // For Med:  count - 1 - value  =  3 - 1 - 1  =  1  (therefore a slight limit = medium quality).
        // For Low:  count - 1 - value  =  3 - 1 - 0  =  1  (therefore more limit = low quality).
        QualitySettings.masterTextureLimit = count - 1 - selectedValue;
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
        Dropdown.OptionData[] options = new Dropdown.OptionData[Screen.resolutions.Length];
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

    public override void ApplySave()
    {
        // Copied from DialogBoxSettings
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

        return go;
    }
}

public class FullScreenToggle : GenericToggle
{
    public override void ApplySave()
    {
        Screen.fullScreen = isOn;
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

        return go;
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

        return go;
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
    public override void ApplySave()
    {
        if (WorldController.Instance != null)
        {
            WorldController.Instance.autosaveManager.SetAutosaveInterval(int.Parse(fieldElement.text));
        }
    }
}