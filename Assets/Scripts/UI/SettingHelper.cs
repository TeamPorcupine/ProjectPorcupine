﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingHelper : MonoBehaviour 
{

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
	public Dropdown qualityDropdown;

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
		qualityDropdown.onValueChanged.AddListener(delegate { OnQualityChange(); } );

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

	public void OnQualityChange()
	{
		gameSettings.qualityIndex = QualitySettings.masterTextureLimit = qualityDropdown.value;
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

	}

	public void OnMusicChange()
	{
		
	}

	public void SaveSetting()
	{
		
	}

	void LoadSetting()
	{
		aliasingDropdown.value = QualitySettings.antiAliasing;
		vSyncDropdown.value =  QualitySettings.vSyncCount;
		qualityDropdown.value = QualitySettings.masterTextureLimit;

		fullScreenToggle.isOn = Screen.fullScreen;
		fpsToggle.isOn = fpsObject.activeSelf;
		langToggle.isOn =langDropDown.activeSelf;
	}

	void CreateResDropDown()
	{

	}
}
