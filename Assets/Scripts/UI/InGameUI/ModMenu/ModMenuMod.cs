#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;

public class ModMenuMod : MonoBehaviour
{
    private string modName;
    
    // Update is called once per frame
    public void Toggle(bool tg)
    {
        if (ModMenu.Loaded)
        {
            ModMenu.SetEnabled(modName, tg);
            ModMenu.DisplaySettings();
        }
    }

    public void Move(bool up)
    {
        ModMenu.ReorderMod(modName, up ? 1 : -1);
    }

    // Use this for initialization
    private void Start()
    {
        modName = gameObject.name;
    }
}
