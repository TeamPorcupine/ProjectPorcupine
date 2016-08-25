#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;
using System.Xml;


public class QuestPrototypes : Prototypes<Quest>
{

    public QuestPrototypes()
    {
        prototypes = new Dictionary<string, Quest>();
        fileName = "Quest.xml";
        listTag = "Quests";
        elementTag = "Quest";

        LoadPrototypesFromFile();
    }



    protected override void LoadPrototype(XmlTextReader reader)
    {
        Quest quest = new Quest();
        try
        {
            quest.ReadXmlPrototype(reader);
        }
        catch (Exception e)
        {
            LogPrototypeError(e, quest.Name);
        }

        SetPrototype(quest.Name, quest);
    }
}
