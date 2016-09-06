#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxManager : MonoBehaviour
{
    // This will just keep a reference to all the dialog boxes since there inactive on start you cant find them.

    public DialogBoxJobList dialogBoxJobList;
    public DialogBoxLoadGame dialogBoxLoadGame;
    public DialogBoxSaveGame dialogBoxSaveGame;
    public DialogBoxOptions dialogBoxOptions;
    public DialogBoxSettings dialogBoxSettings;
    public DialogBoxTrade dialogBoxTrade;
    public DialogBoxAreYouSure dialogBoxAreYouSure;
    public DialogBoxQuests dialogBoxQuests;

    public GameObject DialogBoxGO;

    public void Awake()
    {
        DialogBoxGO = GameObject.Find("Dialog Boxes");

        GameObject tempGoObj;

        tempGoObj = CreateDialogGO("DB_SaveFile", "Save File");
        dialogBoxSaveGame = tempGoObj.GetComponent<DialogBoxSaveGame>();

        tempGoObj = CreateDialogGO("DB_LoadFile", "Load File");
        dialogBoxLoadGame = tempGoObj.GetComponent<DialogBoxLoadGame>();

        tempGoObj = CreateDialogGO("DB_Options", "Options");
        dialogBoxOptions = tempGoObj.GetComponent<DialogBoxOptions>();

        tempGoObj = CreateDialogGO("DB_Settings", "Settings");
        dialogBoxSettings = tempGoObj.GetComponent<DialogBoxSettings>();

        tempGoObj = CreateDialogGO("DB_Trade", "Trade");
        dialogBoxTrade = tempGoObj.GetComponent<DialogBoxTrade>();

        tempGoObj = CreateDialogGO("DB_AreYouSure", "Are You Sure");
        dialogBoxAreYouSure = tempGoObj.GetComponent<DialogBoxAreYouSure>();

        tempGoObj = CreateDialogGO("DB_JobList", "Job List");
        dialogBoxJobList = tempGoObj.GetComponent<DialogBoxJobList>();

        tempGoObj = CreateDialogGO("DB_Quests", "Quests");
        dialogBoxQuests = tempGoObj.GetComponent<DialogBoxQuests>();
        AddQuestList();
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
        buttonQuestGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -30, 0);
        return buttonQuestGameObject.GetComponent<Toggle>();
    }
}
