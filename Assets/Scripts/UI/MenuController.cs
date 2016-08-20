﻿using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {
	//The left build menu
	public GameObject constructorMenu;

	//The sub menus of the build menu (furniture, floor..... later - power, security, drones)
	public GameObject furnitureMenu;
	public GameObject floorMenu;

	//The options and settings
	public GameObject optionsMenu;
	public GameObject settingsMenu;

	// Use this for initialization
	void Start () {
		DeactivateAll ();
	}

	//Deactivates All Menus
	public void DeactivateAll () {
		DeactivateConstructor ();
		settingsMenu.SetActive (false);
		optionsMenu.SetActive (false);
	}

	//Deactivates All Menus Except the settings and options
	public void DeactivateConstructor () {
		DeactivateSubs ();
		constructorMenu.SetActive (false);

	}

	//Deactivates any sub menu of the constrution options
	public void DeactivateSubs () {
		furnitureMenu.SetActive (false);
		floorMenu.SetActive (false);
	}

    //toggles whether menu is active
    public void ToggleMenu(GameObject menu) {
        menu.SetActive(!menu.activeSelf);
    }

	//quit the app wheather in editor or a build version
	public void QuitGame()
	{
		//maybe ask the user if he want to save or is sure they want to quit??

		#if UNITY_EDITOR
	    //alows you to quit in the editor
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	void Update() {
		if (Input.GetKey (KeyCode.Escape)) {
			DeactivateAll ();
		}
	}
}
