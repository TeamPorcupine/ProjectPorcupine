using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml.Serialization;
using System.IO;


public class DialogBoxLoadGame : DialogBoxLoadSaveGame {


	public void OkayWasClicked() {
		string fileName = gameObject.GetComponentInChildren<InputField>().text;

		// TODO: Is the filename valid?  I.E. we may want to ban path-delimiters (/ \ or :) and 
		// maybe periods?      ../../some_important_file

		// Right now fileName is just what was in the dialog box.  We need to pad this out to the full
		// path, plus an extension!
		// In the end, we're looking for something that's going to be similar to this (depending on OS)
		//    C:\Users\Quill18\ApplicationData\MyCompanyName\MyGameName\Saves\SaveGameName123.sav

		// Application.persistentDataPath == C:\Users\<username>\ApplicationData\MyCompanyName\MyGameName\

		string filePath = System.IO.Path.Combine( WorldController.Instance.FileSaveBasePath(), fileName + ".sav" );

		// At this point, filePath should look very much like
		//     C:\Users\Quill18\ApplicationData\MyCompanyName\MyGameName\Saves\SaveGameName123.sav

		if(File.Exists(filePath) == false) {
			// TODO: Do file overwrite dialog box.

			Debug.LogError("File doesn't exist.  What?");
			CloseDialog();
			return;
		}

		CloseDialog();

		LoadWorld(filePath);
	}

	public void LoadWorld(string filePath) {
		// This function gets called when the user confirms a filename
		// from the load dialog box.

		// Get the file name from the save file dialog box

		Debug.Log("LoadWorld button was clicked.");

		WorldController.Instance.LoadWorld( filePath );


	}
}
