#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace Scheduler
{
    /// <summary>
    /// Generic scheduler class for tracking and dispatching ScheduledEvents.
    /// </summary>
    [MoonSharpUserData]
    public class Scheduler
    {
        private static Scheduler instance;
        private List<ScheduledEvent> events;
        private List<ScheduledEvent> eventsToAddNextTick;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler.Scheduler"/> class.
        /// Note: you probably want to use <see cref="Scheduler.Current"/> to get the singleton instance of the main game scheduler.
        /// </summary>
        public Scheduler()
        {
            this.events = new List<ScheduledEvent>();
            this.eventsToAddNextTick = new List<ScheduledEvent>();

            TimeManager.Instance.EveryFrameUnpaused += Update;
        }

        /// <summary>
        /// Get the singleton instance of the main game scheduler.
        /// If it does not exist yet it is created on demand.
        /// </summary>
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

        /// <summary>
        /// Gets the events currently queued for execution by the scheduler.
        /// </summary>
        /// <value>The events.</value>
        public ReadOnlyCollection<ScheduledEvent> Events
        {
            // FIXME: Currently does not include eventsToAddNextTick!
            get
            {
                return new ReadOnlyCollection<ScheduledEvent>(events);
            }
        }

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
            if (PrototypeManager.ScheduledEvent.Has(name) == false)
            {
                UnityDebugger.Debugger.LogWarningFormat("Scheduler", "Tried to schedule an event from a prototype '{0}' which does not exist. Bailing.", name);
                return;
            }

            ScheduledEvent ep = PrototypeManager.ScheduledEvent.Get(name);
            ScheduledEvent evt = new ScheduledEvent(ep, cooldown, timeToWait, repeatsForever, repeats);

            RegisterEvent(evt);
        }

        /// <summary>
        /// Registers a ScheduledEvent to be tracked by the scheduler.
        /// </summary>
        public void RegisterEvent(ScheduledEvent evt)
        {
            if (evt != null)
            {
                if (IsRegistered(evt))
                {
                    UnityDebugger.Debugger.LogFormat("Scheduler", "Event '{0}' registered more than once.", evt.Name);
                }

                eventsToAddNextTick.Add(evt);
            }
        }

        /// <summary>
        /// Determines whether this ScheduledEvent is registered with the scheduler.
        /// </summary>
        public bool IsRegistered(ScheduledEvent evt)
        {
            return (events != null && events.Contains(evt)) || (eventsToAddNextTick != null && eventsToAddNextTick.Contains(evt));
        }

        /// <summary>
        /// Deregisters the event.
        /// NOTE: This actually calls Stop() on the event so that it will no longer be run.
        /// It will be removed on the next call of Update().
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

        /// <summary>
        /// Update the scheduler by the specified deltaTime, running event callbacks as needed.
        /// </summary>
        /// <param name="deltaTime">Delta time in seconds.</param>
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

        /// <summary>
        /// Registers an event prototype with PrototypeManager.
        /// </summary>
        public void RegisterEventPrototype(ScheduledEvent eventPrototype)
        {
            PrototypeManager.ScheduledEvent.Add(eventPrototype);
        }

        public JToken ToJson()
        {
            JArray eventsJArray = new JArray();

            foreach (ScheduledEvent evt in Events)
            {
                if (evt.IsSaveable == false)
                {
                    continue;
                }

                eventsJArray.Add(evt.ToJson());
            }

            return eventsJArray;
        }

        public void FromJson(JToken schedulerToken)
        {
            if (schedulerToken == null)
            {
                return;
            }

            CleanUp();
            JArray schedulerJArray = (JArray)schedulerToken;

            foreach (JToken eventToken in schedulerJArray)
            {
                string name = (string)eventToken["Name"];
                float cooldown = (float)eventToken["Cooldown"];
                float timeToWait = (float)eventToken["TimeToWait"];
                bool repeatsForever = (bool)eventToken["RepeatsForever"];
                int repeatsLeft = (int)eventToken["RepeatsLeft"];

                ScheduleEvent(name, cooldown, timeToWait, repeatsForever, repeatsLeft);
            }

            Update(0); // update the event list
        }

        /// <summary>
        /// Destroy this instance.
        /// </summary>
        public void Destroy()
        {
            instance = null;
        }

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

        /// <summary>
        /// Stops all events and clobbers the queue.
        /// </summary>
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
