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

namespace UnityStandardAssets.Utility
{
    [RequireComponent(typeof(Text))]
    public class FPSCounter : MonoBehaviour
    {
        private const float FPSMeasurePeriod = 0.5f;
        private const string Display = "{0} FPS";
        private int fpsAccumulator = 0;
        private float fpsNextPeriod = 0;
        private int currentFps;
        private Text text;

        private void Start()
        {
            fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;
            text = GetComponent<Text>();
        }
        
        private void Update()
        {
            // measure average frames per second
            fpsAccumulator++;
            if (Time.realtimeSinceStartup > fpsNextPeriod)
            {
                currentFps = (int)(fpsAccumulator / FPSMeasurePeriod);
                fpsAccumulator = 0;
                fpsNextPeriod += FPSMeasurePeriod;
                text.text = string.Format(Display, currentFps);
            }
        }
    }
}
