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

public class ModsManager
{
    private DirectoryInfo[] mods;

    public ModsManager(string dataPath)
    {
        string modsPath = System.IO.Path.Combine(dataPath, "Mods");
        DirectoryInfo modsDir = new DirectoryInfo(modsPath);
        mods = modsDir.GetDirectories();

        LoadPrototypes();
    }

    public void LoadPrototypes()
    {
        PrototypeManager.Furniture.LoadModPrototypesFromFile(mods);
        PrototypeManager.Inventory.LoadModPrototypesFromFile(mods);
        PrototypeManager.Need.LoadModPrototypesFromFile(mods);
        PrototypeManager.Trader.LoadModPrototypesFromFile(mods);

        FurnitureActions.LoadModsScripts(mods);
        NeedActions.LoadModsScripts(mods);
    }

    public DirectoryInfo[] GetMods()
    {
        return mods;
    }
}
