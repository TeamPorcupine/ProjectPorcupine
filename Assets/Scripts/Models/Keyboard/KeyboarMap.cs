#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyboardMap
{
    private Dictionary<InputNames, KeyboadMappedInput> Mapping { get; set; }
    
    public void FromList(List<KeyboadMappedInput> inputs)
    {
        Mapping = inputs.ToDictionary(x => x.InputName, x => x);
    }

    public List<KeyboadMappedInput> ToList()
    {
        if (Mapping == null)
        {
            return new List<KeyboadMappedInput>();
        }

        return Mapping.Values.ToList();
    }

    public bool GetKey(InputNames input)
    {
        if (!Mapping.ContainsKey(input))
        {
            return false;
        }

        KeyboadMappedInput map = Mapping[input];

        return Input.GetKey(map.Primary) ||
               Input.GetKey(map.Alternate);
    }

    public bool GetKeyUp(InputNames input)
    {
        if (!Mapping.ContainsKey(input))
        {
            return false;
        }

        KeyboadMappedInput map = Mapping[input];

        return Input.GetKeyUp(map.Primary) ||
               Input.GetKeyUp(map.Alternate);
    }

    public bool GetKeyDown(InputNames input)
    {
        if (!Mapping.ContainsKey(input))
        {
            return false;
        }

        KeyboadMappedInput map = Mapping[input];

        return Input.GetKeyDown(map.Primary) ||
               Input.GetKeyDown(map.Alternate);
    }
}