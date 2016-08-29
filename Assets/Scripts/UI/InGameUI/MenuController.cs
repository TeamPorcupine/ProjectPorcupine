#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    // The sub menus of the build menu (furniture, floor..... later - power, security, drones).
    public GameObject furnitureMenu;
    public GameObject floorMenu;
    
    public Button buttonConstructor;
    public Button buttonWorld;
    public Button buttonWork;
    public Button buttonOptions;
    public Button buttonSettings;
    public Button buttonQuests;

    private DialogBoxManager dbm;

    // The left build menu.
    private GameObject constructorMenu;

    // Deactivates All Menus.
    public void DeactivateAll()
    {
        constructorMenu.SetActive(false);
        DeactivateSubs();
    }

    // Deactivates any sub menu of the constrution options.
    public void DeactivateSubs()
    {
        furnitureMenu.SetActive(false);
        floorMenu.SetActive(false);
    }

    // Toggles whether menu is active.
    public void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
    }

    public void OnButtonConstruction()
    {
        if (constructorMenu.activeSelf)
        {
            DeactivateAll();
        } 
        else 
        { 
            DeactivateAll();
            constructorMenu.SetActive(true);
        }
    }

    public void OnButtonWork()
    {
        DeactivateAll();
    }

    public void OnButtonWorld()
    {
        if (!WorldController.Instance.IsModal)
        {
            DeactivateAll();
        }
    }

    public void OnButtonQuests()
    {
        if (!WorldController.Instance.IsModal)
        {
            DeactivateAll();
            dbm.dialogBoxQuests.ShowDialog();
        }
    }

    public void OnButtonOptions()
    {
        if (!WorldController.Instance.IsModal)
        {
            DeactivateAll();
            dbm.dialogBoxOptions.ShowDialog();
        }
    }

    public void OnButtonSettings()
    {
        if (!WorldController.Instance.IsModal)
        {
            DeactivateAll();
            dbm.dialogBoxSettings.ShowDialog();
        }
    }

    // Use this for initialization.
    private void Start()
    {
        dbm = GameObject.Find("Dialog Boxes").GetComponent<DialogBoxManager>();

        furnitureMenu = GameObject.Find("MenuFurniture");
        floorMenu = GameObject.Find("MenuFloor");
        constructorMenu = GameObject.Find("MenuConstruction");

        // Add liseners here.
        buttonConstructor.onClick.AddListener(delegate
        {
            OnButtonConstruction();
        });

        buttonWorld.onClick.AddListener(delegate
        {
            OnButtonWorld();
        });

        buttonWork.onClick.AddListener(delegate
        {
            OnButtonWork();
        });

        buttonOptions.onClick.AddListener(delegate
        {
            OnButtonOptions();
        });

        buttonSettings.onClick.AddListener(delegate
        {
            OnButtonSettings();
        });

        buttonQuests = CreateButton("menu_quests");
        buttonQuests.onClick.AddListener(delegate
        {
            OnButtonQuests();
        });

        DeactivateAll();
    }

    private Button CreateButton(string text)
    {
        GameObject buttonQuestGameObject = (GameObject)Instantiate(Resources.Load("UI/MenuButton"), this.gameObject.transform);
        buttonQuestGameObject.name = "Button - " + text;
        Text buttonText = buttonQuestGameObject.transform.GetChild(0).GetComponent<Text>();
        buttonText.text = text;
        buttonText.GetComponent<TextLocalizer>().text = buttonText;
        buttonText.GetComponent<TextLocalizer>().UpdateText();
        return buttonQuestGameObject.GetComponent<Button>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            DeactivateAll();
        }
    }
}
