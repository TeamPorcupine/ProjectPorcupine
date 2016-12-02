using System.Collections;
using DeveloperConsole;
using UnityEngine.UI;
using UnityEngine;

public class GeneralToggle : BaseSettingsElement
{
    public Toggle toggleElement;

    public override GameObject InitializeElement()
    {
        GameObject element = GetBaseElement();

        CreateText(option.name).transform.SetParent(element.transform);
        toggleElement = CreateToggle();

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
        return Settings.GetSetting(option.key, true);
    }
}

public class AutosaveNumberField : BaseSettingsElement
{
    public InputField fieldElement;

    public override GameObject InitializeElement()
    {
        GameObject element = GetBaseElement();

        GameObject go = new GameObject("FieldElement_" + option.name + ": Text");
        go.transform.SetParent(element.transform);
        go.AddComponent<Text>().text = option.name + ": ";

        fieldElement = new GameObject("FieldElement_" + option.name + ": Field").AddComponent<InputField>();
        fieldElement.transform.SetParent(element.transform);
        fieldElement.text = getValue();

        fieldElement.onValidateInput += ValidateInput;

        return element;
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

    public override void SaveElement()
    {
        int value;
        int.TryParse(fieldElement.text, out value);

        Settings.SetSetting(option.key, value);
    }

    public string getValue()
    {
        return Settings.GetSetting(option.key, "5");
    }
}

public static class SettingsMenuFunctions
{
    public static AutosaveNumberField GetAutosaveNumberField()
    {
        return new AutosaveNumberField();
    }

    public static GeneralToggle GetGenericToggle()
    {
        return new GeneralToggle();
    }
}