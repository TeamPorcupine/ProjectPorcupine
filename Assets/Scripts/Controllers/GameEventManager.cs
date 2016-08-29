#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager current;

    private Dictionary<string, GameEvent> gameEvents;

    private void OnEnable()
    {
        current = this;
        LoadLuaScript();
        LoadEvents();
    }

    /// <summary>
    /// Needs to be moved to world.
    /// </summary>
    private void Update()
    {
        foreach (GameEvent gameEvent in gameEvents.Values)
        {
            gameEvent.Update(Time.deltaTime);
        }
    }

    private void LoadEvents()
    {
        gameEvents = new Dictionary<string, GameEvent>();

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "GameEvents");

        LoadEventsFromDirectory(filePath);
    }

    private void LoadLuaScript()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        filePath = System.IO.Path.Combine(filePath, "GameEvent.lua");
        string luaCode = System.IO.File.ReadAllText(filePath);

        new GameEventActions(luaCode);
    }

    private void LoadEventsFromDirectory(string filePath)
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

    private void LoadEvent(string filePath)
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
            }
            while (reader.ReadToNextSibling("Event"));
        }
        else
        {
            Debug.ULogErrorChannel("GameEventManager", "Could not read the event file: " + filePath);
            return;
        }
    }

    private void ReadEventFromXml(XmlReader reader)
    {
        string name = reader.GetAttribute("Name");
        bool repeat = false;
        int maxRepeats = 0;

        List<string> preconditionNames = new List<string>();
        List<string> executeNames = new List<string>();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Repeats":
                    maxRepeats = int.Parse(reader.GetAttribute("MaxRepeats"));
                    repeat = true;
                    break;
                case "Precondition":
                    string preconditionName = reader.GetAttribute("FunctionName");
                    preconditionNames.Add(preconditionName);

                    break;
                case "OnExecute":
                    string executeName = reader.GetAttribute("FunctionName");
                    executeNames.Add(executeName);

                    break;
            }
        }

        if (name.Length >= 1)
        {
            CreateEvent(name, repeat, maxRepeats, preconditionNames.ToArray(), executeNames.ToArray());
        }
    }

    private void CreateEvent(string eventName, bool repeat, int maxRepeats, string[] preconditionNames, string[] executeNames)
    {
        GameEvent gameEvent = new GameEvent(eventName, repeat, maxRepeats);
        gameEvent.RegisterPreconditions(preconditionNames);
        gameEvent.RegisterExecutionActions(executeNames);
        gameEvents[eventName] = gameEvent;
    }
}
