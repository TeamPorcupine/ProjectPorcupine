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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using DiffMatchPatch;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.Networking;

namespace ProjectPorcupine.Localization
{
    /// <summary>
    /// This class makes sure that your localization is up to date.
    /// If we haven't ever started the program then it first downloads the config file, parses it and then
    /// downloads the files using rawgit.com's CDN then adds the date in ISO 8601 format to config.xml.
    /// If we have started ProjectPorcupine then it uses the Github API to check for the latest changes.
    /// If there are changes, then we download the details of the commits, including the changes of each file.
    /// We then use Google's DiffMatchPatch to change the files, so that we don't need to re-download the file.
    /// </summary>
    public static class LocalizationDownloader
    {
        // NOTE: Should be moved to the official TeamPorcupine repository.
        private const string LocalizationRepository = "QuiZr/ProjectPorcupineLocalization/";

        // TODO: Migrate to json or SKON.
        private const string LocalizationConfigName = "config.xml";

        // NOTE: StreamingAssetsPath is read-only on android and iOS.
        private static readonly string LocalizationFolderPath = Path.Combine(Application.streamingAssetsPath, "Localization");

        private static readonly string ConfigPath = Path.Combine(LocalizationFolderPath, LocalizationConfigName);

        public static IEnumerator UpdateLocalization(System.Action onFinished)
        {
            if (!File.Exists(ConfigPath))
            {
                // Download all of the localization
                yield return DownloadLocalization();
            }
            else if (Settings.GetSetting("autoUpdateLocalization", true))
            {
                // Check if we have any updates to the localization
                yield return GetChangesSinceDate();
            }

            UnityDebugger.Debugger.Log("LocalizationDownloader", "Localization has finished downloading.");
            onFinished();
        }

        /// <summary>
        /// Parses the config file and returns all of the available translations.
        /// TODO: Migrate config.xml to json or SKON.
        /// </summary>
        private static ArrayList GetTranslations()
        {
            ArrayList translations = new ArrayList();
            XmlReader reader = XmlReader.Create(ConfigPath);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "language")
                {
                    if (reader.HasAttributes)
                    {
                        string attribute = reader.GetAttribute("code");
                        if (attribute != "en_US")
                        {
                            translations.Add(attribute);
                        }
                    }
                }
            }

            reader.Close();
            return translations;
        }
        
        /// <summary>
        /// Downloads the localization files from the localization repository using rawgit CDN.
        /// If you don't provide any parameters it will just download all of the files.
        /// </summary>
        /// <param name="list">A list of files to download and update.</param>
        /// <param name="hash">If you want to provide a specific commit hash (sha).</param>
        private static IEnumerator DownloadLocalization(string[] list = null, string hash = null)
        {
            if (list == null)
            {
                // Just going to download everything
                UnityWebRequest downloadConfig = UnityWebRequest.Get("https://cdn.rawgit.com/" + LocalizationRepository + GameController.GameVersion + "/" + LocalizationConfigName);
                yield return downloadConfig.Send();
                if (downloadConfig.isError)
                {
                    UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while downloading localization for the first time. Are you connected to the internet? \n" + downloadConfig.error);
                    yield break;
                }

                if (File.Exists(ConfigPath))
                {
                    File.Delete(ConfigPath);
                }

                File.WriteAllBytes(ConfigPath, downloadConfig.downloadHandler.data);
                
                // Download the translation files.
                foreach (string locale in GetTranslations())
                {
                    string path = Path.Combine(LocalizationFolderPath, locale + ".lang");
                    UnityWebRequest www = UnityWebRequest.Get("https://cdn.rawgit.com/" + LocalizationRepository + GameController.GameVersion + "/" + locale + ".lang");
                    yield return www.Send();

                    if (www.isError)
                    {
                        UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while downloading locale " + locale + " for the first time. Are you connected to the internet? \n" + www.error);
                        yield break;
                    }

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    File.WriteAllBytes(path, www.downloadHandler.data);
                }

                WriteLocalizationDate();
            }
            else
            {
                string url;

                // We need a different url if a hash is provided.
                if (string.IsNullOrEmpty(hash))
                {
                    url = "https://cdn.rawgit.com/" + LocalizationRepository + GameController.GameVersion + "/";
                }
                else
                {
                    url = "https://cdn.rawgit.com/" + LocalizationRepository + hash + "/";
                }

                foreach (string file in list)
                {
                    string path = Path.Combine(LocalizationFolderPath, file);
                    UnityWebRequest www = UnityWebRequest.Get(url + file);
                    yield return www.Send();
                    
                    if (www.isError)
                    {
                        UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while downloading file " + file + " using rawgit. Are you sure your connected to the internet? \n" + www.error);
                        yield break;
                    }

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    File.WriteAllBytes(path, www.downloadHandler.data);
                }
            }

            yield return null;
        }

        /// <summary>
        /// This uses the GitHub api to get all of the commits since a date (recorded in config.xml).
        /// This is better for when/if we get very large translation files and we don't want to download all of the files when only one is changed.
        /// TODO: Migrate config.xml to json or SKON.
        /// </summary>
        private static IEnumerator GetChangesSinceDate()
        {
            string lastDate = string.Empty;
            XmlReader reader = XmlReader.Create(ConfigPath);
            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "version")
                    {
                        if (reader.HasAttributes)
                        {
                            lastDate = reader.GetAttribute("date");
                        }
                    }
                }
            } 
            catch (XmlException e)
            {
                UnityDebugger.Debugger.LogError("LocalizationDownloader", "XML Error while parsing config.xml. config.xml's XML must be formatted incorrectly. \n" + e);
            }
            catch (System.Exception e)
            {
                UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while parsing config.xml. \n" + e);
            }

            reader.Close();
            if (string.IsNullOrEmpty(lastDate))
            {
                UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while trying to get the date of the last update. I'm going to re-download everything.");
                yield return DownloadLocalization();

                // We re-download everything because most-likely the're using the old version of the config file that includes the hash rather than the date.
                yield return null;
            }

            // Here is a good example output: https://api.github.com/repos/QuiZr/ProjectPorcupineLocalization/commits?since=2016-10-25T04:15:52Z&sha=Someone_will_come_up_with_a_proper_naming_scheme_later
            string request = "https://api.github.com/repos/" + LocalizationRepository + "commits?since=" + lastDate + "&sha=" + GameController.GameVersion;

            WWW www = new WWW(request);
            yield return www;
                    
            if (!string.IsNullOrEmpty(www.error))
            {
                UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while downloading commits information using the GitHub API. \n" + www.error);
                yield break;
            }

            JArray array = JArray.Parse(www.text);
            List<string> hashes = array.Select(o => (string)o["sha"]).ToList();

            // We need to reverse the hashes because we want to deal with the oldest changes first.
            hashes.Reverse();

            foreach (string hash in hashes)
            {
                // Example hashRequest: https://api.github.com/repos/QuiZr/ProjectPorcupineLocalization/commits/fde139ae1d8fcf82bb145bbc99ed41763202e28f
                string hashRequest = "https://api.github.com/repos/" + LocalizationRepository + "commits/" + hash;
                UnityWebRequest wwwHash = UnityWebRequest.Get(hashRequest);
                yield return wwwHash.Send();
                    
                if (wwwHash.isError)
                {
                    UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while downloading commit " + hash + " using the GitHub API. \n" + wwwHash.error);
                    yield break;
                }

                GithubCommit commit = JsonConvert.DeserializeObject<GithubCommit>(wwwHash.downloadHandler.text);
                foreach (GithubFile file in commit.Files)
                {
                    string path = Path.Combine(LocalizationFolderPath, file.Filename);
                    
                    switch (file.Status)
                    {
                        case "removed":
                            UnityDebugger.Debugger.Log("LocalizationDownloader", "Removing the file " + file.Filename + "from the localization.");
                            if (!File.Exists(path))
                            {
                                UnityDebugger.Debugger.LogError("LocalizationDownloader", "I was going to remove the file " + file.Filename + " from the localization, but it's not even there!");
                                break;
                            }

                            File.Delete(path);
                            break;

                        case "modified":
                            UnityDebugger.Debugger.Log("LocalizationDownloader", "Patching/modifing the file " + file.Filename + ".");

                            // Let us make sure the file exists before we try to modify it.
                            if (!File.Exists(path))
                            {
                                UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while patching file " + file.Filename + " for the localization. The file doesn't exist, so I'll download it.");
                                DownloadLocalization(new string[] { file.Filename }, hash);
                                break;
                            }

                            // Patch the file
                            List<Patch> patches = Patcher.PatchFromText(file.Patch);
                            object[] patch = Patcher.PatchApply(patches, File.ReadAllText(path));

                            // Let's see if the patch applied correctly.
                            bool[] boolArray = (bool[])patch[1];
                            if (boolArray.Length == 0 || !boolArray[0] || !boolArray[1])
                            {
                                UnityDebugger.Debugger.LogError("LocalizationDownloader", "Error while patching file " + file.Filename + " for the localization. I'll just download it.");
                                DownloadLocalization(new string[] { file.Filename }, hash);
                                yield break;
                            }

                            File.WriteAllText(path, patch[0].ToString());
                            break;

                        case "added":
                            UnityDebugger.Debugger.Log("LocalizationDownloader", "Adding the file " + file.Filename + " to the localization.");
                            
                            DownloadLocalization(new string[] { file.Filename }, hash);
                            break;
                        
                        case "renamed":
                            UnityDebugger.Debugger.Log("LocalizationDownloader", "Renaming the file " + file.Previous_filename + " to " + file.Filename + " to the localization.");
                            string oldPath = Path.Combine(LocalizationFolderPath, file.Previous_filename);
                            
                            // If the file we are trying to rename doesn't exist then we can just download the new file.
                            if (!File.Exists(oldPath))
                            {
                                DownloadLocalization(new string[] { file.Filename }, hash);
                                break;
                            }
                            
                            // If the file we are trying to rename to already exists, then just delete it.
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            
                            // Move, AKA rename the file.
                            File.Move(oldPath, path);
                            break;

                        default:
                            string error  = "Error while parsing Github commit: " + hash + 
                                                   ". The file " + file.Filename + " has an unkown status of " + file.Status + ".";
                            UnityDebugger.Debugger.LogError("LocalizationDownloader", error);
                            yield break;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the current date and time to localization.
        /// The GitHub api requires the date to be ISO 8601 format: YYYY-MM-DDTHH:MM:SSZ.
        /// TODO: Migrate config.xml to json or SKON.
        /// </summary>
        private static void WriteLocalizationDate()
        {
            XmlDocument document = new XmlDocument();
            document.Load(ConfigPath);

            XmlNode node = document.SelectSingleNode("//config");
            XmlElement versionElement = document.CreateElement("version");
           
            // The GitHub api requires the date to be ISO 8601 format: YYYY-MM-DDTHH:MM:SSZ.
            versionElement.SetAttribute("date", DateTime.UtcNow.ToString("o"));
            node.InsertBefore(versionElement, document.SelectSingleNode("//languages"));
            
            document.Save(ConfigPath);
        }

        /// <summary>
        /// A file that was modified in a GitHub commit.
        /// </summary>
        private class GithubFile
        {
            /// <summary>
            /// The sha of the file, which is just a string of numbers.
            /// </summary>
            public string Sha { get; set; }

            /// <summary>
            /// The name of the file that was modified.
            /// </summary>
            public string Filename { get; set; }

            /// <summary>
            /// The status of the modification. ie, Deleted, modified, Added or Renamed.
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// The changes done to the file in the format of an Unix patch file.
            /// </summary>
            public string Patch { get; set; }

            /// <summary>
            /// Only shows up if the status is of the renaming type.
            /// </summary>
            public string Previous_filename { get; set; }
        }

        /// <summary>
        /// Based on the GitHub API version 3's json for fetching a list of commits.
        /// </summary>
        private class GithubCommit
        {
            /// <summary>
            /// The sha of the commit, which is just a string of numbers.
            /// </summary>
            public string Sha { get; set; }

            /// <summary>
            /// The list of files that were changed in the commit.
            /// </summary>
            public GithubFile[] Files { get; set; }
        }
    }
}
