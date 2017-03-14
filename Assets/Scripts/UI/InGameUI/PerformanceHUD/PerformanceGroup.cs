#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Xml;

public struct PerformanceGroup
{
    public string name;
    public string[] elementData;
    public Parameter[] parameterData;
    public bool disableUI;

    public PerformanceGroup(string name, string[] elementData, Parameter[] parameterData, bool disableUI)
    {
        this.name = name;
        this.elementData = elementData;
        this.disableUI = disableUI;
        this.parameterData = parameterData;
    }
}

public class PerformanceGroupReader : IPrototypable
{
    public List<PerformanceGroup> groups = new List<PerformanceGroup>();

    public string Type
    {
        get
        {
            return "PerformanceGroup";
        }
    }

    /// <summary>
    /// Reads from the reader provided.
    /// </summary>
    public void ReadXmlPrototype(XmlReader reader)
    {
        string name = reader.GetAttribute("Name");

        bool disableUI = XmlConvert.ToBoolean(reader.GetAttribute("DisableUI").ToLower());

        if (name == null)
        {
            name = string.Empty;
        }

        List<string> elementData = new List<string>();
        List<Parameter> parameterData = new List<Parameter>();

        while (reader.Read())
        {
            if (reader.Name == "Option")
            {
                reader.MoveToContent();
                string className = reader.GetAttribute("ClassName");

                if (string.IsNullOrEmpty(className) == false)
                {
                    elementData.Add(reader.GetAttribute("ClassName"));
                    parameterData.Add(reader != null && reader.ReadToDescendant("Params") ? Parameter.ReadXml(reader) : new Parameter());
                }
            }
            else if (reader.Name == "ComponentGroup")
            {
                if (name != null)
                {
                    groups.Add(new PerformanceGroup(name, elementData.ToArray(), parameterData.ToArray(), disableUI));
                }

                name = reader.GetAttribute("Name");

                if (reader.Name == "DisableUI")
                {
                    disableUI = XmlConvert.ToBoolean(reader.GetAttribute("DisableUI").ToLower());
                }
                else
                {
                    disableUI = false;
                }

                elementData.Clear();
                parameterData.Clear();
            }
        }

        if (name != null)
        {
            groups.Add(new PerformanceGroup(name, elementData.ToArray(), parameterData.ToArray(), disableUI));
        }
    }
}
