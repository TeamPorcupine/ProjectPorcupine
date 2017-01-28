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

    public static FPSAveragePerformanceComponent GetFPSAveragePerformanceComponent()
    {
        return new FPSAveragePerformanceComponent();
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

        public override void Update()
        {
            UITextElement.text = "Mem: " + ((Profiler.GetTotalReservedMemory() / 1024) / 1024) + "mb\nAlloc: " + ((Profiler.GetTotalAllocatedMemory() / 1024) / 1024) + "mb";
        }

        public override GameObject InitializeElement()
        {
            // Build Gameobject
            GameObject element = GetHorizontalBaseElement("Memory", 40, 40, allocatedHeight: 40);

            UITextElement = CreateText("Mem: ...", false, TextAnchor.MiddleRight);
            UITextElement.transform.SetParent(element.transform);

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
            GameObject element = GetHorizontalBaseElement("Network", 40, 40, allocatedHeight: 40);

            UITextElement = CreateText("...ms", false, TextAnchor.MiddleRight);
            UITextElement.transform.SetParent(element.transform);

            return element;
        }

        public override string GetName()
        {
            return "NetworkPerformanceComponent";
        }
    }

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

        private Text UITextElement;

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

                UITextElement.text = string.Format(Display, currentFps);
            }
        }

        public override GameObject InitializeElement()
        {
            // Build Gameobject
            fpsFinishPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;
            GameObject element = GetHorizontalBaseElement("FPS-Average", 40, 40, allocatedHeight: 40);

            UITextElement = CreateText("Avg: ...", false, TextAnchor.MiddleRight);
            UITextElement.transform.SetParent(element.transform);

            return element;
        }

        public override string GetName()
        {
            return "FPSAveragePerformanceComponent";
        }
    }

    /// <summary>
    /// Displays current FPS (over 0.5s period).
    /// </summary>
    public class FPSPerformanceComponent : BasePerformanceHUDElement
    {
        private const float FPSMeasurePeriod = 0.5f;

        private const string GreenDisplay = "<color=lime>{0}</color>";
        private const string RedDisplay = "<color=red>{0}</color>";
        private const string YellowDisplay = "<color=yellow>{0}</color>";

        private int fpsAccumulator = 0;
        private float fpsNextPeriod = 0;
        private int currentFps;

        private Text UITextElement;

        // public override string NameOfComponent()
        //  {
        //       return "UI/TextPerformanceComponentUI";
        //  }

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
                if (currentFps > 55)
                {
                    // A good area to be at
                    UITextElement.text = string.Format(GreenDisplay, currentFps);
                }
                else if (currentFps >= 30 && currentFps <= 55)
                {
                    // Less preferable but playable
                    UITextElement.text = string.Format(YellowDisplay, currentFps);
                }
                else
                {
                    // Too low, most likely due to an error or major slowdown
                    UITextElement.text = string.Format(RedDisplay, currentFps);
                }
            }
        }

        public override GameObject InitializeElement()
        {
            fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;

            // Build Gameobject
            GameObject element = GetHorizontalBaseElement("FPS", 40, 40, allocatedHeight: 40);

            UITextElement = CreateText("FPS: ...", false, TextAnchor.MiddleRight);
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
        private const string Display = "Min: {0}\nMax: {1}";

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
            GameObject element = GetHorizontalBaseElement("FPS-Range", 40, 40, allocatedHeight: 60);

            UITextElement = CreateText("Min: ...\nMax: ...", false, TextAnchor.MiddleRight);
            UITextElement.transform.SetParent(element.transform);
            UITextElement.fontSize = 12;

            return element;
        }

        public override string GetName()
        {
            return "FPSRangePerformanceComponent";
        }
    }
}
