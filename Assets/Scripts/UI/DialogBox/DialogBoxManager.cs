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
using System.IO;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This will just keep a reference to all the dialog boxes since there inactive on start you cant find them.
/// </summary>
[MoonSharpUserData]
public class DialogBoxManager : MonoBehaviour
{
    public DialogBoxJobList dialogBoxJobList;
    public DialogBoxLoadGame dialogBoxLoadGame;
    public DialogBoxSaveGame dialogBoxSaveGame;
    public DialogBoxOptions dialogBoxOptions;
    public DialogBoxSettings dialogBoxSettings;
    public DialogBoxTrade dialogBoxTrade;
    public DialogBoxPromptOrInfo dialogBoxPromptOrInfo;
    public DialogBoxQuests dialogBoxQuests;

    // This dictionary will hold the DialogBoxes to be called by name.
    public Dictionary<string, DialogBox> DialogBoxes;
    public GameObject DialogBoxGO;

    public void Awake()
    {
        DialogBoxes = new Dictionary<string, DialogBox>();
        DialogBoxGO = GameObject.Find("Dialog Boxes");

        GameObject tempGoObj;

        tempGoObj = CreateDialogGO("DB_LoadFile", "Load File");
        dialogBoxLoadGame = tempGoObj.GetComponent<DialogBoxLoadGame>();
        DialogBoxes["Load File"] = dialogBoxLoadGame;

        tempGoObj = CreateDialogGO("DB_Settings", "Settings");
        dialogBoxSettings = tempGoObj.GetComponent<DialogBoxSettings>();
        DialogBoxes["Settings"] = dialogBoxSettings;

        tempGoObj = CreateDialogGO("DB_PromptOrInfo", "Prompt or Info");
        dialogBoxPromptOrInfo = tempGoObj.GetComponent<DialogBoxPromptOrInfo>();
        DialogBoxes["Prompt or Info"] = dialogBoxPromptOrInfo;

        if (SceneController.Instance.IsAtMainScene())
        {
            tempGoObj = CreateDialogGO("DB_SaveFile", "Save File");
            dialogBoxSaveGame = tempGoObj.GetComponent<DialogBoxSaveGame>();
            DialogBoxes["Save File"] = dialogBoxSaveGame;

            tempGoObj = CreateDialogGO("DB_Options", "Options");
            dialogBoxOptions = tempGoObj.GetComponent<DialogBoxOptions>();
            DialogBoxes["Options"] = dialogBoxOptions;

            tempGoObj = CreateDialogGO("DB_Trade", "Trade");
            dialogBoxTrade = tempGoObj.GetComponent<DialogBoxTrade>();
            DialogBoxes["Trade"] = dialogBoxTrade;

            tempGoObj = CreateDialogGO("DB_JobList", "Job List");
            dialogBoxJobList = tempGoObj.GetComponent<DialogBoxJobList>();
            DialogBoxes["Job List"] = dialogBoxJobList;

            tempGoObj = CreateDialogGO("DB_Quests", "Quests");
            dialogBoxQuests = tempGoObj.GetComponent<DialogBoxQuests>();
            DialogBoxes["Quests"] = dialogBoxQuests;
            AddQuestList();
            LoadModdedDialogBoxes();
            AddMainMenuItems();
        }
    }

    /// <summary>
    /// ShowDialogBoxByName shows the dialog box that has the name given.
    /// </summary>
    /// <param name="dialogName">The name of the dialog (a.k.a. the title of the dialog).</param>
    public DialogBox ShowDialogBoxByName(string dialogName)
    {
        if (DialogBoxes.ContainsKey(dialogName))
        {
            DialogBoxes[dialogName].ShowDialog();
            return DialogBoxes[dialogName];
        }
        else
        {
            Debug.ULogErrorChannel("ModDialogBox", "Couldn't find dialog box with name" + dialogName);
            return null;
        }
    }

    /// <summary>
    /// Creates a dialog GameObject from its prefab.
    /// </summary>
    /// <param name="prefabName">The name of the prefab.</param>
    /// <param name="name">The name of the instance of the prefab in the scene.</param>
    /// <returns>The dialog as an instance in the scene.</returns>
    private GameObject CreateDialogGO(string prefabName, string name)
    {
        GameObject tempGoObj = (GameObject)Instantiate(Resources.Load("UI/" + prefabName), DialogBoxGO.transform.position, DialogBoxGO.transform.rotation, DialogBoxGO.transform);
        tempGoObj.name = name;
        return tempGoObj;
    }

    // Temporary location until we have a better dialog manager
    private void AddMainMenuItems()
    {
        GameMenuManager.Instance.AddMenuItem(
            "menu_work",
            () => dialogBoxJobList.ShowDialog(),
            "menu_construction");

        GameMenuManager.Instance.AddMenuItem(
            "menu_world",
            null,
            "menu_work");

        GameMenuManager.Instance.AddMenuItem(
            "menu_quests",
            () => dialogBoxQuests.ShowDialog(),
            "menu_world");

        GameMenuManager.Instance.AddMenuItem(
            "menu_options",
            () =>
            {
                if (dialogBoxSettings.isActiveAndEnabled)
                {
                    dialogBoxSettings.CloseDialog();
                }

                dialogBoxOptions.ShowDialog();
            },
            "menu_quests");
    }

    // Temporary location until we have a proper code-driven UI
    private void AddQuestList()
    {
        Transform layoutRoot = DialogBoxGO.transform.parent.GetComponent<Transform>();
        GameObject go = (GameObject)Instantiate(Resources.Load("UI/QuestsMainScreenBox"), layoutRoot.transform);
        go.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -55, 0);

        Toggle pinButton = CreatePinQuestButton();

        pinButton.onValueChanged.AddListener(go.SetActive);
    }

    private Toggle CreatePinQuestButton()
    {
        GameObject buttonQuestGameObject = (GameObject)Instantiate(Resources.Load("UI/PinToggleButton"), this.gameObject.transform);
        buttonQuestGameObject.name = "ToggleQuestPinButton";
        buttonQuestGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        return buttonQuestGameObject.GetComponent<Toggle>();
    }

    /// <summary>
    /// Loads every Dialog Box in the /StreamingAssets/UI/DialogBoxes/ Folder.
    /// </summary>
    private void LoadModdedDialogBoxes()
    {
        Debug.ULogChannel("ModDialogBox", "Loading xml dialog boxes");
        string dialogBoxPath = Path.Combine(Application.streamingAssetsPath, "UI");
        dialogBoxPath = Path.Combine(dialogBoxPath, "DialogBoxes");
        DirectoryInfo dialogBoxPathInfo = new DirectoryInfo(dialogBoxPath);

        foreach (FileInfo fileInfo in dialogBoxPathInfo.GetFiles())
        {
            switch (fileInfo.Extension)
            {
                case ".xml":
                    Debug.ULogChannel("ModDialogBox", "Found xml element:" + fileInfo.Name);
                    GameObject dialogBoxPrefab = CreateDialogGO("DB_MOD", "Modded Dialog Box");
                    ModDialogBox modDialogBox = dialogBoxPrefab.GetComponent<ModDialogBox>();
                    modDialogBox.LoadFromXML(fileInfo);
                    dialogBoxPrefab.name = modDialogBox.Title;
                    DialogBoxes[modDialogBox.Title] = modDialogBox;
                    break;
                case ".lua":
                    Debug.ULogChannel("ModDialogBox", "Found lua element:" + fileInfo.Name);
                    WorldController.Instance.modsManager.LoadFunctionsInFile(fileInfo, "ModDialogBox");
                    break;
            }
        }
    }
}
