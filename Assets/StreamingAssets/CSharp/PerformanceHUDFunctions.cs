using System.Collections;
using DeveloperConsole;
using ProjectPorcupine.Localization;
using UnityEngine.UI;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
    using UnityEngine.Profiling;
#endif

public static class PerformanceHUDFunctions
{
    public static FPSRangePerformanceComponent GetFPSRangePerformanceComponent()
    {
        return new FPSRangePerformanceComponent();
    }

    public static FPSPerformanceComponent GetFPSPerformanceComponent()
    {
        return new FPSPerformanceComponent();
    }

    public static NetworkPerformanceComponent GetNetworkPerformanceComponent()
    {
        return new NetworkPerformanceComponent();
    }

    public static MemoryPerformanceComponent GetMemoryPerformanceComponent()
    {
        return new MemoryPerformanceComponent();
    }

    /// <summary>
    /// Displays the currently used memory and the currently allocated memory.
    /// </summary>
    public class MemoryPerformanceComponent : BasePerformanceHUDElement
    {
        private Text UITextElement;
        private const string Display = "Mem/Alloc: {0}mb\n{1}mb";

        public override void Update()
        {
            UITextElement.text = string.Format(Display, (Profiler.GetTotalReservedMemory() / 1024) / 1024, (Profiler.GetTotalAllocatedMemory() / 1024) / 1024);
        }

        public override GameObject InitializeElement()
        {
            // Build Gameobject
            GameObject element = GetHorizontalBaseElement("Memory", 80, 60, allocatedHeight: 60, allocatedWidth: 80, alignment: TextAnchor.MiddleLeft);

            UITextElement = CreateText("Mem/Alloc: ...", false, TextAnchor.MiddleCenter);
            UITextElement.transform.SetParent(element.transform);
            UITextElement.fontSize = 14;

            return element;
        }

        public override string GetName()
        {
            return "MemoryPerformanceComponent";
        }
    }


    /// <summary>
    /// Currently not used but may be used in future is just network.
    /// </summary>
    public class NetworkPerformanceComponent : BasePerformanceHUDElement
    {
        private Text UITextElement;

        public override void Update()
        {
            UITextElement.text = "0ms";
        }

        public override GameObject InitializeElement()
        {
            // Build Gameobject
            GameObject element = GetHorizontalBaseElement("Network", 80, 60, allocatedHeight: 60, allocatedWidth: 80, alignment: TextAnchor.MiddleLeft);

            UITextElement = CreateText("...ms", false, TextAnchor.MiddleCenter);
            UITextElement.transform.SetParent(element.transform);

            return element;
        }

        public override string GetName()
        {
            return "NetworkPerformanceComponent";
        }
    }

    /// <summary>
    /// Displays current FPS (over a certain period).
    /// </summary>
    public class FPSPerformanceComponent : BasePerformanceHUDElement
    {
        private float FPSMeasurePeriod = 0.5f;
        private string display = "FPS: ";
        private bool displayColor = true;

        private Color GreenColor = Color.green;
        private Color RedColor = Color.red;
        private Color YellowColor = Color.yellow;

        private int fpsAccumulator = 0;
        private float fpsNextPeriod = 0;
        private int currentFps = 0;

        private Text UITextElement;


        // The shown FPS will be 0 for the first second until it ticks over correctly
        public override void Update()
        {
            // measure average frames per second
            fpsAccumulator++;
            if (Time.realtimeSinceStartup > fpsNextPeriod)
            {
                currentFps = (int)(fpsAccumulator / FPSMeasurePeriod);
                fpsAccumulator = 0;

                fpsNextPeriod += FPSMeasurePeriod;

                // Colour Changing
                if (displayColor)
                {
                    if (currentFps > 55)
                    {
                        UITextElement.color = GreenColor;
                    }
                    else if (currentFps >= 30 && currentFps <= 55)
                    {
                        UITextElement.color = YellowColor;
                    }
                    else
                    {
                        UITextElement.color = RedColor;
                    }
                }

                UITextElement.text = display + currentFps;
            }
        }

        public override GameObject InitializeElement()
        {
            FPSMeasurePeriod = this.parameterData.ContainsKey("MeasurePeriod") ? this.parameterData["MeasurePeriod"].ToFloat() : 0.5f;
            display = this.parameterData.ContainsKey("DisplayText") ? this.parameterData["DisplayText"].ToString() : "FPS: ";
            displayColor = this.parameterData.ContainsKey("DisplayColor") ? this.parameterData["DisplayColor"].ToBool() : true;

            fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;

            // Build Gameobject
            GameObject element = GetHorizontalBaseElement("FPS", 80, 60, allocatedHeight: 60, allocatedWidth: 80, alignment: TextAnchor.MiddleLeft);

            UITextElement = CreateText(display + "...", false, TextAnchor.MiddleCenter);
            UITextElement.text = display + currentFps;

            UITextElement.transform.SetParent(element.transform);
            UITextElement.fontSize = 20;

            return element;
        }

        public override string GetName()
        {
            return "FPSPerformanceComponent";
        }
    }


    /// <summary>
    /// Just measures the max and min range for FPS.
    /// </summary>
    public class FPSRangePerformanceComponent : BasePerformanceHUDElement
    {
        private const float FPSMeasurePeriod = 0.5f;
        private const string Display = "Min/Max: {0}/{1}";

        private int fpsAccumulator = 0;
        private float fpsNextPeriod = 0;

        private int currentFps;
        private int lowestFps = 60;
        private int highestFps = 60;

        private Text UITextElement;

        // The shown FPS will be 0 for the first second until it ticks over correctly
        public override void Update()
        {
            // measure average frames per second
            fpsAccumulator++;
            if (Time.realtimeSinceStartup > fpsNextPeriod)
            {
                currentFps = (int)(fpsAccumulator / FPSMeasurePeriod);
                fpsAccumulator = 0;

                fpsNextPeriod += FPSMeasurePeriod;

                if (currentFps < lowestFps)
                {
                    lowestFps = currentFps;
                }
                else if (currentFps > highestFps)
                {
                    highestFps = currentFps;
                }

                UITextElement.text = string.Format(Display, lowestFps, highestFps);
            }
        }

        public override GameObject InitializeElement()
        {
            fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;

            // Build Gameobject
            GameObject element = GetHorizontalBaseElement("FPS-Range", 80, 60, allocatedHeight: 60, allocatedWidth: 80, alignment: TextAnchor.MiddleLeft);

            UITextElement = CreateText(string.Format(Display, "-", "-"), false, TextAnchor.MiddleCenter);
            UITextElement.transform.SetParent(element.transform);

            return element;
        }

        public override string GetName()
        {
            return "FPSRangePerformanceComponent";
        }
    }
}
