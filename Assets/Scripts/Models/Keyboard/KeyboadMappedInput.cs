#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;

public class KeyboadMappedInput
{
    public InputNames InputName { get; set; }
    public KeyCode Primary { get; set; }
    public KeyCode Alternate { get; set; }

    public KeyboadMappedInput()
    {
        
    }

    public KeyboadMappedInput(InputNames inputName, KeyCode primary, KeyCode alternate)
    {
        InputName = inputName;
        Primary = primary;
        Alternate = alternate;
    }
}