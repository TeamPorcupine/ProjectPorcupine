using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class DialogComponent
{
    [XmlAttribute("Type")]
    public string ObjectType;

    [XmlElement]
    public Vector3 position;

    [XmlElement]
    public object data;
}