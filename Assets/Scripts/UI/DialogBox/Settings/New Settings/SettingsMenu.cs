#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Our main options/settings menu class (will contain all options).
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    private Dictionary<string, BaseSettingsElement[]> options = new Dictionary<string, BaseSettingsElement[]>();

    [SerializeField]
    private GameObject elementRoot;
    [SerializeField]
    private GameObject categoryRoot;
    [SerializeField]
    private GameObject mainRoot;

    [SerializeField]
    private GameObject categoryPrefab;

    [SerializeField]
    private Text categoryHeading;

    // For optimising saving
    private string currentCategory = "";

    // For statics
    private static SettingsMenu instance;

    // Initial State
    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    private void Start()
    {
        LoadCategories();

        if (options.Count > 0)
        {
            DisplayCategory(options.First().Key);
        }
        else
        {
            DisplayCategory("No Settings Loaded");
        }
    }

    /// <summary>
    /// Load our categories.
    /// </summary>
    private void LoadCategories()
    {
        // Clear root
        foreach (Transform child in elementRoot.transform)
        {
            Destroy(child.gameObject);
        }

        // Clear root
        foreach (Transform child in categoryRoot.transform)
        {
            Destroy(child.gameObject);
        }

        options = new Dictionary<string, BaseSettingsElement[]>();

        Dictionary<string, SettingsOption[]> categories = PrototypeManager.SettingsCategories.Values.ToArray().SelectMany(x => x.category).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // This is quite optimised (despite being a forloop on a dictionary), and is only done during start
        foreach (KeyValuePair<string, SettingsOption[]> keyValuePair in categories)
        {
            options.Add(keyValuePair.Key, new BaseSettingsElement[keyValuePair.Value.Length]);

            Button button = (Instantiate(categoryPrefab)).GetComponent<Button>();
            button.GetComponentInChildren<CategoryButtonHandler>().Initialize(keyValuePair.Key);
            button.transform.SetParent(categoryRoot.transform);
            button.name = keyValuePair.Key + ": Button";

            for (int i = 0; i < keyValuePair.Value.Length; i++)
            {
                if (FunctionsManager.SettingsMenu.HasFunction("Get" + keyValuePair.Value[i].className))
                {
                    options[keyValuePair.Key][i] = FunctionsManager.SettingsMenu.Call("Get" + keyValuePair.Value[i].className).ToObject<BaseSettingsElement>();
                    options[keyValuePair.Key][i].option = keyValuePair.Value[i];
                }
                else
                {
                    Debug.LogWarning("Get" + keyValuePair.Value[i].className + "() Doesn't exist");
                }
            }
        }
    }

    public static void DisplayCategory(string category)
    {
        if (instance == null)
        {
            return;
        }

        // Optimisation for saving
        if (instance.currentCategory != string.Empty && instance.currentCategory != category && instance.options.ContainsKey(instance.currentCategory))
        {
            for (int i = 0; i < instance.options[instance.currentCategory].Length; i++)
            {
                if (instance.options[instance.currentCategory][i] != null)
                {
                    instance.options[instance.currentCategory][i].SaveElement();
                }
            }
        }

        instance.categoryHeading.text = category;
        instance.currentCategory = category;

        // Clear root
        foreach (Transform child in instance.elementRoot.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (CategoryButtonHandler button in instance.categoryRoot.GetComponentsInChildren<CategoryButtonHandler>())
        {
            if (button.gameObject.name == category + ": Button")
            {
                button.Clicked();
            }
            else
            {
                button.UnClick();
            }
        }

        if (instance.options.ContainsKey(category) == false)
        {
            return;
        }

        for (int i = 0; i < instance.options[category].Length; i++)
        {
            if (instance.options[category][i] != null)
            {
                instance.options[category][i].InitializeElement().transform.SetParent(instance.elementRoot.transform);
            }
        }
    }

    public void SaveAndApply()
    {
        for (int i = 0; i < options[currentCategory].Length; i++)
        {
            options[currentCategory][i].SaveElement();
        }

        Settings.SaveSettings();
    }

    public void Cancel()
    {
        // Reload (which will redefault settings)
        Settings.LoadSettings();

        mainRoot.SetActive(false);
    }

    public void Default()
    {
        // Reset current category
        for (int i = 0; i < options[currentCategory].Length; i++)
        {
            if (options[currentCategory][i] != null)
            {
                Settings.SetSetting(options[currentCategory][i].option.key, options[currentCategory][i].option.defaultValue);
            }
        }

        DisplayCategory(currentCategory);
    }

    /// <summary>
    /// Will be considerably slower (it does all the options).
    /// </summary>
    public void ResetAll()
    {
        BaseSettingsElement[] values = options.Values.SelectMany(x => x).ToArray();

        // Reset current category
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != null)
            {
                Settings.SetSetting(values[i].option.key, values[i].option.defaultValue);
            }
        }

        DisplayCategory(currentCategory);
    }
}
