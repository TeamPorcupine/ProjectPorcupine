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
    public virtual GameObject InitializeElement()
    {
        return GetBaseElement();
    }

    /// <summary>
    /// Returns a base element, with a horizontal layout.
    /// </summary>
    /// <returns></returns>
    public GameObject GetBaseElement(string elementTitle = "")
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + option.name : elementTitle);
        go.AddComponent<HorizontalLayoutGroup>();

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

    public InputField CreateInputField()
    {
        return Object.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsField")).GetComponent<InputField>();
    }

    public Dropdown CreateComboBox()
    {
        return Object.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsDropdown")).GetComponent<Dropdown>();
    }
}
