#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;

namespace ProjectPorcupine.PowerNetwork
{
    public interface IPluggable
    {
        event Action Reconnecting;

        float InputRate { get; }

        float OutputRate { get; }

        bool IsProducer { get; }

        bool IsConsumer { get; }

        bool IsStorage { get; }

        float StoredAmount { get; set; }

        float StorageCapacity { get; }

        bool IsFull { get; }

        bool IsEmpty { get; }

        string UtilityType { get; }

        string SubType { get; set; }

        void Reconnect();
    }
}
