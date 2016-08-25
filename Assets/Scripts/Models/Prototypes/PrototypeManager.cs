#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using System.IO;


public class PrototypeManager
{

    static public Prototypes<Job> FurnitureJob { get; protected set; }
    static public FurniturePrototypes Furniture { get; protected set; }
    static public InventoryPrototypes Inventory { get; protected set; }
    static public NeedPrototypes Need { get; protected set; }
    static public TraderPrototypes Trader { get; protected set; }
    static public QuestPrototypes Quest { get; protected set; }


    public PrototypeManager()
    {
        FurnitureJob = new Prototypes<Job>();
        Furniture = new FurniturePrototypes();
        Inventory = new InventoryPrototypes();
        Need = new NeedPrototypes();
        Trader = new TraderPrototypes();
        Quest = new QuestPrototypes();

        new FurnitureActions();
        new NeedActions();
    }
}
