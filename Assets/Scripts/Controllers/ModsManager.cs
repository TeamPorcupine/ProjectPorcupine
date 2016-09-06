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
        FunctionsManager.Furniture.LoadScripts(mods, "Furniture.lua");
        FunctionsManager.Need.LoadScripts(mods, "Need.lua");
        FunctionsManager.GameEvent.LoadScripts(mods, "GameEvent.lua");
        FunctionsManager.TileType.LoadScripts(mods, "Tiles.lua");
        FunctionsManager.Quest.LoadScripts(mods, "Quest.lua");
        FunctionsManager.ScheduledEvent.LoadScripts(mods, "ScheduledEvent.lua");

        PrototypeManager.Furniture.LoadPrototypes(mods);
        PrototypeManager.Inventory.LoadPrototypes(mods);
        PrototypeManager.Need.LoadPrototypes(mods);
        PrototypeManager.Trader.LoadPrototypes(mods);
        PrototypeManager.SchedulerEvent.LoadPrototypes(mods);
    }

    public DirectoryInfo[] GetMods()
    {
        return mods;
    }
}
