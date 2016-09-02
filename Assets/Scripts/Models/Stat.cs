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
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class Stat
{
    public string statType;
    public string Name;
    public int Value;

    public Stat()
    {
    }

    protected Stat(Stat other)
    {
        this.statType = other.statType;
        this.Name = other.Name;
    }

    public void ReadXmlPrototype(XmlReader parentReader)
    {
        statType = parentReader.GetAttribute("statType");
        Name = parentReader.GetAttribute("name");
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("statType", statType);
        writer.WriteAttributeString("value", Value.ToString());
    }

    public Stat Clone()
    {
        return new Stat(this);
    }

    public override string ToString()
    {
        return statType + ": " + Value;
    }
}
