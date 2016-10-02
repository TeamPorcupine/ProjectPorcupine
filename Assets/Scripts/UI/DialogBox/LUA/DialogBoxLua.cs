#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxLua : DialogBox
{
    private string title;

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

        XmlSerializer serializer = new XmlSerializer(typeof(DialogBoxLuaInformation));

        try
        {
            DialogBoxLuaInformation DialogBoxInfo = (DialogBoxLuaInformation)serializer.Deserialize(file.OpenRead());
            Title = DialogBoxInfo.title;
            foreach (DialogComponent goInfo in DialogBoxInfo.content)
            {
                // Implement new DialogComponents in here.
                switch (goInfo.ObjectType)
                {
                    case "Text":
                        GameObject TextObject = (GameObject)Instantiate(Resources.Load("Prefab/DialogBoxPrefabs/DialogText"), Content);
                        TextObject.GetComponent<Text>().text = (string)goInfo.data;
                        TextObject.GetComponent<RectTransform>().anchoredPosition = goInfo.position;
                        break;
                }
            }
        }
        catch (System.Exception error)
        {
            Debug.ULogErrorChannel("DialogBoxLua", "Error deserializing data:" + error.Message);
        }
    }
}
