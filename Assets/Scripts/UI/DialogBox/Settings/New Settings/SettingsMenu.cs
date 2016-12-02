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
    // Maybe list??  May not need??  Could just use this to initialies then remove??
    private Dictionary<string, BaseSettingsElement[]> options = new Dictionary<string, BaseSettingsElement[]>();

    [SerializeField]
    private GameObject elementRoot;
    [SerializeField]
    private GameObject categoryRoot;
    [SerializeField]
    private GameObject categoryPrefab;

    private static SettingsMenu instance;

    // Initial State
    private void Awake()
    {
        instance = this;
    }

    public static void Show()
    {
        // Don't know the future use
    }

    // Use this for initialization
    private void Start()
    {
        LoadCategories();
    }

    public static void DisplayCategory(string category)
    {
        if (instance == null)
        {
            return;
        }

        // Clear root
        foreach (Transform child in instance.elementRoot.transform)
        {
            Destroy(child.gameObject);
        }

        Debug.LogWarning(category);

        for (int i = 0; i < instance.options[category].Length; i++)
        {
            Debug.LogWarning("I'm here'):");

            if (instance.options[category][i] != null)
            {
                instance.options[category][i].InitializeElement().transform.SetParent(instance.elementRoot.transform);
            }
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
}
