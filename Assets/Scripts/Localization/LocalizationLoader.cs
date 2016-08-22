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
using ICSharpCode.SharpZipLib.Zip;
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

        /// <summary>
        /// Callback for DownloadLocalizationFromWeb. 
        /// For now it just saves downloaded data (master.zip) in 
        /// Application.streamingAssetsPath/Localization directory.
        /// </summary>
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
                string localizationFolderPath = Path.Combine(Application.streamingAssetsPath, "Localization");

                // Turn's out that System.IO.Compression.GZipStream is not working in unity:
                // http://forum.unity3d.com/threads/cant-use-gzipstream-from-c-behaviours.33973/
                // So I need to use some sort of 3rd party solution.

                // Clear Application.streamingAssetsPath/Localization folder
                DirectoryInfo localizationFolderInfo = new DirectoryInfo(localizationFolderPath);
                foreach (FileInfo file in localizationFolderInfo.GetFiles())
                {
                    // If there are files without that extension then:
                    // a) someone made a change to localization system and didn't update this
                    // b) We are in a wrong directory, so let's hope we didn't delete anything important.
                    if (file.Extension != ".lang" && file.Extension != ".meta" && file.Extension != ".ver")
                    {
                        Logger.LogException(new System.Exception("SOMETHING WENT HORRIBLY WRONG AT DOWNLOADING LOCALIZATION!"));
                        Debug.Break();
                        return;
                    }
                    file.Delete();
                }
                foreach (DirectoryInfo dir in localizationFolderInfo.GetDirectories())
                {
                    dir.Delete(true);
                }

                // Convert array of downloaded bytes to stream.
                using (ZipInputStream zipReadStream = new ZipInputStream(new MemoryStream(www.bytes)))
                {
                    ZipEntry theEntry;

                    // While there are still files inside zip archive.
                    while ((theEntry = zipReadStream.GetNextEntry()) != null)
                    {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);

                        // If there was a subfolder in zip (which there probably is) create one.
                        if (string.IsNullOrEmpty(directoryName) == false)
                        {
                            string directoryFullPath = Path.Combine(localizationFolderPath, directoryName);
                            if (Directory.Exists(directoryFullPath) == false)
                            {
                                Directory.CreateDirectory(directoryFullPath);
                            }
                        }

                        // Read files from stream to files on HDD.
                        // 2048 buffer should be plenty.
                        if (string.IsNullOrEmpty(fileName) == false)
                        {
                            string fullFilePath = Path.Combine(localizationFolderPath, theEntry.Name);
                            using (FileStream fileWriter = File.Create(fullFilePath))
                            {
                                int size = 2048;
                                byte[] fdata = new byte[2048];
                                while (true)
                                {
                                    size = zipReadStream.Read(fdata, 0, fdata.Length);
                                    if (size > 0)
                                    {
                                        fileWriter.Write(fdata, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // At this point we should have an subfolder in Application.streamingAssetsPath/Localization
                // called ProjectPorcupineLocalization-*branch name*. Now we need to move all files from that directory
                // to Application.streamingAssetsPath/Localization.
                FileInfo[] fileInfo = localizationFolderInfo.GetFiles();
                if (fileInfo.Length > 0)
                {
                    Logger.LogError("There should be no files here.");
                }

                DirectoryInfo[] dirInfo = localizationFolderInfo.GetDirectories();
                if (dirInfo.Length > 1)
                {
                    Logger.LogError("There should be only one directory");
                }

                // Move files from ProjectPorcupineLocalization-*branch name* to Application.streamingAssetsPath/Localization.
                string[] filesToMove = Directory.GetFiles(dirInfo[0].FullName);

                foreach (string file in filesToMove)
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(localizationFolderPath, fileName);
                    File.Copy(file, destFile);
                    File.Delete(file);
                }

                // Remove ProjectPorcupineLocalization-*branch name*
                Directory.Delete(dirInfo[0].FullName);

                Logger.Log("New localization downloaded, please restart the game for it to take effect.");
            }
            catch (System.Exception e)
            {
                // Something happen in the file system. 
                // TODO: Handle this properly, for now this is as useful as:
                // http://i.imgur.com/9ArGADw.png
                Logger.LogException(e);
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
