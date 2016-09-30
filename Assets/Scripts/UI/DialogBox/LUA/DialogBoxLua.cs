#region License
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

public class DialogBoxLua : DialogBox
{
    private string title;
    private XmlReader reader;

    public Transform Content { get; protected set; }

    public string Title
    {
        get
        {
            return title;
        }

        protected set
        {
            title = value;
            transform.GetChild(0).GetChild(0).GetComponentInChildren<Text>().text = value;
        }
    }
    
    public override void ShowDialog()
    {
        base.ShowDialog();
    }

    public void LoadFromXML(FileInfo file)
    {
        // TODO: Find a better way to do this. Not user friendly/Expansible.
        // DialogBoxLua -> Dialog Background
        //                 |-> Title
        //                 |-> Content
        Content = transform.GetChild(0).GetChild(1);

        reader = XmlReader.Create(file.OpenRead());
        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Title":
                    Title = reader.ReadElementContentAsString();
                    break;
                case "Content":
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Text":
                                Debug.ULogChannel("DBLua", "Text: " + reader.ReadElementContentAsString());
                                break;
                        }
                    }

                    break;
            }
        }
    }
}
