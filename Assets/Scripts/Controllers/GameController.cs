#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    void Awake()
    {
        enableDontDestroyOnLoad();

        // Load Settings

        // Load Keyboard Mapping
    }

    void Start()
    {
           

    }

    private void enableDontDestroyOnLoad()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }         
}