#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// Contains the description of a single overlay type. Contains LUA function name, id and coloring details.
/// </summary>
[MoonSharpUserData]
public class OverlayDescriptor
{
    /// <summary>
    /// Unique identifier.
    /// TODO: l10n.
    /// </summary>
    public string id;

    /// <summary>
    /// Type of color map used by this descriptor.
    /// </summary>
    public ColorMap colorMap = ColorMap.Jet;

    /// <summary>
    /// Name of function returning int (index of color) given a tile t.
    /// </summary>
    public string luaFunctionName;

    /// <summary>
    /// Bounds for clipping coloring.
    /// </summary>
    public int min = 0, max = 255;

    /// <summary>
    /// Select the type of color map (coloring scheme) you want to use.
    /// TODO: More color maps.
    /// </summary>
    public enum ColorMap
    {
        Jet,
        Random
    }

    /// <summary>
    /// Read an xml file containing a list of protorypes of overlays descriptors.
    /// </summary>
    /// <param name="fileName">Name of xml file.</param>
    /// <returns></returns>
    public static Dictionary<string, OverlayDescriptor> ReadPrototypes(string fileName)
    {
        string xmlFile = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, System.IO.Path.Combine("Overlay", fileName));
        XmlReader xmlReader = XmlReader.Create(xmlFile);

        Dictionary<string, OverlayDescriptor> descriptionsDict = new Dictionary<string, OverlayDescriptor>();

        while (xmlReader.ReadToFollowing("Overlay"))
        {
            if (!xmlReader.IsStartElement() || xmlReader.GetAttribute("id") == null)
            {
                continue;
            }

            XmlReader overlayReader = xmlReader.ReadSubtree();
            descriptionsDict[xmlReader.GetAttribute("id")] = ReadFromXml(overlayReader);
            overlayReader.Close();
        }

        return descriptionsDict;
    }

    /// <summary>
    /// Creates an OverlayDescriptor form a xml subtree with node \<Overlay></Overlay>\.
    /// </summary>
    /// <param name="xmlReader">The subtree pointing to Overlay.</param>
    /// <returns></returns>
    private static OverlayDescriptor ReadFromXml(XmlReader xmlReader)
    {
        xmlReader.Read();
        Debug.Assert(xmlReader.Name == "Overlay", "xmlReader.Name == 'Overlay'");
        OverlayDescriptor ret = new OverlayDescriptor();
        ret.id = xmlReader.GetAttribute("id");
        if (xmlReader.GetAttribute("min") != null)
        {
            ret.min = XmlConvert.ToInt32(xmlReader.GetAttribute("min"));
        }

        if (xmlReader.GetAttribute("max") != null)
        {
            ret.max = XmlConvert.ToInt32(xmlReader.GetAttribute("max"));
        }

        if (xmlReader.GetAttribute("color_map") != null)
        {
            try
            {
                ret.colorMap = (ColorMap)System.Enum.Parse(typeof(ColorMap), xmlReader.GetAttribute("color_map"));
            }
            catch (ArgumentException e)
            {
                Debug.ULogErrorChannel("OverlayMap", "Invalid color map!", e);
            }
        }

        xmlReader.Read();
        ret.luaFunctionName = xmlReader.ReadContentAsString();
        return ret;
    }
}
