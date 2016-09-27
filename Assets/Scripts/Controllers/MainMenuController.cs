#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void Start()
    {
        // Register inputs actions
        KeyboardManager.Instance.RegisterInputAction("Return", KeyboardMappedInputType.KeyDown, LoadNewWorld);

        //Display Main Menu
        UIManager.Instance.RenderMainMenu();
    }

    public void LoadNewWorld()
    {
        SceneManager.LoadScene("_SCENE_");
    }
}