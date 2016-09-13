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

public class KeyboardManager
{
    private static KeyboardManager instance;

    private Dictionary<string, KeyboadMappedInput> mapping;

    // First entry is SHIFT, second CONTROL, third ALT
    private bool[] modifiers = new bool[3] { false, false, false };
    
    public KeyboardManager()
    {
        instance = this;
        mapping = new Dictionary<string, KeyboadMappedInput>();

        TimeManager.Instance.EveryFrameNotModal += (time) => Update();

        ReadXmlOrJsonAfterWeDecide();
    }

    public static KeyboardManager Instance
    {
        get
        {
            if (instance == null)
            {
                new KeyboardManager();
            }

            return instance;
        }

        set
        {
            instance = value;
        }
    }

    public void ReadXmlOrJsonAfterWeDecide()
    {
        // mock data for now until xml vs json is decided
        RegisterInputMapping("MoveCameraEast", KeyboardInputModifier.None, KeyCode.D, KeyCode.RightArrow);
        RegisterInputMapping("MoveCameraWest", KeyboardInputModifier.None, KeyCode.A, KeyCode.LeftArrow);
        RegisterInputMapping("MoveCameraNorth", KeyboardInputModifier.None, KeyCode.W, KeyCode.UpArrow);
        RegisterInputMapping("MoveCameraSouth", KeyboardInputModifier.None, KeyCode.S, KeyCode.DownArrow);

        RegisterInputMapping("ZoomOut", KeyboardInputModifier.None, KeyCode.PageUp);
        RegisterInputMapping("ZoomIn", KeyboardInputModifier.None, KeyCode.PageDown);

        RegisterInputMapping("MoveCameraUp", KeyboardInputModifier.None, KeyCode.Home);
        RegisterInputMapping("MoveCameraDown", KeyboardInputModifier.None, KeyCode.End);

        RegisterInputMapping("GoToPresetCameraPosition1", KeyboardInputModifier.None, KeyCode.F1);
        RegisterInputMapping("GoToPresetCameraPosition2", KeyboardInputModifier.None, KeyCode.F2);
        RegisterInputMapping("GoToPresetCameraPosition3", KeyboardInputModifier.None, KeyCode.F3);
        RegisterInputMapping("GoToPresetCameraPosition4", KeyboardInputModifier.None, KeyCode.F4);
        RegisterInputMapping("GoToPresetCameraPosition5", KeyboardInputModifier.None, KeyCode.F5);
        RegisterInputMapping("SavePresetCameraPosition1", KeyboardInputModifier.Control, KeyCode.F1);
        RegisterInputMapping("SavePresetCameraPosition2", KeyboardInputModifier.Control, KeyCode.F2);
        RegisterInputMapping("SavePresetCameraPosition3", KeyboardInputModifier.Control, KeyCode.F3);
        RegisterInputMapping("SavePresetCameraPosition4", KeyboardInputModifier.Control, KeyCode.F4);
        RegisterInputMapping("SavePresetCameraPosition5", KeyboardInputModifier.Control, KeyCode.F5);

        RegisterInputMapping("SetSpeed1", KeyboardInputModifier.None, KeyCode.Alpha1, KeyCode.Keypad1);
        RegisterInputMapping("SetSpeed2", KeyboardInputModifier.None, KeyCode.Alpha2, KeyCode.Keypad2);
        RegisterInputMapping("SetSpeed3", KeyboardInputModifier.None, KeyCode.Alpha3, KeyCode.Keypad3);
        RegisterInputMapping("DecreaseSpeed", KeyboardInputModifier.None, KeyCode.Minus, KeyCode.KeypadMinus);
        RegisterInputMapping("IncreaseSpeed", KeyboardInputModifier.None, KeyCode.Plus, KeyCode.KeypadPlus);

        RegisterInputMapping("Pause", KeyboardInputModifier.None, KeyCode.Space, KeyCode.Pause);

        RegisterInputMapping("DevMode", KeyboardInputModifier.None, KeyCode.F12);
    }

    public void Update()
    {
        foreach (KeyboadMappedInput input in mapping.Values)
        {
            input.TriggerActionIfInputValid();
        }
    }

    public void RegisterInputAction(string inputName, KeyboardMappedInputType inputType, Action onTrigger)
    {
        if (mapping.ContainsKey(inputName))
        {
            mapping[inputName].OnTrigger = onTrigger;
            mapping[inputName].Type = inputType;
        }
        else
        {
            mapping.Add(
                inputName,
                new KeyboadMappedInput
                {
                    InputName = inputName,
                    OnTrigger = onTrigger,
                    Type = inputType
                });
        }
    }

    public void RegisterInputMapping(string inputName, KeyboardInputModifier inputModifier, params KeyCode[] keyCodes)
    {
        if (mapping.ContainsKey(inputName))
        {
            mapping[inputName].Modifier = inputModifier;
            mapping[inputName].AddKeyCodes(keyCodes);
        }
        else
        {
            mapping.Add(
                inputName,
                new KeyboadMappedInput
                {
                    InputName = inputName,
                    Modifier = inputModifier,
                    KeyCodes = keyCodes.ToList()
                });
        }
    }

    /// <summary>
    /// Destroy this instance.
    /// </summary>
    public void Destroy()
    {
        instance = null;
    }
}