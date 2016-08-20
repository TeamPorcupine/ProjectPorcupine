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

	public Toggle fullScreenToggle;

	//public Slider masterVolume;
	public Slider musicVolume;

	public Resolution[] myResolutions;
	public Dropdown resolutionDropdown;

	public Dropdown aliasingDropdown;
	public Dropdown vSyncDropdown;
	public Dropdown quilityDropdown;

	public GameSettings gameSettings;


	void OnEnable()
	{
		//create an instance of this class
		gameSettings = new GameSettings ();

		//get all avalible resolution for the display
		myResolutions = Screen.resolutions;

		//Add our listeners
		fpsToggle.onValueChanged.AddListener(delegate { OnFPSToggle(); } );
		langToggle.onValueChanged.AddListener(delegate { OnLangageToggle(); } );
		fullScreenToggle.onValueChanged.AddListener(delegate { OnFullScreenToggle(); } );

		resolutionDropdown.onValueChanged.AddListener(delegate { OnResolutionChange(); } );
		aliasingDropdown.onValueChanged.AddListener(delegate { OnAliasingChange(); } );
		vSyncDropdown.onValueChanged.AddListener(delegate { OnVSyncChange(); } );
		quilityDropdown.onValueChanged.AddListener(delegate { OnQuilityChange(); } );

		//masterVolume.onValueChanged.AddListener(delegate { OnMasterChange(); } );
		musicVolume.onValueChanged.AddListener(delegate { OnMusicChange(); } );

		//create the drop down for resolution
		CreateResDropDown();

		//Load the setting
		LoadSetting();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnLangageToggle()
	{
		gameSettings.isVisableLangage = langToggle.isOn;
		langDropDown.SetActive (langToggle.isOn);
	}

	public void OnFPSToggle()
	{
		gameSettings.isVisableFPS = fpsToggle.isOn;
		fpsObject.SetActive (fpsToggle.isOn);
	}

	public void OnFullScreenToggle()
	{
		gameSettings.isFullscreen = Screen.fullScreen = fullScreenToggle.isOn;
	}

	public void OnQuilityChange()
	{
		gameSettings.quilityIndex = QualitySettings.masterTextureLimit = quilityDropdown.value;
	}

	public void OnVSyncChange()
	{
		gameSettings.vSyncIndex = QualitySettings.vSyncCount = vSyncDropdown.value;
	}

	public void OnResolutionChange()
	{
		gameSettings.resultionIndex = resolutionDropdown.value;
	    //need to make work
	}

	public void OnAliasingChange()
	{
		gameSettings.antiAliasingindex = QualitySettings.antiAliasing = aliasingDropdown.value;
	}

	public void OnMasterChange()
	{
		//TODO: add Volume controll later
		//gameSettings.masterVolume = masterVolume.value;
	}

	public void OnMusicChange()
	{
		//TODO: add music later
		//gameSettings.musicVolume = musicVolume.value;
	}

	public void SaveSetting()
	{
		
	}

	void LoadSetting()
	{
		aliasingDropdown.value = QualitySettings.antiAliasing;
		vSyncDropdown.value =  QualitySettings.vSyncCount;
		quilityDropdown.value = QualitySettings.masterTextureLimit;

		fullScreenToggle.isOn = Screen.fullScreen;
		fpsToggle.isOn = fpsObject.activeSelf;
		langToggle.isOn =langDropDown.activeSelf;
	}

	void CreateResDropDown()
	{
		/*
		for (int i = 0; i < myResolutions.Length; i++) {
			resolutionDropdown.AddOptions (myResolutions [i].height.ToString () + "x" + myResolutions [i].width.ToString () + "   " + myResolutions [i].refreshRate.ToString ());
		}
		*/
		//???
		/*
		foreach (Resolution res in myResolutions) {
			resolutionDropdown.AddOptions (res);
		}
		*/
	}
}
