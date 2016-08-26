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
            // Load application localization files
            LoadLocalizationInDirectory(Application.streamingAssetsPath);

            // Load mods localization files
            foreach (DirectoryInfo mod in WorldController.Instance.modsManager.GetMods())
            {
                LoadLocalizationInDirectory(mod.FullName);
            }

            // Attempt to get setting of currently selected language. (Will default to English).
            string lang = Settings.getSetting("localization", "en_US");

            // Setup LocalizationTable with either loaded or defaulted language
            LocalizationTable.currentLanguage = lang;

            // Tell the LocalizationTable that it has been initialized.
            LocalizationTable.LoadingLanguagesFinished();
        }

        /// <summary>
        /// Loads the localization in directory.
        /// </summary>
        /// <param name="path">Arbitrary path to load Localization files from</param>
        private void LoadLocalizationInDirectory(string path)
        {
            // Get the file path.
            string filePath = Path.Combine(path, "Localization");

            if (Directory.Exists(filePath) == false)
            {
                return;
            }

            // Loop through all files.
            // TODO: Think over the extension ".lang", might change that in the future.
            foreach (string file in Directory.GetFiles(filePath, "*.lang"))
            {
                // The file extension is .lang, load it.
                LocalizationTable.LoadLocalizationFile(file);

                // Just write a little debug info into the console.
                Debug.ULogChannel("LocalizationLoader", "Loaded localization at path: " + file);
            }
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
