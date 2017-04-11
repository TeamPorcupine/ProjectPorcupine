#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Just holds a simple text UI component that can be access either through the public value "text"
/// or throught he function call changeText.
/// </summary>
[MoonSharp.Interpreter.MoonSharpUserData]
public abstract class BasePerformanceHUDElement : BaseUIElement
{
    public event EventHandler UpdateHandler;

    public abstract void Update();

    /// <summary>
    /// Update function.
    /// </summary>
    public void UpdateLUA()
    {
        EventHandler invoker = UpdateHandler;
        if (invoker != null)
        {
            invoker(this, null);
        }
    }

    /// <summary>
    /// LUA Initializer.
    /// </summary>
    public void InitializeLUA()
    {
        if (parameterData.ContainsKey("LUAInitializeFunction"))
        {
            FunctionsManager.PerformanceHUD.Call(parameterData["LUAInitializeFunction"].ToString(), this);
        }
    }
}
