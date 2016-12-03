#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Base Class, will be a UI Element
[MoonSharp.Interpreter.MoonSharpUserData]
public abstract class BaseSettingsElement
{
    public SettingsOption option;

    /// <summary>
    /// Save to settings your value.
    /// </summary>
    public abstract void SaveElement();

    /// <summary>
    /// Set ranges, set fields...
    /// Pass it back basically.
    /// </summary>
    public abstract GameObject InitializeElement();

    /// <summary>
    /// Returns a base element, with a horizontal layout.
    /// </summary>
    /// <returns></returns>
    public GameObject GetHorizontalBaseElement(string elementTitle = "", bool expandHeight = true, bool expandWidth = true, TextAnchor alignment = TextAnchor.MiddleCenter, int spacing = 10)
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + option.name : elementTitle);

        HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.childForceExpandHeight = expandHeight;
        layout.childForceExpandWidth = expandWidth;
        layout.childAlignment = alignment;
        layout.spacing = spacing;

        return go;
    }

    /// <summary>
    /// Returns a base element, with a grid layout.
    /// </summary>
    /// <returns></returns>
    public GameObject GetGridBaseElement(string elementTitle = "", int xSize = 200, int ySize = 100)
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + option.name : elementTitle);
        go.AddComponent<GridLayoutGroup>().cellSize = new Vector2(xSize, ySize);

        return go;
    }

    /// <summary>
    /// Returns a base element, with a vertical layout.
    /// </summary>
    /// <returns></returns>
    public GameObject GetVerticalBaseElement(string elementTitle = "", bool expandHeight = true, bool expandWidth = true, TextAnchor alignment = TextAnchor.MiddleCenter, int spacing = 10)
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + option.name : elementTitle);

        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = expandHeight;
        layout.childForceExpandWidth = expandWidth;
        layout.childAlignment = alignment;
        layout.spacing = 10;

        return go;
    }

    public Text CreateText(string withText)
    {
        Text text = Object.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsText")).GetComponent<Text>();
        text.text = withText;

        return text;
    }

    public Toggle CreateToggle()
    {
        return Object.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsToggle")).GetComponent<Toggle>();
    }

    public InputField CreateInputField(string withText)
    {
        InputField field = Object.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsField")).GetComponent<InputField>();
        field.text = withText;

        return field;
    }

    public Slider CreateSlider(float value, Vector2 range, bool wholeNumbers = true)
    {
        Slider slider = Object.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsSlider")).GetComponent<Slider>();

        slider.maxValue = range.y;
        slider.minValue = range.x;
        slider.value = value;
        slider.wholeNumbers = wholeNumbers;

        return slider;
    }

    public Dropdown CreateComboBox()
    {
        return Object.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsDropdown")).GetComponent<Dropdown>();
    }
}
