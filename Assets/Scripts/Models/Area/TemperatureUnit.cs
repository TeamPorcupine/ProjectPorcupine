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
using System.Threading;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class TemperatureUnit
{
    public TemperatureUnit(float kelvin)
    {
        this.temperatureInKelvin = kelvin;
    }

    public void UpdateTemperature(float newTemp)
    {
        temperatureInKelvin = newTemp;
        temperatureInKelvin = Mathf.Clamp(temperatureInKelvin, 0, temperatureInKelvin);
    }

    public void IncreaseTemperature(float amount)
    {
        temperatureInKelvin += amount;
        temperatureInKelvin = Mathf.Clamp(temperatureInKelvin, 0, temperatureInKelvin);
    }

    public override string ToString()
    {
        return "K: " + temperatureInKelvin + " C: " + temperatureInCelsius + " F: " + temperatureInFarenheit;
    }

    public float temperatureInKelvin;

    public float temperatureInCelsius
    {
        get
        {
            return temperatureInKelvin - 273.15f;
        }
    }

    public float temperatureInFarenheit
    {
        get
        {
            return (temperatureInKelvin * 1.8f) - 459.67f;
        }
    }
}