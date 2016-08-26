#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.IO;
using UnityEngine;

public class PrototypeManager
{
    public PrototypeManager()
    {
        FurnitureJob = new BasePrototypes<Job>();
        Furniture = new FurniturePrototypes();
        Inventory = new InventoryPrototypes();
        Need = new NeedPrototypes();
        Trader = new TraderPrototypes();
        Quest = new QuestPrototypes();
    }

    public static BasePrototypes<Job> FurnitureJob
    {
        get;
        protected set;
    }

    public static FurniturePrototypes Furniture
    {
        get;
        protected set;
    }

    public static InventoryPrototypes Inventory
    {
        get;
        protected set;
    }

    public static NeedPrototypes Need
    {
        get;
        protected set;
    }

    public static TraderPrototypes Trader
    {
        get;
        protected set;
    }

    public static QuestPrototypes Quest
    {
        get;
        protected set;
    }
}
