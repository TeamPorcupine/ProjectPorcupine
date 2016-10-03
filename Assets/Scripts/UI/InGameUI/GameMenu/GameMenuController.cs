#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuController : MonoBehaviour
{
    private MenuLeft menuLeft;
    private UnityEngine.Object buttonPrefab;

    // Deactivates All Menus.
    public void DeactivateAll()
    {
        menuLeft.CloseMenu();
        WorldController.Instance.mouseController.ClearMouseMode(true);
    }

    // Toggles whether menu is active.
    public void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
    }

    // Use this for initialization.
    private void Start()
    {
        menuLeft = GameObject.Find("MenuLeft").GetComponent<MenuLeft>();
        buttonPrefab = Resources.Load("UI/MenuButton");

        CreateGameMenu();

        GameMenuManager.Instance.Added += CreateGameMenu;
    }

    private void CreateGameMenu()
    {
        // Clear out all the children of our file list
        while (this.gameObject.transform.childCount > 0)
        {
            Transform child = this.gameObject.transform.GetChild(0);
            child.SetParent(null);  // Become Batman
            Destroy(child.gameObject);
        }

        foreach (GameMenuItem mainMenuItem in GameMenuManager.Instance)
        {
            CreateButton(mainMenuItem);
        }

        DeactivateAll();
    }

    private void CreateButton(GameMenuItem mainMenuItem)
    {
        GameObject gameObject = (GameObject)Instantiate(buttonPrefab, this.gameObject.transform);
        gameObject.name = "Button - " + mainMenuItem.Key;
        gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(mainMenuItem.Key) };

        Button button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(delegate
            {
                if (!WorldController.Instance.IsModal)
                {
                    DeactivateAll();
                    mainMenuItem.Trigger();
                }
            });

        Action localizationFilesChangedHandler = null;
        localizationFilesChangedHandler = delegate
            {
                Transform tf;
                try
                {
                    tf = gameObject.transform;
                }
                catch (MissingReferenceException)
                {
                    // this sometimes gets called when gameObject doesn't exist
                    // if so the gameObject has obviously been destroyed, so deregister
                    // the callback
                    LocalizationTable.CBLocalizationFilesChanged -= localizationFilesChangedHandler;
                    return;
                }

                string menuItemKey = gameObject.name.Replace("Button - ", string.Empty);
                tf.GetComponentInChildren<TextLocalizer>().formatValues = new string[]
                {
                    LocalizationTable.GetLocalization(menuItemKey)
                };
            };
        LocalizationTable.CBLocalizationFilesChanged += localizationFilesChangedHandler;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            DeactivateAll();
        }
    }
}
