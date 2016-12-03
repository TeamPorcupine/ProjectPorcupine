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
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using DiffMatchPatch;


namespace ProjectPorcupine.Localization
{
    /*
                                                 ,  ,
                                               / \/ \
                                              (/ //_ \_
     .-._                                      \||  .  \
      \  '-._                            _,:__.-"/---\_ \
 ______/___  '.    .--------------------'~-'--.)__( , )\ \
`'--.___  _\  /    |             Here        ,'    \)|\ `\|
     /_.-' _\ \ _:,_          Be Dragons           " ||   (
   .'__ _.' \'-/,`-~`                                |/
       '. ___.> /=,|  Abandon hope all ye who enter  |
        / .-'/_ )  '---------------------------------'
        )'  ( /(/
             \\ "
              '=='
    */

    /// <summary>
    /// This class makes sure that your localization is up to date. It does that by comparing latest know commit hash
    /// (which is stored in Application.streamingAssetsPath/Localization/curr.ver) to the latest hash available through
    /// GitHub. If the hashes don't match or curr.ver doesn't exist a new zip containing
    /// localization will be downloaded from GitHub repo. Then, the zip is stored in the memory and waits for
    /// Application.streamingAssetsPath/Localization to be cleaned. When It's empty the zip gets unpacked and saved
    /// to hard drive using ICSharpCode.SharpZipLib.Zip library. Every GitHub zip download has a folder with
    /// *ProjectName*-*BranchName* so all of it's content needs to be moved to Application.streamingAssetsPath/Localization.
    /// After that the folder get's deleted and new curr.ver file is created containing latest hash.
    /// GitHub's branch name corresponds to World.current.gameVersion, so that the changes in localization
    /// for version 0.2 won't affect users who haven't updated yet from version 0.1.
    /// </summary>
    public static class LocalizationDownloader
    {
        private const string LocalizationRepository = "QuiZr/ProjectPorcupineLocalization/";
        private const string LatestCommitURL = "https://api.github.com/repos/" + LocalizationRepository + "/commits/" + GameController.GameVersion;
        private const string LocalizationConfigName = "config.xml";
        // NOTE: StreamingAssetsPath is read-only on android and iOS.
        private static readonly string LocalizationFolderPath = Path.Combine(Application.streamingAssetsPath, "Localization");

        private static readonly string ConfigPath = Path.Combine(LocalizationFolderPath, LocalizationConfigName);

        class GithubFile
        {
            public string sha { get; set; }
            public string filename { get; set; }
            public string status { get; set; }
            public string patch { get; set; }
            
        }
        class GithubCommit
        {
            public string sha { get; set; }
            public GithubFile[] files { get; set; }

        }

        public static IEnumerator UpdateLocalization(System.Action onLocalizationDownloadedCallback)
        {
            
            // Use this to see if the user has auto update enabled.
            Settings.GetSetting("DialogBoxSettings_autoUpdateLocalization", true);

            if (!File.Exists(ConfigPath))
            {
                
                // Download all of the localization
                yield return DownloadLocalization();

            }

            else if (Settings.GetSetting("DialogBoxSettings_autoUpdateLocalization", true))
            {
                //Check if we have any updates to the localization
                yield return GetChangesSinceDate();
            }
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
                            translations.Add(attribute);
                    }

                }
            }
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
                if (File.Exists(ConfigPath))
                {
                    File.Delete(ConfigPath);
                }
                WWW downloadConfig = new WWW("https://cdn.rawgit.com/" + LocalizationRepository + GameController.GameVersion + "/" + LocalizationConfigName);
                yield return downloadConfig;
                if (!string.IsNullOrEmpty(downloadConfig.error))
                {
                    Debug.ULogErrorChannel("LocalizationDownloader", "Error while downloading localization for the first time. Are you connected to the internet? \n" + downloadConfig.error);
                    yield break;
                }

                File.WriteAllBytes(ConfigPath, downloadConfig.bytes);
                // Download the translation files.
                foreach (string locale in GetTranslations())
                {
                    string path = Path.Combine(LocalizationFolderPath, locale + ".lang");
                    WWW www = new WWW("https://cdn.rawgit.com/" + LocalizationRepository + GameController.GameVersion + "/" + locale + ".lang");
                    yield return www;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        Debug.ULogErrorChannel("LocalizationDownloader", "Error while downloading locale " + locale + " for the first time. Are you connected to the internet? \n" + www.error);
                        yield break;
                    }

                    File.WriteAllBytes(path, www.bytes);
                }
                Debug.ULogErrorChannel("LocalizationDownloader", "right here");
                WriteLocalizationDate();
            }
            else {
                string url;

                // We need a different url if a hash is provided.
                if (String.IsNullOrEmpty(hash))
                    url = "https://cdn.rawgit.com/" + LocalizationRepository + GameController.GameVersion + "/";
                else
                    url = "https://cdn.rawgit.com/" + LocalizationRepository + hash + "/";

                foreach(string file in list)
                {
                    string path = Path.Combine(LocalizationFolderPath, file);
                    UnityWebRequest www = UnityWebRequest.Get(url + file);
                    yield return www.Send();
                    
                    if (www.isError)
                    {
                        Debug.ULogErrorChannel("LocalizationDownloader", "Error while downloading file " + file + " using rawgit. Are you sure your connected to the internet? \n" + www.error);
                        yield break;
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
                Debug.ULogErrorChannel("LocalizationDownloader", "XML Error while parsing config.xml. config.xml's XML must be formatted incorrectly. \n" + e);
            }
            catch (System.Exception e)
            {
                Debug.ULogErrorChannel("LocalizationDownloader", "Error while parsing config.xml. \n" + e);
            }
            reader.Close();
            //reader.Close();
            if (string.IsNullOrEmpty(lastDate))
            {
                yield return DownloadLocalization();
                Debug.ULogErrorChannel("LocalizationDownloader", "Error while trying to get the date of the last update. I'm going to re-download everything.");
                // We re-download everything because most-likely the're using the old version of the config file that includes the hash but not the date.
                yield return null;
            }
            // Here is a good example output: https://api.github.com/repos/QuiZr/ProjectPorcupineLocalization/commits?since=2016-10-25T04:15:52Z&sha=Someone_will_come_up_with_a_proper_naming_scheme_later
            //string request = "https://api.github.com/repos/" + LocalizationRepository + "commits?since=" + lastDate + "&sha=" + GameController.GameVersion;
            string request = "https://api.github.com/repos/QuiZr/ProjectPorcupineLocalization/commits?since=2016-10-25T04:15:52Z&sha=Someone_will_come_up_with_a_proper_naming_scheme_later";

            WWW www = new WWW(request);
            yield return www;
                    
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.ULogErrorChannel("LocalizationDownloader", "Error while downloading commits information using the GitHub API. \n" + www.error);
                yield break;
            }
            // Documentation for parsing: http://www.newtonsoft.com/json/help/html/SerializingJSONFragments.htm

            // Now parse the json from github.
            ArrayList files = new ArrayList();

            JArray array = JArray.Parse(www.text);
            List<string> hashes = array.Select(o => (string) o["sha"]).ToList();
            // We need to reverse the hashes because we want to deal with the oldest changes first.
            hashes.Reverse();
            

            foreach (string hash in hashes)
            {
                //Example hashRequest: https://api.github.com/repos/QuiZr/ProjectPorcupineLocalization/commits/fde139ae1d8fcf82bb145bbc99ed41763202e28f
                string hashRequest = "https://api.github.com/repos/" + LocalizationRepository + "commits/" + hash;
                //WWWForm form = new WWWForm();
                //form.AddField("Content-Type", "application/vnd.github.VERSION.sha; charset=utf-8");
                //WWW wwwHash = new WWW(hashRequest, form.headers);
                UnityWebRequest wwwHash = UnityWebRequest.Get(hashRequest);
                yield return wwwHash.Send();
                    
                if (wwwHash.isError)
                {
                    Debug.ULogErrorChannel("LocalizationDownloader", "Error while downloading commit " + hash + " using the GitHub API. \n" + wwwHash.error);
                    yield break;
                }
                Debug.ULogChannel("LocalizationDownloader", "hash downloaded: \n" + wwwHash.downloadHandler.text);
                //diff_match_patch dmp = new diff_match_patchTest();

                GithubCommit commit = JsonConvert.DeserializeObject<GithubCommit>(wwwHash.downloadHandler.text);
                foreach (GithubFile file in commit.files)
                {
                    string path = Path.Combine(LocalizationFolderPath, file.filename);
                    
                    switch (file.status)
                    {
                        case "removed":
                            Debug.ULogChannel("LocalizationDownloader", "Removing the file " + file.filename + "from the localization.");
                            File.Delete(Path.Combine(LocalizationFolderPath, file.filename));
                            break;

                        case "modified":
                            Debug.ULogChannel("LocalizationDownloader", "Patching/modifing the file " + file.filename + ".");

                            //Let us make sure the file exists before we try to modify it.
                            if (!File.Exists(path))
                            {
                                Debug.ULogErrorChannel("LocalizationDownloader", "Error while patching file " + file.filename + " for the localization. The file doesn't exist, so I'll download it.");
                                DownloadLocalization(new string[] {file.filename}, hash);
                                break;
                            }

                            // Patch the file
                            //DiffMatchPatch p = new diff_match_patch();
                            List<Patch> patches = Patcher.patch_fromText(file.patch);
                            System.Object[] patch = Patcher.patch_apply(patches, File.ReadAllText(path) );
                            bool[] boolArray = (bool[])patch[1];
                            if  (boolArray.Length == 0 || !boolArray[0] || !boolArray[1])
                            {
                                Debug.ULogErrorChannel("LocalizationDownloader", "Error while patching file " + file.filename + " for the localization. Try deleting the file.");
                                yield break;
                            }
                            File.WriteAllText(path, patch[0].ToString());
                            break;

                        case "added":
                            Debug.ULogChannel("LocalizationDownloader", "Adding the file " + file.filename + " to the localization.");
                            
                            DownloadLocalization(new string[] {file.filename}, hash);
                            break;

                        default:
                            Debug.ULogErrorChannel("LocalizationDownloader", "Error while parsing Github commit: " + hash + 
                                                   ". The file " + file.filename + " has an unkown status of " + file.status + ".");
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
    }
}
