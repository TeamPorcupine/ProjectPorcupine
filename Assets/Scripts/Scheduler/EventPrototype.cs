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
    public class EventPrototype
    {
        public EventPrototype(string name, Action<ScheduledEvent> onFire)
        {
            this.Name = name;
            this.OnFire = onFire;
        }

        private event Action<ScheduledEvent> OnFire;

        public string Name { get; protected set; }

        public Action<ScheduledEvent> OnFireCallback()
        {
            return this.OnFire;
        }
    }
}
