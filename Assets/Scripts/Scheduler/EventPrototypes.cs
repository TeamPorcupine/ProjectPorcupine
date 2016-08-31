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
using Scheduler;

public class EventPrototypes : XmlPrototypes<ScheduledEvent>
{
    public EventPrototypes() : base("Events.xml", "Events", "Event")
    {
    }

    public void Add(string name, ScheduledEvent eventPrototype)
    {
        if (this.HasPrototype(name))
        {
            Debug.ULogWarningChannel("EventPrototypes", "Trying to register an event with the same name '{0}' as one that has already been registered. Overwriting the old one. (May ignore if unit testing.)", name);
            prototypes[name] = eventPrototype;
        }

        prototypes.Add(name, eventPrototype);
    }

    /// <summary>
    /// Loads the prototype.
    /// </summary>
    /// <param name="reader">The Xml Reader.</param>
    protected override void LoadPrototype(XmlTextReader reader)
    {
        string name = reader.GetAttribute("name");
        string luaFuncName = reader.GetAttribute("onFire");
        ScheduledEvent eventPrototype = new ScheduledEvent(name, luaFuncName);
        SetPrototype(name, eventPrototype);
    }
}
