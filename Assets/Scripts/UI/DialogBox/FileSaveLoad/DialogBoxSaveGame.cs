#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxSaveGame : DialogBoxLoadSaveGame
{
    public override void ShowDialog()
    {
        base.ShowDialog();
        DialogListItem[] listItems = GetComponentsInChildren<DialogListItem>();
        foreach (DialogListItem listItem in listItems)
        {
            listItem.doubleclick = OkayWasClicked;
        }
    }

    public void OkayWasClicked()
    {
        StartCoroutine(OkayWasClickedCoroutine());
    }

    public IEnumerator OkayWasClickedCoroutine()
    {
        bool isOkToSave = true;

        // TODO:
        // check to see if the file already exists
        // if so, ask for overwrite confirmation.
        string fileName = gameObject.GetComponentInChildren<InputField>().text;

        // TODO: Is the filename valid?  I.E. we may want to ban path-delimiters (/ \ or :) and 
        //// maybe periods?      ../../some_important_file

        DialogBoxManager dbm = GameObject.Find("Dialog Boxes").GetComponent<DialogBoxManager>();

        if (fileName == string.Empty)
        {
            dbm.dialogBoxPromptOrInfo.SetAsInfo("message_name_or_file_needed_for_save");
            dbm.dialogBoxPromptOrInfo.ShowDialog();
            yield break;
        }

        // Right now fileName is just what was in the dialog box.  We need to pad this out to the full
        // path, plus an extension!
        // In the end, we're looking for something that's going to be similar to this (depending on OS)
        //    C:\Users\Quill18\ApplicationData\MyCompanyName\MyGameName\Saves\SaveGameName123.sav

        // Application.persistentDataPath == C:\Users\<username>\ApplicationData\MyCompanyName\MyGameName\
        string filePath = System.IO.Path.Combine(GameController.Instance.FileSaveBasePath(), fileName + ".sav");

        // At this point, filePath should look very much like
        ////     C:\Users\Quill18\ApplicationData\MyCompanyName\MyGameName\Saves\SaveGameName123.sav

        if (File.Exists(filePath) == true)
        {
            isOkToSave = false;

            dbm.dialogBoxPromptOrInfo.SetPrompt("prompt_overwrite_existing_file", new string[] { fileName });
            dbm.dialogBoxPromptOrInfo.SetButtons(DialogBoxResult.Yes, DialogBoxResult.No);

            dbm.dialogBoxPromptOrInfo.Closed = () =>
            {
                if (dbm.dialogBoxPromptOrInfo.Result == DialogBoxResult.Yes)
                {
                    isOkToSave = true;
                }
            };

            dbm.dialogBoxPromptOrInfo.ShowDialog();

            if (!isOkToSave)
            {
                while (dbm.dialogBoxPromptOrInfo.gameObject.activeSelf)
                {
                    yield return null;
                }
            }
        }

        if (isOkToSave)
        {
            dbm.dialogBoxPromptOrInfo.SetPrompt("message_saving_game");
            dbm.dialogBoxPromptOrInfo.ShowDialog();

            // Skip a frame so that user will see pop-up
            yield return null;

            Thread t = WorldController.Instance.SaveWorld(filePath);

            // Wait for data to be saved to HDD.
            while (t.IsAlive)
            {
                yield return null;
            }

            dbm.dialogBoxPromptOrInfo.CloseDialog();

            this.CloseDialog();

            dbm.dialogBoxPromptOrInfo.SetAsInfo("message_game_saved");
            dbm.dialogBoxPromptOrInfo.ShowDialog();

            while (dbm.dialogBoxPromptOrInfo.gameObject.activeSelf)
            {
                yield return null;
            }
        }
    }
}
