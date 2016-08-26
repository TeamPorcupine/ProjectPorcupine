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
    private static Dictionary<string, string> settingsDict;
    private static string userSettingsFilePath = System.IO.Path.Combine(
        Application.persistentDataPath, "Settings.xml");
    private static string defaultSettingsFilePath = System.IO.Path.Combine(
        Application.streamingAssetsPath, System.IO.Path.Combine("Settings", "Settings.xml"));

    // Settings.xml file that is created if none exists.
    private const string fallbackSettingsXML = @"
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

    public static string getSetting(string key , string defaultValue)
    {
        // if we haven't already loaded our settings do it now
        if (settingsDict == null) 
        {
            loadSettings();
        }

        string s = getSetting(key);
        if (s == null)
        {
            settingsDict.Add(key, defaultValue);
            saveSettings();
            return defaultValue;
        }
        else
        {
            return s;
        }
    }

    private static string getSetting(string key )
    {
        // if we haven't already loaded our settings do it now
        if (settingsDict == null) 
        {
            loadSettings();
        }

        string value;

        // attempt to get the requested setting, if it is not found log a warning and return the null string
        if (settingsDict.TryGetValue(key, out value) == false)
        {
            Debug.ULogWarningChannel("Settings", "Attempted to access a setting that was not loaded from Settings.xml :\t" + key);
            return null;
        }

        return value;
    }

    public static void setSetting(string key, object obj)
    {
        setSetting(key, obj.ToString());
    }

    public static void setSetting(string key, string value)
    {
        // if we haven't already loaded our settings do it now
        if (settingsDict == null) 
        {
            loadSettings();
        }

        // if we already have a setting with the same name
        if (settingsDict.ContainsKey(key))
        {
            // update the setting
            settingsDict.Remove(key); 
            settingsDict.Add(key, value);
            Debug.ULogChannel("Settings", "Updated setting : " + key + " to value of " + value);
        }
        else 
        {
            // add a new setting to the dict
            settingsDict.Add(key, value);
            Debug.ULogChannel("Settings", "Created new setting : " + key + " to value of " + value);
        }

        saveSettings();
    }

    private static void saveSettings()
    {
        // create an xml document
        XmlDocument xDoc = new XmlDocument();

        // create main settings node
        XmlNode settingsNode = xDoc.CreateElement("Settings");

        foreach (KeyValuePair<string, string> pair in settingsDict)
        {
            // create a new element for each pair in the dict
            XmlElement settingElement = xDoc.CreateElement(pair.Key);
            settingElement.InnerText = pair.Value;
            Debug.ULogChannel("Settings", pair.Key + " : " + pair.Value);

            // add this element inside the Settings element
            settingsNode.AppendChild(settingElement);
        }

        // apend Settings node to the document
        xDoc.AppendChild(settingsNode);

        // save document
        try
        {
            xDoc.Save(userSettingsFilePath);
        }
        catch (Exception e)
        {
            Debug.ULogWarningChannel("Settings", "Settings could not be saved to " + userSettingsFilePath);
            Debug.ULogWarningChannel("Settings", e.Message);
        }
    }

    private static void loadSettings()
    {
        // initilize the settings dict
        settingsDict = new Dictionary<string, string>();
        string furnitureXmlText;

        // load the settings XML file
        // first try the user's private settings file in userSettingsFilePath
        // if that doesn't work fall back to defaultSettingsFilePath
        // if that doesn't work fall back to the hard coded furnitureXmlText above
        if(System.IO.File.Exists(userSettingsFilePath) == false)
        {
            Debug.ULogWarningChannel("Settings", "User settings file could not be found at '" + userSettingsFilePath + "'. Falling back to defaults.");

            furnitureXmlText = defaultSettingsXMLFallback();
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

                furnitureXmlText = defaultSettingsXMLFallback();
            }
        }

        // create an xml document from Settings.xml
        XmlDocument xDoc = new XmlDocument();
        xDoc.LoadXml(furnitureXmlText);
        Debug.ULogChannel("Settings", "Loaded settings");
        Debug.Log(xDoc.InnerText); // Uber Logger doesn't handle multilines.

        // get the Settings node , it's children are the individual settings
        XmlNode settingsNode = xDoc.GetElementsByTagName("Settings").Item(0);
        XmlNodeList settingNodes = settingsNode.ChildNodes;
        Debug.ULogChannel("Settings", settingNodes.Count + " settings loaded");

        // loop for each setting
        foreach (XmlNode node in settingNodes)
        {
            if (node != null)
            {
                // add setting to the settings dict
                settingsDict.Add(node.Name, node.InnerText);
                Debug.ULogChannel("Settings", node.Name + " : " + node.InnerText);
            }
        }
    }

    private static string defaultSettingsXMLFallback()
    {
        string furnitureXmlText = fallbackSettingsXML;

        if (System.IO.File.Exists(defaultSettingsFilePath) == false)
        {
            Debug.ULogWarningChannel("Settings", "Default settings file could not be found at '" + defaultSettingsFilePath + "'. Falling back to Settings.cs defaults.");

            try
            {
                System.IO.File.WriteAllText(defaultSettingsFilePath, fallbackSettingsXML);
            }
            catch (Exception e)
            {
                Debug.ULogWarningChannel("Settings", "Default settings file could not be created at '" + defaultSettingsFilePath + "'.");
                Debug.ULogWarningChannel("Settings", e.Message);
            }
        }
        else
        {
            try
            {
                furnitureXmlText = System.IO.File.ReadAllText(defaultSettingsFilePath);
            }
            catch (Exception e)
            {
                Debug.ULogWarningChannel("Settings", "Settings file at '" + defaultSettingsFilePath + "' could not be read. Falling back to Settings.cs defaults.");
                Debug.ULogWarningChannel("Settings", e.Message);
            }
        }
        return furnitureXmlText;
    }


    public static int getSettingAsInt(string key, int defaultValue)
    {
        // Atempt to get the string value from the dict
        string s = getSetting(key, defaultValue.ToString());

        int i;

        // Atempt to parse the string, if the parse failed return the default value
        if (int.TryParse(s, out i) == false)
        {
            Debug.ULogWarningChannel("Settings", "Could not parse setting " + key + " of value " + s + " to type int");
            return defaultValue;
        }
        else
        {
            // we managed to get the setting we wanted
            return i;
        }
    }

    public static float getSettingAsFloat(string key, float defaultValue)
    {
        // Atempt to get the string value from the dict
        string s = getSetting(key, defaultValue.ToString());

        float f;

        // Atempt to parse the string, if the parse failed return the default value
        if (float.TryParse(s, out f) == false)
        {
            Debug.ULogWarningChannel("Settings", "Could not parse setting " + key + " of value " + s + " to type float");
            return defaultValue;
        }
        else
        {
            // we managed to get the setting we wanted
            return f;
        }
    }

    public static bool getSettingAsBool(string key, bool defaultValue)
    {
        // Atempt to get the string value from the dict
        string s = getSetting(key, defaultValue.ToString());

        bool b;

        // Atempt to parse the string, if the parse failed return the default value
        if (bool.TryParse(s, out b) == false)
        {
            Debug.ULogWarningChannel("Settings", "Could not parse setting " + key + " of value " + s + " to type bool");
            return defaultValue;
        }
        else
        {
            // we managed to get the setting we wanted
            return b;
        }
    }
        
    // TODO : make generic getSettingAs that infers return type from default value
}
