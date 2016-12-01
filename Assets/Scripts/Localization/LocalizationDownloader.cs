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

        private static readonly string ConfigPath = LocalizationFolderPath + Path.DirectorySeparatorChar + LocalizationConfigName;


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
                yield return GetChangesSinceDate();
                
                // Let's check if 
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
        private static IEnumerator DownloadLocalization(ArrayList list = null)
        {
            Debug.ULogErrorChannel("LocalizationDownloader", "right here");
            if (list == null)
            {
                // Just going to download everything
                WWW downloadConfig = new WWW("https://cdn.rawgit.com/" + LocalizationRepository + GameController.GameVersion + "/" + LocalizationConfigName);
                yield return downloadConfig;
                if (!string.IsNullOrEmpty(downloadConfig.error))
                {
                    Debug.ULogErrorChannel("LocalizationDownloader", "Error while downloading localization for the first time. Are you connected to the internet? \n" + downloadConfig.error);
                    yield break;
                }

                File.WriteAllBytes(ConfigPath, downloadConfig.bytes);
                // Download the translation files.
                Debug.ULogErrorChannel("LocalizationDownloader", "right here");
                foreach (string locale in GetTranslations())
                {
                    WWW www = new WWW("https://cdn.rawgit.com/" + LocalizationRepository + GameController.GameVersion + "/" + locale + ".lang");
                    yield return www;
                    
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        Debug.ULogErrorChannel("LocalizationDownloader", "Error while downloading locale " + locale + " for the first time. Are you connected to the internet? \n" + www.error);
                        yield break;
                    }

                    File.WriteAllBytes(LocalizationFolderPath + Path.DirectorySeparatorChar + locale + ".lang", www.bytes);
                }
                Debug.ULogErrorChannel("LocalizationDownloader", "right here");
                WriteLocalizationDate();
            }
            else {

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
            Debug.Log(lastDate);
            if (string.IsNullOrEmpty(lastDate))
            {
                yield return DownloadLocalization();
                Debug.ULogErrorChannel("LocalizationDownloader", "Error while trying to get the date of the last update. I'm going to re-download everything.");
                // We re-download everything because most-likely the're using the old version of the config file that includes the hash but not the date.
                
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
            foreach (string hash in hashes)
            {
                Console.WriteLine(hash);
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
                diff_match_patch dmp = new diff_match_patch();
                var test = dmp.patch_fromText(wwwHash.downloadHandler.text);
                //dmp.patch_apply(test, "");
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
