#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

namespace ProjectPorcupine.Controllers
{
    class GameController : MonoBehaviour
    {
        public ModsManager modsManager;

        void Awake()
        {
            DontDestroyOnLoad(this);

            // Load Localization
            LocalizationLoader localizationLoader = new LocalizationLoader();

            // Load Settings

            // Load Keyboard Mapping
        }

        void Start()
        {


        }

    }
}
