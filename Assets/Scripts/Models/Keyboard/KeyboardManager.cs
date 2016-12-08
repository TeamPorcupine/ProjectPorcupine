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

public class KeyboardManager
{
    private static KeyboardManager instance;

    private Dictionary<string, KeyboadMappedInput> mapping;

    public KeyboardManager()
    {
        instance = this;
        mapping = new Dictionary<string, KeyboadMappedInput>();
        ModalInputFields = new List<InputField>();

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

    public List<InputField> ModalInputFields { get; set; }

    public void RegisterModalInputField(InputField filterField)
    {
        if (!ModalInputFields.Contains(filterField))
        {
            ModalInputFields.Add(filterField);
        }
    }

    public void UnRegisterModalInputField(InputField filterField)
    {
        if (ModalInputFields.Contains(filterField))
        {
            ModalInputFields.Remove(filterField);
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

        RegisterInputMapping("ApplyCameraPreset1", KeyboardInputModifier.None, KeyCode.F1);
        RegisterInputMapping("ApplyCameraPreset2", KeyboardInputModifier.None, KeyCode.F2);
        RegisterInputMapping("ApplyCameraPreset3", KeyboardInputModifier.None, KeyCode.F3);
        RegisterInputMapping("ApplyCameraPreset4", KeyboardInputModifier.None, KeyCode.F4);
        RegisterInputMapping("ApplyCameraPreset5", KeyboardInputModifier.None, KeyCode.F5);
        RegisterInputMapping("AssignCameraPreset1", KeyboardInputModifier.Control, KeyCode.F1);
        RegisterInputMapping("AssignCameraPreset2", KeyboardInputModifier.Control, KeyCode.F2);
        RegisterInputMapping("AssignCameraPreset3", KeyboardInputModifier.Control, KeyCode.F3);
        RegisterInputMapping("AssignCameraPreset4", KeyboardInputModifier.Control, KeyCode.F4);
        RegisterInputMapping("AssignCameraPreset5", KeyboardInputModifier.Control, KeyCode.F5);

        RegisterInputMapping("SetSpeed1", KeyboardInputModifier.None, KeyCode.Alpha1, KeyCode.Keypad1);
        RegisterInputMapping("SetSpeed2", KeyboardInputModifier.None, KeyCode.Alpha2, KeyCode.Keypad2);
        RegisterInputMapping("SetSpeed3", KeyboardInputModifier.None, KeyCode.Alpha3, KeyCode.Keypad3);
        RegisterInputMapping("DecreaseSpeed", KeyboardInputModifier.None, KeyCode.Minus, KeyCode.KeypadMinus);
        RegisterInputMapping("IncreaseSpeed", KeyboardInputModifier.None, KeyCode.Plus, KeyCode.KeypadPlus);

        RegisterInputMapping("RotateFurnitureLeft", KeyboardInputModifier.None, KeyCode.R);
        RegisterInputMapping("RotateFurnitureRight", KeyboardInputModifier.None, KeyCode.T);

        RegisterInputMapping("Pause", KeyboardInputModifier.None, KeyCode.Space, KeyCode.Pause);
        RegisterInputMapping("Return", KeyboardInputModifier.None, KeyCode.Return);

        RegisterInputMapping("DevMode", KeyboardInputModifier.None, KeyCode.F12);
        RegisterInputMapping("DevConsole", KeyboardInputModifier.Control, KeyCode.BackQuote);

        RegisterInputMapping("ToggleCursorTextBox", KeyboardInputModifier.Control, KeyCode.M);
    }

    /// <summary>
    /// This won't care about the focus fields.  Needed for some things like DevConsole.
    /// </summary>
    public void TriggerActionIfValid(string inputName)
    {
        if (mapping.ContainsKey(inputName))
        {
            mapping[inputName].TriggerActionIfInputValid();
        }
    }

    public void Update()
    {
        if (ModalInputFields.Any(f => f.isFocused))
        {
            return;
        }

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

    public void UnRegisterInputAction(string inputName)
    {
        if (mapping.ContainsKey(inputName))
        {
            mapping.Remove(inputName);
        }
    }

    public void RegisterInputMapping(string inputName, KeyboardInputModifier inputModifiers, params KeyCode[] keyCodes)
    {
        if (mapping.ContainsKey(inputName))
        {
            mapping[inputName].Modifiers = inputModifiers;
            mapping[inputName].AddKeyCodes(keyCodes);
        }
        else
        {
            mapping.Add(
                inputName,
                new KeyboadMappedInput
                {
                    InputName = inputName,
                    Modifiers = inputModifiers,
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
