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
    public int lastScreenWidth = 0;

    public void Start()
    {
        lastScreenWidth = Screen.width;
        StartCoroutine("AdjustScale");
        Debug.LogWarning(lastScreenWidth);
    }

    public void Update()
    {
        if (lastScreenWidth != Screen.width)
        {
            lastScreenWidth = Screen.width;
            StartCoroutine("AdjustScale");
        }
    }

    private IEnumerator AdjustScale()
    {
        this.GetComponent<CanvasScaler>().scaleFactor = Screen.width / 1920f;
        yield return null;
    }
}