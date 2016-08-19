using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace ProjectPorcupine.Localization
{
    /**
     * <summary>
     * The central class containing localization information.
     * </summary>
     */
    public static class LocalizationTable
    {
        public enum FallbackMode
        {
            ReturnKey, ReturnEmpty, ReturnEnglish
        }

        //The dictionary that stores all the localization values.
        static Dictionary<string, string> localizationTable = new Dictionary<string, string>();

        //The current language. This will be automatically be set by the LocalizationLoader.
        //Default is English.
        public static string currentLanguage = "en_US";

        //Used by the LocalizationLoader to ensure that the localization files are only loaded once.
        public static bool initialized = false;

        //List with all languages.
        static List<string> registeredLanguages = new List<string>();

        /**
         * <summary>
         * Load a localization file from the harddrive.
         * 
         * <param name="path">
         * The path to the file.
         * </param>
         * </summary>
         */
        public static void LoadLocalizationFile(string path)
        {
            string localizationCode = Path.GetFileNameWithoutExtension(path);
            LoadLocalizationFile(path, localizationCode);
        }

        /**
         * <summary>
         * Load a localization file from the harddrive with a defined localization code.
         * 
         * <para>
         * string path: The path to the file.
         * </para>
         * <para>
         * string localizationCode: The localization code, e.g.: "en_US", "en_UK"
         * </para>
         * </summary>
         */
        public static void LoadLocalizationFile(string path, string localizationCode)
        {
            //Read the contents of the file. This might throw an exception!
            string[] lines = File.ReadAllLines(path);

            //Create an empty char array outside the foreach loop, for optimization reasons.
            //This is used as a storage for the chars in the line.
            char[] chars;

            //The key that the loop assembled.
            string currentKey = "";
            //The value that the loop assembled.
            string currentValue = "";
            //Is the loop already done with figuring out the key?
            bool searchingValue = false;

            foreach(string line in lines)
            {
                //Reuse the array (for reducing RAM usage).
                chars = line.ToCharArray();

                //Set searching value to false.
                searchingValue = false;
                //Set the key to an empty string.
                currentKey = "";
                //Set the value to an empty string.
                currentValue = "";

                //Go through each char contained in this line.
                foreach (char c in chars)
                {
                    if (!searchingValue) //Check if the loop is searching for a value.
                    {
                        if (c != '=') //Check if the current char is an '=', if not, add the char to the key.
                        {
                            //Add the char to the key.
                            currentKey += c;
                        }
                        else
                        {
                            //The char is an '=', set searchingValue to true and ignore the current char.
                            searchingValue = true;
                        }
                    }
                    else
                    {
                        //The loop is searching for a value. Add the current char, regardless of what it is.
                        currentValue += c;
                    }
                }

                //Add the new key+value to the localization table.
                localizationTable.Add(localizationCode + "_" + currentKey, currentValue);
            }

            registeredLanguages.Add(localizationCode);
        }

        /**
         * <summary>
         * Returns the localization for the given key, or the key itself, if no translation exists.
         * </summary>
         * 
         * <para>
         * string key: The key that should be searched for.
         * </para>
         * 
         * <para>
         * params string[] additionalValues: The values that should be inserted.
         * </para>
         */
        public static string GetLocalization(string key, params string[] additionalValues)
        {
            //Return the localization of the advanced method.
            return GetLocalization(key, FallbackMode.ReturnEnglish, currentLanguage, additionalValues);
        }

        /**
         * <summary>
         * Returns the localization for the given key, or the key itself, if no translation exists.
         * </summary>
         */
        public static string GetLocalization(string key, FallbackMode fallbackMode, string language, params string[] additionalValues)
        {
            try //Use a try-catch statement, since this operation might throw a KeyNotFoundException.
            {
                //Ideally, return the correct value.
                return string.Format(localizationTable[language + "_" + key], additionalValues);
            }
            catch
            {
#if UNITY_EDITOR //TODO: Think if #if is a good idea or not.
                //Log a warning into the console if this operation fails.
                Debug.LogWarning("Translation for " + key + " failed: Key not in dictionary.");
#endif

                //Switch the fallback mode.
                switch (fallbackMode)
                {
                    case FallbackMode.ReturnKey:
                        if(additionalValues.Length >= 1)
                        {
                            return key + " " + additionalValues[0];
                        }
                        return key; //Just return the key.
                    case FallbackMode.ReturnEnglish: return GetLocalization(key, FallbackMode.ReturnKey, "en_US", additionalValues); //Return the english equivalent.
                    default: return ""; //Return an empty string.
                }
            }
        }

        public static string[] GetLanguages()
        {
            return registeredLanguages.ToArray();
        }
    }
}
