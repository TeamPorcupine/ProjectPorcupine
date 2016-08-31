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
    public int MinValue;
    public int MaxValue;
    public Character character;
    public int Value;

    public Stat()
    {
    }

    protected Stat(Stat other)
    {
        this.statType = other.statType;
        this.Name = other.Name;
    }

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        statType = reader_parent.GetAttribute("statType");

        XmlReader reader = reader_parent.ReadSubtree();
        List<string> luaActions = new List<string>();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Name":
                    reader.Read();
                    Name = reader.ReadContentAsString();
                    break;
            }
        }
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
