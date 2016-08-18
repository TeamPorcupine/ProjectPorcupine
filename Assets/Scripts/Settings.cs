using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

public static class Settings {

    private static Dictionary<string,string> settingsDict;


	public static string getSetting( string key )
	{

        // if we haven't already loaded our settings do it now
		if (settingsDict == null) 
		{
			loadSettings ();
		}


        string value;

        // attempt to get the requested setting, if it is not found log a warning and return the null string
        if (settingsDict.TryGetValue(key,out value) == false)
        {
            Debug.LogWarning("Atempted to access a setting that was not loaded from Settings.xml :\t" + key);
        }

        return value;
	}

	private static void loadSettings()
	{
        // initilize the settings dict
        settingsDict = new Dictionary<string, string>();

        // get file path of Settings.xml and load the text
		string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Settings.xml");
		string furnitureXmlText = System.IO.File.ReadAllText(filePath);

        // create an xml document from Settings.xml
        XmlDocument xDoc = new XmlDocument();
        xDoc.LoadXml(furnitureXmlText);
        Debug.Log("Loaded settings from : \t" + filePath );

        // get the Settings node , it's children are the individual settings
        XmlNode settingsNode = xDoc.GetElementsByTagName("Settings").Item(0);
        XmlNodeList settingNodes = settingsNode.ChildNodes;
        Debug.Log(settingNodes.Count+" settings loaded");

        //loop for each setting
        foreach (XmlNode node in settingNodes)
        {
            if (node != null)
            {
                // add setting to the settings dict
                settingsDict.Add(node.Name, node.InnerText);

                // commented out to avoid console spam with large Settings.xml files
                // Debug.Log( node.Name + " : " + node.InnerText );
            }
        }
       
	}

}
