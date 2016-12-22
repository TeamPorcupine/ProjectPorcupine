#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Xml;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace Scheduler
{
    /// <summary>
    /// The type of function backing a ScheduledEvent.
    /// Options: CSharp or Lua.
    /// </summary>
    public enum EventType
    {
        CSharp,
        Lua
    }

    /// <summary>
    /// The <see cref="Scheduler.ScheduledEvent"/> class represents an individual
    /// event which is handled by the Scheduler.
    /// May "fire" either a C# or Lua function.
    /// ScheduledEvent is actually blind to the type of function backing the event.
    /// It knows through the EventType property, but this has no impact on the functioning
    /// of the class itself.
    /// The scheduler is solely responsible for wrapping Lua functions in delegates
    /// for handling by the events.
    /// </summary>
    [MoonSharpUserData]
    public class ScheduledEvent : IPrototypable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledEvent"/> class.
        /// This is required to create a Prototype.
        /// </summary>
        public ScheduledEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler.ScheduledEvent"/> class.
        /// This form of the constructor assumes the ScheduledEvent is of the EventType.CSharp type.
        /// </summary>
        /// <param name="name">Name of the event (for serialization etc.).</param>
        /// <param name="onFire">Callback to call when the event fires.</param>
        /// <param name="cooldown">Cooldown in seconds.</param>
        /// <param name="repeatsForever">Whether the event repeats forever (defaults false). If true repeats is ignored.</param>
        /// <param name="repeats">Number of repeats (default 1). Ignored if repeatsForever == true.</param>
        public ScheduledEvent(string name, Action<ScheduledEvent> onFire, float cooldown, bool repeatsForever = false, int repeats = 1)
        {
            this.Name = name;
            this.OnFire = onFire;
            this.Cooldown = cooldown;
            this.TimeToWait = cooldown;
            this.RepeatsForever = repeatsForever;
            this.RepeatsLeft = repeats;
            this.EventType = EventType.CSharp;
            this.IsSaveable = true;
        }

        /// <summary>
        /// Copy constructor for <see cref="Scheduler.ScheduledEvent"/> objects.
        /// </summary>
        /// <param name="other">The event to make a copy of.</param>
        public ScheduledEvent(ScheduledEvent other)
        {
            this.Name = other.Name;
            this.OnFire = other.OnFire;
            this.LuaFunctionName = other.LuaFunctionName;
            this.Cooldown = other.Cooldown;
            this.TimeToWait = other.Cooldown;
            this.RepeatsForever = other.RepeatsForever;
            this.RepeatsLeft = other.RepeatsLeft;
            this.EventType = other.EventType;
            this.IsSaveable = other.IsSaveable;
        }

        /// <summary>
        /// Constructs a <see cref="Scheduler.ScheduledEvent"/> object from a prototype ScheduledEvent.
        /// If eventPrototype is an EventType.Lua event, the Lua function call is
        /// looked up based on the eventPrototype.LuaFunctionName attribute and bound
        /// to a C# callback.
        /// </summary>
        /// <param name="eventPrototype">Event prototype.</param>
        /// <param name="cooldown">Cooldown in seconds.</param>
        /// <param name="timeToWait">Time to wait until next event firing.</param>
        /// <param name="repeatsForever">If set to <c>true</c> repeats forever.</param>
        /// <param name="repeats">Repeats left (only matters if repeatsForever == false).</param>
        public ScheduledEvent(ScheduledEvent eventPrototype, float cooldown, float timeToWait, bool repeatsForever = false, int repeats = 1)
        {
            this.Name = eventPrototype.Name;
            if (eventPrototype.EventType == EventType.CSharp)
            {
                this.OnFire = eventPrototype.OnFire;
            }
            else
            {
                this.OnFire = (evt) => FunctionsManager.ScheduledEvent.Call(eventPrototype.LuaFunctionName, evt);
            }

            this.Cooldown = cooldown;
            this.TimeToWait = timeToWait;
            this.RepeatsForever = repeatsForever;
            this.RepeatsLeft = repeats;
            this.EventType = eventPrototype.EventType;
            this.IsSaveable = true;
        }

        /// <summary>
        /// Construct a <see cref="Scheduler.ScheduledEvent"/> prototype backed by a C# action.
        /// </summary>
        /// <param name="name">Name of the prototype.</param>
        /// <param name="onFire">On fire action.</param>
        public ScheduledEvent(string name, Action<ScheduledEvent> onFire)
        {
            this.Name = name;
            this.OnFire = onFire;
            this.EventType = EventType.CSharp;
        }

        /// <summary>
        /// Construct a <see cref="Scheduler.ScheduledEvent"/> prototype backed by a Lua function.
        /// </summary>
        /// <param name="name">Name of the prototype.</param>
        /// <param name="luaFuctionName">Lua fuction name.</param>
        public ScheduledEvent(string name, string luaFuctionName)
        {
            this.Name = name;
            this.LuaFunctionName = luaFuctionName;
            this.EventType = EventType.Lua;
        }

        private event Action<ScheduledEvent> OnFire;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the type, which is the name. Required to implement IPrototypable.
        /// </summary>
        public string Type
        {
            get { return Name; }
        }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        public EventType EventType { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the lua function.
        /// Is null if EventType is EventType.CSharp.
        /// </summary>
        public string LuaFunctionName { get; protected set; }

        /// <summary>
        /// Gets or sets the number of repeats left.
        /// This value is ignored if RepeatsForever == true.
        /// </summary>
        public int RepeatsLeft { get; protected set; }

        /// <summary>
        /// Gets or sets the cooldown in seconds.
        /// </summary>
        public float Cooldown { get; protected set; }

        /// <summary>
        /// Gets or sets the time to wait until the next event firing in seconds.
        /// </summary>
        public float TimeToWait { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Scheduler.ScheduledEvent"/> repeats forever.
        /// </summary>
        public bool RepeatsForever { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is saveable.
        /// Used by <see cref="Scheduler.Scheduler"/> when serializing itself.
        /// </summary>
        public bool IsSaveable { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is the last shot of the <see cref="Scheduler.ScheduledEvent"/>.
        /// </summary>
        /// <value><c>true</c> if last shot; otherwise, <c>false</c>.</value>
        public bool LastShot
        {
            get
            {
                return RepeatsLeft == 1 && RepeatsForever == false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Scheduler.ScheduledEvent"/> is finished.
        /// </summary>
        /// <value><c>true</c> if finished; otherwise, <c>false</c>.</value>
        public bool Finished
        {
            get
            {
                return RepeatsLeft < 1 && RepeatsForever == false;
            }
        }

        /// <summary>
        /// Advance the event clock by the specified deltaTime, and if it drops less that or equal to zero fire the event, resetting the clock to Cooldown.
        /// Note: This fires the event multiple times if deltaTime is >= 2 * cooldown.
        /// </summary>
        /// <param name="deltaTime">Delta time in seconds (note: game time, not real time).</param>
        public void Update(float deltaTime)
        {
            this.TimeToWait -= deltaTime;

            while (this.TimeToWait <= 0)
            {
                Fire();
                this.TimeToWait += this.Cooldown;
            }
        }

        /// <summary>
        /// Fires the <see cref="Scheduler.ScheduledEvent"/>.
        /// </summary>
        public void Fire()
        {
            if (Finished)
            {
                UnityDebugger.Debugger.Log("ScheduledEvent", "Scheduled event '" + Name + "' finished last repeat already -- not firing again.");
                return;
            }

            if (this.OnFire != null)
            {
                this.OnFire(this);
            }

            this.RepeatsLeft -= 1;
        }

        /// <summary>
        /// Stops the <see cref="Scheduler.ScheduledEvent"/>.
        /// </summary>
        public void Stop()
        {
            RepeatsLeft = 0;
            RepeatsForever = false;
        }

        public void SetCooldown(float newCooldown)
        {
            // Decrement TimeToWait by the amount of time that has passed since the last time this event fired
            TimeToWait = newCooldown - (Cooldown - TimeToWait);
            if (TimeToWait < 0)
            {
                TimeToWait = 0;
            }

            Cooldown = newCooldown;
        }

        public void ReadXmlPrototype(XmlReader reader)
        {
            this.Name = reader.GetAttribute("name");
            this.LuaFunctionName = reader.GetAttribute("onFire");
            this.EventType = EventType.Lua;
        }

        public JObject ToJson()
        {
            JObject eventJObject = new JObject();
            eventJObject.Add("Name", Name);
            eventJObject.Add("Cooldown", Cooldown);
            eventJObject.Add("TimeToWait", TimeToWait);
            eventJObject.Add("RepeatsForever", RepeatsForever);
            eventJObject.Add("RepeatsLeft", RepeatsLeft);

            return eventJObject;
        }
    }
}
