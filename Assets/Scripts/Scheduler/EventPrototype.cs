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
        /// Initializes a new instance of <see cref="Scheduler.EventPrototype"/>.
        /// </summary>
        /// <param name="name">Name (for serialization).</param>
        /// <param name="onFire">On fire event.</param>
        public EventPrototype(string name, Action<ScheduledEvent> onFire, EventType eventType)
        {
            this.Name = name;
            this.OnFire = onFire;
            this.EventType = eventType;
        }

        private event Action<ScheduledEvent> OnFire;

        public string Name { get; protected set; }

        public EventType EventType { get; protected set; }

        public Action<ScheduledEvent> OnFireCallback()
        {
            return this.OnFire;
        }
    }
}
