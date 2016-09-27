#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; protected set; }

    public void Start()
    {
        Instance = this;

        // Display Main Menu.
        GameObject canvas = GameObject.Find("Canvas");
        GameObject mainMenuPrefab = (GameObject)Resources.Load("UI/MainMenu");
        GameObject mainMenu = (GameObject)Instantiate(mainMenuPrefab);
        mainMenu.transform.SetParent(canvas.transform, false);
        mainMenu.SetActive(true);
    }
}