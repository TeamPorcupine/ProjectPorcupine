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
    public Button buttonConstruction;
    public Button buttonWorld;
    public Button buttonWork;
    public Button buttonOptions;
    public Button buttonQuests;

    private DialogBoxManager dialogManager;

    // The left build menu.
    private GameObject constructorMenu;
    private MenuLeft menuLeft;

    // Toggles whether menu is active.
    public void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
    }

    public void OnButtonConstruction()
    {
        if (menuLeft.CurrentlyOpen != null && menuLeft.CurrentlyOpen.gameObject.name == "ConstructionMenu")
        {
            menuLeft.CloseMenu();
        }
        else
        {
            menuLeft.OpenMenu("ConstructionMenu");
        }
    }

    public void OnButtonWork()
    {
        menuLeft.CloseMenu();
    }

    public void OnButtonWorld()
    {
        if (!WorldController.Instance.IsModal)
        {
            menuLeft.CloseMenu();
        }
    }

    public void OnButtonQuests()
    {
        if (!WorldController.Instance.IsModal)
        {
            menuLeft.CloseMenu();
            dialogManager.dialogBoxQuests.ShowDialog();
        }
    }

    public void OnButtonOptions()
    {
        menuLeft.CloseMenu();

        if (dialogManager.dialogBoxSettings.isActiveAndEnabled)
        {
            dialogManager.dialogBoxSettings.CloseDialog();
        }

        dialogManager.dialogBoxOptions.ToggleDialog();
    }

    // Use this for initialization.
    private void Start()
    {
        dialogManager = GameObject.Find("Dialog Boxes").GetComponent<DialogBoxManager>();
        menuLeft = GameObject.Find("MenuLeft").GetComponent<MenuLeft>();

        // Add liseners here.
        buttonConstruction.onClick.AddListener(delegate
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

        buttonQuests = CreateButton("menu_quests");
        buttonQuests.onClick.AddListener(delegate
        {
            OnButtonQuests();
        });

        menuLeft.CloseMenu();
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
            menuLeft.CloseMenu();
        }
    }
}
