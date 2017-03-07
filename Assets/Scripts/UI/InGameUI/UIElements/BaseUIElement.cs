#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Linq;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

// Every specific UI element comes from this
public abstract class BaseUIElement
{
    /// <summary>
    /// Internal option data.
    /// </summary>
    public Parameter parameterData;

    /// <summary>
    /// Returns the name of this object for internal reasons.
    /// Mainly for error debugging and/or gameobject name.
    /// </summary>
    public abstract string GetName();

    /// <summary>
    /// Set ranges, set fields...
    /// Pass it back basically.
    /// </summary>
    public abstract GameObject InitializeElement();

    protected GameObject GetFluidHorizontalBaseElement(string elementTitle = "", bool stretchX = false, bool stretchY = false, TextAnchor alignment = TextAnchor.MiddleCenter, int spacing = 10, int allocatedHeight = 60, int allocatedWidth = 220)
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + GetName() : elementTitle);
        AllocateSpaceForGameObject(go, allocatedHeight, allocatedWidth);

        HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.childForceExpandHeight = stretchY;
        layout.childForceExpandWidth = stretchX;
        layout.childAlignment = alignment;
        layout.spacing = spacing;

        return go;
    }

    protected GameObject GetFluidVerticalBaseElement(string elementTitle = "", bool stretchX = false, bool stretchY = false, TextAnchor alignment = TextAnchor.MiddleCenter, int spacing = 10, int allocatedHeight = 60, int allocatedWidth = 220)
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + GetName() : elementTitle);
        AllocateSpaceForGameObject(go, allocatedHeight, allocatedWidth);

        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = stretchY;
        layout.childForceExpandWidth = stretchX;
        layout.childAlignment = alignment;
        layout.spacing = spacing;

        return go;
    }

    /// <summary>
    /// Returns a base element, with a grid layout.
    /// </summary>
    /// <returns></returns>
    protected GameObject GetGridBaseElement(string elementTitle = "", int xSize = 97, int ySize = 37, TextAnchor alignment = TextAnchor.MiddleCenter, int spacingX = 5, int spacingY = 5, int allocatedHeight = 60, int allocatedWidth = 220)
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + GetName() : elementTitle);
        AllocateSpaceForGameObject(go, allocatedHeight, allocatedWidth);

        GridLayoutGroup layout = go.AddComponent<GridLayoutGroup>();
        layout.childAlignment = alignment;
        layout.spacing = new Vector2(spacingX, spacingY);
        layout.cellSize = new Vector2(xSize, ySize);

        return go;
    }

    /// <summary>
    /// Returns a base element, with a horizontal layout.
    /// </summary>
    /// <returns></returns>
    protected GameObject GetHorizontalBaseElement(string elementTitle = "", int xSize = 95, int ySize = 80, TextAnchor alignment = TextAnchor.MiddleCenter, int spacing = 10, int allocatedHeight = 60, int allocatedWidth = 220)
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + GetName() : elementTitle);
        AllocateSpaceForGameObject(go, allocatedHeight, allocatedWidth);

        GridLayoutGroup layout = go.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        layout.constraintCount = 1;
        layout.childAlignment = alignment;
        layout.spacing = new Vector2(spacing, 0);
        layout.cellSize = new Vector2(xSize, ySize);

        return go;
    }

    /// <summary>
    /// Returns a base element, with a vertical layout.
    /// </summary>
    /// <returns></returns>
    protected GameObject GetVerticalBaseElement(string elementTitle = "", int xSize = 100, int ySize = 80, TextAnchor alignment = TextAnchor.MiddleCenter, int spacing = 10, int allocatedHeight = 60, int allocatedWidth = 220)
    {
        GameObject go = new GameObject(elementTitle == string.Empty ? "Element_" + GetName() : elementTitle);
        AllocateSpaceForGameObject(go, allocatedHeight, allocatedWidth);

        GridLayoutGroup layout = go.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 1;
        layout.childAlignment = alignment;
        layout.spacing = new Vector2(0, spacing);
        layout.cellSize = new Vector2(xSize, ySize);

        return go;
    }

    protected void AllocateSpaceForGameObject(GameObject toAllocate, int allocatedHeight = 60, int allocatedWidth = 220)
    {
        LayoutElement baseLayout = toAllocate.AddComponent<LayoutElement>();
        baseLayout.minWidth = allocatedWidth;
        baseLayout.minHeight = allocatedHeight;
    }

    protected Text CreateText(string withText, bool autoFit = false, TextAnchor alignment = TextAnchor.MiddleLeft)
    {
        Text text = GameObject.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsText")).GetComponent<Text>();
        text.text = LocalizationTable.GetLocalization(withText);
        text.alignment = alignment;

        if (autoFit == true)
        {
            text.gameObject.AddComponent<TextScaling>();
        }

        return text;
    }

    protected Toggle CreateToggle(string type)
    {
        return GameObject.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/Settings" + type)).GetComponent<Toggle>();
    }

    protected InputField CreateInputField(string withText)
    {
        InputField field = GameObject.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsField")).GetComponent<InputField>();
        field.text = withText;

        return field;
    }

    protected Slider CreateSlider(float value, Vector2 range, bool wholeNumbers = true)
    {
        Slider slider = GameObject.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsSlider")).GetComponent<Slider>();

        slider.maxValue = range.y;
        slider.minValue = range.x;
        slider.value = value;
        slider.wholeNumbers = wholeNumbers;

        return slider;
    }

    protected Dropdown CreateEmptyDropdown()
    {
        return GameObject.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsDropdown")).GetComponent<Dropdown>();
    }

    protected Dropdown CreateDropdownFromText(string[] textOptions, int value)
    {
        Dropdown dropdown = GameObject.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsDropdown")).GetComponent<Dropdown>();
        dropdown.AddOptions(textOptions.ToList());
        dropdown.value = value;

        return dropdown;
    }

    protected Dropdown CreateDropdownFromOptionData(Dropdown.OptionData[] optionDataOptions, int value)
    {
        Dropdown dropdown = GameObject.Instantiate(Resources.Load<GameObject>("UI/SettingsMenu/SettingsDropdown")).GetComponent<Dropdown>();
        dropdown.AddOptions(optionDataOptions.ToList());
        dropdown.value = value;

        return dropdown;
    }
}
