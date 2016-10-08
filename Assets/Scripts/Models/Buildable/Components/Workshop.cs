#region License
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

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    [XmlRoot("Component")]
    [BuildableComponentName("Workshop")]
    public class Workshop : BuildableComponent
    {
        // constants for parameters
        public const string CurProcessingTimeParamName = "cur_processing_time";
        public const string MaxProcessingTimeParamName = "max_processing_time";
        public const string CurProcessedInvParamName = "cur_processed_inv";
        public const string CurProductionChainParamName = "cur_production_chain";
        
        private event Action<bool> OnRunningStateChanged;

        [XmlElement("ProductionChain")]
        public List<ProductionChain> PossibleProductions { get; set; }

        [XmlElement("UsedAnimations")]
        public UsedAnimations UsedAnimation { get; set; }
        
        [XmlIgnore]
        public bool IsRunning { get; private set; }
        
        [XmlIgnore]
        private List<ComponentContextMenu> WorkshopMenuActions { get; set; }

        public override string GetDescription()
        {
            StringBuilder sb = new StringBuilder();
            string prodChain = FurnitureParams[CurProductionChainParamName].ToString();
            sb.AppendLine(!string.IsNullOrEmpty(prodChain) ? string.Format("Production: {0}", prodChain) : "No selected production");
            return sb.ToString();
        }
        
        public override void FixedFrequencyUpdate(float deltaTime)
        {
            // if there is enough input, do the processing and store item to output
            // - remove items from input
            // - add param to reflect factory can provide output (has output inside)
            //   - as output will be produced after time, it is possible that output spot can be ocupied meanwhile
            // - process for specified time
            // - if output slot is free, provide output (if not, keep output 'inside' factory)
            if (ParentFurniture.IsBeingDestroyed)
            {
                return;
            }

            string curSetupChainName = FurnitureParams[CurProductionChainParamName].ToString();

            if (!string.IsNullOrEmpty(curSetupChainName))
            {
                ProductionChain prodChain = GetProductionChainByName(curSetupChainName);

                // if there is no processing in progress
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

                    // trigger running state change
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

                            // processing done, can fetch input for another processing
                            FurnitureParams[CurProcessedInvParamName].SetValue(0);
                        }
                    }

                    // trigger running state change
                    if (!IsRunning)
                    {
                        OnRunningStateChanged(IsRunning = true);
                    }
                }

                // create possible jobs for factory(hauling input)
                HaulingJobForInputs(prodChain);
            }
        }

        public override List<ContextMenuAction> GetContextMenu()
        {
            return WorkshopMenuActions.Select(x => CreateComponentContextMenuItem(x)).ToList();
        }

        protected override void Initialize()
        {
            // check if context menu is needed
            if (PossibleProductions.Count > 1)
            {
                WorkshopMenuActions = new List<ComponentContextMenu>();

                FurnitureParams.AddParameter(new Parameter(CurProductionChainParamName, null));
                foreach (ProductionChain chain in PossibleProductions)
                {
                    string prodChainName = chain.Name;
                    WorkshopMenuActions.Add(new ComponentContextMenu()
                    {
                        Name = prodChainName,
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
                    Debug.ULogWarningChannel(ComponentLogChannel, "Furniture {0} is marked as factory, but has no production chain", ParentFurniture.Name);
                }
            }

            // add dynamic params here
            FurnitureParams.AddParameter(new Parameter(CurProcessingTimeParamName, 0f));
            FurnitureParams.AddParameter(new Parameter(MaxProcessingTimeParamName, 0f));
            FurnitureParams.AddParameter(new Parameter(CurProcessedInvParamName, 0));
            IsRunning = false;
            OnRunningStateChanged += RunningStateChanged;
            ParentFurniture.Removed += WorkshopRemoved;
            ParentFurniture.IsOperatingChanged += (furniture) => RunningStateChanged(IsRunning && furniture.IsOperating);
        }

        private void PlaceInventories(List<TileObjectTypeAmount> outPlacement)
        {
            foreach (TileObjectTypeAmount outPlace in outPlacement)
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

        private void ConsumeInventories(List<KeyValuePair<Tile, int>> flaggedForTaking)
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

        private void PlaceInventoryToWorkshopInput(Job job)
        {
            job.CancelJob();
            foreach (Inventory heldInventory in job.HeldInventory.Values)
            {
                if (heldInventory.StackSize > 0)
                {
                    World.Current.InventoryManager.PlaceInventory(job.tile, heldInventory);
                    job.tile.Inventory.Locked = true;
                }
            }
        }

        private void UnlockInventoryAtInput(Furniture furniture, ProductionChain prodChain)
        {
            foreach (Item inputItem in prodChain.Input)
            {
                // check input slots for req. item:                        
                Tile tile = World.Current.GetTileAt(
                    furniture.Tile.X + inputItem.SlotPosX,
                    furniture.Tile.Y + inputItem.SlotPosY,
                    furniture.Tile.Z);

                if (tile.Inventory != null && tile.Inventory.Locked)
                {
                    tile.Inventory.Locked = false;
                    Debug.ULogChannel(ComponentLogChannel, "Inventory {0} at tile {1} is unlocked", tile.Inventory, tile);
                }
            }
        }

        private void WorkshopRemoved(Furniture furniture)
        {
            string oldProductionChainName = furniture.Parameters[CurProductionChainParamName].Value;

            // unlock all inventories at input if there is something left
            UnlockInventoryAtInput(ParentFurniture, GetProductionChainByName(oldProductionChainName));
        }

        private void RunningStateChanged(bool newIsRunningState)
        {
            if (UsedAnimation != null)
            {
                if (newIsRunningState == true && !string.IsNullOrEmpty(UsedAnimation.Running))
                {
                    ParentFurniture.Animation.SetState(UsedAnimation.Running);
                }
                else if (newIsRunningState == false && !string.IsNullOrEmpty(UsedAnimation.Idle))
                {
                    ParentFurniture.Animation.SetState(UsedAnimation.Idle);
                }
            }
        }

        private void ChangeCurrentProductionChain(Furniture furniture, string newProductionChainName)
        {
            string oldProductionChainName = furniture.Parameters[CurProductionChainParamName].Value;
            bool isProcessing = furniture.Parameters[CurProcessedInvParamName].ToInt() > 0;

            // if selected production really changes and nothing is being processed now
            if (isProcessing || newProductionChainName.Equals(oldProductionChainName))
            {
                return;
            }

            furniture.Jobs.CancelAll();
            furniture.Parameters[CurProductionChainParamName].SetValue(newProductionChainName);

            // unlock all inventories at input if there is something left
            ProductionChain oldProdChain = GetProductionChainByName(oldProductionChainName);
            if (oldProdChain != null)
            {
                UnlockInventoryAtInput(furniture, oldProdChain);
            }
            else
            {
                Debug.ULogWarningChannel(ComponentLogChannel, "Workshop old production chain is null for some reason.");
            }
        }

        private void HaulingJobForInputs(ProductionChain prodChain)
        {
            // for all inputs in production chain
            foreach (Item reqInputItem in prodChain.Input)
            {
                // if there is no hauling job for input object type, create one
                Job furnJob;
                string requiredType = reqInputItem.ObjectType;
                bool existingHaulingJob = ParentFurniture.Jobs.HasJobWithPredicate(x => x.RequestedItems.ContainsKey(requiredType), out furnJob);
                if (!existingHaulingJob)
                {
                    Tile inTile = World.Current.GetTileAt(
                                      ParentFurniture.Tile.X + reqInputItem.SlotPosX,
                                      ParentFurniture.Tile.Y + reqInputItem.SlotPosY,
                                      ParentFurniture.Tile.Z);

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
                        Job job = new Job(
                                     inTile,
                                     null,  // beware: passed jobObjectType is expected Furniture only !!
                                     null,
                                     0.4f,
                                     new RequestedItem[] { new RequestedItem(desiredInv, desiredAmount) },
                                     Job.JobPriority.Medium,
                                     false,
                                     false,
                                     false);
                        job.JobDescription = string.Format("Hauling '{0}' to '{1}'", desiredInv, ParentFurniture.Name);
                        job.OnJobWorked += PlaceInventoryToWorkshopInput;
                        ParentFurniture.Jobs.Add(job);
                    }
                }
            }
        }

        private List<TileObjectTypeAmount> CheckForInventoryAtOutput(ProductionChain prodChain)
        {
            List<TileObjectTypeAmount> outPlacement = new List<TileObjectTypeAmount>();

            // processing is done, try to spit the output
            // check if output can be placed in world
            foreach (Item outObjType in prodChain.Output)
            {
                int amount = outObjType.Amount;

                // check ouput slots for products:                        
                Tile outputTile = World.Current.GetTileAt(
                    ParentFurniture.Tile.X + outObjType.SlotPosX,
                    ParentFurniture.Tile.Y + outObjType.SlotPosY,
                    ParentFurniture.Tile.Z);

                bool tileHasOtherFurniture = outputTile.Furniture != null && outputTile.Furniture != ParentFurniture;

                if (!tileHasOtherFurniture &&
                    (outputTile.Inventory == null ||
                    (outputTile.Inventory.Type == outObjType.ObjectType && outputTile.Inventory.StackSize + amount <= outputTile.Inventory.MaxStackSize)))
                {
                    // out product can be placed here
                    outPlacement.Add(new TileObjectTypeAmount()
                    {
                        Tile = outputTile,
                        IsEmpty = outputTile.Inventory == null,
                        ObjectType = outObjType.ObjectType,
                        Amount = outObjType.Amount
                    });
                }
            }

            return outPlacement;
        }
        
        private List<KeyValuePair<Tile, int>> CheckForInventoryAtInput(ProductionChain prodChain)
        {
            List<KeyValuePair<Tile, int>> flaggedForTaking = new List<KeyValuePair<Tile, int>>();
            foreach (Item reqInputItem in prodChain.Input)
            {
                // check input slots for req. item:                        
                Tile tile = World.Current.GetTileAt(
                    ParentFurniture.Tile.X + reqInputItem.SlotPosX,
                    ParentFurniture.Tile.Y + reqInputItem.SlotPosY,
                    ParentFurniture.Tile.Z);

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
        
        private class TileObjectTypeAmount
        {
            public Tile Tile { get; set; }

            public bool IsEmpty { get; set; }

            public string ObjectType { get; set; }

            public int Amount { get; set; }
        }
    }
}
