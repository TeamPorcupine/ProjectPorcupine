using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class Parameter {
    private string name;
    private string value;
    private Dictionary<string, Parameter> contents;

    public Parameter(string name, string value) 
    {
        this.name = name;
        this.value = value;
    }


    public Parameter(string name, object value) 
    {
        this.name = name;
        this.value = value.ToString();
    }


    public Parameter(string name) 
    {
        this.name = name;
        contents = new Dictionary<string, Parameter>();
    }


    public Parameter(Parameter other)
    {
        this.name = other.GetName();
        this.contents = new Dictionary<string, Parameter>(other.GetDictionary());
    }

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


    private Dictionary<string, Parameter> GetDictionary()
    {
        return contents;
    }

    public string ToString() 
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

    public void ChangeFloatValue(float value)
    {
        this.value = "" + (ToFloat() + value);
    }

    public string GetName()
    {
        return name;
    }

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

    public bool HasContents() 
    {
        return contents != null;
    }

    public void WriteXmlParamGroup(XmlWriter writer)
    {

        writer.WriteStartElement("ParamGroup");
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
        //        Debug.Log("**PROPER RELOAD**");
        int cycleCount = 0;
        //        while (!(reader.IsStartElement()) && cycleCount <40)
        //        {
        //            //            reader.MoveToContent();
        //            reader.Skip();
        //            reader.Read();
        //            //            reader.ReadStartElement();
        //            Debug.Log(cycleCount + ": " + reader.NodeType);
        //            cycleCount++;
        //        }
        XmlReader subReader = reader.ReadSubtree();
        Parameter paramGroup = new Parameter(subReader.GetAttribute("name"));

        // Advance to the first inner element
        subReader.Read();
        subReader.Read();
        //        int i = 0;

        while(subReader.ReadToNextSibling("Param"))
        {
            //            Debug.Log(subReader.Name);
            //            return paramGroup;
            string k = subReader.GetAttribute("name");
            if (subReader.NodeType == XmlNodeType.Whitespace || subReader.NodeType ==  XmlNodeType.EndElement)
                continue;


            if (subReader.NodeType == XmlNodeType.Element)
            {
                if (subReader.IsEmptyElement)
                {
                    string v = subReader.GetAttribute("value");
                    paramGroup[k] = new Parameter(k, v);

                }
                else
                {
                    paramGroup[k] = Parameter.ReadXml(subReader);

                }
            }
            //            switch (reader.Name)
            //            {
            //                case "Param":
            //                    //                    reader.Read();
            //                    string v = subReader.GetAttribute("value");
            //                    paramGroup[k] = new Parameter(k,v);
            //
            //                    Debug.Log(i + "Cycle Mid");
            //                    break;
            //                case "ParamGroup":
            //                    paramGroup[k] = Parameter.ReadXml(subReader);
            //
            //                    Debug.Log(i + "Cycle Mid");
            //                    break;
            //            }
            //            Debug.Log(i + "Cycle End");
        }
        subReader.Close();
        // Advance reader past the end element
        //        Debug.Log("EndTag: " + reader.Name);
        //        Debug.Log("PostEndTag: " + reader.Name);
        //        Debug.Log("We really should print here!");
        return paramGroup;
    }
}
