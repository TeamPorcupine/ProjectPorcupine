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
        GameObject element = GetHorizontalBaseElement("Toggle", 80, 40, TextAnchor.MiddleLeft);

        Text text = CreateText(option.name + ": ", true);
        text.transform.SetParent(element.transform);

        toggleElement = CreateToggle();
        toggleElement.transform.SetParent(element.transform);
        toggleElement.isOn = getValue();

        LayoutElement layout = toggleElement.gameObject.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;

        toggleElement.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
        toggleElement.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        toggleElement.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        toggleElement.GetComponent<RectTransform>().localPosition = new Vector3(140, 0, 0);

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
        GameObject element = GetFluidHorizontalBaseElement("InputField", true, false, TextAnchor.MiddleLeft, 10);

        CreateText(option.name + ": ", true).transform.SetParent(element.transform);

        fieldElement = CreateInputField(getValue());
        fieldElement.transform.SetParent(element.transform);
        fieldElement.textComponent.alignment = TextAnchor.MiddleCenter;

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

    public override GameObject InitializeElement()
    {
        GameObject element = GetFluidHorizontalBaseElement("Slider", false, false, TextAnchor.MiddleLeft, 10);

        format = option.name + " ({0:00}): ";

        textElement = CreateText(string.Format(format, getValue(), false, 18 / GameObject.FindObjectOfType<Canvas>().scaleFactor));
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