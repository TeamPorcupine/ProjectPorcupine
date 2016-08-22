#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
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
        // TODO: Change this to the official repo before PR.
        private readonly string localizationRepositoryZip = "https://github.com/QuiZr/ProjectPorcupineLocalization/archive/master.zip";

        private readonly string currentLocalizationVersion = "THIS_IS_PRE_ALFA_SO_I_THINK_THAT_IT_SHOULDN'T_MATTER_FOR_NOW";

        // Object for downloading localization data from web.
        private WWW www;

        // For now Unity's implementation of .net WebClient will do just fine,
        // especially that it doesn't have problems with downloading from https.
        private IEnumerator DownloadLocalizationFromWeb()
        {
            // If there were some files downloading previously (maybe user tried to download the newest
            // language pack and mashed a download button?) just cancel them and start a new one.
            if (www != null)
            {
                www.Dispose();
            }

            Logger.LogVerbose("Localization files download has started");

            www = new WWW(localizationRepositoryZip);

            Logger.LogVerbose("Localization files download has finished!");

            // Wait for www to download current localization files.
            yield return www;

            // Almost like a callback call
            OnDownloadLocalizationComplete();
        }

        // Callback for DownloadLocalizationFromWeb. 
        // For now it just saves downloaded data (master.zip) in 
        // Application.streamingAssetsPath/Localization directory.
        private void OnDownloadLocalizationComplete()
        {
            if (www.isDone != true)
            {
                // This should never happen.
                Logger.LogException(new System.Exception("OnDownloadLocalizationComplete got called before www finished downloading."));
                www.Dispose();
                return;
            }

            if (www.error != null)
            {
                // This could be a thing when for example user has no internet connection.
                Logger.LogError("Error while downloading localizations file: " + www.error);
                return;
            }

            try
            {
                // TODO: Make something with this data (unpack, verify version, replace files).
                // For now just debug write to HDD.
                string localizationFolderPath = Path.Combine(Application.streamingAssetsPath, "Localization");

                BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine(localizationFolderPath, "master.zip"), FileMode.OpenOrCreate));

                writer.Write(www.bytes);
                writer.Close();
            }
            catch (System.Exception e)
            {
                // Something happen in file system. 
                // TODO: Handle this properly, for now this is as useful as:
                // http://i.imgur.com/9ArGADw.png
                throw e;
            }
        }

        private void OnDestroy()
        {
            // Make sure that any downloads in progress will be canceled when the game closes.
            if (www != null)
            {
                www.Dispose();
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
            StartCoroutine(DownloadLocalizationFromWeb());

            // Get the file path.
            string filePath = Path.Combine(Application.streamingAssetsPath, "Localization");

            // Loop through all files.
            foreach (string file in Directory.GetFiles(filePath))
            {
                // Check if the file is really a .lang file, and nothing else.
                // TODO: Think over the extension ".lang", might change that in the future.
                if (file.EndsWith(".lang"))
                {
                    // The file extension is .lang, load it.
                    LocalizationTable.LoadLocalizationFile(file);

                    // Just write a little debug info into the console.
                    Logger.LogVerbose("Loaded localization at path\n" + file);
                }
            }

            // Attempt to get setting of currently selected language. (Will default to English).
            string lang = Settings.getSetting("localization", "en_US");

            // Setup LocalizationTable with either loaded or defaulted language
            LocalizationTable.currentLanguage = lang;

            // Tell the LocalizationTable that it has been initialized.
            LocalizationTable.initialized = true;
        }
    }
}
