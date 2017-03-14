#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

/// <summary>
/// For XML reader.
/// </summary>
public struct SettingsOption
{
    public string name;
    public string key;
    public string defaultValue;
    public string className;
    public Parameter options;

    public SettingsOption(string name, string key, string defaultValue, string className, Parameter parameter = null)
    {
        this.name = name;
        this.key = key;
        this.defaultValue = defaultValue;
        this.className = className;
        this.options = parameter;
    }

    /// <summary>
    /// A nice little helper (pass it a reader class that is up to the subtree).
    /// </summary>
    public SettingsOption(XmlReader reader)
    {
        name = reader.GetAttribute("Name");
        key = reader.GetAttribute("Key");
        defaultValue = reader.GetAttribute("DefaultValue");
        className = reader.GetAttribute("ClassName");
        this.options = (reader != null && reader.ReadToDescendant("Params")) ? Parameter.ReadXml(reader) : new Parameter();
    }
}

/// <summary>
/// Holds the category and its options (and name).
/// There is no need to implement a write method (to write to an xml document),
/// since the template will only ever be read from.
/// </summary>
public class SettingsCategory : IPrototypable
{
    // Its in format Category::Heading
    public Dictionary<string, Dictionary<string, SettingsOption[]>> categories = new Dictionary<string, Dictionary<string, SettingsOption[]>>();

    public string Type
    {
        get
        {
            return "SettingsCategory";
        }
    }

    /// <summary>
    /// Reads from the reader provided.
    /// </summary>
    public void ReadXmlPrototype(XmlReader reader)
    {
        string name = reader.GetAttribute("Name");
        if (name != null)
        {
            categories.Add(name, new Dictionary<string, SettingsOption[]>());
        }

        string currentHeading = string.Empty;
        List<SettingsOption> options = new List<SettingsOption>();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "OptionHeading":
                    // Assign then switch over
                    if (name != null && currentHeading != null && currentHeading != string.Empty && options.Count > 0)
                    {
                        categories[name].Add(currentHeading, options.ToArray());
                    }

                    currentHeading = reader.GetAttribute("Name");
                    options.Clear();
                    break;
                case "Option":
                    options.Add(new SettingsOption(reader));
                    break;
                case "Category":
                    // Assign then clear
                    if (name != null && currentHeading != null && currentHeading != string.Empty && options.Count > 0)
                    {
                        categories[name].Add(currentHeading, options.ToArray());
                        options.Clear();
                    }

                    name = reader.GetAttribute("Name");

                    if (name != null)
                    {
                        categories.Add(name, new Dictionary<string, SettingsOption[]>());
                    }

                    break;
            }
        }

        if (name != null && currentHeading != null && currentHeading != string.Empty && options.Count > 0)
        {
            categories[name].Add(currentHeading, options.ToArray());
        }
    }
}