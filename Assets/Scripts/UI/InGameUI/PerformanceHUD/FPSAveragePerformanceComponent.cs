#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;

/// <summary>
/// Displays the average FPS over a 30 second period.
/// </summary>
public class FPSAveragePerformanceComponent : BasePerformanceComponent
{
    private const float FPSMeasurePeriod = 5f;
    private const string Display = "Avg: {0}";

    private int fpsAccumulator = 0;
    private float fpsFinishPeriod = 0;
    private int currentFps;

    private TextPerformanceComponentUI component;

    public override int PriorityID()
    {
        // By default will be first component shown
        return 0;
    }

    public override BasePerformanceComponentUI UIComponent()
    {
        return component;
    }

    public override string NameOfComponent()
    {
        return "UI/TextPerformanceComponentUI";
    }

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

            component.ChangeText(string.Format(Display, currentFps));
        }
    }

    public override void Start(BasePerformanceComponentUI componentUI)
    {
        component = (TextPerformanceComponentUI)componentUI;

        fpsFinishPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;

        component.ChangeText("Avg: ...");
    }
}
