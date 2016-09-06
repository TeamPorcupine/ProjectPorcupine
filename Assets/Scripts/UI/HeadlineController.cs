﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using System.IO;
using System.Xml;

using UnityEngine;
using UnityEngine.UI;

public class HeadlineController : MonoBehaviour
{
    public Text textBox;

    private void Start()
    {
        string filePath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "Headlines.xml");
        string xmlText = System.IO.File.ReadAllText(filePath);
        XmlDocument doc = new XmlDocument();
        doc.Load(new StringReader(xmlText));
        HeadlineGenerator headlineGenerator = World.Current.CreateHeadlineGenerator(doc.SelectSingleNode("Headlines"), UpdateHeadline);
        UpdateHeadline(headlineGenerator.CurrentDisplayText);
    }

    private void UpdateHeadline(string newHeadline)
    {
        Debug.ULogChannel("Headline", newHeadline);
        textBox.text = newHeadline;
    }
}