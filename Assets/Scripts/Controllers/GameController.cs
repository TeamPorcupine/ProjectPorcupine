#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ProjectPorcupine.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public KeyboardManager KeyboardManager;

    // If true, a modal dialog box is open, so normal inputs should be ignored.
    public bool IsModal;

    public static GameController Instance { get; protected set; }

    // Load the main scene.
    public void LoadNewWorld()
    {
        SceneManager.LoadScene("_SCENE_");
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

    private void Awake()
    {
        EnableDontDestroyOnLoad();

        this.gameObject.AddComponent<LocalizationLoader>();
    }

    private void Start()
    {
        // Load settings.
        Settings.LoadSettings();

        // Load Keyboard Mapping.
        KeyboardManager = KeyboardManager.Instance;
    }

    private void Update()
    {
        TimeManager.Instance.Update(Time.deltaTime);
    }

    private void EnableDontDestroyOnLoad()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }
}