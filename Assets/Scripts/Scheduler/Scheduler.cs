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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Scheduler
{
    /// <summary>
    /// Generic scheduler class for tracking and dispatching ScheduledEvents.
    /// </summary>
    public class Scheduler : IXmlSerializable
    {
        private static Scheduler instance;
        private List<ScheduledEvent> events;
        private List<ScheduledEvent> eventsToAddNextTick;

        public Scheduler()
        {
            this.events = new List<ScheduledEvent>();
            this.eventsToAddNextTick = new List<ScheduledEvent>();

            Debug.ULogChannel("Scheduler", "Loading Lua stripts");

            // FIXME: Are these actually needed here?
            LuaUtilities.RegisterGlobal(typeof(Inventory));
            LuaUtilities.RegisterGlobal(typeof(Job));
            LuaUtilities.RegisterGlobal(typeof(ModUtils));
            LuaUtilities.RegisterGlobal(typeof(World));
            LoadScripts();
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

        public ReadOnlyCollection<ScheduledEvent> Events
        {
            get
            {
                return new ReadOnlyCollection<ScheduledEvent>(events);
            }
        }

        #region LuaHandling

        public static void LoadScripts()
        {
            string luaFilePath = Path.Combine(Application.streamingAssetsPath, "LUA");
            luaFilePath = Path.Combine(luaFilePath, "Events.lua");
            LuaUtilities.LoadScriptFromFile(luaFilePath);
        }

        public static void LoadModsScripts(DirectoryInfo[] mods)
        {
            foreach (DirectoryInfo mod in mods)
            {
                string luaModFile = Path.Combine(mod.FullName, "Events.lua");
                if (File.Exists(luaModFile))
                {
                    LuaUtilities.LoadScriptFromFile(luaModFile);
                }
            }
        }

        #endregion

        /// <summary>
        /// Schedules an event from a prototype.
        /// </summary>
        /// <param name="name">Name of the event (for serialization etc.).</param>
        /// <param name="cooldown">Cooldown in seconds.</param>
        /// <param name="repeatsForever">Whether the event repeats forever (defaults false). If true repeats is ignored.</param>
        /// <param name="repeats">Number of repeats (default 1). Ignored if repeatsForever=true.</param>
        public void ScheduleEvent(string name, float cooldown, bool repeatsForever = false, int repeats = 1)
        {
            ScheduleEvent(name, cooldown, cooldown, repeatsForever, repeats);
        }

        /// <summary>
        /// Schedules an event from a prototype.
        /// </summary>
        /// <param name="name">Name of the event (for serialization etc.).</param>
        /// <param name="cooldown">Cooldown in seconds.</param>
        /// <param name="timeToWait">Time to wait before next firing in seconds.</param>
        /// <param name="repeatsForever">Whether the event repeats forever (defaults false). If true repeats is ignored.</param>
        /// <param name="repeats">Number of repeats (default 1). Ignored if repeatsForever=true.</param>
        public void ScheduleEvent(string name, float cooldown, float timeToWait, bool repeatsForever = false, int repeats = 1)
        {
            if (PrototypeManager.SchedulerEvent.Has(name) == false)
            {
                Debug.ULogWarningChannel("Scheduler", "Tried to schedule an event from a prototype '{0}' which does not exist. Bailing.", name);
                return;
            }

            ScheduledEvent ep = PrototypeManager.SchedulerEvent.Get(name);
            ScheduledEvent evt = new ScheduledEvent(ep, cooldown, timeToWait, repeatsForever, repeats);

            RegisterEvent(evt);
        }

        public void RegisterEvent(ScheduledEvent evt)
        {
            if (evt != null)
            {
                if (IsRegistered(evt))
                {
                    Debug.ULogChannel("Scheduler", "Event '{0}' registered more than once.", evt.Name);
                }

                eventsToAddNextTick.Add(evt);
            }
        }

        public bool IsRegistered(ScheduledEvent evt)
        {
            return (events != null && events.Contains(evt)) || (eventsToAddNextTick != null && eventsToAddNextTick.Contains(evt));
        }

        /// <summary>
        /// Deregisters the event.
        /// NOTE: This actually calls Stop() on the event so that it will no longer be run.
        /// It will be removed on the next call of ClearFinishedEvents().
        /// </summary>
        /// <param name="evt">Event to deregister.</param>
        public void DeregisterEvent(ScheduledEvent evt)
        {
            if (evt != null)
            {
                evt.Stop();
            }
        }

        /// <summary>
        /// Time to next event in seconds.
        /// </summary>
        /// <returns>The to next event (-1 if there are no events).</returns>
        public float TimeToNextEvent()
        {
            if ((events == null || events.Count == 0) && (eventsToAddNextTick == null || eventsToAddNextTick.Count == 0))
            {
                return -1f;
            }

            // additions to the event list which were queued up last tick
            events.AddRange(eventsToAddNextTick);
            eventsToAddNextTick.Clear();

            return events.Min((e) => e.TimeToWait);
        }

        public void Update(float deltaTime)
        {
            if ((events == null || events.Count == 0) && (eventsToAddNextTick == null || eventsToAddNextTick.Count == 0))
            {
                // no events in the queue
                return;
            }

            // additions to the event list which were queued up last tick
            events.AddRange(eventsToAddNextTick);
            eventsToAddNextTick.Clear();

            foreach (ScheduledEvent evt in events)
            {
                evt.Update(deltaTime);
            }

            // TODO: this is an O(n) operation every tick.
            // Potentially this could be optimized by delaying purging!
            ClearFinishedEvents();
        }

        public void RegisterEventPrototype(string name, ScheduledEvent eventPrototype)
        {
            PrototypeManager.SchedulerEvent.Add(name, eventPrototype);
        }

        #region IXmlSerializable implementation

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Debug.ULogChannel("Scheduler", "Reading save file...", Events.Count);
            CleanUp();

            if (reader.ReadToDescendant("Event"))
            {
                do
                {
                    // there are no sensible defaults for name and cooldown so we do want to throw an exception if their xml is borked
                    string name = reader.GetAttribute("name");
                    float cooldown = float.Parse(reader.GetAttribute("cooldown"));

                    float timeToWait = cooldown;
                    float.TryParse(reader.GetAttribute("timeToWait"), out timeToWait);

                    bool repeatsForever = false;
                    bool.TryParse(reader.GetAttribute("repeatsForever"), out repeatsForever);

                    int repeatsLeft = 0;
                    int.TryParse(reader.GetAttribute("repeatsLeft"), out repeatsLeft);

                    this.ScheduleEvent(name, cooldown, timeToWait, repeatsForever, repeatsLeft);
                }
                while (reader.ReadToNextSibling("Event"));
            }
            else
            {
                Debug.ULogWarningChannel("Scheduler", "Malformed 'Scheduler' serialization: does not have any 'Event' elements.");
            }

            this.Update(0); // update the event list
            Debug.ULogChannel("Scheduler", "Save file loaded. Event queue contains {0} events.", Events.Count);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Scheduler");
            foreach (ScheduledEvent evt in Events)
            {
                evt.WriteXml(writer);
            }

            writer.WriteEndElement();
        }

        #endregion

        private void ClearFinishedEvents()
        {
            if (events != null)
            {
                events.RemoveAll((evt) => evt.Finished);
            }

            if (eventsToAddNextTick != null)
            {
                eventsToAddNextTick.RemoveAll((evt) => evt.Finished);
            }
        }

        private void CleanUp()
        {
            if (events != null)
            {
                foreach (ScheduledEvent evt in events)
                {
                    evt.Stop();
                }
            }

            if (eventsToAddNextTick != null)
            {
                foreach (ScheduledEvent evt in eventsToAddNextTick)
                {
                    evt.Stop();
                }
            }

            events = new List<ScheduledEvent>();
            eventsToAddNextTick = new List<ScheduledEvent>();
        }
    }
}
