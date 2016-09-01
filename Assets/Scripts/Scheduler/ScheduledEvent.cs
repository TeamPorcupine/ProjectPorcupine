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
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;

namespace Scheduler
{
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
    public class ScheduledEvent : IXmlSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler.ScheduledEvent"/> class.
        /// This form of the constructor assumes the ScheduledEvent is of the EventType.CSharp type.
        /// </summary>
        /// <param name="name">Name of the event (for serialization etc.).</param>
        /// <param name="onFire">Callback to call when the event fires.</param>
        /// <param name="cooldown">Cooldown in seconds.</param>
        /// <param name="repeatsForever">Whether the event repeats forever (defaults false). If true repeats is ignored.</param>
        /// <param name="repeats">Number of repeats (default 1). Ignored if repeatsForever=true.</param>
        public ScheduledEvent(string name, Action<ScheduledEvent> onFire, float cooldown, bool repeatsForever = false, int repeats = 1)
        {
            this.Name = name;
            this.OnFire = onFire;
            this.Cooldown = cooldown;
            this.TimeToWait = cooldown;
            this.RepeatsForever = repeatsForever;
            this.RepeatsLeft = repeats;
            this.EventType = EventType.CSharp;
        }

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
        }

        public ScheduledEvent(ScheduledEvent eventPrototype, float cooldown, float timeToWait, bool repeatsForever = false, int repeats = 1)
        {
            this.Name = eventPrototype.Name;
            if (eventPrototype.EventType == EventType.CSharp)
            {
                this.OnFire = eventPrototype.OnFire;
            }
            else
            {
                this.OnFire = (evt) => LuaUtilities.CallFunction(eventPrototype.LuaFunctionName, evt);
            }

            this.Cooldown = cooldown;
            this.TimeToWait = timeToWait;
            this.RepeatsForever = repeatsForever;
            this.RepeatsLeft = repeats;
            this.EventType = eventPrototype.EventType;
        }

        public ScheduledEvent(string name, Action<ScheduledEvent> onFire)
        {
            this.Name = name;
            this.OnFire = onFire;
            this.EventType = EventType.CSharp;
        }

        public ScheduledEvent(string name, string luaFuctionName)
        {
            this.Name = name;
            this.LuaFunctionName = luaFuctionName;
            this.EventType = EventType.Lua;
        }

        private event Action<ScheduledEvent> OnFire;

        public string Name { get; protected set; }

        public EventType EventType { get; protected set; }

        public string LuaFunctionName { get; protected set; }

        public int RepeatsLeft { get; protected set; }

        public float Cooldown { get; protected set; }

        public float TimeToWait { get; protected set; }

        public bool RepeatsForever { get; protected set; }

        public bool LastShot
        {
            get
            {
                return RepeatsLeft == 1 && RepeatsForever == false;
            }
        }

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

        public void Fire()
        {
            if (Finished)
            {
                Debug.ULogChannel("ScheduledEvent", "Scheduled event '" + Name + "' finished last repeat already -- not firing again.");
                return;
            }

            if (this.OnFire != null)
            {
                this.OnFire(this);
            }

            this.RepeatsLeft -= 1;
        }

        public void Stop()
        {
            RepeatsLeft = 0;
            RepeatsForever = false;
        }

        #region IXmlSerializable implementation

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Event");
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("cooldown", this.Cooldown.ToString());
            writer.WriteAttributeString("timeToWait", this.TimeToWait.ToString());
            if (this.RepeatsForever)
            {
                writer.WriteAttributeString("repeatsForever", this.RepeatsForever.ToString());
            }
            else
            {
                writer.WriteAttributeString("repeatsLeft", this.RepeatsLeft.ToString());
            }

            writer.WriteEndElement();
        }

        #endregion
    }
}
