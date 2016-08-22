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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Linq;
using ProjectPorcupine.Localization;
using UnityEngine;

/// <summary>
/// A Character is an entity on the map that can move between tiles and,
/// for now, grabs jobs from the work queue and performs this.
/// Later, the Character class will likely be refactored (possibly with
/// sub-classes or interfaces) to support friendly workers, enemies, etc...
/// </summary>
[MoonSharpUserData]
public class Character : IXmlSerializable, ISelectable
{
    /// <summary>
    /// Returns a float representing the Character's X position, which can
    /// be part-way between two tiles during movement.
    /// </summary>
    public float X
    {
        get
        {
            if (NextTile == null)
            {
                return CurrTile.X;
            }

            return Mathf.Lerp(CurrTile.X, NextTile.X, movementPercentage);
        }
    }

    /// <summary>
    /// Returns a float representing the Character's Y position, which can
    /// be part-way between two tiles during movement.
    /// </summary>
    public float Y
    {
        get
        {
            if (NextTile == null)
            {
                return CurrTile.Y;
            }

            return Mathf.Lerp(CurrTile.Y, NextTile.Y, movementPercentage);
        }
    }
    Need[] needs;
    /// <summary>
    /// The tile the Character is considered to still be standing in.
    /// </summary>
    public Tile CurrTile
    {
        get 
        {
            return _currTile;
        }

        protected set
        {
            if (_currTile != null)
            {
                _currTile.characters.Remove(this);
            }

            _currTile = value;
            _currTile.characters.Add(this);
        }
    }

    private Tile _currTile;

    /// <summary>
    /// The Character's current goal tile (not necessarily the next one
    /// he or she will be entering). If we aren't moving, then destTile = currTile .
    /// </summary>
    private Tile DestTile
    {
        get 
        { 
            return _destTile;
        }

        set
        {
            if (_destTile != value)
            {
                _destTile = value;
                pathAStar = null;   // If this is a new destination, then we need to invalidate pathfinding.
            }
        }
    }

    /// Tile where job should be carried out.
    public Tile JobTile
    {
        get
        {
            if (jobTile == null && myJob == null)
                return null;
            return jobTile ?? myJob.tile;
        }
    }

    private Tile _destTile;

    /// The next tile in the pathfinding sequence (the one we are about to enter).
    private Tile NextTile;

    /// Goes from 0 to 1 as we move from CurrTile to NextTile
    private float movementPercentage;

    /// Holds the path to reach DestTile.
    private Path_AStar pathAStar;

    /// Tiles per second
    private float speed = 5f;

    /// A callback to trigger when character information changes (notably, the position)
    public event Action<Character> cbCharacterChanged;

    /// Our job, if any.
    public Job myJob { get; protected set; }

    /// Tile where job should be carried out, if different from myJob.tile
    private Tile jobTile;

    /// Name of the Character
    public string name;

    // The item we are carrying (not gear/equipment)
    public Inventory inventory;

    /// Use only for serialization
    public Character()
    {
        needs = new Need[World.current.needPrototypes.Count];
        LoadNeeds ();
    }

    public Character(Tile tile)
    {
        CurrTile = DestTile = NextTile = tile;
        LoadNeeds ();
    }

    void LoadNeeds()
    {
        needs = new Need[World.current.needPrototypes.Count];
        World.current.needPrototypes.Values.CopyTo (needs, 0);
        for (int i = 0; i < World.current.needPrototypes.Count; i++)
        {
            Need need = needs[i];
            needs[i] = need.Clone();
            needs[i].character = this;
        }
    }
    
    private void GetNewJob()
    {
        float needPercent = 0;
        Need need = null;
        foreach (Need n in needs)
        {
            if (n.Amount > needPercent)
            {
                need = n;
                needPercent = n.Amount;
            }
        }
        if (needPercent > 50 && needPercent < 100 && need != null)
            myJob = new Job (null, need.restoreNeedFurn.objectType, need.CompleteJobNorm, need.restoreNeedTime, null, Job.JobPriority.High, false, true, false);
        if (needPercent == 100 && need != null && need.completeOnFail)
            myJob = new Job (CurrTile, null, need.CompleteJobCrit, need.restoreNeedTime*10, null, Job.JobPriority.High, false, true, true);
        // Get the first job on the queue.
        if (myJob == null)
            myJob = World.current.jobQueue.Dequeue();

        if (myJob == null)
        {
            Debug.Log (name + " did not find a job.");
            myJob = new Job(
                CurrTile,
                "Waiting",
                null,
                UnityEngine.Random.Range (0.1f, 0.5f),
                null,
                Job.JobPriority.Low,
                false);
        }
        else
        {
            if (myJob.tile == null) {
                Debug.Log (name + " found a job.");
            }
            else
            {
                Debug.Log (name + " found a job at x " + myJob.tile.X + " y " + myJob.tile.Y + ".");
            }
        }

        // Get our destination from the job
        DestTile = myJob.tile;

        // If the dest tile does not have neighbours it's very
		if ((DestTile == null || DestTile.HasNeighboursOfType(TileType.Floor) || DestTile.HasNeighboursOfType(TileType.Ladder)) == false)
        {
            Logger.LogVerbose("No neighbouring floor tiles! Abandoning job.");
            AbandonJob(false);
            return;
        }

        myJob.cbJobStopped += OnJobStopped;

        // Immediately check to see if the job tile is reachable.
        // NOTE: We might not be pathing to it right away (due to
        // requiring materials), but we still need to verify that the
        // final location can be reached.
        Profiler.BeginSample("PathGeneration");
        if (myJob.isNeed)
            pathAStar = new Path_AStar (World.current, CurrTile, DestTile, need.restoreNeedFurn.objectType, 0, false, true);    // This will calculate a path from curr to dest.
        else
            pathAStar = new Path_AStar (World.current, CurrTile, DestTile);
        Profiler.EndSample();

        if (pathAStar != null && pathAStar.Length() == 0)
        {
            Logger.LogVerbose("Path_AStar returned no path to target job tile!");
            Debug.Log("Path_AStar returned no path to target job tile!");
            AbandonJob(false);
            return;
        }

        if (myJob.adjacent)
        {
            IEnumerable<Tile> reversed = pathAStar.Reverse();
            reversed = reversed.Skip(1);
            pathAStar = new Path_AStar(new Queue<Tile>(reversed.Reverse()));
            DestTile = pathAStar.EndTile();
            jobTile = DestTile;
        }
        else
        {
            jobTile = myJob.tile;
        }
    }

    private void Update_DoJob(float deltaTime)
    {
        // Check if I already have a job.
        if (myJob == null) 
        {
            GetNewJob();

            // This should only be the case, if there was a pathing issue
            // or insufficent materials after getting the job.
            // In that case, just return early.
            if (myJob == null)
            {
                return;
            }
        }

        // Make sure all materials are in place.
        if (CheckForJobMaterials())
        { 
            // If we get here, then the job has all the material that it needs.
            // Lets make sure that our destination tile is the job site tile.
            DestTile = JobTile;

            // Check if we have reached the destination tiles.
            if (CurrTile == DestTile)
            {
                // We are at the correct tile for our job, so
                // execute the job's "DoWork", which is mostly
                // going to countdown jobTime and potentially
                // call its "Job Complete" callback.
                myJob.DoWork(deltaTime);
            }
        }
        //calculate needs
        foreach (Need n in needs)
        {
            n.Update (deltaTime);
        }
    }

    /// <summary>
    /// Checks weather the current job has all the materials in place and if not instructs the working character to get the materials there first.
    /// Only ever returns true if all materials for the job are at the job location and thus signals to the calling code, that it can proceed with job execution.
    /// </summary>
    /// <returns></returns>
    private bool CheckForJobMaterials()
    {
        if (myJob != null && myJob.isNeed && myJob.critical == false)
        {
            myJob.tile = jobTile = new Path_AStar (World.current, CurrTile, null, myJob.jobObjectType, 0, false, true).EndTile ();
        }
        if (myJob == null || myJob.HasAllMaterial())
        {
            return true; //we can return early
        }
        else
        {
            // Do a quick check, if any inventories with the desired objectType exists.
            Inventory desired = myJob.GetFirstDesiredInventory ();
            if (!World.current.inventoryManager.QuickCheck (desired.objectType))
            {
                // If not, abandon the job and return false.
                Debug.Log (name + " does not have everything they need to complete their job.");
                AbandonJob(true);
                return false;
            }
        }

        // At this point we know, that the job still needs materials.
        // First we check if we carry any materials the job wants by chance.
        if (inventory != null)
        {
            if (myJob.DesiresInventoryType(inventory) > 0)
            {
                // If so, deliver the goods.
                // Walk to the job tile, then drop off the stack into the job.
                if (CurrTile == JobTile)
                {
                    // We are at the job's site, so drop the inventory
                    World.current.inventoryManager.PlaceInventory(myJob, inventory);
                    myJob.DoWork(0); // This will call all cbJobWorked callbacks, because even though
                                     // we aren't progressing, it might want to do something with the fact
                                     // that the requirements are being met.

                    // at this point we should dump anything in our inventory
                    DumpExcessInventory();
                }
                else
                {
                    // We still need to walk to the job site.
                    DestTile = JobTile;
                    return false;
                }
            }
            else
            {
                // We are carrying something, but the job doesn't want it!
                // Dump the inventory so we can be ready to carry what the job actually wants.
                DumpExcessInventory();
            }
        }
        else
        {
            // At this point, the job still requires inventory, but we aren't carrying it!

            // Are we standing on a tile with goods that are desired by the job?
            Logger.LogVerbose("Standing on Tile check");
            if (CurrTile.inventory != null &&
                myJob.DesiresInventoryType(CurrTile.inventory) > 0 && !CurrTile.inventory.isLocked &&
                (myJob.canTakeFromStockpile || CurrTile.furniture == null || CurrTile.furniture.IsStockpile() == false))
            {
                // Pick up the stuff!
                Logger.LogVerbose("Pick up the stuff");

                World.current.inventoryManager.PlaceInventory(
                    this,
                    CurrTile.inventory,
                    myJob.DesiresInventoryType(CurrTile.inventory));
            }
            else
            {
                // Walk towards a tile containing the required goods.
                Logger.LogVerbose("Walk to the stuff");
                Logger.LogVerbose(myJob.canTakeFromStockpile);

                // Find the first thing in the Job that isn't satisfied.
                Inventory desired = myJob.GetFirstDesiredInventory();

                if (CurrTile != NextTile)
                {
                    // We are still moving somewhere, so just bail out.
                    return false;
                }

                // Any chance we already have a path that leads to the items we want?

                // Check that we have an end tile and that it has content.
                if (pathAStar != null && pathAStar.EndTile() != null && pathAStar.EndTile().inventory != null &&
                    
                    // Check if it is a stockpile and we are allowed to grab from it or just not a stockpile
                    !(pathAStar.EndTile().furniture != null && (myJob.canTakeFromStockpile == false && pathAStar.EndTile().furniture.IsStockpile() == true)) &&

                    // Check if contains the desired objectType
                    (pathAStar.EndTile().inventory.objectType == desired.objectType))
                {
                    // We are already moving towards a tile that contains what we want!
                    // so....do nothing?
                }
                else
                {
                    Path_AStar newPath = World.current.inventoryManager.GetPathToClosestInventoryOfType(
                                             desired.objectType,
                                             CurrTile,
                                             desired.maxStackSize - desired.stackSize,
                                             myJob.canTakeFromStockpile );

                    if (newPath == null || newPath.Length() < 1)
                    {
                        //Logger.Log("pathAStar is null and we have no path to object of type: " + desired.objectType);
                        // Cancel the job, since we have no way to get any raw materials!
                        Logger.LogVerbose("No tile contains objects of type '" + desired.objectType + "' to satisfy job requirements.");
                        AbandonJob(true);
                        return false;
                    }

                    Logger.Log("pathAStar returned with length of: " + newPath.Length());

                    DestTile = newPath.EndTile();

                    // Since we already have a path calculated, let's just save that.
                    pathAStar = newPath;

                    // Ignore first tile, because that's what we're already in.
                    NextTile = newPath.Dequeue();
                }

                // One way or the other, we are now on route to an object of the right type.
                return false;
            }
        }

        return false; // We can't continue until all materials are satisfied.
    }

    /// <summary>
    /// This function instructs the character to null its inventory.
    /// However in the fuure it should actually look for a place to dump the materials and then do so.
    /// </summary>
    private void DumpExcessInventory()
    {
        // TODO: Look for Places accepting the inventory in the following order:
        // - Jobs also needing this item (this could serve us when building Walls, as the character could transport ressources for multiple walls at once)
        // - Stockpiles (as not to clutter the floor)
        // - Floor

        ////if (World.current.inventoryManager.PlaceInventory(CurrTile, inventory) == false)
        ////{
        ////    Logger.LogError("Character tried to dump inventory into an invalid tile (maybe there's already something here). FIXME: Setting inventory to null and leaking for now");
        ////    // FIXME: For the sake of continuing on, we are still going to dump any
        ////    // reference to the current inventory, but this means we are "leaking"
        ////    // inventory.  This is permanently lost now.
        ////}

        inventory = null;
    }

    public void AbandonJob(bool intoWaitingQueue)
    {
        Debug.Log (name + " abandoned their job.");
        if (myJob == null)
        {
            return;
        }
        if (myJob.isNeed)
        {
            myJob.cbJobStopped -= OnJobStopped;
            myJob = null;
            return;
        }
        // Character does not have to move,
        // so destination tile is the one
        // he is standing on.
        NextTile = DestTile = CurrTile;

        // Drops the priority a level, to lowest.
        myJob.DropPriority();

        if (intoWaitingQueue)
        {
            // If the job gets abandoned because of missing materials,
            // put it into the waiting queue.
            // Also create a callback for when an inventory gets created.
            // Lastly, remove the job from "myJob".
            World.current.jobWaitingQueue.Enqueue(myJob);
            World.current.cbInventoryCreated += OnInventoryCreated;
            myJob.cbJobStopped -= OnJobStopped;
            myJob = null;
        }
        else
        {
            // If the job gets abandoned because of pathing issues or something else,
            // just put it into the normal job queue and remove the job from "myJob".
            World.current.jobQueue.Enqueue(myJob);
            myJob.cbJobStopped -= OnJobStopped;
            myJob = null;
        }
    }

    private void Update_DoMovement(float deltaTime)
    {
        if (CurrTile == DestTile)
        {
            pathAStar = null;
            return; // We're already were we want to be.
        }

        // currTile = The tile I am currently in (and may be in the process of leaving)
        // nextTile = The tile I am currently entering
        // destTile = Our final destination -- we never walk here directly, but instead use it for the pathfinding
        if (NextTile == null || NextTile == CurrTile)
        {
            // Get the next tile from the pathfinder.
            if (pathAStar == null || pathAStar.Length() == 0)
            {
                // Generate a path to our destination
                pathAStar = new Path_AStar(World.current, CurrTile, DestTile);  // This will calculate a path from curr to dest.
                if (pathAStar.Length() == 0)
                {
                    Logger.LogError("Path_AStar returned no path to destination!");
                    AbandonJob(false);
                    return;
                }

                // Let's ignore the first tile, because that's the tile we're currently in.
                NextTile = pathAStar.Dequeue();
            }

            // Grab the next waypoint from the pathing system!
            NextTile = pathAStar.Dequeue();

            if (NextTile == CurrTile)
            {
                // Logger.LogError("Update_DoMovement - nextTile is currTile?");
            }
        }

        // At this point we should have a valid nextTile to move to.

        // What's the total distance from point A to point B?
        // We are going to use Euclidean distance FOR NOW...
        // But when we do the pathfinding system, we'll likely
        // switch to something like Manhattan or Chebyshev distance
        float distToTravel = Mathf.Sqrt(
                                 Mathf.Pow(CurrTile.X - NextTile.X, 2) +
                                 Mathf.Pow(CurrTile.Y - NextTile.Y, 2));

        if (NextTile.IsEnterable() == ENTERABILITY.Never)
        {
            //// Most likely a wall got built, so we just need to reset our pathfinding information.
            //// FIXME: Ideally, when a wall gets spawned, we should invalidate our path immediately,
            ////            so that we don't waste a bunch of time walking towards a dead end.
            ////            To save CPU, maybe we can only check every so often?
            ////            Or maybe we should register a callback to the OnTileChanged event?
            //// Logger.LogError("FIXME: A character was trying to enter an unwalkable tile.");
            NextTile = null;    // our next tile is a no-go
            pathAStar = null;   // clearly our pathfinding info is out of date.
            return;
        }
        else if (NextTile.IsEnterable() == ENTERABILITY.Soon)
        {
            // We can't enter the NOW, but we should be able to in the
            // future. This is likely a DOOR.
            // So we DON'T bail on our movement/path, but we do return
            // now and don't actually process the movement.
            return;
        }

        // How much distance can be travel this Update?
        float distThisFrame = speed / NextTile.movementCost * deltaTime;

        // How much is that in terms of percentage to our destination?
        float percThisFrame = distThisFrame / distToTravel;

        // Add that to overall percentage travelled.
        movementPercentage += percThisFrame;

        if (movementPercentage >= 1)
        {
            // We have reached our destination

            //// TODO: Get the next tile from the pathfinding system.
            ////       If there are no more tiles, then we have TRULY
            ////       reached our destination.

            CurrTile = NextTile;
            movementPercentage = 0;

            // FIXME?  Do we actually want to retain any overshot movement?
        }
    }

    /// Runs every "frame" while the simulation is not paused
    public void Update(float deltaTime)
    {
        Update_DoJob(deltaTime);

        Update_DoMovement(deltaTime);

        if (cbCharacterChanged != null)
        {
            cbCharacterChanged(this);
        }
    }

    private void OnJobStopped(Job j)
    {
        // Job completed (if non-repeating) or was cancelled.
        j.cbJobStopped -= OnJobStopped;

        if (j != myJob)
        {
            Logger.LogError("Character being told about job that isn't his. You forgot to unregister something.");
            return;
        }

        myJob = null;
    }

    private void OnInventoryCreated(Inventory inv)
    {
        // First remove the callback.
        World.current.cbInventoryCreated -= OnInventoryCreated;

        // Get the relevant job and dequeue it from the waiting queue.
        Job job = World.current.jobWaitingQueue.Dequeue();
        // Check if the initial job still exists.
        // It could have been deleted through the user
        // cancelling the job manually.
        if (job != null)
        {
            // Get the (first) desired inventory for the job.
            Inventory desired = job.GetFirstDesiredInventory();

            // Checking if the objectType from the created inventory
            // and the objectType from the desired one match.
            if (inv.objectType == desired.objectType)
            {
                // If so, enqueue the job onto the (normal)
                // job queue.
                World.current.jobQueue.Enqueue(job);
            }
            else
            {
                // If not, (re)enqueue the job onto the waiting queu
                // and also register a callback for the future.
                World.current.jobWaitingQueue.Enqueue(job);
                World.current.cbInventoryCreated += OnInventoryCreated;
            }
        }
    }

    #region IXmlSerializable implementation

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", CurrTile.X.ToString());
        writer.WriteAttributeString("Y", CurrTile.Y.ToString());
        string needString = "";
        foreach (Need n in needs)
        {
            int storeAmount = (int)n.Amount * 10;
            needString = needString + n.needType + ";" + storeAmount.ToString() + ":";
        }
        writer.WriteAttributeString("needs", needString);
    }

    public void ReadXml(XmlReader reader)
    {
        if (reader.GetAttribute("needs") == null)
            return;
        string[] needListA = reader.GetAttribute("needs").Split(new Char[] {':'});
        foreach (string s in needListA)
        {
            string[] needListB = s.Split(new Char[] { ';' });
            foreach (Need n in needs)
            {
                if (n.needType == needListB[0])
                {
                    int storeAmount;
                    if (int.TryParse(needListB[1], out storeAmount))
                    {
                        n.Amount = (float)storeAmount / 10;
                    }
                    else
                    {
                        Logger.LogError("Character.ReadXml() expected an int when deserializing needs");
                    }
                }
            }
        }
    }

    #endregion

    #region ISelectableInterface implementation

    public string GetName()
    {
        return name;
    }

    public string GetDescription()
    {
        string needText = "";
        foreach (Need n in needs)
        {
            needText += "\n" + LocalizationTable.GetLocalization (n.localisationID, n.DisplayAmount);
        }
        return "A human astronaut." + needText;
    }

    public string GetHitPointString()
    {
        return "100/100";
    }

    #endregion
}
