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

public class SliderUpdater : MonoBehaviour
{
    /// <summary>
    /// What format is the text to be displayed as.
    /// </summary>
    public string format = "({0})";

    /// <summary>
    /// Cast value as int.
    /// </summary>
    public bool castValueAsInt = false;

    /// <summary>
    /// The text component.
    /// </summary>
    public Text text;

    /// <summary>
    /// Update text.
    /// </summary>
    public void ValueUpdate(float value)
    {
        text.text = string.Format(format, castValueAsInt ? (int)value : value);
    }

    /// <summary>
    /// Update text.
    /// </summary>
    public void ValueUpdate(int value)
    {
        text.text = string.Format(format, value);
    }
}
