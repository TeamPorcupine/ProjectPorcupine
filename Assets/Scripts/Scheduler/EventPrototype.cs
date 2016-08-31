#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;

namespace Scheduler
{
    public enum EventType
    {
        CSharp,
        Lua
    }

    // EventPrototype prototype is responsible for matching a Name attribute used
    // for serialization and lookup with an Action<ScheduledEvent> attribute which
    // is the code executed when an event is fired.
    // The Scheduler is responsible for wrapping Lua calls up into a C# delegate
    // when constructing these prototypes for Lua events.
    // The EventType enum records when this occurs, but has no impact on the actual
    // internal functioning of the EventPrototype or ScheduledEvent classes.
    public class EventPrototype
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Scheduler.EventPrototype"/> backed by a C# function.
        /// </summary>
        /// <param name="name">Name (for serialization).</param>
        /// <param name="onFire">On fire event.</param>
        public EventPrototype(string name, Action<ScheduledEvent> onFire)
        {
            this.Name = name;
            this.OnFire = onFire;
            this.EventType = EventType.CSharp;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scheduler.EventPrototype"/> backed by a Lua function.
        /// </summary>
        /// <param name="name">Name (for serialization).</param>
        /// <param name="luaFunctionName">Name of the Lua function for the event.</param>
        public EventPrototype(string name, string luaFunctionName)
        {
            this.Name = name;
            this.LuaFunctionName = luaFunctionName;
            this.EventType = EventType.Lua;
        }

        private event Action<ScheduledEvent> OnFire;

        public string Name { get; protected set; }

        public string LuaFunctionName { get; protected set; }

        public EventType EventType { get; protected set; }

        public Action<ScheduledEvent> OnFireCallback()
        {
            return this.OnFire;
        }
    }
}
