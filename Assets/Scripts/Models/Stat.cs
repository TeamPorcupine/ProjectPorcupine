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

    public Stat Clone()
    {
        return new Stat(this);
    }

    
    public override string ToString()
    {
        return statType + ": " + Value;
    }
}
