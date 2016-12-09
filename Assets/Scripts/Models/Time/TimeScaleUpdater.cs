#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleUpdater : MonoBehaviour
{
    int oldTimeScalePosition;
    // Follows the time manager array
    // 0.1, 0.5, 1, 2, 4, 8.  Then the final '7'th one is pause
    [SerializeField]
    Sprite[] imageTimeScaleArray;

    Image imageElement;

    // Use this for initialization
    void Start()
    {
        if (TimeManager.Instance != null)
        {
            oldTimeScalePosition = TimeManager.Instance.timeScalePosition;
        }
        else
        {
            // Just disable it
            oldTimeScalePosition = -1;
        }

        imageElement = GetComponentInChildren<Image>();

        if (oldTimeScalePosition >= 0 && oldTimeScalePosition < imageTimeScaleArray.Length)
        {
            imageElement.sprite = imageTimeScaleArray[oldTimeScalePosition];
        }
    }

    // Update is called once per frame
    // Update sprite if necessary
    void Update()
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
            else if (oldTimeScalePosition != TimeManager.Instance.timeScalePosition)
            {
                oldTimeScalePosition = TimeManager.Instance.timeScalePosition;

                if (oldTimeScalePosition >= 0 && oldTimeScalePosition < imageTimeScaleArray.Length)
                {
                    imageElement.sprite = imageTimeScaleArray[oldTimeScalePosition];
                }
            }
        }

        imageElement.gameObject.SetActive(oldTimeScalePosition != -1);
    }

    public void IncreaseSpeed()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.IncreaseTimeScale();
            TimeManager.Instance.IsPaused = false;
        }
    }

    public void DecreaseSpeed()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.DecreaseTimeScale();
            TimeManager.Instance.IsPaused = false;
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
}
