#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleUpdater : MonoBehaviour
{
    private int oldTimeScalePosition;

    // Follows the time manager array
    // 0.1, 0.5, 1, 2, 4, 8.  Then the final '7'th one is pause
    [SerializeField]
    private Sprite timePaused;
    [SerializeField]
    private Sprite timeUnPaused;
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
        }

        UpdateVisuals();
    }

    public void PauseSpeed()
    {
        if (TimeManager.Instance != null)
        {
            // Toggle
            TimeManager.Instance.IsPaused = !TimeManager.Instance.IsPaused;
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (TimeManager.Instance == null)
        {
            return;
        }

        if (TimeManager.Instance.IsPaused)
        {
            textDisplay.text = "||";

            foreach (Image img in sliderBackground)
            {
                img.color = colorTimeScaleArray[colorTimeScaleArray.Length - 1];
            }

            imageElement.sprite = timePaused;
        }
        else
        {
            textDisplay.text = TimeManager.Instance.TimeScale.ToString() + "x";

            foreach (Image img in sliderBackground)
            {
                img.color = colorTimeScaleArray[TimeManager.Instance.TimeScalePosition];
            }

            imageElement.sprite = timeUnPaused;
        }

        slider.value = TimeManager.Instance.TimeScalePosition;
    }

    // Use this for initialization
    private void Start()
    {
        imageElement = GetComponentInChildren<Image>();
        slider = GetComponentInChildren<Slider>();

        // Hardcoded till timemanager gets a little bit of attention and becomes less constant heavy
        slider.minValue = 0;
        slider.maxValue = 5;
        textDisplay = GetComponentInChildren<Text>();
        sliderBackground = slider.GetComponentsInChildren<Image>();

        if (TimeManager.Instance != null)
        {
            oldTimeScalePosition = TimeManager.Instance.TimeScalePosition;
        }
        else
        {
            // Just disable it
            imageElement.gameObject.SetActive(false);
            textDisplay.gameObject.SetActive(false);
            slider.gameObject.SetActive(false);
            return;
        }

        UpdateVisuals();
    }
}
