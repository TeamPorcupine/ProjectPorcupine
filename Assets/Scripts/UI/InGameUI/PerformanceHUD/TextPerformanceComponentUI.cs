#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine.UI;

/// <summary>
/// Just holds a simple text UI component that can be access either through the public value "text"
/// or throught he function call changeText.
/// </summary>
public class TextPerformanceComponentUI : BasePerformanceComponentUI
{
    /// <summary>
    /// The text UI element.
    /// </summary>
    public Text text;

    public void ChangeText(string newText)
    {
        text.text = newText;
    }
}
