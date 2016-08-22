#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

//   Object -> MonoBehaviour -> DialogBox -> DialogBoxLoadSaveGame ->
//														DialogBoxSaveGame
//														DialogBoxLoadGame
//

public class DialogBoxLoadSaveGame : DialogBox
{

    public GameObject fileListItemPrefab;
    public Transform fileList;

    public static readonly Color secondaryColor = new Color(0.9f, 0.9f,0.9f);

    /// <summary>
    /// If directory doesn't exist EnsureDirectoryExists will create one.
    /// </summary>
    /// <param name="directoryPath">Full directory path.</param>
    public void EnsureDirectoryExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath) == false)
        {
            Debug.LogWarning("Directory: " + directoryPath + " doesn't exist - creating.");
            Directory.CreateDirectory(directoryPath);
        }
    }

    public override void ShowDialog()
    {
        base.ShowDialog();

        // Get list of files in save location
        string saveDirectoryPath = WorldController.Instance.FileSaveBasePath();

        EnsureDirectoryExists(saveDirectoryPath);

        DirectoryInfo saveDir = new DirectoryInfo(saveDirectoryPath);


        FileInfo[] saveGames = saveDir.GetFiles().OrderByDescending(f => f.CreationTime).ToArray();

        // Our save dialog has an input field, which the fileListItems fill out for
        // us when we click on them
        InputField inputField = gameObject.GetComponentInChildren<InputField>();

        // Build file list by instantiating fileListItemPrefab

        for (int i = 0; i < saveGames.Length; i++)
        {
            FileInfo file = saveGames[i];
            GameObject go = (GameObject)GameObject.Instantiate(fileListItemPrefab);

            // Make sure this gameobject is a child of our list box
            go.transform.SetParent(fileList);

            // file contains something like "C:\Users\UserName\......\Project Porcupine\Saves\SomeFileName.sav"
            // Path.GetFileName(file) returns "SomeFileName.sav"
            // Path.GetFileNameWithoutExtension(file) returns "SomeFileName"

            string fileName = Path.GetFileNameWithoutExtension(file.FullName);

            go.GetComponentInChildren<Text>().text = string.Format("{0}\n<size=11><i>{1}</i></size>", fileName, file.CreationTime);

            DialogListItem listItem = go.GetComponent<DialogListItem>();
            listItem.fileName = fileName;
            listItem.inputField = inputField;

            go.GetComponent<Image>().color = (i % 2 == 0 ? Color.white : secondaryColor);
        }

		// Set scroll sensitivity based on the save-item count
		fileList.GetComponentInParent<ScrollRect> ().scrollSensitivity = fileList.childCount / 2;
    }

    public override void CloseDialog()
    {
        // Clear out all the children of our file list

        while (fileList.childCount > 0)
        {
            Transform c = fileList.GetChild(0);
            c.SetParent(null);	// Become Batman
            Destroy(c.gameObject);
        }

        // We COULD clear out the inputField field here, but I think
        // it makes sense to leave the old filename in there to make
        // overwriting easier?
        // Alternatively, we could either:
        //   a) Clear out the text box
        //	 b) Append an incremental number to it so that it automatically does
        //		something like "SomeFileName 13"

        base.CloseDialog();
    }
}
