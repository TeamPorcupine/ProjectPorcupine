using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class OverlayDescriptor
{
    
    /// <summary>
    /// Select the type of color map (coloring scheme) you want to use
    /// </summary>
    public enum ColorMap { Jet, Random };

    public string id;
    public ColorMap colorMap = ColorMap.Jet;
    public string luaFunctionName;
    public int min = 0, max = 255;

    static OverlayDescriptor ReadFromXml(XmlReader xmlReader)
    {
        xmlReader.Read();
        Debug.Assert(xmlReader.Name == "Overlay");
        OverlayDescriptor ret = new OverlayDescriptor();
        ret.id = xmlReader.GetAttribute("id");
        if(xmlReader.GetAttribute("min") != null) ret.min = XmlConvert.ToInt32(xmlReader.GetAttribute("min"));
        if (xmlReader.GetAttribute("max") != null) ret.max = XmlConvert.ToInt32(xmlReader.GetAttribute("max"));
        if (xmlReader.GetAttribute("color_map") != null)
        {
            try
            {
                ret.colorMap = (ColorMap) System.Enum.Parse(typeof(ColorMap), xmlReader.GetAttribute("color_map"));
            }
            catch (ArgumentException e) {
                Debug.LogWarning("Invalid color map!");
            }
        }
        xmlReader.Read();
        ret.luaFunctionName = xmlReader.ReadContentAsString();
        Debug.Log(string.Format("Reading overlay prototype with id {0} and LUA function {1}", ret.id, ret.luaFunctionName));
        return ret;
    }

    public static Dictionary<string, OverlayDescriptor> ReadPrototypes(string fileName)
    {
        string XmlFile = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath,
            System.IO.Path.Combine("Overlay", fileName));
        XmlReader xmlReader = XmlReader.Create(XmlFile);

        Dictionary<string, OverlayDescriptor> descriptionsDict = new Dictionary<string, OverlayDescriptor>();

        while (xmlReader.ReadToFollowing("Overlay"))
        {
            if (!xmlReader.IsStartElement() || xmlReader.GetAttribute("id") == null) continue;

            XmlReader overlayReader = xmlReader.ReadSubtree();
            descriptionsDict[xmlReader.GetAttribute("id")] = ReadFromXml(overlayReader);
            overlayReader.Close();
        }

        return descriptionsDict;
    }
}
