using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class GameEventManager : MonoBehaviour {

    static public GameEventManager current;

    Dictionary<string, GameEvent> events;

    void OnEnable()
    {
        current = this;

        LoadEvents();
    }

    void LoadEvents()
    {
        events = new Dictionary<string, GameEvent>();

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "GameEvents");

        LoadEventsFromDirectory(filePath);
    }

    void LoadEventsFromDirectory(string filePath)
    {

        string[] subDirs = Directory.GetDirectories(filePath);
        foreach (string sd in subDirs)
        {
            LoadEventsFromDirectory(sd);
        }

        string[] filesInDir = Directory.GetFiles(filePath);
        foreach (string fn in filesInDir)
        {
            LoadEvent(fn);
        }

    }

    void LoadEvent(string filePath)
    {
        if (!filePath.Contains(".xml") || filePath.Contains(".meta"))
        {
            return;
        }

        string xmlText = System.IO.File.ReadAllText(filePath);
        XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

        if (reader.ReadToDescendant("Events") && reader.ReadToDescendant("Event"))
        {
            do
            {
                ReadEventFromXml(reader);
            } while(reader.ReadToNextSibling("Event"));
        }
        else
        {
            Logger.LogError("Could not read the event file: " + filePath);
            return;
        }
    }

    void ReadEventFromXml(XmlReader reader)
    {
        //Debug.Log("ReadSpriteFromXml");
        string name = reader.GetAttribute("Name");

        List<string> functionNames = new List<string>();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "OnExecute":
                    string functionName = reader.GetAttribute("FunctionName");
                    functionNames.Add(functionName);

                    break;
            }
        }

        if (name.Length >= 1)
        {
            CreateEvent(name, functionNames.ToArray());
        }
    }

    void CreateEvent(string eventName, string[] functionNames)
    {
        GameEvent gameEvent = new GameEvent(eventName);

        foreach(string funcName in functionNames){
            gameEvent.RegisterExecutionAction(funcName);
        }

        events[eventName] = gameEvent;
    }
}
