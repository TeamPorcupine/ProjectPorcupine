using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

/// <summary>
/// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa).
/// </summary>
public partial class Furniture : IXmlSerializable, ISelectable, IContextActionProvider
{
    // constants for parameters
    const string CUR_PROCESSING_TIME_PARAM_NAME = "cur_processing_time";
    const string MAX_PROCESSING_TIME_PARAM_NAME = "max_processing_time";
    const string CUR_PROCESSED_INV_PARAM_NAME = "cur_processed_inv";
    const string CUR_PRODUCTION_CHAIN_PARAM_NAME = "cur_production_chain";

    private FactoryInfo factoryData;
    
    private List<FactoryContextMenu> factoryMenuActions;

    private void UpdateFactory(float deltaTime)
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
            FactoryInfo.ProductionChain prodChain = GetProductionChainInfo(curSetupChainName);
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

    private void HaulingJobForInputs(FactoryInfo.ProductionChain prodChain)
    {
        // for all inputs in production chain
        foreach (var reqInputItem in prodChain.Input)
        {
            // if there is no hauling job for input object type, create one
            if (jobs.FirstOrDefault(x => x.JobObjectType == reqInputItem.ObjectType) == null)
            {
                Tile inTile = World.Current.GetTileAt(Tile.X + reqInputItem.SlotPosX, Tile.Y + reqInputItem.SlotPosY);

                // TODO: this is from LUA .. looks like some hack
                if (inTile.Inventory != null && inTile.Inventory.StackSize == inTile.Inventory.maxStackSize)
                {
                    CancelJobs();
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
                    var jb = new Job(inTile, desiredInv, null, 0.4f,
                        new Inventory[] { new Inventory(desiredInv, desiredAmount, 0) },
                        Job.JobPriority.Medium, false, false, false);
                    jb.OnJobWorked += jobWorkedAction;
                    AddJob(jb);
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

    private List<TileObjectTypeAmount> CheckForInventoryAtOutput(FactoryInfo.ProductionChain prodChain)
    {
        List<TileObjectTypeAmount> outPlacement = new List<TileObjectTypeAmount>();
        // processing is done, try to spit the output
        // check if output can be placed in world

        foreach (var outObjType in prodChain.Output)
        {
            int amount = outObjType.Amount;

            // check ouput slots for products:                        
            Tile tt = World.Current.GetTileAt(
                Tile.X + outObjType.SlotPosX, Tile.Y + outObjType.SlotPosY);

            if (tt.Inventory == null || tt.Inventory.objectType == outObjType.ObjectType
                && tt.Inventory.StackSize + amount <= tt.Inventory.maxStackSize)
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

    private List<KeyValuePair<Tile, int>> CheckForInventoryAtInput(FactoryInfo.ProductionChain prodChain)
    {
        List<KeyValuePair<Tile, int>> flaggedForTaking = new List<KeyValuePair<Tile, int>>();
        foreach (var reqInputItem in prodChain.Input)
        {
            // check input slots for req. item:                        
            Tile tt = World.Current.GetTileAt(Tile.X + reqInputItem.SlotPosX, Tile.Y + reqInputItem.SlotPosY);

            if (tt.Inventory != null && tt.Inventory.objectType == reqInputItem.ObjectType
                && tt.Inventory.StackSize >= reqInputItem.Amount)
            {
                flaggedForTaking.Add(new KeyValuePair<Tile, int>(tt, reqInputItem.Amount));
            }
        }

        return flaggedForTaking;
    }

    private FactoryInfo.ProductionChain GetProductionChainInfo(string curSetupChainName)
    {
        FactoryInfo.ProductionChain cChain = null;
        foreach (var chain in factoryData.PossibleProductions)
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

