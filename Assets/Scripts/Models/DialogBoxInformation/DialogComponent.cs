﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class DialogComponent
{
    [XmlAttribute("name")]
    public string name;

    [XmlAttribute("type")]
    public string ObjectType;

    [XmlElement("Position")]
    public Vector3 position;

    [XmlElement("Size")]
    public Vector2 size;

    [XmlElement("Data")]
    public object data;
}