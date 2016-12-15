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
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleUpdater : MonoBehaviour
{
    [SerializeField]
    private Sprite timePaused;
    [SerializeField]
    private Sprite timeUnPaused;

    // Follows the time manager array
    // 0.1, 0.5, 1, 2, 4, 8.  Then the final '7'th one is pause
    [SerializeField]
    private Color[] colorTimeScaleArray;

    private Image imageElement;
    private Image[] sliderBackground;
    private Text textDisplay;
    private Slider slider;

    public void SetSpeed(float value)
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetTimeScalePosition((int)value);

            UpdateVisuals(TimeManager.Instance.IsPaused ? -1 : TimeManager.Instance.TimeScalePosition, TimeManager.Instance.IsPaused ? "||" : TimeManager.Instance.TimeScale.ToString() + "x");
        }
    }

    public void PauseSpeed()
    {
        if (TimeManager.Instance != null)
        {
            // Toggle
            TimeManager.Instance.IsPaused = !TimeManager.Instance.IsPaused;

            UpdateVisuals(TimeManager.Instance.IsPaused ? -1 : TimeManager.Instance.TimeScalePosition, TimeManager.Instance.IsPaused ? "||" : TimeManager.Instance.TimeScale.ToString() + "x");
        }
    }

    // This way we can update it regardless of its actual value
    // If timeScalePosition < 0 then its paused
    private void UpdateVisuals(int timeScalePosition, string text)
    {
        textDisplay.text = text;

        foreach (Image img in sliderBackground)
        {
            if (colorTimeScaleArray.Length > 0)
            {
                if (timeScalePosition < 0)
                {
                    img.color = colorTimeScaleArray[colorTimeScaleArray.Length - 1];
                }
                else if (timeScalePosition < colorTimeScaleArray.Length)
                {
                    img.color = colorTimeScaleArray[timeScalePosition];
                }
            }
        }

        imageElement.sprite = (timeScalePosition < 0) ? timePaused : timeUnPaused;
        slider.value = timeScalePosition;
    }

    private void Awake()
    {
        imageElement = GetComponentInChildren<Image>();
        slider = GetComponentInChildren<Slider>();
        textDisplay = GetComponentInChildren<Text>();
        sliderBackground = slider.GetComponentsInChildren<Image>();
    }

    // Initalisz
    private void OnEnable()
    {
        if (TimeManager.Instance == null)
        {
            // Just disable it
            this.gameObject.SetActive(false);
            return;
        }

        float[] timeScales = TimeManager.Instance.GetTimeScaleArrayCopy();

        // Hardcoded till timemanager gets a little bit of attention and becomes less constant heavy
        slider.minValue = 0;
        slider.maxValue = timeScales.Length;

        // This will scale it nicely and make sure we always have colors
        if (colorTimeScaleArray.Length < timeScales.Length)
        {
            // Get last index
            int oldCap = colorTimeScaleArray.Length - 1;

            Array.Resize(ref colorTimeScaleArray, timeScales.Length);

            // Now we have proper sized array so iterate starting at last index
            for (int i = oldCap; i < colorTimeScaleArray.Length; i++)
            {
                // Sure it'll be white but it'll be something :D
                colorTimeScaleArray[i] = Color.white;
            }
        }

        UpdateVisuals(TimeManager.Instance.IsPaused ? -1 : TimeManager.Instance.TimeScalePosition, TimeManager.Instance.IsPaused ? "||" : TimeManager.Instance.TimeScale.ToString() + "x");
    }
}
