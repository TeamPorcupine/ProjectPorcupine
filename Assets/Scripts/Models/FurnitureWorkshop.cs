using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot("Workshop")]
public class FurnitureWorkshop
{
    #region Serialization
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
       
    public List<ProductionChain> PossibleProductions { get; set; }

    public static FurnitureWorkshop Deserialize(XmlReader xmlReader)
    {
        // deserialize FActoryINfo into factoryData
        XmlSerializer serializer = new XmlSerializer(typeof(FurnitureWorkshop));
        return (FurnitureWorkshop)serializer.Deserialize(xmlReader);
    }

    #endregion

    // constants for parameters
    const string CUR_PROCESSING_TIME_PARAM_NAME = "cur_processing_time";
    const string MAX_PROCESSING_TIME_PARAM_NAME = "max_processing_time";
    const string CUR_PROCESSED_INV_PARAM_NAME = "cur_processed_inv";
    const string CUR_PRODUCTION_CHAIN_PARAM_NAME = "cur_production_chain";

    [XmlIgnore]
    public List<WorkshopContextMenu> WorkshopMenuActions { get; protected set; }

    private Furniture furniture;

    protected Parameter furnParameters { get { return furniture.Parameters; } }

    public string GetDescription()
    {
        StringBuilder sb = new StringBuilder();
        var prodChain = furnParameters[CUR_PRODUCTION_CHAIN_PARAM_NAME].ToString();
        if (!string.IsNullOrEmpty(prodChain))
        {
            sb.AppendLine(string.Format("Production: {0}", prodChain));
        }
        else
        {
            sb.AppendLine("No selected production");
        }
        return sb.ToString();
    }

    public void Initialize()
    {
        WorkshopMenuActions = new List<WorkshopContextMenu>();
        //this.furniture = furniture;

        // check if context menu is needed
        if (PossibleProductions.Count > 1)
        {
            furnParameters.AddParameter(new Parameter(CUR_PRODUCTION_CHAIN_PARAM_NAME, null));
            foreach (var chain in PossibleProductions)
            {
                string prodChainName = chain.Name;
                WorkshopMenuActions.Add(new WorkshopContextMenu()
                {
                    ProductionChainName = prodChainName,
                    Function = (furn, prod) =>
                    {
                        furn.Parameters[CUR_PRODUCTION_CHAIN_PARAM_NAME].SetValue(prod);
                    }
                });
            }
        }
        else
        {
            if (PossibleProductions.Count == 1)
                furnParameters.AddParameter(new Parameter(CUR_PRODUCTION_CHAIN_PARAM_NAME,
                    PossibleProductions[0].Name));
            else
                Debug.ULogWarning("Furniture {0} is marked as factory, but has no production chain", furniture.Name);
        }

        // add dynamic params here
        furnParameters.AddParameter(new Parameter(CUR_PROCESSING_TIME_PARAM_NAME, 0f));
        furnParameters.AddParameter(new Parameter(MAX_PROCESSING_TIME_PARAM_NAME, 0f));
        furnParameters.AddParameter(new Parameter(CUR_PROCESSED_INV_PARAM_NAME, 0));
    }

    public void SetParentFurniture(Furniture furniture)
    {
        this.furniture = furniture;
    }

    public void Update(float deltaTime)
    {
        // if there is enough input, do the processing and store item to output
        // - remove items from input
        // - add param to reflect factory can provide output (has output inside)
        //   - as output will be produced after time, it is possible that output spot can be ocupied meanwhile
        // - process for specified time
        // - if output slot is free, provide output (if not, keep output 'inside' factory)

        var curSetupChainName = furnParameters[CUR_PRODUCTION_CHAIN_PARAM_NAME].ToString();

        if (!string.IsNullOrEmpty(curSetupChainName))
        {
            FurnitureWorkshop.ProductionChain prodChain = GetProductionChainInfo(curSetupChainName);
            // if there is no processing in progress
            if (furnParameters[CUR_PROCESSED_INV_PARAM_NAME].ToInt() == 0)
            {
                // check input slots for input inventory               
                List<KeyValuePair<Tile, int>> flaggedForTaking = CheckForInventoryAtInput(prodChain);
                // if all the input requirements are ok, you can start processing:
                if (flaggedForTaking.Count == prodChain.Input.Count)
                {
                    // consume input inventory
                    ConsumeInventory(flaggedForTaking);

                    furnParameters[CUR_PROCESSED_INV_PARAM_NAME].SetValue(prodChain.Output.Count);

                    // reset processing timer and set max time for processing for this prod. chain
                    furnParameters[CUR_PROCESSING_TIME_PARAM_NAME].SetValue(0f);
                    furnParameters[MAX_PROCESSING_TIME_PARAM_NAME].SetValue(prodChain.ProcessingTime);
                }
            }
            else
            {
                // processing is in progress
                furnParameters[CUR_PROCESSING_TIME_PARAM_NAME].ChangeFloatValue(deltaTime);

                if (furnParameters[CUR_PROCESSING_TIME_PARAM_NAME].ToFloat() >=
                    furnParameters[MAX_PROCESSING_TIME_PARAM_NAME].ToFloat())
                {
                    List<TileObjectTypeAmount> outPlacement = CheckForInventoryAtOutput(prodChain);

                    // if output placement was found for all products, place them
                    if (outPlacement.Count == prodChain.Output.Count)
                    {
                        PlaceInventory(outPlacement);
                        // processing done, can fetch input for another processing
                        furnParameters[CUR_PROCESSED_INV_PARAM_NAME].SetValue(0);
                    }
                }
            }
            // create possible jobs for factory(hauling input)
            HaulingJobForInputs(prodChain);
        }
    }

    private void HaulingJobForInputs(FurnitureWorkshop.ProductionChain prodChain)
    {
        // for all inputs in production chain
        foreach (var reqInputItem in prodChain.Input)
        {
            // if there is no hauling job for input object type, create one
            var existingHaulingJob = furniture.jobs.Any(x => x.inventoryRequirements.ContainsKey(reqInputItem.ObjectType));
            if (!existingHaulingJob)
            {
                Tile inTile = World.Current.GetTileAt(furniture.Tile.X + reqInputItem.SlotPosX, furniture.Tile.Y + reqInputItem.SlotPosY);

                //// TODO: this is from LUA .. looks like some hack
                if (inTile.Inventory != null && inTile.Inventory.StackSize == inTile.Inventory.maxStackSize)
                {
                    furniture.CancelJobs();
                    return;
                }

                string desiredInv = reqInputItem.ObjectType;
                int desiredAmount = PrototypeManager.Inventory.GetPrototype(desiredInv).maxStackSize;
                if (inTile.Inventory != null && inTile.Inventory.objectType == reqInputItem.ObjectType &&
                    inTile.Inventory.StackSize <= desiredAmount)
                {
                    desiredAmount = desiredAmount - inTile.Inventory.StackSize;
                }

                Action<Job> jobWorkedAction = (job) =>
                {
                    job.CancelJob();
                    foreach (var jobInvReq in job.inventoryRequirements)
                    {
                        Inventory inv = jobInvReq.Value;
                        if (inv != null && inv.StackSize > 0)
                        {
                            World.Current.inventoryManager.PlaceInventory(job.tile, jobInvReq.Value);
                            job.tile.Inventory.locked = true;
                        }
                    }
                };

                if (desiredAmount > 0)
                {
                    // beware: passed jobObjectType is expected Furniture only !!
                    var jb = new Job(inTile, null, null, 0.4f,
                        new Inventory[] { new Inventory(desiredInv, desiredAmount, 0) },
                        Job.JobPriority.Medium, false, false, false);
                    jb.OnJobWorked += jobWorkedAction;
                    furniture.AddJob(jb);
                }
            }
        }
    }

    private static void PlaceInventory(List<TileObjectTypeAmount> outPlacement)
    {
        foreach (var outPlace in outPlacement)
        {
            if (outPlace.IsEmpty)
                World.Current.inventoryManager.PlaceInventory(outPlace.Tile,
                    new Inventory(outPlace.ObjectType, outPlace.Amount));
            else
                outPlace.Tile.Inventory.StackSize += outPlace.Amount;
        }
    }

    private List<TileObjectTypeAmount> CheckForInventoryAtOutput(FurnitureWorkshop.ProductionChain prodChain)
    {
        List<TileObjectTypeAmount> outPlacement = new List<TileObjectTypeAmount>();
        // processing is done, try to spit the output
        // check if output can be placed in world

        foreach (var outObjType in prodChain.Output)
        {
            int amount = outObjType.Amount;

            // check ouput slots for products:                        
            Tile tt = World.Current.GetTileAt(
                furniture.Tile.X + outObjType.SlotPosX, furniture.Tile.Y + outObjType.SlotPosY);

            bool tileHasOtherFurniture = tt.Furniture != null && tt.Furniture != furniture;

            if (!tileHasOtherFurniture && (tt.Inventory == null || tt.Inventory.objectType == outObjType.ObjectType
                && tt.Inventory.StackSize + amount <= tt.Inventory.maxStackSize))
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

    private static void ConsumeInventory(List<KeyValuePair<Tile, int>> flaggedForTaking)
    {
        foreach (var toConsume in flaggedForTaking)
        {
            toConsume.Key.Inventory.StackSize -= toConsume.Value;
            // TODO: this should be handled somewhere else
            if (toConsume.Key.Inventory.StackSize <= 0)
                toConsume.Key.Inventory = null;
        }
    }

    private List<KeyValuePair<Tile, int>> CheckForInventoryAtInput(FurnitureWorkshop.ProductionChain prodChain)
    {
        List<KeyValuePair<Tile, int>> flaggedForTaking = new List<KeyValuePair<Tile, int>>();
        foreach (var reqInputItem in prodChain.Input)
        {
            // check input slots for req. item:                        
            Tile tt = World.Current.GetTileAt(furniture.Tile.X + reqInputItem.SlotPosX, furniture.Tile.Y + reqInputItem.SlotPosY);

            if (tt.Inventory != null && tt.Inventory.objectType == reqInputItem.ObjectType
                && tt.Inventory.StackSize >= reqInputItem.Amount)
            {
                flaggedForTaking.Add(new KeyValuePair<Tile, int>(tt, reqInputItem.Amount));
            }
        }

        return flaggedForTaking;
    }

    private FurnitureWorkshop.ProductionChain GetProductionChainInfo(string curSetupChainName)
    {
        FurnitureWorkshop.ProductionChain cChain = null;
        foreach (var chain in PossibleProductions)
        {
            if (chain.Name.Equals(curSetupChainName))
            {
                cChain = chain;
                break;
            }
        }

        return cChain;
    }
}

public class TileObjectTypeAmount
{
    public Tile Tile { get; set; }
    public bool IsEmpty { get; set; }
    public string ObjectType { get; set; }
    public int Amount { get; set; }
}

public class WorkshopContextMenu
{
    public string ProductionChainName { get; set; }
    public Action<Furniture, string> Function { get; set; }
}



