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
        // if we haven't already loaded our settings do it now
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

    public static int GetSettingAsInt(string key, int defaultValue)
    {
        // Atempt to get the string value from the dict
        string s = GetSetting(key, defaultValue.ToString());

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

    public static float GetSettingAsFloat(string key, float defaultValue)
    {
        // Atempt to get the string value from the dict
        string s = GetSetting(key, defaultValue.ToString());

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

    public static bool GetSettingAsBool(string key, bool defaultValue)
    {
        // Atempt to get the string value from the dict
        string s = GetSetting(key, defaultValue.ToString());

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

        SaveSettings();
    }

    private static string GetSetting(string key)
    {
        // if we haven't already loaded our settings do it now
        if (settingsDict == null)
        {
            LoadSettings();
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

    private static void LoadSettings()
    {
        // initilize the settings dict
        settingsDict = new Dictionary<string, string>();

        // get file path of Settings.xml and load the text
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, settingsFilePath);
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);

        // create an xml document from Settings.xml
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(furnitureXmlText);
        Debug.ULogChannel("Settings", "Loaded settings from : \t" + filePath);
        Debug.Log(xmlDoc.InnerText); // Uber Logger doesn't handle multilines.

        // get the Settings node , it's children are the individual settings
        XmlNode settingsNode = xmlDoc.GetElementsByTagName("Settings").Item(0);
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

    private static void SaveSettings()
    {
        // create an xml document
        XmlDocument xmlDoc = new XmlDocument();

        // create main settings node
        XmlNode settingsNode = xmlDoc.CreateElement("Settings");

        foreach (KeyValuePair<string, string> pair in settingsDict)
        {
            // create a new element for each pair in the dict
            XmlElement settingElement = xmlDoc.CreateElement(pair.Key);
            settingElement.InnerText = pair.Value;
            Debug.ULogChannel("Settings", pair.Key + " : " + pair.Value);

            // add this element inside the Settings element
            settingsNode.AppendChild(settingElement);
        }

        // apend Settings node to the document
        xmlDoc.AppendChild(settingsNode);

        // get file path of Settings.xml 
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, settingsFilePath);

        // save document
        xmlDoc.Save(filePath);
    }

    // TODO : make generic GetSettingAs that infers return type from default value
}
