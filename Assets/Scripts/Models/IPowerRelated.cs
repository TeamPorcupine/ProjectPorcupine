#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;

public interface IPowerRelated
{
    event Action<IPowerRelated> PowerValueChanged; 

    /// <summary>
    /// Amount of power object produce or consume. Positive for produce, negative for consume.
    /// </summary>
    float PowerValue { get; }

    /// <summary>
    /// Determines whether object produce or consume power.
    /// </summary>
    bool IsPowerConsumer { get; }

    /// <summary>
    /// Determines whether object is plugged to power grid and has power.
    /// </summary>
    bool HasPower();
}
