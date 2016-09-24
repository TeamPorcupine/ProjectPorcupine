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

    public KeyboardInputModifier Modifiers { get; set; }

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
        if (ModifiersActive())
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

    private bool ModifiersActive()
    {
        KeyboardInputModifier currentlyPressed = KeyboardInputModifier.None;

        if (GetKeyModifier(KeyboardInputModifier.Shift))
        {
            currentlyPressed = currentlyPressed | KeyboardInputModifier.Shift;
        }

        if (GetKeyModifier(KeyboardInputModifier.Control))
        {
            currentlyPressed = currentlyPressed | KeyboardInputModifier.Control;
        }

        if (GetKeyModifier(KeyboardInputModifier.Alt))
        {
            currentlyPressed = currentlyPressed | KeyboardInputModifier.Alt;
        }

        return currentlyPressed == Modifiers;
    }

    private bool GetKeyModifier(KeyboardInputModifier modifier)
    {
        switch (modifier)
        {
            case KeyboardInputModifier.None:
                return !(GetKeyModifier(KeyboardInputModifier.Shift) || GetKeyModifier(KeyboardInputModifier.Control) || GetKeyModifier(KeyboardInputModifier.Alt));
            case KeyboardInputModifier.Shift:
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            case KeyboardInputModifier.Control:
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            case KeyboardInputModifier.Alt:
                return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            default:
                return false;
        }
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