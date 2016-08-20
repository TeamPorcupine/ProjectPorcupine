using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingHelper : MonoBehaviour {

	//langage Option
	public Toggle langToggle;
	public GameObject langDropDown;

	//FPS Option
	public Toggle fpsToggle;
	public GameObject fpsObject;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void toggleLang()
	{
		
		langDropDown.SetActive (langToggle.isOn);
	}

	public void toggleFPS()
	{

		fpsObject.SetActive (fpsToggle.isOn);
	}
}
