#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Our main options/settings menu class (will contain all options).
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    // For statics
    private static SettingsMenu instance;

    private Dictionary<string, Dictionary<string, BaseSettingsElement[]>> options = new Dictionary<string, Dictionary<string, BaseSettingsElement[]>>();

    [SerializeField]
    private GameObject elementRoot;
    [SerializeField]
    private GameObject categoryRoot;
    [SerializeField]
    private GameObject mainRoot;

    [SerializeField]
    private GameObject categoryPrefab;
    [SerializeField]
    private GameObject headingPrefab;

    [SerializeField]
    private Text categoryHeading;

    // For optimising saving
    private string currentCategory = string.Empty;

    public static void Open()
    {
        if (instance == null)
        {
            return;
        }

        GameController.Instance.IsModal = true;
        GameController.Instance.soundController.OnButtonSFX();

        if (instance.options.Count > 0)
        {
            DisplayCategory(instance.options.First().Key);
        }
        else
        {
            DisplayCategory("No Settings Loaded");
        }

        instance.mainRoot.SetActive(true);
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
            foreach (string headingName in instance.options[instance.currentCategory].Keys)
            {
                for (int i = 0; i < instance.options[instance.currentCategory][headingName].Length; i++)
                {
                    if (instance.options[instance.currentCategory][headingName][i] != null)
                    {
                        instance.options[instance.currentCategory][headingName][i].SaveElement();
                    }
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

        foreach (ColorButton button in instance.categoryRoot.GetComponentsInChildren<ColorButton>())
        {
            if (button.gameObject.name != category)
            {
                button.RevertColor();
            }
        }

        if (instance.options.ContainsKey(category) == false)
        {
            return;
        }

        if (instance.currentCategory != string.Empty && instance.options.ContainsKey(instance.currentCategory))
        {
            foreach (string headingName in instance.options[instance.currentCategory].Keys)
            {
                // Create heading prefab
                SettingsHeading heading = Instantiate(instance.headingPrefab).GetComponent<SettingsHeading>();
                heading.SetText(headingName);
                heading.transform.SetParent(instance.elementRoot.transform);

                for (int i = 0; i < instance.options[instance.currentCategory][headingName].Length; i++)
                {
                    if (instance.options[instance.currentCategory][headingName][i] != null)
                    {
                        heading.AddObjectToRoot(instance.options[instance.currentCategory][headingName][i].InitializeElement());
                    }
                }
            }
        }
    }

    // This singular function is enough evidence to convince someone who doesn't believe in climate change
    // That we should have optimisation here (most likely in the source of a 'changes' made tracker
    public void SaveAndApply()
    {
        foreach (string headingName in options[instance.currentCategory].Keys)
        {
            for (int i = 0; i < options[currentCategory][headingName].Length; i++)
            {
                if (options[currentCategory][headingName][i] != null)
                {
                    options[currentCategory][headingName][i].SaveElement();
                }
            }
        }

        Settings.SaveSettings();

        foreach (string categoryName in options.Keys)
        {
            foreach (string headingName in options[categoryName].Keys)
            {
                for (int i = 0; i < options[categoryName][headingName].Length; i++)
                {
                    if (options[categoryName][headingName][i] != null)
                    {
                        options[categoryName][headingName][i].ApplySave();
                    }
                }
            }
        }

        GameController.Instance.IsModal = false;
        GameController.Instance.soundController.OnButtonSFX();
        mainRoot.SetActive(false);
    }

    public void Cancel()
    {
        // Open a dialog box to double check
        DialogBoxPromptOrInfo check = (DialogBoxPromptOrInfo)WorldController.Instance.dialogBoxManager.ShowDialogBoxByName("Prompt or Info");
        check.SetPrompt("Are you sure you want to close the menu and cancel all settings?");
        check.SetButtons(new DialogBoxResult[] { DialogBoxResult.Yes, DialogBoxResult.No });
        check.Closed =
            () =>
            {
                switch (check.Result)
                {
                    case DialogBoxResult.Yes:
                        // Reload (which will redefault settings)
                        Settings.LoadSettings();
                        GameController.Instance.IsModal = false;
                        GameController.Instance.soundController.OnButtonSFX();
                        break;
                    case DialogBoxResult.No:
                        GameController.Instance.soundController.OnButtonSFX();
                        mainRoot.SetActive(true);
                        break;
                }
            };

        mainRoot.SetActive(false);
        check.ShowDialog();
    }

    public void Default()
    {
        // Reset current category
        foreach (string headingName in instance.options[instance.currentCategory].Keys)
        {
            for (int i = 0; i < instance.options[instance.currentCategory][headingName].Length; i++)
            {
                if (instance.options[instance.currentCategory][headingName][i] != null)
                {
                    Settings.SetSetting(options[instance.currentCategory][headingName][i].option.key, options[instance.currentCategory][headingName][i].option.defaultValue);
                }
            }
        }

        DisplayCategory(currentCategory);
    }

    /// <summary>
    /// Will be considerably slower (it does all the options).
    /// </summary>
    public void ResetAll()
    {
        foreach (string headingName in options.Keys)
        {
            BaseSettingsElement[] values = options[headingName].Values.SelectMany(x => x).ToArray();

            // Reset current category
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    Settings.SetSetting(values[i].option.key, values[i].option.defaultValue);
                }
            }
        }

        DisplayCategory(currentCategory);
    }

    // Initial State
    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    private void Start()
    {
        // This just makes sure that the localisation is done
        // It won't always work,
        // especially if slow internet so cross fingers lol, since there is no 'localization finished thing'
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();

        Debug.LogWarning("Started");

        LoadCategories();

        if (options.Count > 0)
        {
            DisplayCategory(options.First().Key);
        }
        else
        {
            DisplayCategory("No Settings Loaded");
        }

        yield return null;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Cancel();
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

        options = new Dictionary<string, Dictionary<string, BaseSettingsElement[]>>();

        Dictionary<string, Dictionary<string, SettingsOption[]>> categories = PrototypeManager.SettingsCategories.Values.ToArray().SelectMany(x => x.categories).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (string currentName in categories.Keys)
        {
            ColorButton button = Instantiate(categoryPrefab).GetComponent<ColorButton>();
            button.transform.SetParent(categoryRoot.transform);
            button.name = currentName;
            button.SetText(currentName);
            options.Add(currentName, new Dictionary<string, BaseSettingsElement[]>());

            // This is quite optimised (despite being a forloop on a dictionary), and is only done during start
            foreach (KeyValuePair<string, SettingsOption[]> keyValuePair in categories[currentName])
            {
                options[currentName].Add(keyValuePair.Key, new BaseSettingsElement[keyValuePair.Value.Length]);

                for (int i = 0; i < keyValuePair.Value.Length; i++)
                {
                    if (FunctionsManager.SettingsMenu.HasFunction("Get" + keyValuePair.Value[i].className))
                    {
                        options[currentName][keyValuePair.Key][i] = FunctionsManager.SettingsMenu.Call("Get" + keyValuePair.Value[i].className).ToObject<BaseSettingsElement>();
                        options[currentName][keyValuePair.Key][i].option = keyValuePair.Value[i];
                    }
                    else
                    {
                        Debug.LogWarning("Get" + keyValuePair.Value[i].className + "() Doesn't exist");
                    }
                }
            }
        }
    }
}
