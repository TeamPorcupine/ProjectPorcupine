#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// Holds temperature data in Kelvin but can be accessed in both celsius and farenheit.
/// </summary>
[MoonSharpUserData]
public class TemperatureUnit
{
    /// <summary>
    /// The internal temperature of the system.
    /// Its stored in kelvin.
    /// </summary>
    private float internalTemperature;

    /// <summary>
    /// Create from Kelvin units.
    /// </summary>
    /// <param name="tempInKelvin"> Temperature in Kelvin. </param>
    public TemperatureUnit(float tempInKelvin)
    {
        this.internalTemperature = tempInKelvin;
    }

    /// <summary>
    /// The current temperature in Kelvin.
    /// </summary>
    public float TemperatureInKelvin
    {
        get
        {
            return internalTemperature;
        }

        set
        {
            internalTemperature = value;
            internalTemperature = Mathf.Clamp(internalTemperature, 0, internalTemperature);

            if (float.IsInfinity(internalTemperature) || float.IsNaN(internalTemperature))
            {
                internalTemperature = 0;
            }
        }
    }

    /// <summary>
    /// The current temperature in Celsius.
    /// </summary>
    public float TemperatureInCelsius
    {
        get
        {
            return internalTemperature - 273.15f;
        }

        set
        {
            internalTemperature = value + 273.15f;
        }
    }

    /// <summary>
    /// The current temperature in Farenheit.
    /// </summary>
    public float TemperatureInFarenheit
    {
        get
        {
            return (internalTemperature * 1.8f) - 459.67f;
        }

        set
        {
            // Its 5/9 because that's equal to approx 0.5555... so it's more accurate like this.
            internalTemperature = (value + 459.67f) * (5 / 9);
        }
    }

    /// <summary>
    /// The current temperature in Rankine.
    /// </summary>
    public float TemperatureInRankine
    {
        get
        {
            return internalTemperature * 1.8f;
        }

        set
        {
            internalTemperature = value * (5 / 9);
        }
    }

    /// <summary>
    /// Returns a string of the current temperature in format: K: x, C: C(x), F: F(x).
    /// Where C(x) and F(x) mean the celsius/farenheit conversion of x.
    /// </summary>
    /// <remarks> Good for debuggging. </remarks>
    public override string ToString()
    {
        return "K: " + TemperatureInKelvin + " C: " + TemperatureInCelsius + " F: " + TemperatureInFarenheit + " R: " + TemperatureInRankine;
    }

    public string ToFarenheitCelsiusString()
    {
        return string.Format("({0:0.00}C)\n({1:0.00}F)\n", TemperatureInCelsius, TemperatureInFarenheit);
    }
}
