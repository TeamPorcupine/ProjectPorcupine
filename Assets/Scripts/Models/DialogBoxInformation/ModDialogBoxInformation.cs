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
using System.Xml.Serialization;

public class ModDialogBoxInformation
{
    [XmlElement("Title")]
    public string title;

    [XmlElement("Control")]
    public DialogComponent[] content;

    [XmlElement("Buttons")]
    public DialogBoxResult[] buttons;

    [XmlIgnore]
    public Dictionary<string, string> Actions = new Dictionary<string, string>();

    [XmlArray("Actions")]
    [XmlArrayItem("Action", Type = typeof(DictionaryEntry))]
    public DictionaryEntry[] ActionsList
    {
        get
        {
            // Make an array of DictionaryEntries to return   
            DictionaryEntry[] ret = new DictionaryEntry[Actions.Count];
            int i = 0;
            DictionaryEntry de;

            // Iterate through Stuff to load items into the array.   
            foreach (KeyValuePair<string, string> props in Actions)
            {
                de = new DictionaryEntry();
                de.Key = props.Key;
                de.Value = props.Value;
                ret[i] = de;
                i++;
            }

            return ret;
        }

        set
        {
            Actions.Clear();
            for (int i = 0; i < value.Length; i++)
            {
                Actions.Add((string)value[i].Key, (string)value[i].Value);
            }
        }
    }
}