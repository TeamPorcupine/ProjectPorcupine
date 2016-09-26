﻿#region License
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
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ProjectPorcupine.Jobs;

[Serializable]
[XmlRoot("Workshop")]
public class FurnitureWorkshop
{
    // constants for parameters
    public const string CurProcessingTimeParamName = "cur_processing_time";
    public const string MaxProcessingTimeParamName = "max_processing_time";
    public const string CurProcessedInvParamName = "cur_processed_inv";
    public const string CurProductionChainParamName = "cur_production_chain";

    private Furniture furniture;

    private event Action<bool> OnRunningStateChanged;

    [XmlElement("ProductionChain")]
    public List<ProductionChain> PossibleProductions { get; set; }

    [XmlElement("UsedAnimations")]
    public UsedAnimations UsedAnimation { get; set; }

    [XmlIgnore]
    public List<WorkshopContextMenu> WorkshopMenuActions { get; protected set; }

    [XmlIgnore]
    public bool IsRunning { get; private set; }
    
    protected Parameter FurnitureParams
    {
        get { return furniture.Parameters; }
    }
    
    public static FurnitureWorkshop Deserialize(XmlReader xmlReader)
    {
        // deserialize FActoryINfo into factoryData
        XmlSerializer serializer = new XmlSerializer(typeof(FurnitureWorkshop));
        return (FurnitureWorkshop)serializer.Deserialize(xmlReader);
    }

    public string GetDescription()
    {
        StringBuilder sb = new StringBuilder();
        string prodChain = FurnitureParams[CurProductionChainParamName].ToString();
        sb.AppendLine(!string.IsNullOrEmpty(prodChain) ? string.Format("Production: {0}", prodChain) : "No selected production");
        return sb.ToString();
    }

    public void Initialize()
    {
        WorkshopMenuActions = new List<WorkshopContextMenu>();

        // check if context menu is needed
        if (PossibleProductions.Count > 1)
        {
            FurnitureParams.AddParameter(new Parameter(CurProductionChainParamName, null));
            foreach (var chain in PossibleProductions)
            {
                string prodChainName = chain.Name;
                WorkshopMenuActions.Add(new WorkshopContextMenu()
                {
                    ProductionChainName = prodChainName,
                    Function = ChangeCurrentProductionChain
                });
            }
        }
        else
        {
            if (PossibleProductions.Count == 1)
            {
                FurnitureParams.AddParameter(new Parameter(CurProductionChainParamName, PossibleProductions[0].Name));
            }
            else
            {
                Debug.ULogWarning("Furniture {0} is marked as factory, but has no production chain", furniture.Name);
            }
        }

        // add dynamic params here
        FurnitureParams.AddParameter(new Parameter(CurProcessingTimeParamName, 0f));
        FurnitureParams.AddParameter(new Parameter(MaxProcessingTimeParamName, 0f));
        FurnitureParams.AddParameter(new Parameter(CurProcessedInvParamName, 0));
        IsRunning = false;
        OnRunningStateChanged += RunningStateChanged;
    }

    public void SetParentFurniture(Furniture furniture)
    {
        this.furniture = furniture;
    }

    public void Update(float deltaTime)
    {
        //// if there is enough input, do the processing and store item to output
        //// - remove items from input
        //// - add param to reflect factory can provide output (has output inside)
        ////   - as output will be produced after time, it is possible that output spot can be ocupied meanwhile
        //// - process for specified time
        //// - if output slot is free, provide output (if not, keep output 'inside' factory)

        var curSetupChainName = FurnitureParams[CurProductionChainParamName].ToString();

        if (!string.IsNullOrEmpty(curSetupChainName))
        {
            ProductionChain prodChain = GetProductionChainByName(curSetupChainName);
            //// if there is no processing in progress
            if (FurnitureParams[CurProcessedInvParamName].ToInt() == 0)
            {                
                // check input slots for input inventory               
                List<KeyValuePair<Tile, int>> flaggedForTaking = CheckForInventoryAtInput(prodChain);

                // if all the input requirements are ok, you can start processing:
                if (flaggedForTaking.Count == prodChain.Input.Count)
                {
                    // consume input inventory
                    ConsumeInventories(flaggedForTaking);

                    FurnitureParams[CurProcessedInvParamName].SetValue(prodChain.Output.Count);

                    // reset processing timer and set max time for processing for this prod. chain
                    FurnitureParams[CurProcessingTimeParamName].SetValue(0f);
                    FurnitureParams[MaxProcessingTimeParamName].SetValue(prodChain.ProcessingTime);                    
                }
                //// trigger running state change
                if (IsRunning)
                {
                    OnRunningStateChanged(IsRunning = false);
                }
            }
            else
            {                
                // processing is in progress
                FurnitureParams[CurProcessingTimeParamName].ChangeFloatValue(deltaTime);

                if (FurnitureParams[CurProcessingTimeParamName].ToFloat() >=
                    FurnitureParams[MaxProcessingTimeParamName].ToFloat())
                {
                    List<TileObjectTypeAmount> outPlacement = CheckForInventoryAtOutput(prodChain);

                    // if output placement was found for all products, place them
                    if (outPlacement.Count == prodChain.Output.Count)
                    {
                        PlaceInventories(outPlacement);
                        //// processing done, can fetch input for another processing
                        FurnitureParams[CurProcessedInvParamName].SetValue(0);
                    }
                }
                //// trigger running state change
                if (!IsRunning)
                {
                    OnRunningStateChanged(IsRunning = true);
                }
            }

            // create possible jobs for factory(hauling input)
            HaulingJobForInputs(prodChain);
        }
    }
    
    private static void ChangeCurrentProductionChain(Furniture furniture, string newProductionChainName)
    {
        Parameter oldProductionChainName = furniture.Parameters[CurProductionChainParamName];
        bool isProcessing = furniture.Parameters[CurProcessedInvParamName].ToInt() > 0;

        // if selected production really changes and nothing is being processed now
        if (isProcessing || newProductionChainName.Equals(oldProductionChainName))
        {
            return;
        }

        furniture.Jobs.CancelAll();
        furniture.Parameters[CurProductionChainParamName].SetValue(newProductionChainName);
    }

    private static void PlaceInventories(List<TileObjectTypeAmount> outPlacement)
    {
        foreach (var outPlace in outPlacement)
        {
            if (outPlace.IsEmpty)
            {
                World.Current.InventoryManager.PlaceInventory(outPlace.Tile, new Inventory(outPlace.ObjectType, outPlace.Amount));
            }
            else
            {
                outPlace.Tile.Inventory.StackSize += outPlace.Amount;
            }
        }
    }
    
    private static void ConsumeInventories(List<KeyValuePair<Tile, int>> flaggedForTaking)
    {
        foreach (KeyValuePair<Tile, int> toConsume in flaggedForTaking)
        {
            toConsume.Key.Inventory.StackSize -= toConsume.Value;
            //// TODO: this should be handled somewhere else
            if (toConsume.Key.Inventory.StackSize <= 0)
            {
                toConsume.Key.Inventory = null;
            }
        }
    }

    private static void PlaceInventoryToWorkshopInput(Job job)
    {
        job.CancelJob();
        foreach (KeyValuePair<string, Inventory> heldInventory in job.HeldInventory)
        {
            Inventory inv = heldInventory.Value;
            if (inv != null && inv.StackSize > 0)
            {
                World.Current.InventoryManager.PlaceInventory(job.tile, heldInventory.Value);
                job.tile.Inventory.Locked = true;
            }
        }
    }

    private void RunningStateChanged(bool newIsRunningState)
    {
        if (UsedAnimation != null)
        {
            if (newIsRunningState == true && !string.IsNullOrEmpty(UsedAnimation.Running))
            {
                furniture.Animation.SetState(UsedAnimation.Running);
            }
            else if (newIsRunningState == false && !string.IsNullOrEmpty(UsedAnimation.Idle))
            {
                furniture.Animation.SetState(UsedAnimation.Idle);
            }
        }
    }

    private void HaulingJobForInputs(ProductionChain prodChain)
    {
        // for all inputs in production chain
        foreach (var reqInputItem in prodChain.Input)
        {
            // if there is no hauling job for input object type, create one
            Job furnJob;
            string requiredType = reqInputItem.ObjectType;
            bool existingHaulingJob = furniture.Jobs.HasJobWithPredicate(x => x.RequestedItems.ContainsKey(requiredType), out furnJob);
            if (!existingHaulingJob)
            {
                Tile inTile = World.Current.GetTileAt(
                                  furniture.Tile.X + reqInputItem.SlotPosX,
                                  furniture.Tile.Y + reqInputItem.SlotPosY,
                                  furniture.Tile.Z);

                // create job for desired input resource
                string desiredInv = reqInputItem.ObjectType;
                int desiredAmount = PrototypeManager.Inventory.Get(desiredInv).maxStackSize;
                if (inTile.Inventory != null && inTile.Inventory.Type == reqInputItem.ObjectType &&
                    inTile.Inventory.StackSize <= desiredAmount)
                {
                    desiredAmount = desiredAmount - inTile.Inventory.StackSize;
                }

                if (desiredAmount > 0)
                {
                    var jb = new Job(
                                 inTile, 
                                 null,  // beware: passed jobObjectType is expected Furniture only !!
                                 null, 
                                 0.4f,
                                 new RequestedItem[] { new RequestedItem(desiredInv, desiredAmount) },
                                 Job.JobPriority.Medium,
                                 false,
                                 false,
                                 false);
                    jb.JobDescription = string.Format("Hauling '{0}' to '{1}'", desiredInv, furniture.Name);
                    jb.OnJobWorked += PlaceInventoryToWorkshopInput;
                    furniture.Jobs.Add(jb);
                }
            }
        }
    }
    
    private List<TileObjectTypeAmount> CheckForInventoryAtOutput(ProductionChain prodChain)
    {
        var outPlacement = new List<TileObjectTypeAmount>();
        //// processing is done, try to spit the output
        //// check if output can be placed in world
        foreach (Item outObjType in prodChain.Output)
        {
            int amount = outObjType.Amount;

            // check ouput slots for products:                        
            Tile tt = World.Current.GetTileAt(
                furniture.Tile.X + outObjType.SlotPosX,
                furniture.Tile.Y + outObjType.SlotPosY,
                furniture.Tile.Z);

            bool tileHasOtherFurniture = tt.Furniture != null && tt.Furniture != furniture;

            if (!tileHasOtherFurniture && 
                (tt.Inventory == null || 
                (tt.Inventory.Type == outObjType.ObjectType && tt.Inventory.StackSize + amount <= tt.Inventory.MaxStackSize)))
            {
                // out product can be placed here
                outPlacement.Add(new TileObjectTypeAmount()
                {
                    Tile = tt,
                    IsEmpty = tt.Inventory == null,
                    ObjectType = outObjType.ObjectType,
                    Amount = outObjType.Amount
                });
            }
        }

        return outPlacement;
    }

    private List<KeyValuePair<Tile, int>> CheckForInventoryAtInput(ProductionChain prodChain)
    {
        var flaggedForTaking = new List<KeyValuePair<Tile, int>>();
        foreach (var reqInputItem in prodChain.Input)
        {
            // check input slots for req. item:                        
            Tile tile = World.Current.GetTileAt(
                furniture.Tile.X + reqInputItem.SlotPosX,
                furniture.Tile.Y + reqInputItem.SlotPosY,
                furniture.Tile.Z);

            if (tile.Inventory != null && tile.Inventory.Type == reqInputItem.ObjectType
                && tile.Inventory.StackSize >= reqInputItem.Amount)
            {
                flaggedForTaking.Add(new KeyValuePair<Tile, int>(tile, reqInputItem.Amount));
            }
        }

        return flaggedForTaking;
    }

    private ProductionChain GetProductionChainByName(string productionChainName)
    {
        return PossibleProductions.FirstOrDefault(chain => chain.Name.Equals(productionChainName));        
    }
   
    [Serializable]
    public class Item
    {
        [XmlAttribute("objectType")]
        public string ObjectType { get; set; }
        [XmlAttribute("amount")]
        public int Amount { get; set; }
        [XmlAttribute("slotPosX")]
        public int SlotPosX { get; set; }
        [XmlAttribute("slotPosY")]
        public int SlotPosY { get; set; }
    }

    [Serializable]
    public class ProductionChain
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("processingTime")]
        public float ProcessingTime { get; set; }

        public List<Item> Input { get; set; }

        public List<Item> Output { get; set; }
    }

    [Serializable]
    public class UsedAnimations
    {
        [XmlAttribute("idle")]
        public string Idle { get; set; }

        [XmlAttribute("running")]
        public string Running { get; set; }
    }
    
    private class TileObjectTypeAmount
    {
        public Tile Tile { get; set; }

        public bool IsEmpty { get; set; }

        public string ObjectType { get; set; }

        public int Amount { get; set; }
    }
}
