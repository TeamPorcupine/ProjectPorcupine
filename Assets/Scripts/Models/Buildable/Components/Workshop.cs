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
        
        public Workshop()
        {
        }
        
        private Workshop(Workshop other) : base(other)
        {
            ParamDefinitions = other.ParamDefinitions;
            PossibleProductions = other.PossibleProductions;
        }

        [XmlElement("ParameterDefinitions")]
        public ParameterDefinitions ParamDefinitions { get; set; }

        [XmlElement("ProductionChain")]
        public List<ProductionChain> PossibleProductions { get; set; }
                
        [XmlIgnore]
        private List<ComponentContextMenu> WorkshopMenuActions { get; set; }

        private Parameter CurrentProductionChainName
        {
            get
            {
                return FurnitureParams[ParamDefinitions.CurrentProductionChainName.ParameterName];
            }
        }

        private Parameter CurrentProcessingTime
        {
            get
            {
                return FurnitureParams[ParamDefinitions.CurrentProcessingTime.ParameterName];
            }
        }

        private Parameter MaximalProcessingTime
        {
            get
            {
                return FurnitureParams[ParamDefinitions.MaxProcessingTime.ParameterName];
            }
        }

        private Parameter IsProcessing
        {
            get
            {
                return FurnitureParams[ParamDefinitions.IsProcessing.ParameterName];
            }
        }
        
        public override BuildableComponent Clone()
        {
            return new Workshop(this);
        }

        public override IEnumerable<string> GetDescription()
        {
            if (PossibleProductions.Count > 1)
            {
                StringBuilder sb = new StringBuilder();
                string prodChain = CurrentProductionChainName.ToString();
                sb.AppendLine(!string.IsNullOrEmpty(prodChain) ? string.Format("Production: {0}", prodChain) : "No selected production");
                yield return sb.ToString();
            }
            else
            {
                yield return null;
            }
        }

        public override bool CanFunction()
        {
            var curSetupChainName = CurrentProductionChainName.ToString();

            if (!string.IsNullOrEmpty(curSetupChainName))
            {
                ProductionChain prodChain = GetProductionChainByName(curSetupChainName);
                //// create possible jobs for factory(hauling input)
                HaulingJobForInputs(prodChain);
            }

            return true;
        }

        public override void FixedFrequencyUpdate(float deltaTime)
        {
            //// if there is enough input, do the processing and store item to output
            //// - remove items from input
            //// - add param to reflect factory can provide output (has output inside)
            ////   - as output will be produced after time, it is possible that output spot can be ocupied meanwhile
            //// - process for specified time
            //// - if output slot is free, provide output (if not, keep output 'inside' factory)

            if (ParentFurniture.IsBeingDestroyed)
            {
                return;
            }

            var curSetupChainName = CurrentProductionChainName.ToString();

            if (!string.IsNullOrEmpty(curSetupChainName))
            {
                ProductionChain prodChain = GetProductionChainByName(curSetupChainName);
                //// if there is no processing in progress
                if (IsProcessing.ToInt() == 0)
                {
                    // check input slots for input inventory               
                    List<KeyValuePair<Tile, int>> flaggedForTaking = CheckForInventoryAtInput(prodChain);

                    // if all the input requirements are ok, you can start processing:
                    if (flaggedForTaking.Count == prodChain.Input.Count)
                    {
                        // consume input inventory
                        ConsumeInventories(flaggedForTaking);

                        IsProcessing.SetValue(1); // check if it can be bool

                        // reset processing timer and set max time for processing for this prod. chain
                        CurrentProcessingTime.SetValue(0f);
                        MaximalProcessingTime.SetValue(prodChain.ProcessingTime);
                    }                  
                }
                else
                {
                    // processing is in progress
                    CurrentProcessingTime.ChangeFloatValue(deltaTime);

                    if (CurrentProcessingTime.ToFloat() >=
                        MaximalProcessingTime.ToFloat())
                    {
                        List<TileObjectTypeAmount> outPlacement = CheckForInventoryAtOutput(prodChain);

                        // if output placement was found for all products, place them
                        if (outPlacement.Count == prodChain.Output.Count)
                        {
                            PlaceInventories(outPlacement);
                            //// processing done, can fetch input for another processing
                            IsProcessing.SetValue(0);
                        }
                    }                    
                }
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

                //FurnitureParams.AddParameter(new Parameter(CurProductionChainParamName, null));
                CurrentProductionChainName.SetValue(null);
                foreach (var chain in PossibleProductions)
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
                    //FurnitureParams.AddParameter(new Parameter(CurProductionChainParamName, PossibleProductions[0].Name));
                    CurrentProductionChainName.SetValue(PossibleProductions[0].Name);
                }
                else
                {
                    Debug.ULogWarningChannel(ComponentLogChannel, "Furniture {0} is marked as factory, but has no production chain", ParentFurniture.Name);
                }
            }

            // add dynamic params here
            //FurnitureParams.AddParameter(new Parameter(CurProcessingTimeParamName, 0f));
            //FurnitureParams.AddParameter(new Parameter(MaxProcessingTimeParamName, 0f));
            //FurnitureParams.AddParameter(new Parameter(CurProcessedInvParamName, 0));
            CurrentProcessingTime.SetValue(0);
            MaximalProcessingTime.SetValue(0);
            IsProcessing.SetValue(0);
            
            ParentFurniture.Removed += WorkshopRemoved;
        }

        private void PlaceInventories(List<TileObjectTypeAmount> outPlacement)
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
            string oldProductionChainName = furniture.Parameters[ParamDefinitions.CurrentProductionChainName.ParameterName].Value;

            // unlock all inventories at input if there is something left
            UnlockInventoryAtInput(ParentFurniture, GetProductionChainByName(oldProductionChainName));
        }

        private void ChangeCurrentProductionChain(Furniture furniture, string newProductionChainName)
        {
            string oldProductionChainName = furniture.Parameters[ParamDefinitions.CurrentProductionChainName.ParameterName].Value;
            bool isProcessing = furniture.Parameters[ParamDefinitions.IsProcessing.ParameterName].ToInt() > 0;

            // if selected production really changes and nothing is being processed now
            if (isProcessing || newProductionChainName.Equals(oldProductionChainName))
            {
                return;
            }

            furniture.Jobs.CancelAll();
            furniture.Parameters[ParamDefinitions.CurrentProductionChainName.ParameterName].SetValue(newProductionChainName);

            // check for null, production chain can be selected for the first time
            if (oldProductionChainName != null)
            {
                // unlock all inventories at input if there is something left
                ProductionChain oldProdChain = GetProductionChainByName(oldProductionChainName);
                UnlockInventoryAtInput(furniture, oldProdChain);
            }        
        }

        private void HaulingJobForInputs(ProductionChain prodChain)
        {
            bool isProcessing = IsProcessing.ToInt() > 0;
            //// for all inputs in production chain
            foreach (var reqInputItem in prodChain.Input)
            {
                if (isProcessing && !reqInputItem.HasHopper)
                {
                    continue;
                }
                //// if there is no hauling job for input object type, create one
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
                    int desiredAmount = reqInputItem.Amount;

                    if (reqInputItem.HasHopper)
                    {
                        desiredAmount = PrototypeManager.Inventory.Get(desiredInv).maxStackSize;
                    }

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
                                     new RequestedItem[] { new RequestedItem(desiredInv, desiredAmount, desiredAmount) },
                                     Job.JobPriority.Medium,
                                     false,
                                     false,
                                     false);
                        
                        jb.JobDescription = string.Format("Hauling '{0}' to '{1}'", desiredInv, ParentFurniture.Name);
                        jb.OnJobWorked += PlaceInventoryToWorkshopInput;
                        ParentFurniture.Jobs.Add(jb);
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
                    ParentFurniture.Tile.X + outObjType.SlotPosX,
                    ParentFurniture.Tile.Y + outObjType.SlotPosY,
                    ParentFurniture.Tile.Z);

                bool tileHasOtherFurniture = tt.Furniture != null && tt.Furniture != ParentFurniture;

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
            [XmlAttribute("hasHopper")]
            public bool HasHopper { get; set; }
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
        public class ParameterDefinitions
        {
            // constants for parameters
            public const string CurProcessingTimeParamName = "cur_processing_time";
            public const string MaxProcessingTimeParamName = "max_processing_time";
            public const string CurProcessedInvParamName = "cur_processed_inv";
            public const string CurProductionChainParamName = "cur_production_chain";

            public ParameterDefinition CurrentProcessingTime { get; set; }

            public ParameterDefinition MaxProcessingTime { get; set; }

            public ParameterDefinition IsProcessing { get; set; }

            public ParameterDefinition CurrentProductionChainName { get; set; }

            public ParameterDefinitions()
            {
                // default values if not defined from outside
                CurrentProcessingTime = new ParameterDefinition(CurProcessingTimeParamName);
                MaxProcessingTime = new ParameterDefinition(MaxProcessingTimeParamName);
                IsProcessing = new ParameterDefinition(CurProcessedInvParamName);
                CurrentProductionChainName = new ParameterDefinition(CurProductionChainParamName);
            }
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
