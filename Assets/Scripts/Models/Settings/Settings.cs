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
using Newtonsoft.Json;
using UnityEngine;

public static class Settings
{
    // Settings.xml file that is created if none exists.
    private const string FallbackSettingJson = @"
{
	'worldWidth' : 101,
	'worldHeight' : 101,
	'localization' : 'en_US',
	'DialogBoxSettings_fullScreenToggle' : true,
	'DialogBoxSettings_qualityDropdown' : 2,
	'DialogBoxSettings_vSyncDropdown' : 0,
	'DialogBoxSettings_resolutionDropdown' : 0,
	'DialogBoxSettings_musicVolume' : 1,
	'DialogBoxSettingsDevConsole_performanceGroup' : 1,
	'DialogBoxSettingsDevConsole_developerModeToggle' : false,
	'DialogBoxSettingsDevConsole_devConsoleToggle' : true,
	'DialogBoxSettingsDevConsole_consoleFontSize' : 15,
	'DialogBoxSettingsDevConsole_timeStampToggle' : true,
    'DialogBoxSettingsDevConsole_scrollSensitivity': 6,
	'ZoomLerp' : 10,
	'ZoomSensitivity' : 3,
	'AutosaveInterval' : 10,
	'AutosaveFiles' : 5, 
}
";

    private static readonly string DefaultSettingsFilePath = System.IO.Path.Combine(
        Application.streamingAssetsPath, System.IO.Path.Combine("Settings", "Settings.json"));

    private static Dictionary<string, string> settingsDict;

    private static string userSettingsFilePath = System.IO.Path.Combine(
        Application.persistentDataPath, "Settings.json");

    static Settings()
    {
        LoadSettings();
    }

    public static string GetSettingWithOverwrite(string key, string defaultValue)
    {
        if (settingsDict == null)
        {
            UnityDebugger.Debugger.LogError("Settings", "Settings Dictionary was not loaded!");
            return defaultValue;
        }

        string setting = GetSetting(key, defaultValue);
        if (setting != null)
        {
            return setting;
        }

        settingsDict.Add(key, defaultValue);

        SaveSettings();

        return defaultValue;
    }

    public static void SetSetting(string key, object obj)
    {
        SetSetting(key, obj.ToString());
    }

    public static void SetSetting(string key, string value)
    {
        if (settingsDict == null)
        {
            UnityDebugger.Debugger.LogError("Settings", "Settings Dictionary was not loaded!");
            return;
        }

        // If we already have a setting with the same name,
        if (settingsDict.ContainsKey(key))
        {
            // update the setting.
            settingsDict.Remove(key);
            settingsDict.Add(key, value);
            UnityDebugger.Debugger.Log("Settings", "Updated setting : " + key + " to value of " + value);
        }
        else
        {
            // add a new setting to the dict.
            settingsDict.Add(key, value);
            UnityDebugger.Debugger.Log("Settings", "Created new setting : " + key + " to value of " + value);
        }
    }

    public static T GetSetting<T>(string key, T defaultValue)
        where T : IConvertible
    {
        if (settingsDict == null)
        {
            UnityDebugger.Debugger.LogError("Settings", "Settings Dictionary was not loaded!");
            return defaultValue;
        }

        string value;
        if (settingsDict.TryGetValue(key, out value))
        {
            try
            {
                T converted = (T)Convert.ChangeType(value, typeof(T));
                return converted;
            }
            catch (Exception exception)
            {
                UnityDebugger.Debugger.LogErrorFormat("Settings", "Exception {0} whyle trying to convert {1} to type {2}", exception.Message, value, typeof(T));
                return defaultValue;
            }
        }

        UnityDebugger.Debugger.LogWarning("Settings", "Attempted to access a setting that was not loaded from Settings.json:\t" + key);
        return defaultValue;
    }

    public static void SaveSettings()
    {
        UnityDebugger.Debugger.Log("Settings", "Settings have changed, so there are settings to save!");

        string jsonData = JsonConvert.SerializeObject(settingsDict, Newtonsoft.Json.Formatting.Indented);
        UnityDebugger.Debugger.Log("Settings", "Saving settings :: " + jsonData);

        // Save the document.
        try
        {
            using (StreamWriter writer = new StreamWriter(userSettingsFilePath))
            {
                writer.WriteLine(jsonData);
            }
        }
        catch (Exception e)
        {
            UnityDebugger.Debugger.LogWarning("Settings", "Settings could not be saved to " + userSettingsFilePath);
            UnityDebugger.Debugger.LogWarning("Settings", e.Message);
        }
    }

    public static void LoadSettings()
    {
        string settingsJsonText;

        // Load the settings Json file.
        // First try the user's private settings file in userSettingsFilePath.
        // If that doesn't work fall back to defaultSettingsFilePath.
        // If that doesn't work fall back to the hard coded FallbackSettingJson above.
        if (System.IO.File.Exists(userSettingsFilePath) == false)
        {
            UnityDebugger.Debugger.Log("Settings", "User settings file could not be found at '" + userSettingsFilePath + "'. Falling back to defaults.");

            settingsJsonText = DefaultSettingsJsonFallback();
        }
        else
        {
            try
            {
                settingsJsonText = System.IO.File.ReadAllText(userSettingsFilePath);
            }
            catch (Exception e)
            {
                UnityDebugger.Debugger.LogWarning("Settings", "User settings file could not be found at '" + userSettingsFilePath + "'. Falling back to defaults.");
                UnityDebugger.Debugger.LogWarning("Settings", e.Message);

                settingsJsonText = DefaultSettingsJsonFallback();
            }
        }

        settingsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsJsonText);
    }

    private static string DefaultSettingsJsonFallback()
    {
        string settingsJson = FallbackSettingJson;

        if (System.IO.File.Exists(DefaultSettingsFilePath) == false)
        {
            UnityDebugger.Debugger.LogWarning("Settings", "Default settings file could not be found at '" + DefaultSettingsFilePath + "'. Falling back to Settings.cs defaults.");

            try
            {
                System.IO.File.WriteAllText(DefaultSettingsFilePath, FallbackSettingJson);
            }
            catch (Exception e)
            {
                UnityDebugger.Debugger.LogWarning("Settings", "Default settings file could not be created at '" + DefaultSettingsFilePath + "'.");
                UnityDebugger.Debugger.LogWarning("Settings", e.Message);
            }
        }
        else
        {
            try
            {
                settingsJson = System.IO.File.ReadAllText(DefaultSettingsFilePath);
            }
            catch (Exception e)
            {
                UnityDebugger.Debugger.LogWarning("Settings", "Settings file at '" + DefaultSettingsFilePath + "' could not be read. Falling back to Settings.cs defaults.");
                UnityDebugger.Debugger.LogWarning("Settings", e.Message);
            }
        }

        return settingsJson;
    }
}
