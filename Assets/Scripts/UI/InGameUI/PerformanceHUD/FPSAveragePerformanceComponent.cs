#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the average FPS over a 30 second period.
/// </summary>
public class FPSAveragePerformanceComponent : BasePerformanceHUDElement
{
    private const float FPSMeasurePeriod = 5f;
    private const string Display = "Avg: {0}";

    private int fpsAccumulator = 0;
    private float fpsFinishPeriod = 0;
    private int currentFps;

    //public override string NameOfComponent()
    //{
    //    return "UI/TextPerformanceComponentUI";
    // }

    // The shown FPS will be 0 for the first second until it ticks over correctly
    public override void Update()
    {
        // measure average frames per second
        fpsAccumulator++;
        if (Time.realtimeSinceStartup > fpsFinishPeriod)
        {
            currentFps = (int)(fpsAccumulator / FPSMeasurePeriod);

            fpsAccumulator = 0;

            fpsFinishPeriod += FPSMeasurePeriod;

            //UITextElement.text = string.Format(Display, currentFps);
        }
    }

    public override GameObject InitializeElement()
    {
        // Build Gameobject
        fpsFinishPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;

        //UITextElement.text = "Avg: ...";

        return null;
    }
}
