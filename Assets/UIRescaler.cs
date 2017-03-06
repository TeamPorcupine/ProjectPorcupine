#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIRescaler : MonoBehaviour 
{
    private const float MaxScale = 2f;
    private const float MinScale = .5f;

    private int lastScreenWidth = 0;
    private float scale = 1f;

    public float Scale 
    {
        get
        {
            return scale;
        }

        set
        {
            if (value > MaxScale)
            {
                scale = MaxScale;
            }
            else if (value < MinScale)
            {
                scale = MinScale;
            }
            else 
            {
                scale = value;
            }
        }
    }

    public void Start()
    {
        lastScreenWidth = Screen.width;

        AdjustScale();
    }

    public void Update()
    {
//        if (lastScreenWidth != Screen.width)
        {
            lastScreenWidth = Screen.width;
            AdjustScale();
        }
    }

    private void AdjustScale()
    {
        Settings.GetSetting("ui_scale", out scale);
        this.GetComponent<CanvasScaler>().scaleFactor = (Screen.height / 720f) * scale;
    }
}