using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class DialogBoxLuaInformation
{
    [XmlElement("Title")]
    public string title;

    [XmlElement("Content")]
    public DialogComponent[] content;

    [XmlElement("Buttons")]
    public DialogBoxResult[] buttons;

    [XmlElement("Actions")]
    public EventActions events;
}

