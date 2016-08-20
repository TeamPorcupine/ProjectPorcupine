using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ProjectPorcupine.Localization
{
    /// <summary>
    /// The central class containing localization information.
    /// </summary>
    public static class LocalizationTable
    {
        private enum FallbackMode
        {
            ReturnKey,
            ReturnDefaultLanguage
        }

        //The dictionary that stores all the localization values.
        private static Dictionary<string, Dictionary<string,string>> localizationTable = new Dictionary<string, Dictionary<string, string>>();
        private static HashSet<string> missingKeysLogged = new HashSet<string>();

        private static readonly string defaultLanguage = "en_US";

        //The current language. This will be automatically be set by the LocalizationLoader.
        //Default is English.
        public static string currentLanguage = defaultLanguage;

        // Used by the LocalizationLoader to ensure that the localization files are only loaded once.
        public static bool initialized = false;

        /// <summary>
        /// Load a localization file from the harddrive.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public static void LoadLocalizationFile(string path)
        {
            string localizationCode = Path.GetFileNameWithoutExtension(path);
            LoadLocalizationFile(path, localizationCode);
        }

        /// <summary>
        /// Load a localization file from the harddrive with a defined localization code.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="localizationCode">The localization code, e.g.: "en_US", "en_UK"</param>
        private static void LoadLocalizationFile(string path, string localizationCode)
        {           
            //Read the contents of the file. This might throw an exception!
            try
            {
                localizationTable[localizationCode] = new Dictionary<string, string>();
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string[] keyValuePair = line.Split('=');
                    if (keyValuePair.Length != 2)
                    {
                        throw new InvalidOperationException(string.Format("Invalid format of localization string. Actual {0}", line));
                    }
                    localizationTable[localizationCode].Add(keyValuePair[0], keyValuePair[1]);
                }
            }
            catch (FileNotFoundException exception)
            {
                Logger.LogException(new Exception(string.Format("There is no localization file for {0}", localizationCode), exception));
            }            
        }

        /// <summary>
        /// Returns the localization for the given key, or the key itself, if no translation exists.
        /// </summary>
        /// <param name="key">The key that should be searched for.</param>
        /// <param name="additionalValues">The values that should be inserted.</param>
        /// <returns></returns>
        public static string GetLocalization(string key, params string[] additionalValues)
        {
            //Return the localization of the advanced method.
            return GetLocalization(key, FallbackMode.ReturnDefaultLanguage, currentLanguage, additionalValues);
        }

        /// <summary>
        /// Returns the localization for the given key, or the key itself, if no translation exists.
        /// </summary>
        private static string GetLocalization(string key, FallbackMode fallbackMode, string language, params string[] additionalValues)
        {
            if (localizationTable.ContainsKey(language) && localizationTable[language].ContainsKey(key))
            {
                return string.Format(localizationTable[language][key], additionalValues);
            }
            if (!missingKeysLogged.Contains(key))
            {
                missingKeysLogged.Add(key);
                Logger.LogWarning("Translation for " + key + " failed: Key not in dictionary.");
            }
            //Switch the fallback mode.
            switch (fallbackMode)
            {
                case FallbackMode.ReturnKey:
                    return additionalValues != null && additionalValues.Length >= 1 ? key + " " + additionalValues[0] : key;
                case FallbackMode.ReturnDefaultLanguage:
                    return GetLocalization(key, FallbackMode.ReturnKey, defaultLanguage, additionalValues); //Return the english equivalent.
                default: return string.Empty; //Return an empty string.
            }
        }

        public static string[] GetLanguages()
        {
            return localizationTable.Keys.ToArray();
        }
    }
}