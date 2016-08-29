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
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Scheduler
{
    /// <summary>
    /// Generic scheduler class for tracking and dispatching ScheduledEvents.
    /// </summary>
    public class Scheduler : IXmlSerializable
    {
        private static Scheduler instance;

        public Scheduler()
        {
            this.Events = new List<ScheduledEvent>();
            this.EventPrototypes = GenerateEventPrototypes();
        }

        public static Scheduler Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new Scheduler();
                }

                return instance;
            }
        }

        public List<ScheduledEvent> Events { get; protected set; }

        public Dictionary<string, EventPrototype> EventPrototypes { get; protected set; }

        public void RegisterEvent(ScheduledEvent evt)
        {
            if (evt != null)
            {
                if (IsRegistered(evt))
                {
                    Debug.ULogChannel("Scheduler", "Event '{0}' registered more than once.", evt.Name);
                }

                Events.Add(evt);
            }
        }

        public bool IsRegistered(ScheduledEvent evt)
        {
            return Events != null && Events.Contains(evt);
        }

        public void DeregisterEvent(ScheduledEvent evt)
        {
            if (Events != null)
            {
                Events.Remove(evt);
            }
        }

        public void PurgeEventList()
        {
            if (Events != null)
            {
                Events.RemoveAll((e) => e.Finished);
            }
        }

        /// <summary>
        /// Time to next event in seconds.
        /// </summary>
        /// <returns>The to next event (-1 if there are no events).</returns>
        public float TimeToNextEvent()
        {
            if (Events == null || Events.Count == 0)
            {
                return -1f;
            }

            return Events.Min((e) => e.TimeToWait);
        }

        public void Update(float deltaTime)
        {
            if (Events == null || Events.Count == 0)
            {
                return;
            }

            foreach (ScheduledEvent evt in Events)
            {
                evt.Update(deltaTime);
            }

            // TODO: this is an O(n) operation every tick.
            // Potentially this could be optimized by delaying purging!
            PurgeEventList();
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
            throw new NotImplementedException();
        }

        #endregion

        private static Dictionary<string, EventPrototype> GenerateEventPrototypes()
        {
            // FIXME: Just adding in a simple log pinging event in lieu of 
            // properly reading prototypes from config files.
            Dictionary<string, EventPrototype> prototypes = new Dictionary<string, EventPrototype>();
            prototypes.Add(
                "ping_log",
                new EventPrototype(
                    "ping_log",
                    (evt) => Debug.ULogChannel("ScheduledEventTest", "Event {0} fired", evt.Name)));
            return prototypes;
        }
    }
}
