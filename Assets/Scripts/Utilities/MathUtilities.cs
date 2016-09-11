#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;

/// <summary>
/// Different mathematical calculations. 
/// </summary>
public static class MathUtilities
{
    /// <summary>
    /// If a - b is less than double.Epsilon value then they are treated as equal.
    /// </summary>
    /// <returns>true if a - b &lt; tolerance else false.</returns>
    public static bool AreEqual(this double a, double b)
    {
        if (a.CompareTo(b) == 0)
        {
            return true;
        }

        return Math.Abs(a - b) < double.Epsilon;
    }

    /// <summary>
    /// If value is lower than double.Epsilon value then value is treated as zero.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>true if value is lower than tolerance value.</returns>
    public static bool IsZero(this double value)
    {
        return Math.Abs(value) < double.Epsilon;
    }

    /// <summary>
    /// If a - b is less than double.Epsilon value then they are treated as equal.
    /// </summary>
    /// <returns>true if a - b &lt; tolerance else false.</returns>
    public static bool AreEqual(this float a, float b)
    {
        if (a.CompareTo(b) == 0)
        {
            return true;
        }

        return Math.Abs(a - b) < double.Epsilon;
    }

    /// <summary>
    /// If value is lower than double.Epsilon value then value is treated as zero.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>true if value is lower than tolerance value.</returns>
    public static bool IsZero(this float value)
    {
        return Math.Abs(value) < double.Epsilon;
    }

    /// <summary>
    /// Clamps value between min and max and returns value.
    /// </summary>
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
        {
            return min;
        }

        return value.CompareTo(max) > 0 ? max : value;
    }
}
