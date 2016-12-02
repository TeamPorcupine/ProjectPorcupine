#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Xml;
using System.Collections.Generic;

/// <summary>
/// Holds the category and its options (and name).
/// No need to write, only will ever read.
/// </summary>
public class SettingsCategory : IPrototypable
{
    public string Type
    {
        get
        {
            return "SettingsCategory";
        }
    }

    public Dictionary<string, SettingsOption[]> category = new Dictionary<string, SettingsOption[]>();

    /// <summary>
    /// Reads from the reader provided.
    /// </summary>
    public void ReadXmlPrototype(XmlReader reader)
    {
        string name = reader.GetAttribute("Name");
        List<SettingsOption> optionsList = new List<SettingsOption>();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Option":
                    optionsList.Add(new SettingsOption(reader));
                    break;
                case "Category":
                    if (optionsList.Count > 0 && name != null)
                    {
                        // Assign then clear
                        category.Add(name, optionsList.ToArray());
                        optionsList.Clear();
                    }

                    name = reader.GetAttribute("Name");
                    break;
            }
        }

        if (name != null)
        {
            category.Add(name, optionsList.ToArray());
        }
    }
}

public struct SettingsOption
{
    public string name;
    public string key;
    public string defaultValue;
    public string className;

    public SettingsOption(string name, string key, string defaultValue, string className)
    {
        this.name = name;
        this.key = key;
        this.defaultValue = defaultValue;
        this.className = className;
    }

    /// <summary>
    /// A nice little helper (pass it a reader class that is up to the subtree)
    /// </summary>
    public SettingsOption(XmlReader reader)
    {
        name = reader.GetAttribute("Name");
        key = reader.GetAttribute("Key");
        defaultValue = reader.GetAttribute("DefaultValue");
        className = reader.GetAttribute("ClassName");
    }
}