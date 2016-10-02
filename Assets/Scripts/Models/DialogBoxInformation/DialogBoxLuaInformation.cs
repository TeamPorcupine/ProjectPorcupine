using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class DialogBoxLuaInformation
{
    [XmlElement]
    public string title;
    [XmlElement]
    public DialogComponent[] content;
}

