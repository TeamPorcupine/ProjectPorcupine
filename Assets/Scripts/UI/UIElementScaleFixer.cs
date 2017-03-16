#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

// HACK: This is a needed workaround for any most any UI ELeemnt that is instantiated after start.
// If this script isn't attached it may cause errors at different scalings.
public class UIElementScaleFixer : MonoBehaviour 
{
    public void Start() 
    {
        transform.localScale = Vector3.one;
    }
}
