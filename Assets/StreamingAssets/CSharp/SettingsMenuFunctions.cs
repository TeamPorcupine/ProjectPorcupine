using System.Collections;
using DeveloperConsole;
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
}

/// <summary>
/// A generic toggle.
/// </summary>
public class GenericToggle : BaseSettingsElement
{
    protected Toggle toggleElement;

    public override GameObject InitializeElement()
    {
        GameObject element = GetHorizontalBaseElement("Toggle", false, false, TextAnchor.MiddleCenter, 10);

        CreateText(option.name + ": ").transform.SetParent(element.transform);
        toggleElement = CreateToggle();

        LayoutElement toggleLayout = toggleElement.gameObject.AddComponent<LayoutElement>();
        toggleLayout.minWidth = 40;
        toggleLayout.minHeight = 40;

        toggleElement.transform.SetParent(element.transform);

        toggleElement.isOn = getValue();
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

    public override GameObject InitializeElement()
    {
        GameObject element = GetHorizontalBaseElement("InputField", false, false, TextAnchor.MiddleCenter, 10);

        CreateText(option.name + ": ").transform.SetParent(element.transform);

        fieldElement = CreateInputField(getValue());
        fieldElement.transform.SetParent(element.transform);
        fieldElement.textComponent.alignment = TextAnchor.MiddleCenter;

        LayoutElement ourLayout = fieldElement.gameObject.AddComponent<LayoutElement>();
        ourLayout.minWidth = 80;
        ourLayout.minHeight = 40;

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

    public override GameObject InitializeElement()
    {
        GameObject element = GetHorizontalBaseElement("Slider", false, false, TextAnchor.MiddleCenter, 10);

        format = option.name + " ({0}): ";

        textElement = CreateText(string.Format(format, getValue()));
        textElement.transform.SetParent(element.transform);

        sliderElement = CreateSlider(getValue(), new Vector2(0, 1), false);
        sliderElement.transform.SetParent(element.transform);
        sliderElement.onValueChanged.AddListener((float value) => { textElement.text = string.Format(format, value); });

        LayoutElement ourLayout = sliderElement.gameObject.AddComponent<LayoutElement>();
        ourLayout.minWidth = 80;
        ourLayout.minHeight = 20;

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

public class SoundSlider : GenericSlider
{
    public override GameObject InitializeElement()
    {
        GameObject go = base.InitializeElement();

        // Set it from 0 - 100 (still reflective of 0-1, but shows from 0 - 100)
        sliderElement.onValueChanged.RemoveAllListeners();
        sliderElement.onValueChanged.AddListener((float value) => { textElement.text = string.Format(format, (int)(value * 100)); });
        sliderElement.onValueChanged.Invoke(sliderElement.value);

        return go;
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
        sliderElement.minValue = 10;
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