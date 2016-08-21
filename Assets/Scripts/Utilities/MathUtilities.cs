#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Linq;

/// <summary>
/// Different mathemathical calculations. 
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

    /// <returns>Returns null if both arguments are null, otherwise the sum of the arguments.</returns>
    public static double? Add(double? amount1, double? amount2)
    {
        return Sum(amount1, amount2);
    }

    /// <returns>Returns null if all values are null, otherwise the sum of the values.</returns>
    public static double? Sum(params double?[] values)
    {
        if (values.Any(value => value.HasValue))
        {
            return values.Sum(value => value ?? 0d);
        }

        return null;
    }

    /// <returns>Returns null if the denominator is null or 0, otherwise the division.</returns>
    public static double? Divide(double? numerator, double? denominator)
    {
        if (!denominator.HasValue || denominator.Value.IsZero())
        {
            return null;
        }

        return (numerator ?? 0d) / denominator.Value;
    }

    /// <returns>Returns null if both arguments are null, otherwise the difference between the arguments.</returns>
    public static double? Subtract(double? amount1, double? amount2)
    {
        if (amount1.HasValue || amount2.HasValue)
        {
            return (amount1 ?? 0d) - (amount2 ?? 0d);
        }

        return null;
    }
}
