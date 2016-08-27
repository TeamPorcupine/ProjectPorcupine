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
    private static string settingsFilePath = System.IO.Path.Combine("Settings", "Settings.xml");

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
        // if we haven't already loaded our settings do it now
        if (settingsDict == null) 
        {
            LoadSettings();
        }

        // If we already have a setting with the same name.
        if (settingsDict.ContainsKey(key))
        {
            // Update the setting.
            settingsDict.Remove(key); 
            settingsDict.Add(key, value);
            Debug.ULogChannel("Settings", "Updated setting : " + key + " to value of " + value);
        }
        else 
        {
            // Add a new setting to the dict.
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

        // Attempt to parse the string, if the parse failed return the default value.
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

        // Attempt to parse the string, if the parse failed return the default value.
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

        // Attempt to parse the string, if the parse failed return the default value.
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

        // Attempt to get the requested setting, if it is not found log a warning and return the null string.
        if (settingsDict.TryGetValue(key, out value) == false)
        {
            Debug.ULogWarningChannel("Settings", "Attempted to access a setting that was not loaded from Settings.xml :\t" + key);
            return null;
        }

        return value;
    }

    private static void SaveSettings()
    {
        // Create an xml document.
        XmlDocument xmlDocument = new XmlDocument();

        // Create main settings node.
        XmlNode settingsNode = xmlDocument.CreateElement("Settings");

        foreach (KeyValuePair<string, string> pair in settingsDict)
        {
            // Create a new element for each pair in the dict.
            XmlElement settingElement = xmlDocument.CreateElement(pair.Key);
            settingElement.InnerText = pair.Value;
            Debug.ULogChannel("Settings", pair.Key + " : " + pair.Value);

            // Add this element inside the Settings element.
            settingsNode.AppendChild(settingElement);
        }

        // Append Settings node to the document.
        xmlDocument.AppendChild(settingsNode);

        // Get file path of Settings.xml .
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, settingsFilePath);

        // Save document.
        xmlDocument.Save(filePath);
    }

    private static void LoadSettings()
    {
        // Initilize the settings dict.
        settingsDict = new Dictionary<string, string>();

        // Get file path of Settings.xml and load the text.
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, settingsFilePath);
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);

        // Create an xml document from Settings.xml.
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(furnitureXmlText);
        Debug.ULogChannel("Settings", "Loaded settings from : \t" + filePath);

        // Uber Logger doesn't handle multilines.
        Debug.Log(xmlDocument.InnerText);

        // Get the Settings node , it's children are the individual settings.
        XmlNode settingsNode = xmlDocument.GetElementsByTagName("Settings").Item(0);
        XmlNodeList settingNodes = settingsNode.ChildNodes;
        Debug.ULogChannel("Settings", settingNodes.Count + " settings loaded");

        // Loop for each setting.
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

    // TODO : make generic getSettingAs that infers return type from default value
}
