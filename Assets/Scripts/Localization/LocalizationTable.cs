#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ProjectPorcupine.Localization
{
    /// <summary>
    /// The central class containing localization information.
    /// </summary>
    public static class LocalizationTable
    {
        // The current language. This will be automatically be set by the LocalizationLoader.
        // Default is English.
        public static string currentLanguage = defaultLanguage;

        // Used by the LocalizationLoader to ensure that the localization files are only loaded once.
        public static bool initialized = false;

        private static readonly string defaultLanguage = "en_US";

        // The dictionary that stores all the localization values.
        private static Dictionary<string, Dictionary<string, string>> localizationTable = new Dictionary<string, Dictionary<string, string>>();

        // Keeps track of what keys we've already logged are missing.
        private static HashSet<string> missingKeysLogged = new HashSet<string>();

        private enum FallbackMode
        {
            ReturnKey, ReturnDefaultLanguage
        }

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
        /// Returns the localization for the given key, or the key itself, if no translation exists.
        /// </summary>
        /// <param name="key">The key that should be searched for.</param>
        /// <param name="additionalValues">The values that should be inserted.</param>
        /// <returns></returns>
        public static string GetLocalization(string key, params string[] additionalValues)
        {
            // Return the localization of the advanced method.
            return GetLocalization(key, FallbackMode.ReturnDefaultLanguage, currentLanguage, additionalValues);
        }

        /// <summary>
        /// Gets all languages present in library.
        /// </summary>
        public static string[] GetLanguages()
        {
            return localizationTable.Keys.ToArray();
        }

        /// <summary>
        /// Load a localization file from the harddrive with a defined localization code.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="localizationCode">The localization code, e.g.: "en_US", "en_UK".</param>
        private static void LoadLocalizationFile(string path, string localizationCode)
        {
            try
            {
                localizationTable[localizationCode] = new Dictionary<string, string>();
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string[] keyValuePair = line.Split(new char[] { '=' }, 2);
                    if (keyValuePair.Length != 2)
                    {
                        Debug.LogErrorFormat("Invalid format of localization string. Actual {0}", line);
                        continue;
                    }

                    localizationTable[localizationCode].Add(keyValuePair[0], keyValuePair[1]);
                }
            }
            catch (FileNotFoundException exception)
            {
                Debug.LogError(new Exception(string.Format("There is no localization file for {0}", localizationCode), exception));
            }
        }

        /// <summary>
        /// Returns the localization for the given key, or the key itself, if no translation exists.
        /// </summary>
        private static string GetLocalization(string key, FallbackMode fallbackMode, string language, params string[] additionalValues)
        {
            string value;
            if (localizationTable.ContainsKey(language) && localizationTable[language].TryGetValue(key, out value))
            {
                return string.Format(value, additionalValues);
            }

            if (!missingKeysLogged.Contains(key))
            {
                missingKeysLogged.Add(key);
                Debug.LogWarning(string.Format("Translation for {0} in {1} language failed: Key not in dictionary.", key, language));
            }

            switch (fallbackMode)
            {
            case FallbackMode.ReturnKey:
                return additionalValues != null && additionalValues.Length >= 1 ? key + " " + additionalValues[0] : key;
            case FallbackMode.ReturnDefaultLanguage:
                return GetLocalization(key, FallbackMode.ReturnKey, defaultLanguage, additionalValues);
            default:
                return string.Empty;
            }
        }
    }
}
