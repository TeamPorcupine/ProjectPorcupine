using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

	public GameObject menu_Furniture;
	public GameObject menu_Floor;
	public GameObject menu_Options;
	public GameObject menu_Settings;

	// Use this for initialization
	void Start () {
		DeactivateMenus ();
	}
	

	public void DeactivateMenus()
	{
		menu_Furniture.SetActive (false);
		menu_Floor.SetActive(false);
		menu_Options.SetActive(false);
		menu_Settings.SetActive(false);
	}


}
