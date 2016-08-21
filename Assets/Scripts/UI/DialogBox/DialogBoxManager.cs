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