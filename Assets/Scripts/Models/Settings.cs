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
using System.Xml;
using UnityEngine;

public static class Settings
{
    // Settings.xml file that is created if none exists.
    private const string FallbackSettingsXml = @"
<Settings>
  <worldWidth>101</worldWidth>
  <worldHeight>101</worldHeight>
  <localization>en_US</localization>
  <DialogBoxSettings_fullScreenToggle>True</DialogBoxSettings_fullScreenToggle>
  <DialogBoxSettings_qualityDropdown>2</DialogBoxSettings_qualityDropdown>
  <DialogBoxSettings_vSyncDropdown>0</DialogBoxSettings_vSyncDropdown>
  <DialogBoxSettings_resolutionDropdown>0</DialogBoxSettings_resolutionDropdown>
  <DialogBoxSettings_musicVolume>1</DialogBoxSettings_musicVolume>
  <DialogBoxSettingsDevConsole_performanceGroup>1</DialogBoxSettingsDevConsole_performanceGroup>
  <DialogBoxSettingsDevConsole_consoleInputGroup>0</DialogBoxSettingsDevConsole_consoleInputGroup>
  <DialogBoxSettingsDevConsole_developerModeToggle>False</DialogBoxSettingsDevConsole_developerModeToggle>
  <DialogBoxSettingsDevConsole_devConsoleToggle>True</DialogBoxSettingsDevConsole_devConsoleToggle>
  <DialogBoxSettingsDevConsole_consoleFontSize>15</DialogBoxSettingsDevConsole_consoleFontSize>
  <DialogBoxSettingsDevConsole_timeStampToggle>True</DialogBoxSettingsDevConsole_timeStampToggle>
  <ZoomLerp>10</ZoomLerp>
  <ZoomSensitivity>3</ZoomSensitivity>
  <AutosaveInterval>10</AutosaveInterval>
  <AutosaveFiles>5</AutosaveFiles>
</Settings>
";

    private static readonly string DefaultSettingsFilePath = System.IO.Path.Combine(
        Application.streamingAssetsPath, System.IO.Path.Combine("Settings", "Settings.xml"));

    private static Dictionary<string, string> settingsDict;

    private static string userSettingsFilePath = System.IO.Path.Combine(
        Application.persistentDataPath, "Settings.xml");

    static Settings()
    {
        LoadSettings();
    }

    public static string GetSettingWithOverwrite(string key, string defaultValue)
    {
        if (settingsDict == null)
        {
            Debug.ULogErrorChannel("Settings", "Settings Dictionary was not loaded!");
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
            Debug.ULogErrorChannel("Settings", "Settings Dictionary was not loaded!");
            return;
        }

        // If we already have a setting with the same name,
        if (settingsDict.ContainsKey(key))
        {
            // update the setting.
            settingsDict.Remove(key);
            settingsDict.Add(key, value);
            Debug.ULogChannel("Settings", "Updated setting : " + key + " to value of " + value);
        }
        else
        {
            // add a new setting to the dict.
            settingsDict.Add(key, value);
            Debug.ULogChannel("Settings", "Created new setting : " + key + " to value of " + value);
        }
    }

    public static T GetSetting<T>(string key, T defaultValue)
        where T : IConvertible
    {
        if (settingsDict == null)
        {
            Debug.ULogErrorChannel("Settings", "Settings Dictionary was not loaded!");
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
                Debug.ULogErrorChannel("Settings", "Exception {0} whyle trying to convert {1} to type {2}", exception.Message, value, typeof(T));
                return defaultValue;
            }
        }

        Debug.ULogWarningChannel("Settings", "Attempted to access a setting that was not loaded from Settings.xml:\t" + key);
        return defaultValue;
    }

    public static void SaveSettings()
    {
        Debug.ULogChannel("Settings", "Settings have changed, so there are settings to save!");

        // Create an xml document.
        XmlDocument doc = new XmlDocument();

        // Create main settings node.
        XmlNode settingsNode = doc.CreateElement("Settings");

        foreach (KeyValuePair<string, string> pair in settingsDict)
        {
            // Create a new element for each pair in the dict.
            XmlElement settingElement = doc.CreateElement(pair.Key);
            settingElement.InnerText = pair.Value;
            Debug.ULogChannel("Settings", "Saving setting :: " + pair.Key + " : " + pair.Value);

            // Add this element inside the Settings element.
            settingsNode.AppendChild(settingElement);
        }

        // Apend Settings node to the document.
        doc.AppendChild(settingsNode);

        // Save the document.
        try
        {
            doc.Save(userSettingsFilePath);
        }
        catch (Exception e)
        {
            Debug.ULogWarningChannel("Settings", "Settings could not be saved to " + userSettingsFilePath);
            Debug.ULogWarningChannel("Settings", e.Message);
        }
    }

    public static void LoadSettings()
    {
        // Initialize the settings dict.
        settingsDict = new Dictionary<string, string>();
        string settingsXmlText;

        // Load the settings XML file.
        // First try the user's private settings file in userSettingsFilePath.
        // If that doesn't work fall back to defaultSettingsFilePath.
        // If that doesn't work fall back to the hard coded furnitureXmlText above.
        if (System.IO.File.Exists(userSettingsFilePath) == false)
        {
            Debug.ULogChannel("Settings", "User settings file could not be found at '" + userSettingsFilePath + "'. Falling back to defaults.");

            settingsXmlText = DefaultSettingsXmlFallback();
        }
        else
        {
            try
            {
                settingsXmlText = System.IO.File.ReadAllText(userSettingsFilePath);
            }
            catch (Exception e)
            {
                Debug.ULogWarningChannel("Settings", "User settings file could not be found at '" + userSettingsFilePath + "'. Falling back to defaults.");
                Debug.ULogWarningChannel("Settings", e.Message);

                settingsXmlText = DefaultSettingsXmlFallback();
            }
        }

        // Create an xml document from the loaded string.
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(settingsXmlText);
        Debug.ULogChannel("Settings", "Loaded settings");
        Debug.ULogChannel("Settings", doc.InnerText);

        // Get the Settings node. Its children are the individual settings.
        XmlNode settingsNode = doc.GetElementsByTagName("Settings").Item(0);
        XmlNodeList settingNodes = settingsNode.ChildNodes;
        Debug.ULogChannel("Settings", settingNodes.Count + " settings loaded");

        // Loop for each setting
        foreach (XmlNode node in settingNodes)
        {
            if (node != null)
            {
                // and add setting to the settings dict.
                settingsDict.Add(node.Name, node.InnerText);
                Debug.ULogChannel("Settings", "Setting loaded :: " + node.Name + " : " + node.InnerText);
            }
        }
    }

    private static string DefaultSettingsXmlFallback()
    {
        string settingsXml = FallbackSettingsXml;

        if (System.IO.File.Exists(DefaultSettingsFilePath) == false)
        {
            Debug.ULogWarningChannel("Settings", "Default settings file could not be found at '" + DefaultSettingsFilePath + "'. Falling back to Settings.cs defaults.");

            try
            {
                System.IO.File.WriteAllText(DefaultSettingsFilePath, FallbackSettingsXml);
            }
            catch (Exception e)
            {
                Debug.ULogWarningChannel("Settings", "Default settings file could not be created at '" + DefaultSettingsFilePath + "'.");
                Debug.ULogWarningChannel("Settings", e.Message);
            }
        }
        else
        {
            try
            {
                settingsXml = System.IO.File.ReadAllText(DefaultSettingsFilePath);
            }
            catch (Exception e)
            {
                Debug.ULogWarningChannel("Settings", "Settings file at '" + DefaultSettingsFilePath + "' could not be read. Falling back to Settings.cs defaults.");
                Debug.ULogWarningChannel("Settings", e.Message);
            }
        }

        return settingsXml;
    }
}
