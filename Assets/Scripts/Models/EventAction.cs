using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

/// <summary>
/// This class handles LUA actions take in response to events triggered within C# or LUA. For each event name (e.g. OnUpdate, ...) there
/// is a list of LUA function that are registered and will be called once the event with that name is fired.
/// </summary>
[MoonSharpUserData]
public class EventAction : IXmlSerializable
{
    /// <summary>
    /// Stores a list of LUA functions for each type of event (eventName). All will be called at once.
    /// </summary>
    protected Dictionary<string, List<string>> actionsList = new Dictionary<string, List<string>>();

    /// <summary>
    /// Used to transfer registere actions to new object.
    /// </summary>
    /// <returns>A new object copy of this.</returns>
    public EventAction Clone()
    {
        EventAction evt = new EventAction();

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
            Debug.LogError(string.Format("The element is not an Action, but a \"{0}\"", reader.Name));
        }

        string name = reader.GetAttribute("event");
        if (name == null)
        {
            Debug.LogError(string.Format("The attribute \"event\" is a mandatory for an \"Action\" element."));
        }

        string functionName = reader.GetAttribute("functionName");
        if (functionName == null)
        {
            Debug.LogError(string.Format("No function name was provided for the Action {0}.", name));
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
    /// Register a function named luaFunc, that gets fired in respnse to an action named actionName.
    /// </summary>
    /// <param name="actionName">Name of event triggering action.</param>
    /// <param name="luaFunc">Lua function to add to list of actions.</param>
    public void Register(string actionName, string luaFunc)
    {
        ////Debug.Log(string.Format("Registering the LUA function {0} to Action {1}.", luaFunc, actionName));
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
        ////Debug.Log(string.Format("Registering the LUA function {0} to Action {1}.", luaFunc, actionName));
        if (!actionsList.ContainsKey(actionName) || actionsList[actionName] == null)
        {
            return;
        }
        actionsList[actionName].Remove(luaFunc);
    }

    /// <summary>
    /// "Fire" the event named actionName, resulting in all lua functions being called.
    /// </summary>
    /// <param name="actionName">Name of the action being triggered.</param>
    /// <param name="target">Object, passed to LUA function as 1-argument (TODO: make it an object).</param>
    /// <param name="deltaTime">Time since last Trigger of this event.</param>
    public void Trigger(string actionName, Furniture target, float deltaTime = 0f)
    {
        if (!actionsList.ContainsKey(actionName) || actionsList[actionName] == null)
        {
            ////Debug.LogWarning(string.Format("The action \"{0}\" is associated with no LUA function.", actionName));
            return;
        }
        else
        {
            FurnitureActions.CallFunctionsWithFurniture(actionsList[actionName].ToArray(), target, deltaTime);
        }

    }

}
