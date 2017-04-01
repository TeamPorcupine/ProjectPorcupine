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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Base Class, will be a UI Element
[MoonSharp.Interpreter.MoonSharpUserData]
public abstract class BaseSettingsElement : BaseUIElement
{
    public SettingsOption option;

    public bool valueChanged;

    /// <summary>
    /// Apply your setting.  You can use variables in this.
    /// Do a setting.setSetting beforehand.
    /// </summary>
    public event EventHandler ApplySettingHandler;

    /// <summary>
    /// Undo your setting.  You should do a setting.getSetting call
    /// To get the latest setting info.
    /// </summary>
    public event EventHandler CancelSettingHandler;

    /// <summary>
    /// LUA Initializer.
    /// </summary>
    public void InitializeLUA()
    {
        if (parameterData.ContainsKey("LUAInitializeFunction"))
        {
            FunctionsManager.SettingsMenu.Call(parameterData["LUAInitializeFunction"].ToString(), this);
        }
    }

    public abstract void ApplySetting();

    public abstract void CancelSetting();

    public void ApplySettingLUA()
    {
        // Please C# 4.6
        EventHandler invoker = ApplySettingHandler;
        if (invoker != null)
        {
            invoker(this, null);
        }
    }

    public void CancelSettingLUA()
    {
        // Please C# 4.6
        EventHandler invoker = CancelSettingHandler;
        if (invoker != null)
        {
            invoker(this, null);
        }
    }

    /// <summary>
    /// The name of the settings element.
    /// </summary>
    public override string GetName()
    {
        return option.name;
    }
}