#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
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

        // TODO: Change this to the official repo before PR.
        private static readonly string LastCommitGithubApiLocation = "https://api.github.com/repos/QuiZr/ProjectPorcupineLocalization/commits/" + World.current.currentGameVersion;

        private static readonly string LocalizationFolderPath = Path.Combine(Application.streamingAssetsPath, "Localization");

        // Object for downloading localization data from web.
        private static WWW www;

        /// <summary>
        /// Check if there are any new updates for localization. TODO: Add a choice for a user to not update them right now.
        /// </summary>
        public static IEnumerator CheckIfCurrentLocalizationIsUpToDate(Action onLocalizationDownloadedCallback)
        {
            // Check current version of localization based on commit hash.
            string currentLocalizationVersion;
            try
            {
                currentLocalizationVersion = File.OpenText(Path.Combine(LocalizationFolderPath, "curr.ver")).ReadToEnd();
            }
            catch (FileNotFoundException e)
            {
                Debug.ULogWarningChannel("LocalizationDownloader", (e.Message));
                currentLocalizationVersion = string.Empty;
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.ULogWarningChannel("LocalizationDownloader", (e.Message));
                Directory.CreateDirectory(LocalizationFolderPath);
                currentLocalizationVersion = string.Empty;
            }
            catch (System.Exception e)
            {
                Debug.ULogErrorChannel("LocalizationDownloader", (e.ToString()));
                yield break;
            }

            // Check the latest localization version through the GitHub API.
            WWW versionChecker = new WWW(LastCommitGithubApiLocation);

            yield return versionChecker;

            if (versionChecker.error != null)
            {
                // This could be a thing when for example user has no internet connection.
                Debug.ULogErrorChannel("LocalizationDownloader", "Error while checking for available localization updates: " + www.error);
                yield break;
            }

            // Let's try to filter that response and get the latest hash from it.
            // There is a possibility that the versionChecker.text will be corrupted
            // (i.e. when you pull the Ethernet plug while downloading so thats why 
            // a little try-catch block is there.
            string latestCommitHash = string.Empty;
            try
            {
                latestCommitHash = GetHashOfLastCommitFromAPIResponse(versionChecker.text);
            }
            catch
            {
                yield break;
            }

            if (latestCommitHash != currentLocalizationVersion)
            {
                // There are still some updates available. We should probably notify
                // user about it and offer him an option to download it right now.
                // For now... Let's just force it >.> Beginners task!
                Debug.ULogChannel("LocalizationDownloader", "There is an update for localization files!");
                yield return DownloadLocalizationFromWeb(onLocalizationDownloadedCallback);
            }

            // Create curr.ver file to keep track of the latest localization version.
            // Could we use the Settings class for this? Beginners task!
            try
            {
                string currVerPath = Path.Combine(LocalizationFolderPath, "curr.ver");
                using (StreamWriter currVer = File.CreateText(currVerPath))
                {
                    currVer.Write(latestCommitHash);
                }
            }
            catch (Exception e)
            {
                // Not a big deal:
                // Next time the LocalizationDownloader will force an update.
                Debug.ULogError("LocalizationDownloader", "Not critical error at writing curr.ver file" + e.Message);
                throw;
            }
        }

        // For now Unity's implementation of .net WebClient will do just fine,
        // especially that it doesn't have problems with downloading from https.
        private static IEnumerator DownloadLocalizationFromWeb(Action onLocalizationDownloadedCallback)
        {
            // If there were some files downloading previously (maybe user tried to download the newest
            // language pack and mashed a download button?) just cancel them and start a new one.
            if (www != null)
            {
                www.Dispose();
            }

            Debug.ULogChannel("LocalizationDownloader", "Localization files download has started");

            www = new WWW(LocalizationRepositoryZipLocation);

            // Wait for www to download current localization files.
            yield return www;

            Debug.ULogChannel("LocalizationDownloader", "Localization files download has finished!");

            // Almost like a callback call
            OnDownloadLocalizationComplete(onLocalizationDownloadedCallback);
        }

        /// <summary>
        /// Callback for DownloadLocalizationFromWeb. 
        /// It replaces current content of localizationFolderPath with fresh, downloaded one.
        /// </summary>
        private static void OnDownloadLocalizationComplete(Action onLocalizationDownloadedCallback)
        {
            if (www.isDone == false)
            {
                // This should never happen.
                Debug.ULogErrorChannel("LocalizationDownloader", new System.Exception("OnDownloadLocalizationComplete got called before www finished downloading.").ToString());
                www.Dispose();
                return;
            }

            if (www.error != null)
            {
                // This could be a thing when for example user has no internet connection.
                Debug.ULogErrorChannel("LocalizationDownloader", "Error while downloading localizations file: " + www.error);
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
                    if (file.Extension != ".lang" && file.Extension != ".meta" && file.Extension != ".ver" && file.Extension != ".md")
                    {
                        Debug.ULogErrorChannel("LocalizationDownloader", new System.Exception("SOMETHING WENT HORRIBLY WRONG AT DOWNLOADING LOCALIZATION!").ToString());
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
                    Debug.ULogErrorChannel("LocalizationDownloader", "There should be no files here.");
                }

                DirectoryInfo[] dirInfo = localizationFolderInfo.GetDirectories();
                if (dirInfo.Length > 1)
                {
                    Debug.ULogErrorChannel("LocalizationDownloader", "There should be only one directory");
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

                Debug.ULogChannel("LocalizationDownloader", "New localization files downloaded!");
            }
            catch (System.Exception e)
            {
                // Something happen in the file system. 
                // TODO: Handle this properly, for now this is as useful as:
                // http://i.imgur.com/9ArGADw.png
                Debug.ULogErrorChannel("LocalizationDownloader", e.ToString());
            }

            onLocalizationDownloadedCallback();
        }

        /// <summary>
        /// This is a really wonky way of parsing JSON. I didn't want to include something like
        /// Json.NET library purely for this functionality but if we will be using it somewhere else
        /// this need to change. DO NOT TOUCH and this will be fine >.>
        /// </summary>
        /// <param name="githubApiResponse">GitHub API response</param>
        /// <returns></returns>
        private static string GetHashOfLastCommitFromAPIResponse(string githubApiResponse)
        {
            // Index of the first char of hash. 
            int index = githubApiResponse.IndexOf("sha\":\"");

            if (index == -1)
            {
                // Either the response was damaged or GitHub API returned an error.
                Debug.ULogErrorChannel("LocalizationDownloader", "Error at parsing JSON");
                throw new Exception("Error at parsing JSON");
            }

            // + 6 == "sha\":\"" length
            index += 6;

            char currentChar = githubApiResponse[index];

            // Hash of the commit.
            string hash = string.Empty;
            hash += currentChar;
            while (true)
            {
                // Check if this is the end of the commit string.
                // I don't think that i need to escape that quote mark 
                // but oh well - it works either way..
                index++;
                currentChar = githubApiResponse[index];

                if (currentChar == '\"')
                {
                    break;
                }

                hash += currentChar;

                if (index + 1 == githubApiResponse.Length - 1)
                {
                    Debug.ULogErrorChannel("LocalizationDownloader", "Error at parsing JSON");
                    throw new Exception("Error at parsing JSON");
                }
            }

            return hash;
        }
    }
}