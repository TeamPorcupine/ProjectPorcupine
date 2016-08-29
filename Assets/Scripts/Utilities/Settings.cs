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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public static class Settings 
{
    // Settings.xml file that is created if none exists.
    private const string FallbackSettingsXML = @"
<Settings>
  <worldWidth>101</worldWidth>
  <worldHeight>101</worldHeight>
  <localization>en_US</localization>
  <DialogBoxSettings_langToggle>True</DialogBoxSettings_langToggle>
  <DialogBoxSettings_fpsToggle>True</DialogBoxSettings_fpsToggle>
  <DialogBoxSettings_fullScreenToggle>True</DialogBoxSettings_fullScreenToggle>
  <DialogBoxSettings_qualityDropdown>2</DialogBoxSettings_qualityDropdown>
  <DialogBoxSettings_vSyncDropdown>0</DialogBoxSettings_vSyncDropdown>
  <DialogBoxSettings_resolutionDropdown>0</DialogBoxSettings_resolutionDropdown>
  <DialogBoxSettings_aliasingDropdown>2</DialogBoxSettings_aliasingDropdown>
  <DialogBoxSettings_musicVolume>1</DialogBoxSettings_musicVolume>
  <ZoomLerp>10</ZoomLerp>
  <ZoomSensitivity>3</ZoomSensitivity>
</Settings>
";

    private static readonly string DefaultSettingsFilePath = System.IO.Path.Combine(
        Application.streamingAssetsPath, System.IO.Path.Combine("Settings", "Settings.xml"));
    
    private static Dictionary<string, string> settingsDict;

    private static string userSettingsFilePath = System.IO.Path.Combine(
        Application.persistentDataPath, "Settings.xml");
    
    public static string GetSetting(string key, string defaultValue)
    {
        // If we haven't already loaded our settings do it now.
        if (settingsDict == null) 
        {
            LoadSettings();
        }

        string s = GetSetting(key);
        if (s == null)
        {
            settingsDict.Add(key, defaultValue);
            SaveSettings();
            return defaultValue;
        }
        else
        {
            return s;
        }
    }

    public static void SetSetting(string key, object obj)
    {
        SetSetting(key, obj.ToString());
    }

    public static void SetSetting(string key, string value)
    {
        // If we haven't already loaded our settings do it now.
        if (settingsDict == null) 
        {
            LoadSettings();
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

        SaveSettings();
    }

    public static int GetSettingAsInt(string key, int defaultValue)
    {
        // Attempt to get the string value from the dict.
        string s = GetSetting(key, defaultValue.ToString());

        int i;

        // Attempt to parse the string. If the parse failed return the default value.
        if (int.TryParse(s, out i) == false)
        {
            Debug.ULogWarningChannel("Settings", "Could not parse setting " + key + " of value " + s + " to type int");
            return defaultValue;
        }
        else
        {
            // We managed to get the setting we wanted.
            return i;
        }
    }

    public static float GetSettingAsFloat(string key, float defaultValue)
    {
        // Attempt to get the string value from the dict.
        string s = GetSetting(key, defaultValue.ToString());

        float f;

        // Attempt to parse the string. If the parse failed return the default value.
        if (float.TryParse(s, out f) == false)
        {
            Debug.ULogWarningChannel("Settings", "Could not parse setting " + key + " of value " + s + " to type float");
            return defaultValue;
        }
        else
        {
            // We managed to get the setting we wanted.
            return f;
        }
    }

    public static bool GetSettingAsBool(string key, bool defaultValue)
    {
        // Attempt to get the string value from the dict.
        string s = GetSetting(key, defaultValue.ToString());

        bool b;

        // Attempt to parse the string. If the parse failed return the default value.
        if (bool.TryParse(s, out b) == false)
        {
            Debug.ULogWarningChannel("Settings", "Could not parse setting " + key + " of value " + s + " to type bool");
            return defaultValue;
        }
        else
        {
            // We managed to get the setting we wanted.
            return b;
        }
    }

    private static string GetSetting(string key)
    {
        // If we haven't already loaded our settings do it now.
        if (settingsDict == null) 
        {
            LoadSettings();
        }

        string value;

        // Attempt to get the requested setting.
        // If it is not found log a warning and return the null string.
        if (settingsDict.TryGetValue(key, out value) == false)
        {
            Debug.ULogWarningChannel("Settings", "Attempted to access a setting that was not loaded from Settings.xml:\t" + key);
            return null;
        }

        return value;
    }

    private static void SaveSettings()
    {
        // Create an xml document.
        XmlDocument doc = new XmlDocument();

        // Create main settings node.
        XmlNode settingsNode = doc.CreateElement("Settings");

        foreach (KeyValuePair<string, string> pair in settingsDict)
        {
            // Create a new element for each pair in the dict.
            XmlElement settingElement = doc.CreateElement(pair.Key);
            settingElement.InnerText = pair.Value;
            Debug.ULogChannel("Settings", pair.Key + " : " + pair.Value);

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

    private static void LoadSettings()
    {
        // Initialize the settings dict.
        settingsDict = new Dictionary<string, string>();
        string furnitureXmlText;

        // Load the settings XML file.
        // First try the user's private settings file in userSettingsFilePath.
        // If that doesn't work fall back to defaultSettingsFilePath.
        // If that doesn't work fall back to the hard coded furnitureXmlText above.
        if (System.IO.File.Exists(userSettingsFilePath) == false)
        {
            Debug.ULogChannel("Settings", "User settings file could not be found at '" + userSettingsFilePath + "'. Falling back to defaults.");

            furnitureXmlText = DefaultSettingsXMLFallback();
        }
        else
        {
            try
            {
                furnitureXmlText = System.IO.File.ReadAllText(userSettingsFilePath);
            }
            catch (Exception e)
            {
                Debug.ULogWarningChannel("Settings", "User settings file could not be found at '" + userSettingsFilePath + "'. Falling back to defaults.");
                Debug.ULogWarningChannel("Settings", e.Message);

                furnitureXmlText = DefaultSettingsXMLFallback();
            }
        }

        // Create an xml document from the loaded string.
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(furnitureXmlText);
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
                Debug.ULogChannel("Settings", node.Name + " : " + node.InnerText);
            }
        }
    }

    private static string DefaultSettingsXMLFallback()
    {
        string furnitureXmlText = FallbackSettingsXML;

        if (System.IO.File.Exists(DefaultSettingsFilePath) == false)
        {
            Debug.ULogWarningChannel("Settings", "Default settings file could not be found at '" + DefaultSettingsFilePath + "'. Falling back to Settings.cs defaults.");

            try
            {
                System.IO.File.WriteAllText(DefaultSettingsFilePath, FallbackSettingsXML);
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
                furnitureXmlText = System.IO.File.ReadAllText(DefaultSettingsFilePath);
            }
            catch (Exception e)
            {
                Debug.ULogWarningChannel("Settings", "Settings file at '" + DefaultSettingsFilePath + "' could not be read. Falling back to Settings.cs defaults.");
                Debug.ULogWarningChannel("Settings", e.Message);
            }
        }

        return furnitureXmlText;
    }

    // TODO: Make generic getSettingAs that infers return type from default value.
}
