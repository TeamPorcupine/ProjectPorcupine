using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {
	public GameObject constructorMenu;

	public GameObject optionsMenu;
	public GameObject settingsMenu;

	public GameObject furnitureMenu;
	public GameObject floorMenu;

	// Use this for initialization
	void Start () {
		DeactivateAll ();
	}
	
	public void DeactivateAll () {
		DeactivateConstructor ();
		settingsMenu.SetActive (false);
		optionsMenu.SetActive (false);

	}

	public void DeactivateConstructor () {
		DeactivateSubs ();
		constructorMenu.SetActive (false);

	}

	public void DeactivateSubs () {
		furnitureMenu.SetActive (false);
		floorMenu.SetActive (false);
	}
}
