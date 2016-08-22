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

    public DialogBoxLoadGame dialogBoxLoadGame;
    public DialogBoxSaveGame dialogBoxSaveGame;
    public DialogBoxOptions dialogBoxOptions;
    public DialogBoxSettings dialogBoxSettings;
    public DialogBoxTrade dialogBoxTrade;
}