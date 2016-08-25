#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.IO;
using UnityEngine;

public class ModsManager : MonoBehaviour
{
    static public ModsManager current;

    private DirectoryInfo[] mods;

    void OnEnable()
    {
        current = this;

        string dataPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        string modsPath = System.IO.Path.Combine(dataPath, "Mods");
        DirectoryInfo modsDir = new DirectoryInfo(modsPath);
        mods = modsDir.GetDirectories();
    }

    public DirectoryInfo[] GetMods()
    {
        return mods;
    }
}
