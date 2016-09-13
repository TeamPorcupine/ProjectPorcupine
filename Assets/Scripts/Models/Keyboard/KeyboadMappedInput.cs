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

public class KeyboadMappedInput
{
    public KeyboadMappedInput()
    {
        KeyCodes = new List<KeyCode>();
    }

    public string InputName { get; set; }

    public List<KeyCode> KeyCodes { get; set; }

    public KeyboardMappedInputType Type { get; set; }

    public KeyboardInputModifier Modifier { get; set; }

    public Action OnTrigger { get; set; }

    public void AddKeyCodes(KeyCode[] keycodes)
    {
        foreach (KeyCode keycode in keycodes)
        {
            if (!KeyCodes.Contains(keycode))
            {
                KeyCodes.Add(keycode);
            }
        }
    }

    public void TriggerActionIfInputValid()
    {
        if (UserUsedInputThisFrame())
        {
            if (OnTrigger != null)
            {
                OnTrigger();
            }
        }
    }

    private bool UserUsedInputThisFrame()
    {
        if (ModifierActive())
        {
            switch (Type)
            {
                case KeyboardMappedInputType.Key:
                    return GetKey();
                case KeyboardMappedInputType.KeyUp:
                    return GetKeyUp();
                case KeyboardMappedInputType.KeyDown:
                    return GetKeyDown();
            } 
        }

        return false;
    }

    private bool ModifierActive()
    {
        switch (Modifier)
        {
            case KeyboardInputModifier.None:
                return !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                        || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                        || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
            case KeyboardInputModifier.Shift:
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            case KeyboardInputModifier.Control:
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            case KeyboardInputModifier.Alt:
                return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        return false;
    }

    private bool GetKey()
    {
        return KeyCodes.Any(Input.GetKey);
    }

    private bool GetKeyUp()
    {
        return KeyCodes.Any(Input.GetKeyUp);
    }

    private bool GetKeyDown()
    {
        return KeyCodes.Any(Input.GetKeyUp);
    }
}