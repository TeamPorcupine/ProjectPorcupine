#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Xml;
using MoonSharp.Interpreter;
using ProjectPorcupine.Localization;

/// <summary>
/// Contains the description of a single overlay type. Contains LUA function name, id and coloring details.
/// </summary>
[MoonSharpUserData]
public class OverlayDescriptor : IPrototypable
{
    public OverlayDescriptor()
    {
        ColorMap = ColorMapOption.Jet;
        Min = 0;
        Max = 255;
    }

    /// <summary>
    /// Select the type of color map (coloring scheme) you want to use.
    /// TODO: More color maps.
    /// </summary>
    public enum ColorMapOption
    {
        Jet,
        Random,
        Palette
    }

    /// <summary>
    /// Unique identifier.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the localized name.
    /// </summary>
    public string Name
    {
        get { return LocalizationTable.GetLocalization("overlay_" + Type); }
    }

    /// <summary>
    /// Type of color map used by this descriptor.
    /// </summary>
    public ColorMapOption ColorMap { get; private set; }

    /// <summary>
    /// Name of function returning int (index of color) given a tile t.
    /// </summary>
    public string LuaFunctionName { get; private set; }

    /// <summary>
    /// Gets the min bound for clipping coloring.
    /// </summary>
    public int Min { get; private set; }

    /// <summary>
    /// Gets the max bound for clipping coloring.
    /// </summary>
    public int Max { get; private set; }

    /// <summary>
    /// Creates an OverlayDescriptor form a xml subtree with node \<Overlay></Overlay>\.
    /// </summary>
    /// <param name="xmlReader">The subtree pointing to Overlay.</param>
    public void ReadXmlPrototype(XmlReader xmlReader)
    {
        Type = xmlReader.GetAttribute("type");

        if (xmlReader.GetAttribute("min") != null)
        {
            Min = XmlConvert.ToInt32(xmlReader.GetAttribute("min"));
        }

        if (xmlReader.GetAttribute("max") != null)
        {
            Max = XmlConvert.ToInt32(xmlReader.GetAttribute("max"));
        }

        if (xmlReader.GetAttribute("colorMap") != null)
        {
            try
            {
                ColorMap = (ColorMapOption)Enum.Parse(typeof(ColorMapOption), xmlReader.GetAttribute("colorMap"));
            }
            catch (ArgumentException e)
            {
                UnityDebugger.Debugger.LogErrorFormat("OverlayMap", "Invalid color map!\n{0}", e.Message);
            }
        }

        xmlReader.Read();
        LuaFunctionName = xmlReader.ReadContentAsString();
    }
}
