#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

namespace ProjectPorcupine.PowerNetwork
{
    public interface IPlugable
    {
        float InputRate { get; }

        float OutputRate { get; }

        bool IsProducer { get; }

        bool IsConsumer { get; }

        bool IsAccumulator { get; }

        float AccumulatedAmount { get; set; }

        float AccumulatorCapacity { get; }

        bool IsFull { get; }

        bool IsEmpty { get; }
    }
}
