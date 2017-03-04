#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine.UI;

/// <summary>
/// Just holds a simple text UI component that can be access either through the public value "text"
/// or throught he function call changeText.
/// </summary>
[MoonSharp.Interpreter.MoonSharpUserData]
public abstract class BasePerformanceHUDElement : BaseUIElement
{
    /// <summary>
    /// Update function.
    /// </summary>
    public abstract void Update();
}
