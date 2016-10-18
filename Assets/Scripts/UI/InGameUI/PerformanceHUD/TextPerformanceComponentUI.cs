﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;
using UnityEngine.UI;

public class TextPerformanceComponentUI : BasePerformanceComponentUI
{
    public Text text;

    public override GameObject componentPrefab()
    {
        text = GetComponent<Text>();

        return gameObject;
    }

    public void changeText(string newText)
    {
        text.text = newText;
    }
}
