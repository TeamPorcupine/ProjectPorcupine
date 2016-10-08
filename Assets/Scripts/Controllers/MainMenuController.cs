#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using UnityStandardAssets.Utility;

public class MainMenuController : MonoBehaviour
{
    public ModsManager modsManager;

    public static MainMenuController Instance { get; protected set; }

    // Use this for initialization.
    public void OnEnable()
    {
        new PrototypeManager();
    }

    public void Start()
    {
        Instance = this;

        new SpriteManager();
        new AudioManager();
        modsManager = new ModsManager();

        TimeManager.Instance.IsPaused = true;

        // Create a Background.
        Instantiate(Resources.Load("Prefab/BackgroundMainMenu"));

        GameObject canvas = GameObject.Find("Canvas");

        // Create a Title.
        GameObject title = (GameObject)Instantiate(Resources.Load("UI/TitleMainMenu"));
        title.transform.SetParent(canvas.transform, false);
        title.SetActive(true);

        // Display Main Menu.
        GameObject mainMenu = (GameObject)Instantiate(Resources.Load("UI/MainMenu"));
        mainMenu.transform.SetParent(canvas.transform, false);
        mainMenu.SetActive(true);

        // Create dialogBoxes.
        GameObject dialogBoxes = new GameObject("Dialog Boxes");
        dialogBoxes.transform.SetParent(canvas.transform, false);
        dialogBoxes.AddComponent<DialogBoxManager>();

        // Instantiate a FPSCounter.
        GameObject menuTop = (GameObject)Instantiate(Resources.Load("UI/MenuTop"));
        menuTop.name = "MenuTop";
        menuTop.transform.SetParent(canvas.transform, false);
        menuTop.SetActive(true);
        GameObject fpsCounter = FindObjectOfType<FPSCounter>().gameObject;
        fpsCounter.SetActive(Settings.GetSetting("DialogBoxSettings_fpsToggle", true));
    }
}