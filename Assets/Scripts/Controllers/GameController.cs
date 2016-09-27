#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

public class GameController : MonoBehaviour
{
    public KeyboardManager keyboardManager;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        EnableDontDestroyOnLoad();
    }

    private void Start()
    {
        // Load settings.
        Settings.LoadSettings();

        // Load Keyboard Mapping.
        keyboardManager = KeyboardManager.Instance;
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