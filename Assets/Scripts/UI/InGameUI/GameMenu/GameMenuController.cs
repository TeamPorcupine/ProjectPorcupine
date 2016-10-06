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
        DeactivateAll();

        GameMenuManager.Instance.Added += OnMenuItemAdded;
    }

    private void CreateGameMenu()
    {
        foreach (GameMenuItem gameMenuItem in GameMenuManager.Instance)
        {
            CreateButton(gameMenuItem);
        }
    }

    private void OnMenuItemAdded(GameMenuItem gameMenuItem, int position)
    {
        GameObject gameObject = CreateButton(gameMenuItem);
        gameObject.transform.SetSiblingIndex(position);
    }

    private GameObject CreateButton(GameMenuItem gameMenuItem)
    {
        GameObject gameObject = (GameObject)Instantiate(buttonPrefab, this.gameObject.transform);
        gameObject.name = "Button - " + gameMenuItem.Key;
        gameObject.transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[] { LocalizationTable.GetLocalization(gameMenuItem.Key) };

        Button button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(delegate
            {
                if (!WorldController.Instance.IsModal)
                {
                    DeactivateAll();
                    gameMenuItem.Trigger();
                }
            });

        Action localizationFilesChangedHandler = null;
        localizationFilesChangedHandler = delegate
            {
                Transform transform;
                try
                {
                    transform = gameObject.transform;
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
                transform.GetComponentInChildren<TextLocalizer>().formatValues = new string[]
                {
                    LocalizationTable.GetLocalization(menuItemKey)
                };
            };
        LocalizationTable.CBLocalizationFilesChanged += localizationFilesChangedHandler;

        return gameObject;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            DeactivateAll();
        }
    }
}
