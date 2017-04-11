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
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProjectPorcupine.Jobs;

namespace ProjectPorcupine.Buildable.Components
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [XmlRoot("Component")]
    [BuildableComponentName("Workshop")]
    public class Workshop : BuildableComponent
    {        
        public Workshop()
        {
        }
        
        private Workshop(Workshop other) : base(other)
        {
            ParamsDefinitions = other.ParamsDefinitions;
            PossibleProductions = other.PossibleProductions;
            RunConditions = other.RunConditions;
            HaulConditions = other.HaulConditions;
            Efficiency = other.Efficiency;
        }

        [XmlElement("ParameterDefinitions")]
        [JsonProperty("ParameterDefinitions")]
        public WorkShopParameterDefinitions ParamsDefinitions { get; set; }

        public Parameter CurrentProcessingTime
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.CurrentProcessingTime.ParameterName];
            }
        }

        public Parameter MaxProcessingTime
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.MaxProcessingTime.ParameterName];
            }
        }
        
        /// <summary>
        /// Means that there is input being processed (input was consumed and is inside machine).
        /// </summary>
        public Parameter InputProcessed
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.InputProcessed.ParameterName];
            }
        }
        
        [XmlIgnore]
        public Parameter IsRunning
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.IsRunning.ParameterName];
            }

            set 
            {
                FurnitureParams[ParamsDefinitions.IsRunning.ParameterName] = value;
            }
        }

        public Parameter CurrentProductionChainName
        { 
            get
            {
                return FurnitureParams[ParamsDefinitions.CurrentProductionChainName.ParameterName];
            }
        }

        public Parameter InputHaulingJobsCount
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.InputHaulingJobsCount.ParameterName];
            }
        }

        public Parameter HasAllNeededInputInventory
        {
            get
            {
                return FurnitureParams[ParamsDefinitions.HasAllNeededInputInventory.ParameterName];
            }
        }

        [XmlElement("ProductionChain")]
        [JsonProperty("ProductionChain")]
        public List<ProductionChain> PossibleProductions { get; set; }

        [XmlElement("RunConditions")]
        [JsonProperty("RunConditions")]
        public Conditions RunConditions { get; set; }

        [XmlElement("HaulConditions")]
        [JsonProperty("HaulConditions")]
        public Conditions HaulConditions { get; set; }

        [XmlElement("Efficiency")]
        [JsonProperty("Efficiency")]
        public SourceDataInfo Efficiency { get; set; }

        public override bool RequiresSlowUpdate
        {
            get
            {
                return true;
            }
        }   
                
        [XmlIgnore]
        private List<ComponentContextMenu> WorkshopMenuActions { get; set; }
        
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
                float curProcessingTime = CurrentProcessingTime.ToFloat();
                if (!ParentFurniture.HasCustomProgressReport && curProcessingTime > 0f)
                {
                    float perc = 0f;
                    float maxProcessingTime = MaxProcessingTime.ToFloat();
                    if (maxProcessingTime != 0f)
                    {
                        perc = curProcessingTime * 100f / maxProcessingTime;
                        if (perc > 100f)
                        {
                            perc = 100f;
                        }
                    }

                    sb.AppendLine(string.Format("Progress: {0:0}%", perc));
                }

                int numHaulingJobs = InputHaulingJobsCount.ToInt();
                if (numHaulingJobs > 0)
                {
                    sb.AppendLine(string.Format("Hauling jobs: {0}", numHaulingJobs));
                }

                yield return sb.ToString();
            }
            else
            {
                yield return null;
            }
        }

        public override bool CanFunction()
        {
            bool canWork = false;
            string curSetupChainName = CurrentProductionChainName.ToString();
            
            if (!string.IsNullOrEmpty(curSetupChainName))
            {
                canWork = true;
                ProductionChain prodChain = GetProductionChainByName(curSetupChainName);
                //// create possible jobs for factory(hauling input)
                bool areAllHaulParamReqsFulfilled = true;
                if (HaulConditions != null)
                {
                    areAllHaulParamReqsFulfilled = AreParameterConditionsFulfilled(RunConditions.ParamConditions);
                }

                if (areAllHaulParamReqsFulfilled)
                {
                    HaulingJobForInputs(prodChain);
                    canWork = true;
                }
                
                componentRequirements = Requirements.None;
            }
            else
            {
                componentRequirements = Requirements.Production;
            }

            return canWork;
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

            if (RunConditions != null && AreParameterConditionsFulfilled(RunConditions.ParamConditions) == false)
            {
                IsRunning.SetValue(false);
                return;
            }

            float efficiency = 1f;
            if (Efficiency != null)
            {
                efficiency = RetrieveFloatFor(Efficiency, ParentFurniture);
            }
            
            string curSetupChainName = CurrentProductionChainName.ToString();

            if (!string.IsNullOrEmpty(curSetupChainName))
            {
                ProductionChain prodChain = GetProductionChainByName(curSetupChainName);

                // if there is no processing in progress
                if (InputProcessed.ToInt() == 0)
                {
                    // check input slots for input inventory               
                    List<KeyValuePair<Tile, int>> flaggedForTaking = CheckForInventoryAtInput(prodChain);

                    // if all the input requirements are ok, you can start processing:
                    if (flaggedForTaking.Count == prodChain.Input.Count)
                    {
                        // consume input inventory
                        ConsumeInventories(flaggedForTaking);

                        InputProcessed.SetValue(1); // check if it can be bool
                        IsRunning.SetValue(true);

                        // reset processing timer and set max time for processing for this prod. chain
                        CurrentProcessingTime.SetValue(0f);
                        MaxProcessingTime.SetValue(prodChain.ProcessingTime);
                        HasAllNeededInputInventory.SetValue(true);
                    }
                    else
                    {
                        HasAllNeededInputInventory.SetValue(false);
                    } 
                }
                else
                {
                    // processing is in progress
                    CurrentProcessingTime.ChangeFloatValue(deltaTime * efficiency);
                    IsRunning.SetValue(true);

                    if (CurrentProcessingTime.ToFloat() >=
                        MaxProcessingTime.ToFloat())
                    {
                        List<TileObjectTypeAmount> outPlacement = CheckForInventoryAtOutput(prodChain);

                        // if output placement was found for all products, place them
                        if (outPlacement.Count == prodChain.Output.Count)
                        {
                            PlaceInventories(outPlacement);
                            //// processing done, can fetch input for another processing
                            InputProcessed.SetValue(0);
                            IsRunning.SetValue(false);
                            CurrentProcessingTime.SetValue(0f);
                        }
                    }                    
                }
            }
        }

        public override List<ContextMenuAction> GetContextMenu()
        {
            if (WorkshopMenuActions != null)
            {
                return WorkshopMenuActions.Select(x => CreateComponentContextMenuItem(x)).ToList();
            }
            else
            {
                return null;
            }
        }

        protected override void Initialize()
        {
            if (ParamsDefinitions == null)
            {
                // don't need definition for all furniture, just use defaults
                ParamsDefinitions = new WorkShopParameterDefinitions();
            }

            // check if context menu is needed
            if (PossibleProductions.Count > 1)
            {
                componentRequirements = Requirements.Production;

                WorkshopMenuActions = new List<ComponentContextMenu>();
                
                CurrentProductionChainName.SetValue(null);
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
                    CurrentProductionChainName.SetValue(PossibleProductions[0].Name);
                }
                else
                {
                    UnityDebugger.Debugger.LogWarningFormat(ComponentLogChannel, "Furniture {0} is marked as factory, but has no production chain", ParentFurniture.Type);
                }
            }

            // add dynamic params here
            CurrentProcessingTime.SetValue(0);
            MaxProcessingTime.SetValue(0);
            InputProcessed.SetValue(0);
            
            ParentFurniture.Removed += WorkshopRemoved;
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
                World.Current.InventoryManager.ConsumeInventory(toConsume.Key, toConsume.Value);
            }
        }

        private void PlaceInventoryToWorkshopInput(Job job)
        {
            job.CancelJob();
            foreach (Inventory heldInventory in job.DeliveredItems.Values)
            {
                if (heldInventory.StackSize > 0)
                {
                    World.Current.InventoryManager.PlaceInventory(job.tile, heldInventory);
                    job.tile.Inventory.Locked = true;
                }
            }
        }

        private void UnlockInventoryAtInput(Furniture furniture)
        {
            // go though all productions and unlock the inputs
            foreach (ProductionChain prodChain in PossibleProductions)
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
                        UnityDebugger.Debugger.LogFormat(ComponentLogChannel, "Inventory {0} at tile {1} is unlocked", tile.Inventory, tile);
                    }
                }
            }
        }

        private void WorkshopRemoved(Furniture furniture)
        {
            // unlock all inventories at input if there is something left
            UnlockInventoryAtInput(ParentFurniture);
        }

        private void ChangeCurrentProductionChain(Furniture furniture, string newProductionChainName)
        {
            string oldProductionChainName = furniture.Parameters[ParamsDefinitions.CurrentProductionChainName.ParameterName].Value;
            bool isProcessing = furniture.Parameters[ParamsDefinitions.InputProcessed.ParameterName].ToInt() > 0;

            // if selected production really changes and nothing is being processed now
            if (isProcessing || newProductionChainName.Equals(oldProductionChainName))
            {
                return;
            }

            furniture.Jobs.CancelAll();
            furniture.Parameters[ParamsDefinitions.CurrentProductionChainName.ParameterName].SetValue(newProductionChainName);
            
            // unlock all inventories at input if there is something left
            UnlockInventoryAtInput(furniture);
        }

        private void HaulingJobForInputs(ProductionChain prodChain)
        {
            int numHaulingJobs = 0;
            bool isProcessing = InputProcessed.ToInt() > 0;
            //// for all inputs in production chain
            foreach (Item reqInputItem in prodChain.Input)
            {                
                if (isProcessing && !reqInputItem.HasHopper)
                {
                    continue;
                }

                //// if there is no hauling job for input object type, create one                
                Job furnJob;
                string requiredType = reqInputItem.ObjectType;
                bool existingHaulingJob = ParentFurniture.Jobs.HasJobWithPredicate(x => x.RequestedItems.ContainsKey(requiredType), out furnJob);

                if (existingHaulingJob)
                {
                    numHaulingJobs++;
                }
                else
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
                        desiredAmount = PrototypeManager.Inventory.Get(desiredInv).MaxStackSize;
                    }

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
                                     new RequestedItem[] { new RequestedItem(desiredInv, desiredAmount, desiredAmount) },
                                     Job.JobPriority.Medium,
                                     false,
                                     false,
                                     false);
                        
                        job.Description = string.Format("Hauling '{0}' to '{1}'", desiredInv, ParentFurniture.GetName());
                        job.OnJobWorked += PlaceInventoryToWorkshopInput;
                        ParentFurniture.Jobs.Add(job);
                        numHaulingJobs++;
                    }
                }
            }

            InputHaulingJobsCount.SetValue(numHaulingJobs);
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
        [JsonObject(MemberSerialization.OptOut)]
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
        [JsonObject(MemberSerialization.OptOut)]
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
        [JsonObject(MemberSerialization.OptOut)]
        public class WorkShopParameterDefinitions
        {
            // constants for parameters
            public const string CurProcessingTimeParamName = "cur_processing_time";
            public const string MaxProcessingTimeParamName = "max_processing_time";
            public const string CurProcessedInvParamName = "cur_processed_inv";
            public const string CurIsRunningParamName = "workshop_is_running";
            public const string CurProductionChainParamName = "cur_production_chain";
            public const string InputHaulingJobsCountParamName = "input_hauling_job_count";
            public const string HasAllNeededInputInventoryParamName = "has_all_needed_input_inv";
            
            public WorkShopParameterDefinitions()
            {
                // default values if not defined from outside
                CurrentProcessingTime = new ParameterDefinition(CurProcessingTimeParamName);
                MaxProcessingTime = new ParameterDefinition(MaxProcessingTimeParamName);
                InputProcessed = new ParameterDefinition(CurProcessedInvParamName);
                IsRunning = new ParameterDefinition(CurIsRunningParamName);
                CurrentProductionChainName = new ParameterDefinition(CurProductionChainParamName);
                InputHaulingJobsCount = new ParameterDefinition(InputHaulingJobsCountParamName);
                HasAllNeededInputInventory = new ParameterDefinition(HasAllNeededInputInventoryParamName);
            }

            public ParameterDefinition CurrentProcessingTime { get; set; }

            public ParameterDefinition MaxProcessingTime { get; set; }

            public ParameterDefinition InputProcessed { get; set; }

            public ParameterDefinition IsRunning { get; set; }

            public ParameterDefinition CurrentProductionChainName { get; set; }

            public ParameterDefinition InputHaulingJobsCount { get; set; }

            public ParameterDefinition HasAllNeededInputInventory { get; set; }
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
