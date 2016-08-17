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

    public override void ShowDialog()
    {
        base.ShowDialog();

        // Get list of files in save location
        string saveDirectoryPath = WorldController.Instance.FileSaveBasePath();

        // If directory doesn't exist that means that either user never saved game before
        // or something went horribly wrong and WorldController.Instance.FileSaveBasePath()
        // returns massively invalid path for some reason.
        if (Directory.Exists(saveDirectoryPath) == false)
        {
            Debug.LogError(saveDirectoryPath + " doesn't exist - display \"No saves available\" overlay?");
            Directory.CreateDirectory(saveDirectoryPath);
        }

        DirectoryInfo saveDir = new DirectoryInfo(saveDirectoryPath);


        FileInfo[] saveGames = saveDir.GetFiles().OrderByDescending(f => f.CreationTime).ToArray();

        // Our save dialog has an input field, which the fileListItems fill out for
        // us when we click on them
        InputField inputField = gameObject.GetComponentInChildren<InputField>();

        // Build file list by instantiating fileListItemPrefab

        foreach (FileInfo file in saveGames)
        {
            GameObject go = (GameObject)GameObject.Instantiate(fileListItemPrefab);

            // Make sure this gameobject is a child of our list box
            go.transform.SetParent(fileList);

            // file contains something like "C:\Users\UserName\......\Project Porcupine\Saves\SomeFileName.sav"
            // Path.GetFileName(file) returns "SomeFileName.sav"
            // Path.GetFileNameWithoutExtension(file) returns "SomeFileName"

            go.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(file.FullName);

            go.GetComponent<DialogListItem>().inputField = inputField;
        }

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
