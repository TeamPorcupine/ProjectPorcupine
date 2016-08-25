#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


public class Prototypes<T>
{

    protected Dictionary<string, T> prototypes;
    protected string fileName;
    protected string listTag;
    protected string elementTag;



    public Prototypes()
    {
        prototypes = new Dictionary<string, T>();
    }



    /// <summary>
    /// Determines whether there is a prototype with the specified type.
    /// </summary>
    /// <returns><c>true</c> if there is a prototype with the specified type; otherwise, <c>false</c>.</returns>
    /// <param name="type">Type.</param>
    public bool HasPrototype(string type)
    {
        return prototypes.ContainsKey(type);
    }

    /// <summary>
    /// Returns the prototype with the specified type.
    /// </summary>
    /// <returns>The prototype.</returns>
    /// <param name="type">Type.</param>
    public T GetPrototype(string type)
    {
        if (HasPrototype(type))
        {
            return prototypes[type];
        }
        return default(T);
    }

    /// <summary>
    /// Returns the prototype at the specified index.
    /// </summary>
    /// <returns>The prototype.</returns>
    /// <param name="index">Index.</param>
    public T GetPrototype(int index)
    {
        return prototypes.ElementAt(index).Value;
    }

    /// <summary>
    /// Adds the given prototype.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <param name="proto">Proto.</param>
    public void SetPrototype(string type, T proto)
    {
        prototypes[type] = proto;
    }



    /// <summary>
    /// Returns the prototype Keys.
    /// </summary>
    public Dictionary<string, T>.KeyCollection Keys()
    {
        return prototypes.Keys;
    }

    /// <summary>
    /// Returns the amount of prototypes.
    /// </summary>
    public int Count()
    {
        return prototypes.Count;
    }

    /// <summary>
    /// Copies the prototypes to the given array.
    /// </summary>
    /// <param name="array">Array.</param>
    /// <param name="index">Index.</param>
    public void CopyTo(T[] array, int index)
    {
        prototypes.Values.CopyTo(array, index);
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
    /// <param name="mods">Mods.</param>
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
    /// <param name="xmlText">Xml text.</param>
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
                
                } while (reader.ReadToNextSibling(elementTag));
            }
            else
            {
                Debug.LogError("The furniture prototype definition file doesn't have any '" + elementTag + "' elements.");
            }
        }
        else
        {
            Debug.LogError("Did not find a '" + listTag + "' element in the prototype definition file.");
        }
    }

    /// <summary>
    /// Loads the prototype.
    /// </summary>
    /// <param name="reader">Reader.</param>
    protected virtual void LoadPrototype(XmlTextReader reader)
    {
        
    }

    protected void LogPrototypeError(Exception e, string type)
    {
        Debug.LogError("Error reading furniture prototype for: " + type + Environment.NewLine + "Exception: " + e.Message + Environment.NewLine + "StackTrace: " + e.StackTrace);
    }
}
