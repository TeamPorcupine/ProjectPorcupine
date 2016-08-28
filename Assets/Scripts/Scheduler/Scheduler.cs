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
    public class Scheduler
    {
        public Scheduler()
        {
            if (Current == null)
            {
                Current = this;
            }
        }

        public Scheduler Current { get; protected set; }

        public List<ScheduledEvent> Events { get; protected set; }

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
            if (Events == null)
            {
                return false;
            }

            return Events.Contains(evt);
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

        public float TimeToNextEvent()
        {
            if (Events == null || Events.Count == 0)
            {
                return null;
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
        }
    }
}
