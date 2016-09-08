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
    
    public KeyboardManager()
    {
        mapping = new Dictionary<string, KeyboadMappedInput>();
        ReadXmlOrJsonAfterWeDecide();
    }

    public static KeyboardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new KeyboardManager();
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
        RegisterInputMapping("MoveCameraEast", KeyCode.D, KeyCode.LeftArrow);
        RegisterInputMapping("MoveCameraWest", KeyCode.A, KeyCode.RightArrow);
        RegisterInputMapping("MoveCameraNorth", KeyCode.W, KeyCode.UpArrow);
        RegisterInputMapping("MoveCameraSouth", KeyCode.S, KeyCode.DownArrow);
        RegisterInputMapping("ZoomOut", KeyCode.PageUp);
        RegisterInputMapping("ZoomIn", KeyCode.PageDown);
        RegisterInputMapping("MoveCameraUp", KeyCode.Home);
        RegisterInputMapping("MoveCameraDown", KeyCode.End);
        RegisterInputMapping("SetSpeed1", KeyCode.Alpha1, KeyCode.Keypad1);
        RegisterInputMapping("SetSpeed2", KeyCode.Alpha2, KeyCode.Keypad2);
        RegisterInputMapping("SetSpeed3", KeyCode.Alpha3, KeyCode.Keypad3);
        RegisterInputMapping("DecreaseSpeed", KeyCode.Minus, KeyCode.KeypadMinus);
        RegisterInputMapping("IncreaseSpeed", KeyCode.Plus, KeyCode.KeypadPlus);
        RegisterInputMapping("Pause", KeyCode.Space, KeyCode.Pause);
    }

    public void Update(bool isModal)
    {
        if (isModal)
        {
            // A modal dialog box is open. Bail.
            return;
        }

        foreach (KeyboadMappedInput input in mapping.Values)
        {
            input.TrigerActionIfInputValid();
        }
    }

    public void RegisterInputAction(string inputName, KeyboardMappedInputType inputType, Action onTrigger)
    {
        if (mapping.ContainsKey(inputName))
        {
            mapping[inputName].OnTriger = onTrigger;
            mapping[inputName].Type = inputType;
        }
        else
        {
            mapping.Add(
                inputName,
                new KeyboadMappedInput
                {
                    InputName = inputName,
                    OnTriger = onTrigger,
                    Type = inputType
                });
        }
    }

    public void RegisterInputMapping(string inputName, params KeyCode[] keyCodes)
    {
        if (mapping.ContainsKey(inputName))
        {
            mapping[inputName].AddKeyCodes(keyCodes);
        }
        else
        {
            mapping.Add(
                inputName,
                new KeyboadMappedInput
                {
                    InputName = inputName,
                    KeyCodes = keyCodes.ToList()
                });
        }
    }
}