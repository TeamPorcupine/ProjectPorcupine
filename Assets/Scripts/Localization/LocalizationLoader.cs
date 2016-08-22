#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.IO;
using System.Collections;

namespace ProjectPorcupine.Localization
{
    /**
     * <summary>
     * This class will load all localizations inside the /../StreamingAssets/Localization folder.
     * </summary>
     */
    [AddComponentMenu("Localization/Localization Loader")]
    public class LocalizationLoader : MonoBehaviour
    {
        //Initialize the localization files before Unity loads the scene entirely.
        //Used to ensure that the TextLocalizer scripts won't throw errors.
        void Awake()
        {
            //Check if the languages have already been loaded before.
            if (LocalizationTable.initialized)
            {
                //Return in this case.
                return;
            }

            //Get the filepath.
            string filePath = Path.Combine(Application.streamingAssetsPath, "Localization");
            
            //Loop through all files.
            foreach(string file in Directory.GetFiles(filePath))
            {
                //Check if the file is really a .lang file, and nothing else.
                //TODO: Think over the extension ".lang", might change that in the future.
                if(file.EndsWith(".lang"))
                {
                    //The file extention is .lang, load it.
                    LocalizationTable.LoadLocalizationFile(file);

                    // Just write a little debug info into the console.
                    Logger.LogVerbose("Loaded localization at path\n" + file);
                }
            }


            // Atempt to get setting of currently selected language. (Will default to English).
            string lang = Settings.getSetting("localization", "en_US");

            // setup LocalizationTable with either loaded or defaulted language
            LocalizationTable.currentLanguage = lang;


            //Tell the LocalizationTable that it has been initialized.
            LocalizationTable.initialized = true;
        }
    }
}
