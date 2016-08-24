#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections;

public class DialogBoxManager : MonoBehaviour
{

    // This will just keep a reference to all the dialog boxes since there inactive on start you cant find them.
    public MenuController mc;
    public DialogBoxLoadGame dialogBoxLoadGame;
    public DialogBoxSaveGame dialogBoxSaveGame;
    public DialogBoxOptions dialogBoxOptions;
    public DialogBoxSettings dialogBoxSettings;
    public DialogBoxTrade dialogBoxTrade;
    public DialogBoxAreYouSure dialogBoxAreYouSure;

    void Awake()
    {
        GameObject Controllers = GameObject.Find("Dialog Boxes");
        GameObject tempGoObj;

        mc = GameObject.Find("Dialog Boxes").GetComponent<MenuController>();

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/DB_SaveFile"), Controllers.transform.position, Controllers.transform.rotation,  Controllers.transform);
        tempGoObj.name = "Save File";
        dialogBoxSaveGame = tempGoObj.GetComponent<DialogBoxSaveGame>();

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/DB_LoadFile"), Controllers.transform.position, Controllers.transform.rotation,  Controllers.transform);
        tempGoObj.name = "Load File";
        dialogBoxLoadGame = tempGoObj.GetComponent<DialogBoxLoadGame>();

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/DB_Options"), Controllers.transform.position, Controllers.transform.rotation,  Controllers.transform);
        tempGoObj.name = "Options";
        dialogBoxOptions = tempGoObj.GetComponent<DialogBoxOptions>();

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/DB_Settings"), Controllers.transform.position, Controllers.transform.rotation,  Controllers.transform);
        tempGoObj.name = "Settings";
        dialogBoxSettings = tempGoObj.GetComponent<DialogBoxSettings>();

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/DB_Trade"), Controllers.transform.position, Controllers.transform.rotation,  Controllers.transform);
        tempGoObj.name = "Trade";
        dialogBoxTrade = tempGoObj.GetComponent<DialogBoxTrade>();

        tempGoObj = (GameObject)Instantiate(Resources.Load("UI/DB_AreYouSure"), Controllers.transform.position, Controllers.transform.rotation,  Controllers.transform);
        tempGoObj.name = "Are You Sure";
        dialogBoxAreYouSure = tempGoObj.GetComponent<DialogBoxAreYouSure>();

    }
}