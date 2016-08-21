using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogBoxOptions : DialogBox {

	public Button buttonResume;
	public Button buttonQuit;

	void OnEnable()
	{
		// Add liseners
		buttonQuit.onClick.AddListener(delegate { OnButtonQuitGame(); } );
		buttonResume.onClick.AddListener(delegate { this.CloseDialog(); } );
	}

	// quit the app wheather in editor or a build version
	public void OnButtonQuitGame()
	{
		// maybe ask the user if he want to save or is sure they want to quit??

		#if UNITY_EDITOR
		//alows you to quit in the editor
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	void Update() {
		if (Input.GetKey (KeyCode.Escape)) {
			this.CloseDialog ();
		}
	}
}
