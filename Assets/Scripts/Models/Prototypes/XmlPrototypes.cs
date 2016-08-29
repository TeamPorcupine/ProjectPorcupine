#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class XmlPrototypes<T> : BasePrototypes<T>
{
    protected string fileName;
    protected string listTag;
    protected string elementTag;

    public XmlPrototypes(string fileName, string listTag, string elementTag) : base()
    {
        this.fileName = fileName;
        this.listTag = listTag;
        this.elementTag = elementTag;

        LoadPrototypesFromFile();
    }

    /// <summary>
    /// Loads the prototypes from an xml file.
    /// </summary>
    public void LoadPrototypesFromFile()
    {
        string dataPath = Path.Combine(Application.streamingAssetsPath, "Data");
        string filePath = Path.Combine(dataPath, fileName);
        string xmlText  = File.ReadAllText(filePath);

        LoadPrototypesFromText(xmlText);
    }

    /// <summary>
    /// Loads the mod prototypes from an xml file.
    /// </summary>
    /// <param name="mods">Mods directories.</param>
    public void LoadModPrototypesFromFile(DirectoryInfo[] mods)
    {
        foreach (DirectoryInfo mod in mods)
        {
            string xmlModFile = Path.Combine(mod.FullName, fileName);
            if (File.Exists(xmlModFile))
            {
                string xmlModText = File.ReadAllText(xmlModFile);
                LoadPrototypesFromText(xmlModText);
            }
        }
    }

    /// <summary>
    /// Loads the prototypes from a text.
    /// </summary>
    /// <param name="xmlText">Xml text to parse.</param>
    public void LoadPrototypesFromText(string xmlText) 
    {
        XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

        if (reader.ReadToDescendant(listTag))
        {
            if (reader.ReadToDescendant(elementTag))
            {
                do
                {
                    LoadPrototype(reader);
                }
                while (reader.ReadToNextSibling(elementTag));
            }
            else
            {
                Debug.ULogErrorChannel("XmlPrototypes", "The furniture prototype definition file doesn't have any '" + elementTag + "' elements.");
            }
        }
        else
        {
            Debug.ULogErrorChannel("XmlPrototypes", "Did not find a '" + listTag + "' element in the prototype definition file.");
        }
    }

    /// <summary>
    /// Loads the prototype.
    /// </summary>
    /// <param name="reader">The Xml Reader.</param>
    protected virtual void LoadPrototype(XmlTextReader reader)
    {
    }

    /// <summary>
    /// Logs the prototype error.
    /// </summary>
    /// <param name="e">An Exception instance.</param>
    /// <param name="type">The prototype type.</param>
    protected void LogPrototypeError(Exception e, string type)
    {
        // Leaving this for Unitys console because UberLogger doesn't show multiline messages correctly.
        Debug.LogError("Error reading furniture prototype for: " + type + Environment.NewLine + "Exception: " + e.Message + Environment.NewLine + "StackTrace: " + e.StackTrace);
    }
}
