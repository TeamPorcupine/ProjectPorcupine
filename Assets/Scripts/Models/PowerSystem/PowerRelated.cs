#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

public class PowerRelated
{
    public float InputRate { get; set; }

    public float OutputRate { get; set; }

    public float Capacity { get; set; }

    public float AccumulatedPower { get; set; }

    public bool IsEmpty
    {
        get
        {
            return AccumulatedPower.IsZero();
        }
    }

    public bool IsFull
    {
        get
        {
            return AccumulatedPower.AreEqual(Capacity);
        }
    }

    public bool IsPowerProducer
    {
        get
        {
            return InputRate.IsZero() && OutputRate > 0.0f;
        }
    }

    public bool IsPowerConsumer
    {
        get
        {
            return OutputRate.IsZero() && InputRate > 0.0f;
        }
    }

    public bool IsPowerAccumulator
    {
        get
        {
            return Capacity > 0.0f;
        }
    }
}
