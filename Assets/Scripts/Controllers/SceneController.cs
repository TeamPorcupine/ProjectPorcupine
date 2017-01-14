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

public class SceneController
{
    // Our current scenes Names
    public const string MainSceneName = "_World";
    public const string MainMenuSceneName = "MainMenu";

    public static string loadWorldFromFileName;

    public static Vector3 NewWorldSize;
    public static int Seed;
    public static bool GenerateAsteroids = true;
    public static string GeneratorFile = "Default.xml";

    private static SceneController instance;

    public static SceneController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SceneController();
            }

            return instance;
        }
    }

    // Load the main scene.
    public void LoadNewWorld(int width, int height, int depth, int seed, string generatorFile, bool generateAsteroids = true)
    {
        NewWorldSize = new Vector3(width, height, depth); 
        Seed = seed;
        GeneratorFile = generatorFile;
        GenerateAsteroids = generateAsteroids;
        CleanInstancesBeforeLoadingScene();
        SceneManager.LoadScene(MainSceneName);
    }

    public void ConfigureNewWorld()
    {
        GameObject.FindObjectOfType<DialogBoxManager>().dialogBoxNewGame.ShowDialog();
    }

    // Load a save file.
    public void LoadWorld(string fileName)
    {
        loadWorldFromFileName = fileName;
        CleanInstancesBeforeLoadingScene();
        SceneManager.LoadScene(MainSceneName);
    }

    // Load Main Menu.
    public void LoadMainMenu()
    {
        CleanInstancesBeforeLoadingScene();
        SceneManager.LoadScene(MainMenuSceneName);
    }

    // Quit the app whether in editor or a build version.
    public void QuitGame()
    {
        // Maybe ask the user if he want to save or is sure they want to quit??
        #if UNITY_EDITOR
        // Allows you to quit in the editor.
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // Return the name of the current scene.
    public string GetCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }

    // Return the name of the current scene.
    public bool IsAtIntroScene()
    {
        return (GetCurrentScene() == MainMenuSceneName) ? true : false;
    }

    // Return the name of the current scene.
    public bool IsAtMainScene()
    {
        return (GetCurrentScene() == MainSceneName) ? true : false;
    }

    private void CleanInstancesBeforeLoadingScene()
    {
        if (WorldController.Instance != null)
        {
            WorldController.Instance.Destroy();
        }

        ProjectPorcupine.Localization.LocalizationTable.UnregisterDelegates();
    }
}