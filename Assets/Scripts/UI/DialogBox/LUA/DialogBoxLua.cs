using System.Collections;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxLua : DialogBox
{
    public Transform content { get; protected set; }
    private string _title;
    public string Title {
        get{
            return _title;
        }
        protected set
        {
            _title = value;
            transform.GetChild(0).GetChild(0).GetComponentInChildren<Text>().text = value;
        }
    }

    private XmlReader reader;

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
        content = transform.GetChild(0).GetChild(1);

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
                        switch(reader.Name)
                        {
                            case "Text":
                                Debug.ULogChannel("DBLua","Text: " + reader.ReadElementContentAsString());
                                break;
                        }
                    }
                    break;
            }
        }

    }
}
