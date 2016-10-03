using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class DialogComponent
{
    [XmlAttribute("type")]
    public string ObjectType;

    [XmlElement("Position")]
    public Vector3 position;
    
    [XmlElement("Data")]
    public object data;
}