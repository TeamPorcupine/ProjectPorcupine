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
    [SerializeField]
    private Color[] colorTimeScaleArray; // Follows the time manager array 0.1, 0.5, 1, 2, 4, 8.  Then the final '7'th one is pause

    private Image imageElement;
    private Image[] sliderBackground;
    private Text textDisplay;
    private Slider slider;

    private bool isPaused;
    private bool userDriven = true;

    public void SetSpeed(float value)
    {
        if (userDriven && TimeManager.Instance != null)
        {
            TimeManager.Instance.SetTimeScalePosition((int)value);
            TimeManager.Instance.IsPaused = false;

            // This is basically just a dirty flag to force the UI to redo
            // Doesn't effect gameplay.
            isPaused = true;
        }
    }

    public void PauseSpeed()
    {
        if (TimeManager.Instance != null)
        {
            // Toggle
            TimeManager.Instance.IsPaused = !TimeManager.Instance.IsPaused;
        }
    }

    private bool DoUpdate()
    {
        if (TimeManager.Instance == null)
        {
            return false;
        }

        if (isPaused != TimeManager.Instance.IsPaused)
        {
            return true;
        }

        if (slider.value != TimeManager.Instance.TimeScalePosition)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// This way it'll update anytime.
    /// </summary>
    private void Update()
    {
        if (DoUpdate())
        {
            UpdateVisuals(TimeManager.Instance.TimeScalePosition, TimeManager.Instance.IsPaused, TimeManager.Instance.IsPaused ? "||" : TimeManager.Instance.TimeScale.ToString() + "x");
        }
    }

    // This way we can update it regardless of its actual value
    // If timeScalePosition < 0 then its paused
    private void UpdateVisuals(int timeScalePosition, bool isPaused, string text)
    {
        textDisplay.text = text;

        foreach (Image img in sliderBackground)
        {
            if (colorTimeScaleArray.Length > 0)
            {
                if (isPaused)
                {
                    img.color = colorTimeScaleArray[colorTimeScaleArray.Length - 1];
                }
                else if (timeScalePosition < colorTimeScaleArray.Length)
                {
                    img.color = colorTimeScaleArray[timeScalePosition];
                }
            }
        }

        imageElement.sprite = isPaused ? timePaused : timeUnPaused;

        // Due to Unity's system, we need to implement this kind of hack -_-, ik stupid.
        userDriven = false;
        slider.value = timeScalePosition;
        userDriven = true;

        this.isPaused = isPaused;
    }

    private void Awake()
    {
        imageElement = GetComponentInChildren<Image>();
        slider = GetComponentInChildren<Slider>();
        textDisplay = GetComponentInChildren<Text>();
        sliderBackground = slider.GetComponentsInChildren<Image>();
    }

    // Initalise
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

        isPaused = TimeManager.Instance.IsPaused;
        slider.value = TimeManager.Instance.TimeScalePosition;

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
    }
}
