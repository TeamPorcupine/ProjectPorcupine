using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class GameEventManager : MonoBehaviour {

    static public GameEventManager current;

    Dictionary<string, GameEvent> gameEvents;

    void OnEnable()
    {
        current = this;

        LoadLuaScript();
        LoadEvents();
    }

    /// <summary>
    /// Needs to be moved to world
    /// </summary>
    void Update(){
        foreach(GameEvent gameEvent in gameEvents.Values){
            gameEvent.Update();
        }
    }

    void LoadEvents()
    {
        gameEvents = new Dictionary<string, GameEvent>();

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "GameEvents");

        LoadEventsFromDirectory(filePath);
    }

    void LoadLuaScript()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        filePath = System.IO.Path.Combine(filePath, "GameEvent.lua");
        string luaCode = System.IO.File.ReadAllText(filePath);

        new GameEventActions(luaCode);
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

        List<string> preconditionNames = new List<string>();
        List<string> onExecuteNames = new List<string>();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Precondition":
                    string preconditionName = reader.GetAttribute("FunctionName");
                    preconditionNames.Add(preconditionName);

                    break;
                case "OnExecute":
                    string onExecuteName = reader.GetAttribute("FunctionName");
                    onExecuteNames.Add(onExecuteName);

                    break;
            }
        }

        if (name.Length >= 1)
        {
            CreateEvent(name, preconditionNames.ToArray(), onExecuteNames.ToArray());
        }
    }

    void CreateEvent(string eventName, string[] preconditionNames, string[] onExecuteNames)
    {
        GameEvent gameEvent = new GameEvent(eventName);

        gameEvent.RegisterPreconditions(preconditionNames);
        gameEvent.RegisterExecutionActions(onExecuteNames);

        gameEvents[eventName] = gameEvent;
    }
}
