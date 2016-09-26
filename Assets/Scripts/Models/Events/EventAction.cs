﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// This class handles LUA actions take in response to events triggered within C# or LUA. For each event name (e.g. OnUpdate, ...) there
/// is a list of LUA function that are registered and will be called once the event with that name is fired.
/// </summary>
[MoonSharpUserData]
public class EventActions : IXmlSerializable
{
    /// <summary>
    /// Stores a list of LUA functions for each type of event (eventName). All will be called at once.
    /// </summary>
    protected Dictionary<string, List<string>> actionsList = new Dictionary<string, List<string>>();

    /// <summary>
    /// Used to transfer register actions to new object.
    /// </summary>
    /// <returns>A new object copy of this.</returns>
    public EventActions Clone()
    {
        EventActions evt = new EventActions();

        evt.actionsList = new Dictionary<string, List<string>>(actionsList);

        return evt;
    }

    /// <summary>
    /// Fill the values of this using an xml specification.
    /// </summary>
    /// <param name="reader">Reader pointing to an Action tag.</param>
    public void ReadXml(XmlReader reader)
    {
        reader.Read();
        if (reader.Name != "Action")
        {
            Debug.ULogErrorChannel("EventActions", string.Format("The element is not an Action, but a \"{0}\"", reader.Name));
        }

        string name = reader.GetAttribute("event");
        if (name == null)
        {
            Debug.ULogErrorChannel("EventActions", string.Format("The attribute \"event\" is a mandatory for an \"Action\" element."));
        }

        string functionName = reader.GetAttribute("functionName");
        if (functionName == null)
        {
            Debug.ULogErrorChannel("EventActions", string.Format("No function name was provided for the Action {0}.", name));
        }

        Register(name, functionName);
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        foreach (string evt in actionsList.Keys)
        {
            writer.WriteStartElement("Action");

            foreach (string func in actionsList[evt])
            {
                writer.WriteAttributeString("event", evt);
                writer.WriteAttributeString("functionName", func);
            }

            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Register a function named luaFunc, that gets fired in response to an action named actionName.
    /// </summary>
    /// <param name="actionName">Name of event triggering action.</param>
    /// <param name="luaFunc">Lua function to add to list of actions.</param>
    public void Register(string actionName, string luaFunc)
    {
        if (!actionsList.ContainsKey(actionName) || actionsList[actionName] == null)
        {
            actionsList[actionName] = new List<string>();
        }

        actionsList[actionName].Add(luaFunc);
    }

    /// <summary>
    /// Deregister a function named luaFunc, from the action.
    /// </summary>
    /// <param name="actionName">Name of event triggering action.</param>
    /// <param name="luaFunc">Lua function to add to list of actions.</param>
    public void Deregister(string actionName, string luaFunc)
    {
        if (!actionsList.ContainsKey(actionName) || actionsList[actionName] == null)
        {
            return;
        }

        actionsList[actionName].Remove(luaFunc);
    }

    /// <summary>
    /// Fire the event named actionName, resulting in all lua functions being called.
    /// </summary>
    /// <param name="actionName">Name of the action being triggered.</param>
    /// <param name="target">Object, passed to LUA function as 1-argument.</param>
    /// <param name="deltaTime">Time since last Trigger of this event.</param>
    public void Trigger<T>(string actionName, T target, float deltaTime = 0f)
    {
        if (!actionsList.ContainsKey(actionName) || actionsList[actionName] == null)
        {
            return;
        }
        else
        {
            FunctionsManager.Get(target.GetType().ToString()).CallWithInstance(actionsList[actionName].ToArray(), target, deltaTime);
        }
    }

    /// <summary>
    /// Determines whether this instance has any events named actionName.
    /// </summary>
    /// <returns><c>true</c> if this instance has any events named actionName; otherwise, <c>false</c>.</returns>
    /// <param name="actionName">Action name.</param>
    public bool HasEvent(string actionName)
    {
        return actionsList.ContainsKey(actionName);
    }

    /// <summary>
    /// Determines whether this instance has any events.
    /// </summary>    
    public bool HasEvents()
    {
        return actionsList.Count > 0;
    }
}
