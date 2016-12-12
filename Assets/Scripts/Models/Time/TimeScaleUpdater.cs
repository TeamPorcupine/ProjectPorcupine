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
    private Sprite[] imageTimeScaleArray;
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

        textDisplay.text = TimeManager.Instance.TimeScale.ToString() + "x";

        foreach (Image img in sliderBackground)
        {
            img.color = colorTimeScaleArray[TimeManager.Instance.TimeScalePosition];
        }

        if (TimeManager.Instance.TimeScalePosition >= 0 && TimeManager.Instance.TimeScalePosition < imageTimeScaleArray.Length)
        {
            imageElement.sprite = imageTimeScaleArray[TimeManager.Instance.TimeScalePosition];
        }
    }

    public void PauseSpeed()
    {
        if (TimeManager.Instance != null)
        {
            // Toggle
            TimeManager.Instance.IsPaused = !TimeManager.Instance.IsPaused;
        }

        if (TimeManager.Instance.IsPaused)
        {
            textDisplay.text = "||";

            foreach (Image img in sliderBackground)
            {
                img.color = colorTimeScaleArray[colorTimeScaleArray.Length - 1];
            }
        }
        else
        {
            textDisplay.text = TimeManager.Instance.TimeScale.ToString() + "x";

            foreach (Image img in sliderBackground)
            {
                img.color = colorTimeScaleArray[TimeManager.Instance.TimeScalePosition];
            }
        }

        if (TimeManager.Instance.TimeScalePosition >= 0 && TimeManager.Instance.TimeScalePosition < imageTimeScaleArray.Length)
        {
            imageElement.sprite = imageTimeScaleArray[TimeManager.Instance.TimeScalePosition];
        }
    }

    // Use this for initialization
    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            oldTimeScalePosition = TimeManager.Instance.TimeScalePosition;
        }
        else
        {
            // Just disable it
            oldTimeScalePosition = -1;
        }

        imageElement = GetComponentInChildren<Image>();
        slider = GetComponentInChildren<Slider>();
        slider.minValue = 0;
        slider.maxValue = imageTimeScaleArray.Length - 1;
        textDisplay = GetComponentInChildren<Text>();
        sliderBackground = slider.GetComponentsInChildren<Image>();


        if (oldTimeScalePosition >= 0 && oldTimeScalePosition < imageTimeScaleArray.Length)
        {
            imageElement.sprite = imageTimeScaleArray[oldTimeScalePosition];
        }
    }

    // Update is called once per frame
    // Update sprite if necessary
    private void Update()
    {
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.IsPaused)
            {
                // Last Element
                oldTimeScalePosition = imageTimeScaleArray.Length - 1;

                if (oldTimeScalePosition >= 0 && oldTimeScalePosition < imageTimeScaleArray.Length)
                {
                    imageElement.sprite = imageTimeScaleArray[oldTimeScalePosition];
                }
            }
            else if (oldTimeScalePosition != TimeManager.Instance.TimeScalePosition)
            {
                oldTimeScalePosition = TimeManager.Instance.TimeScalePosition;

                if (oldTimeScalePosition >= 0 && oldTimeScalePosition < imageTimeScaleArray.Length)
                {
                    imageElement.sprite = imageTimeScaleArray[oldTimeScalePosition];
                }
            }
        }

        imageElement.gameObject.SetActive(oldTimeScalePosition != -1);
    }
}
