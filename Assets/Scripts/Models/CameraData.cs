#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public struct Preset
{
    public Vector3 position;
    public float zoomLevel;
}

public class CameraData : IXmlSerializable
{
    public Vector3 position;
    public float zoomLevel;
    public Preset[] presets;

    public void ReadXml(XmlReader reader)
    {
        presets = new Preset[5];

        do
        {
            float x = float.Parse(reader.GetAttribute("X"));
            float y = float.Parse(reader.GetAttribute("Y"));
            float z = float.Parse(reader.GetAttribute("Z"));
            position = new Vector3(x, y, z);
            zoomLevel = float.Parse(reader.GetAttribute("zoomLevel"));

            while (reader.Read())
            {
                // Read until the end of the character.
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                switch (reader.Name)
                {
                    case "PresetPositions":
                        if (reader.ReadToDescendant("Position"))
                        {
                            int index = 0;

                            do
                            {
                                presets[index].position.x = float.Parse(reader.GetAttribute("X"));
                                presets[index].position.y = float.Parse(reader.GetAttribute("Y"));
                                presets[index].position.z = float.Parse(reader.GetAttribute("Z"));

                                index++;
                            }
                            while (reader.ReadToNextSibling("Position"));
                        }

                        break;

                    case "PresetZoomLevels":
                        if (reader.ReadToDescendant("Level"))
                        {
                            int index = 0;
                            do
                            {
                                presets[index].zoomLevel = float.Parse(reader.GetAttribute("Value"));
                                index++;
                            }
                            while (reader.ReadToNextSibling("Level"));
                        }

                        break;
                }
            }
        }
        while (reader.ReadToNextSibling("CameraData"));
    }

    public void WriteXml(XmlWriter writer)
    {
        if (presets == null)
        {
            Debug.ULogErrorChannel("CameraData", "Tried to serialize non existent camera presets");
            return;
        }

        writer.WriteAttributeString("X", Camera.main.transform.position.x.ToString());
        writer.WriteAttributeString("Y", Camera.main.transform.position.y.ToString());
        writer.WriteAttributeString("Z", Camera.main.transform.position.z.ToString());
        writer.WriteAttributeString("zoomLevel", Camera.main.orthographicSize.ToString());

        writer.WriteStartElement("PresetPositions");
        foreach (Preset preset in presets)
        {
            writer.WriteStartElement("Position");
            writer.WriteAttributeString("X", preset.position.x.ToString());
            writer.WriteAttributeString("Y", preset.position.y.ToString());
            writer.WriteAttributeString("Z", preset.position.z.ToString());
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        writer.WriteStartElement("PresetZoomLevels");
        foreach (Preset preset in presets)
        {
            writer.WriteStartElement("Level");
            writer.WriteAttributeString("Value", preset.zoomLevel.ToString());
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// This does absolutely nothing.
    /// This is required to implement IXmlSerializable.
    /// </summary>
    /// <returns>NULL and NULL.</returns>
    public XmlSchema GetSchema()
    {
        return null;
    }
}
