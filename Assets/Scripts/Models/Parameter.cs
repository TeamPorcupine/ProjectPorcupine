#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System.Linq;

[MoonSharpUserData]
public class Parameter {
    // Name is primarily to simplify writing to XML, and will be the same as the key used to access it in a higher up Parameter when read from XML
    private string name;
    // Value is stored as a string and converted as needed, this simplifies storing multiple value types.
    private string value;
    // If this Parameter contains other Parameters, contents will contain the actual parameters
    private Dictionary<string, Parameter> contents;

    public Parameter(string name, string value) 
    {
        this.name = name;
        this.value = value;
    }

    // Constructor with object parameter allows it to easily create a Parameter with any object that has a string representation (primarily for use if that string
    //  representation can be converted back to the original value, such as with a float. Not suitable for holding a Parameter object.
    public Parameter(string name, object value) 
    {
        this.name = name;
        this.value = value.ToString();
    }

    // Parameter with no value assumes it is being used for Parameter with contents, and initialized the dictionary
    public Parameter(string name) 
    {
        this.name = name;
        contents = new Dictionary<string, Parameter>();
    }

    // Copy constructur, should properly handle copying both types of Parameters (singular and group)
    public Parameter(Parameter other)
    {
        this.name = other.GetName();
        if (other.HasContents())
        {
            this.contents = other.GetDictionary();
        }
        this.value = other.ToString();
    }

    // Iterator to simplify usage, this works properly in Lua
    public Parameter this[string key]
    {
        get
        {
            return contents[key];
        }
        set
        {
            contents[key] = value;
        }
    }


    // Provides a deep clone of the dictionary, to ensure contained Parameters aren't linked between old and new objects
    private Dictionary<string, Parameter> GetDictionary()
    {
        return contents.ToDictionary(entry => entry.Key, 
            entry => new Parameter((Parameter) entry.Value));
//        return new Dictionary<string, Parameter>(contents);
    }

    public override string ToString() 
    {
        return value;
    }

    public float ToFloat() 
    {
        float returnValue = 0;
        float.TryParse(value, out returnValue);
        return returnValue;
    }

    public void SetValue(string value) 
    {
        this.value = value;
    }

    public void SetValue(object value)
    {
        this.value = value.ToString();
    }

    // Change value by a float, primarily here to approximate old parameter system usage
    public void ChangeFloatValue(float value)
    {
        this.value = "" + (ToFloat() + value);
    }

    public string GetName()
    {
        return name;
    }

    // Converts contents Keys from KeyCollection to plain array
    public string[] Keys()
    {
        string[] keys = new string[contents.Keys.Count];
        contents.Keys.CopyTo(keys, 0);;
        return keys;
    }

    public bool ContainsKey(string key)
    {
        return contents.ContainsKey(key);
    }

    public void AddParameter(Parameter parameter)
    {
        contents[parameter.GetName()] = parameter;
    }

    // Primary method to differentiate an unknown Parameter between a singular Parameter and Group Parameter
    public bool HasContents() 
    {
        return contents != null;
    }

    public void WriteXmlParamGroup(XmlWriter writer)
    {

        writer.WriteStartElement("Param");
        writer.WriteAttributeString("name", name);
        if (!value.Equals(""))
        {
            writer.WriteAttributeString("value", value);
        }
        foreach (string k in contents.Keys)
        {
            this["k"].WriteXml(writer);
        }
        writer.WriteEndElement();
    }

    public void WriteXmlParam(XmlWriter writer)
    {       
        writer.WriteStartElement("Param");
        writer.WriteAttributeString("name", name);
        writer.WriteAttributeString("value", value);
        writer.WriteEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        if (HasContents())
        {
            WriteXmlParamGroup(writer);
        }
        else
        {
            WriteXmlParam(writer);
        }

    }


    public static Parameter ReadXml(XmlReader reader)
    {
        XmlReader subReader = reader.ReadSubtree();
        Parameter paramGroup = new Parameter(subReader.GetAttribute("name"));

        // Advance to the first inner element. Two reads are needed to ensure we don't get stuck on containing Element, or an EndElement
        subReader.Read();
        subReader.Read();

        while(subReader.ReadToNextSibling("Param"))
        {
            string k = subReader.GetAttribute("name");
            // Sometimes the look will get stuck on a Whitespace or an EndElement and error, so continue to next loop if we encounter one
            if (subReader.NodeType == XmlNodeType.Whitespace || subReader.NodeType ==  XmlNodeType.EndElement)
                continue;
            // Somewhat redundant check to make absolutely sure we're on an Element
            if (subReader.NodeType == XmlNodeType.Element)
            {
                // An empty element is a singular Param such as <Param name="name" value="value />
                if (subReader.IsEmptyElement)
                {
                    string v = subReader.GetAttribute("value");
                    paramGroup[k] = new Parameter(k, v);

                }
                // This must be a group element, so we recurse and dive deeper
                else
                {
                    paramGroup[k] = Parameter.ReadXml(subReader);

                }
            }
        }
        subReader.Close();
        return paramGroup;
    }
}
