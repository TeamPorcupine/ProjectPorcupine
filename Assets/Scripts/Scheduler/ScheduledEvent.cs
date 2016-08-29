﻿#region License
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

namespace Scheduler
{
    /// <summary>
    /// The <see cref="Scheduler.ScheduledEvent"/> class represents an individual event which is handled by the Scheduler.
    /// May "fire" either a C# or Lua function.
    /// </summary>
    public class ScheduledEvent : IXmlSerializable
    {
        // TODO: Lua event handling!
        // TODO: Serialization!
        private string luaFunctionName; // not used for anything yet

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler.ScheduledEvent"/> class.
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
        }

        public ScheduledEvent(ScheduledEvent other)
        {
            this.Name = other.Name;
            this.OnFire = other.OnFire;
            this.Cooldown = other.Cooldown;
            this.TimeToWait = other.Cooldown;
            this.RepeatsForever = other.RepeatsForever;
            this.RepeatsLeft = other.RepeatsLeft;
        }

        public ScheduledEvent(EventPrototype eventPrototype, float cooldown, float timeToWait, bool repeatsForever = false, int repeats = 1)
        {
            this.Name = eventPrototype.Name;
            this.OnFire = eventPrototype.OnFireCallback();
            this.Cooldown = cooldown;
            this.TimeToWait = timeToWait;
            this.RepeatsForever = repeatsForever;
            this.RepeatsLeft = repeats;
        }

        private event Action<ScheduledEvent> OnFire;

        public string Name { get; protected set; }

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
