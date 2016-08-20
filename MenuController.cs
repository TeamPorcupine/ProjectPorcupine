using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

	public GameObject DropdownLanguage;
	public GameObject FPS;
	[Space(10)]
	public GameObject ConstructorMenu;
	[Space(10)]
	public GameObject OptionsMenu;
	public GameObject SettingsMenu;
	[Space(10)]
	public GameObject FurnitureMenu;
	public GameObject FloorMenu;


	// Use this for initialization
	void Start () {
		DeactiveMenus ();
	}


	public void DeactiveMenus()
	{
		DeactiveSubMenus ();
		OptionsMenu.SetActive (false);
		SettingsMenu.SetActive (false);
		ConstructorMenu.SetActive (false);

	}

	public void DeactiveSubMenus()
	{
		FurnitureMenu.SetActive (false);
		FloorMenu.SetActive (false);

	}

	public void ToggleFPS()
	{
		
	}

	public void ToggleLanguage()
	{
		
	}

	public void QuitGame()
	{
		Application.Quit ();
	}
}
