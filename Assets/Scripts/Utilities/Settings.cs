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
            Logger.LogWarning("Atempted to access a setting that was not loaded from Settings.xml :\t" + key);
            return null;
        }

        return value;
    }
    public static void setSetting(string key, object obj){
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
            Logger.Log("updated setting : " + key + " to value of " + value);
        }
        else 
        {
            // add a new setting to the dict
            settingsDict.Add(key, value);
            Logger.Log("created new setting : " + key + " to value of " + value);
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
            Logger.LogVerbose(pair.Key + " : " + pair.Value);

            // add this element inside the Settings element
            settingsNode.AppendChild(settingElement);
        }

        // apend Settings node to the document
        xDoc.AppendChild(settingsNode);

        // get file path of Settings.xml 
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, settingsFilePath);

        // save document
        xDoc.Save(filePath);
    }

    private static void loadSettings()
    {
        // initilize the settings dict
        settingsDict = new Dictionary<string, string>();

        // get file path of Settings.xml and load the text
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, settingsFilePath);
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);

        // create an xml document from Settings.xml
        XmlDocument xDoc = new XmlDocument();
        xDoc.LoadXml(furnitureXmlText);
        Logger.Log("Loaded settings from : \t" + filePath);
        Logger.LogVerbose(xDoc.InnerText);

        // get the Settings node , it's children are the individual settings
        XmlNode settingsNode = xDoc.GetElementsByTagName("Settings").Item(0);
        XmlNodeList settingNodes = settingsNode.ChildNodes;
        Logger.Log(settingNodes.Count + " settings loaded");

        // loop for each setting
        foreach (XmlNode node in settingNodes)
        {
            if (node != null)
            {
                // add setting to the settings dict
                settingsDict.Add(node.Name, node.InnerText);
                Logger.LogVerbose(node.Name + " : " + node.InnerText);
            }
        }
    }

    public static int getSettingAsInt(string key, int defaultValue){

        // Atempt to get the string value from the dict
        string s = getSetting(key, defaultValue.ToString());

        int i;

        // Atempt to parse the string, if the parse failed return the default value
        if (int.TryParse(s, out i) == false)
        {
            Logger.LogWarning("Could not parse setting " + key + " of value " + s + " to type int");
            return defaultValue;
        }
        else
        {
            // we managed to get the setting we wanted
            return i;
        }
    }

    public static float getSettingAsFloat(string key, float defaultValue){

        // Atempt to get the string value from the dict
        string s = getSetting(key, defaultValue.ToString());

        float f;

        // Atempt to parse the string, if the parse failed return the default value
        if (float.TryParse(s, out f) == false)
        {
            Logger.LogWarning("Could not parse setting " + key + " of value " + s + " to type float");
            return defaultValue;
        }
        else
        {
            // we managed to get the setting we wanted
            return f;
        }
    }

    public static bool getSettingAsBool(string key, bool defaultValue){

        // Atempt to get the string value from the dict
        string s = getSetting(key, defaultValue.ToString());

        bool b;

        // Atempt to parse the string, if the parse failed return the default value
        if (bool.TryParse(s, out b) == false)
        {
            Logger.LogWarning("Could not parse setting " + key + " of value " + s + " to type bool");
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
