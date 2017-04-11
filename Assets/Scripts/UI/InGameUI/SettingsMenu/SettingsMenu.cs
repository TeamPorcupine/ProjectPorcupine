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
using ProjectPorcupine.Localization;
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
    private List<BaseSettingsElement> changesTracker = new List<BaseSettingsElement>();

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

        RectTransform rectTransform = instance.mainRoot.GetComponent<RectTransform>();
        if (rectTransform.sizeDelta.x > Screen.width * 0.8f)
        {
            rectTransform.sizeDelta = new Vector2(Screen.width * 0.8f, rectTransform.sizeDelta.y);
        }

        if (rectTransform.sizeDelta.y > Screen.height * 0.8f)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Screen.height * 0.8f);
        }

        // Optimisation for saving
        if (instance.currentCategory != string.Empty && instance.currentCategory != category && instance.options.ContainsKey(instance.currentCategory))
        {
            foreach (string headingName in instance.options[instance.currentCategory].Keys)
            {
                for (int i = 0; i < instance.options[instance.currentCategory][headingName].Length; i++)
                {
                    BaseSettingsElement elementCopy = instance.options[instance.currentCategory][headingName][i];

                    if (elementCopy != null && elementCopy.valueChanged)
                    {
                        instance.changesTracker.Add(elementCopy);
                        instance.Apply();
                    }
                }
            }
        }

        instance.categoryHeading.text = LocalizationTable.GetLocalization(category);
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
            else
            {
                button.SelectColor();
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
                        BaseSettingsElement element = instance.options[instance.currentCategory][headingName][i];
                        heading.AddObjectToRoot(element.InitializeElement());
                        element.valueChanged = false;
                    }
                }
            }
        }
    }

    public void Apply()
    {
        if (options.ContainsKey(currentCategory))
        {
            changesTracker.AddRange(options[currentCategory].Values.SelectMany(x => x).Where(x => x != null && x.valueChanged));
        }

        for (int i = 0; i < changesTracker.Count; i++)
        {
            changesTracker[i].ApplySetting();
            changesTracker[i].ApplySettingLUA();
        }
    }

    public void Save()
    {
        Apply();

        Settings.SaveSettings();
        changesTracker.Clear();

        GameController.Instance.IsModal = false;
        GameController.Instance.soundController.OnButtonSFX();
        mainRoot.SetActive(false);
    }

    public void Cancel()
    {
        // Open a dialog box to double check
        DialogBoxPromptOrInfo check;

        if (WorldController.Instance != null)
        {
            check = WorldController.Instance.dialogBoxManager.dialogBoxPromptOrInfo;
        }
        else if (MainMenuController.Instance != null)
        {
            check = MainMenuController.Instance.dialogBoxManager.dialogBoxPromptOrInfo;
        }
        else
        {
            mainRoot.SetActive(false);
            return;
        }

        check.SetPrompt("confirm_settings_menu_close");
        check.SetButtons(new DialogBoxResult[] { DialogBoxResult.Yes, DialogBoxResult.No });
        check.Closed =
            () =>
            {
                switch (check.Result)
                {
                    case DialogBoxResult.Yes:
                        // CANCEL
                        if (options.ContainsKey(currentCategory))
                        {
                            changesTracker.AddRange(options[currentCategory].Values.SelectMany(x => x).Where(x => x != null && x.valueChanged));
                        }

                        Settings.LoadSettings();

                        for (int i = 0; i < changesTracker.Count; i++)
                        {
                            changesTracker[i].CancelSetting();
                            changesTracker[i].CancelSettingLUA();
                        }

                        GameController.Instance.IsModal = false;
                        GameController.Instance.soundController.OnButtonSFX();
                        mainRoot.SetActive(false);

                        break;
                    case DialogBoxResult.No:
                        GameController.Instance.soundController.OnButtonSFX();
                        break;
                }
            };

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

        changesTracker.Clear();

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

        changesTracker.Clear();

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
        // This just makes sure that the localization is done
        // It won't always work,
        // especially if slow internet so cross fingers lol, since there is no 'localization finished thing'
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();

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
        if (Input.GetKey(KeyCode.Escape) && mainRoot.activeInHierarchy == true)
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
            button.SetText(LocalizationTable.GetLocalization(currentName));
            options.Add(currentName, new Dictionary<string, BaseSettingsElement[]>());

            // This is quite optimised (despite being a forloop on a dictionary), and is only done during start
            foreach (KeyValuePair<string, SettingsOption[]> keyValuePair in categories[currentName])
            {
                options[currentName].Add(keyValuePair.Key, new BaseSettingsElement[keyValuePair.Value.Length]);

                for (int i = 0; i < keyValuePair.Value.Length; i++)
                {
                    if (FunctionsManager.SettingsMenu.HasFunction("Get" + keyValuePair.Value[i].className))
                    {
                        BaseSettingsElement element = FunctionsManager.SettingsMenu.Call("Get" + keyValuePair.Value[i].className).ToObject<BaseSettingsElement>();
                        element.option = keyValuePair.Value[i];
                        element.parameterData = keyValuePair.Value[i].options;
                        element.InitializeLUA();
                        options[currentName][keyValuePair.Key][i] = element;
                    }
                    else if (keyValuePair.Value[i].name != null)
                    {
                        Debug.LogWarning("Get" + keyValuePair.Value[i].className + "() Doesn't exist");
                    }
                }
            }
        }
    }
}
