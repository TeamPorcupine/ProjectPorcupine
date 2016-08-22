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
    public static class LocalizationDownloader
    {
        // TODO: Change this to the official repo before PR.
        private static readonly string LocalizationRepositoryZipLocation = "https://github.com/QuiZr/ProjectPorcupineLocalization/archive/" + World.current.currentGameVersion + ".zip";

        private static readonly string LocalizationFolderPath = Path.Combine(Application.streamingAssetsPath, "Localization");

        // Object for downloading localization data from web.
        private static WWW www;

        /// <summary>
        /// Check if there are any new updates for localization. TODO: Add a choice for a user to not update them right now.
        /// </summary>
        public static IEnumerator CheckIfCurrentLocalizationIsUpToDate()
        {
            // Check current version of localization
            string currentLocalizationVersion;
            try
            {
                currentLocalizationVersion = File.OpenText(Path.Combine(LocalizationFolderPath, "curr.ver")).ReadToEnd();
            }
            catch (FileNotFoundException e)
            {
                Debug.LogWarning(e.Message);
                currentLocalizationVersion = string.Empty;
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.LogWarning(e.Message);
                Directory.CreateDirectory(LocalizationFolderPath);
                currentLocalizationVersion = string.Empty;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                yield break;
            }

            // Download curr.ver file from localization repository and check
            // if it's matching the localizationFolderPath/curr.ver file
            string avaibleVersionLocation = "https://raw.githubusercontent.com/QuiZr/ProjectPorcupineLocalization/" + World.current.currentGameVersion + "/curr.ver";
            WWW versionChecker = new WWW(avaibleVersionLocation);

            yield return versionChecker;

            if (versionChecker.error != null)
            {
                // This could be a thing when for example user has no internet connection.
                Debug.LogError("Error while checking for available localization updates: " + www.error);
                yield break;
            }

            if (versionChecker.text != currentLocalizationVersion)
            {
                // There are still some updates available. We should probably notify
                // user about it and offer him an option to download it right now.
                // For now... Let's just force it >.> Beginners task!
                Debug.Log("There is an update for localization files!");
                yield return DownloadLocalizationFromWeb();
            }
        }

        // For now Unity's implementation of .net WebClient will do just fine,
        // especially that it doesn't have problems with downloading from https.
        private static IEnumerator DownloadLocalizationFromWeb()
        {
            // If there were some files downloading previously (maybe user tried to download the newest
            // language pack and mashed a download button?) just cancel them and start a new one.
            if (www != null)
            {
                www.Dispose();
            }

            Debug.Log("Localization files download has started");

            www = new WWW(LocalizationRepositoryZipLocation);

            // Wait for www to download current localization files.
            yield return www;

            Debug.Log("Localization files download has finished!");

            // Almost like a callback call
            OnDownloadLocalizationComplete();
        }

        /// <summary>
        /// Callback for DownloadLocalizationFromWeb. 
        /// It replaces current content of localizationFolderPath with fresh, downloaded one.
        /// </summary>
        private static void OnDownloadLocalizationComplete()
        {
            if (www.isDone == false)
            {
                // This should never happen.
                Debug.LogError(new System.Exception("OnDownloadLocalizationComplete got called before www finished downloading."));
                www.Dispose();
                return;
            }

            if (www.error != null)
            {
                // This could be a thing when for example user has no internet connection.
                Debug.LogError("Error while downloading localizations file: " + www.error);
                return;
            }

            try
            {
                // Turn's out that System.IO.Compression.GZipStream is not working in unity:
                // http://forum.unity3d.com/threads/cant-use-gzipstream-from-c-behaviours.33973/
                // So I need to use some sort of 3rd party solution.

                // Clear Application.streamingAssetsPath/Localization folder
                DirectoryInfo localizationFolderInfo = new DirectoryInfo(LocalizationFolderPath);
                foreach (FileInfo file in localizationFolderInfo.GetFiles())
                {
                    // If there are files without that extension then:
                    // a) someone made a change to localization system and didn't update this
                    // b) We are in a wrong directory, so let's hope we didn't delete anything important.
                    if (file.Extension != ".lang" && file.Extension != ".meta" && file.Extension != ".ver")
                    {
                        Debug.LogError(new System.Exception("SOMETHING WENT HORRIBLY WRONG AT DOWNLOADING LOCALIZATION!"));
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
                            string directoryFullPath = Path.Combine(LocalizationFolderPath, directoryName);
                            if (Directory.Exists(directoryFullPath) == false)
                            {
                                Directory.CreateDirectory(directoryFullPath);
                            }
                        }

                        // Read files from stream to files on HDD.
                        // 2048 buffer should be plenty.
                        if (string.IsNullOrEmpty(fileName) == false)
                        {
                            string fullFilePath = Path.Combine(LocalizationFolderPath, theEntry.Name);
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
                    Debug.LogError("There should be no files here.");
                }

                DirectoryInfo[] dirInfo = localizationFolderInfo.GetDirectories();
                if (dirInfo.Length > 1)
                {
                    Debug.LogError("There should be only one directory");
                }

                // Move files from ProjectPorcupineLocalization-*branch name* to Application.streamingAssetsPath/Localization.
                string[] filesToMove = Directory.GetFiles(dirInfo[0].FullName);

                foreach (string file in filesToMove)
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(LocalizationFolderPath, fileName);
                    File.Copy(file, destFile, true);
                    File.Delete(file);
                }

                // Remove ProjectPorcupineLocalization-*branch name*
                Directory.Delete(dirInfo[0].FullName);

                // Maybe there is an easy fix to that restart-need thing? 
                // Beginners task!
                Debug.Log("New localization downloaded, please restart the game for it to take effect.");
            }
            catch (System.Exception e)
            {
                // Something happen in the file system. 
                // TODO: Handle this properly, for now this is as useful as:
                // http://i.imgur.com/9ArGADw.png
                Debug.LogError(e);
            }
        }
    }
}