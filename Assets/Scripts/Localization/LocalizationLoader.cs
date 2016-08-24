#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.IO;
using UnityEngine;

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
        /// <summary>
        /// Scans Application.streamingAssetsPath/Localization folder in search for .lang files and load's them
        /// to the LocalizationTable.
        /// </summary>
        public void UpdateLocalizationTable()
        {
            // Get the file path.
            string filePath = Path.Combine(Application.streamingAssetsPath, "Localization");
            
            // Loop through all files.
            // TODO: Think over the extension ".lang", might change that in the future.
            foreach (string file in Directory.GetFiles(filePath, "*.lang"))
            {
                // The file extention is .lang, load it.
                LocalizationTable.LoadLocalizationFile(file);

                // Just write a little debug info into the console.
                Debug.Log("Loaded localization at path\n" + file);
            }

            foreach (DirectoryInfo mod in WorldController.Instance.modsManager.GetMods())
            {
                foreach (FileInfo file in mod.GetFiles("*.lang"))
                {
                    // Load the mod localization.
                    LocalizationTable.LoadLocalizationFile(file.FullName);

                    // Just write a little debug info into the console.
                    Debug.ULogChannel("LocalizationLoader", "Loaded localization at path: " + file);
                }
            }

            // Attempt to get setting of currently selected language. (Will default to English).
            string lang = Settings.getSetting("localization", "en_US");

            // Setup LocalizationTable with either loaded or defaulted language
            LocalizationTable.currentLanguage = lang;

            // Tell the LocalizationTable that it has been initialized.
            LocalizationTable.LoadingLanguagesFinished();
        }

        // Initialize the localization files before Unity loads the scene entirely.
        // Used to ensure that the TextLocalizer scripts won't throw errors.
        private void Awake()
        {
            // Check if the languages have already been loaded before.
            if (LocalizationTable.initialized)
            {
                // Return in this case.
                return;
            }

            // Update localization from the internet.
            StartCoroutine(LocalizationDownloader.CheckIfCurrentLocalizationIsUpToDate(delegate { UpdateLocalizationTable(); }));

            UpdateLocalizationTable();
        }
    }
}
