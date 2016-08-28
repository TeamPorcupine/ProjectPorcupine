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
using ProjectPorcupine.Localization;
using UnityEngine;

public enum Facing
{
    NORTH,
    EAST,
    SOUTH,
    WEST
}

/// <summary>
/// A Character is an entity on the map that can move between tiles and,
/// for now, grabs jobs from the work queue and performs this.
/// Later, the Character class will likely be refactored (possibly with
/// sub-classes or interfaces) to support friendly workers, enemies, etc...
/// </summary>
[MoonSharpUserData]
public class Character : IXmlSerializable, ISelectable, IContextActionProvider
{
    /// Name of the Character.
    public string name;

    /// The item we are carrying (not gear/equipment).
    public Inventory inventory;

    /// Holds all character animations.
    public CharacterAnimation animation;

    /// Is the character walking or idle.
    public bool IsWalking;

    /// What direction our character is looking.
    public Facing CharFacing;

    private Need[] needs;

    /// Destination tile of the character.
    private Tile destTile;

    /// Current tile the character is standing on.
    private Tile currTile;

    /// The next tile in the pathfinding sequence (the one we are about to enter).
    private Tile nextTile;

    /// Goes from 0 to 1 as we move from CurrTile to nextTile.
    private float movementPercentage;

    /// Holds the path to reach DestTile.
    private Path_AStar pathAStar;

    /// Tiles per second.
    private float speed = 5f;

    /// Tile where job should be carried out, if different from MyJob.tile.
    private Tile jobTile;

    private bool selected = false;

    private Color characterColor;

    /// Use only for serialization
    public Character()
    {
        needs = new Need[World.Current.needPrototypes.Count];
        LoadNeeds();
    }

    public Character(Tile tile)
    {
        CurrTile = DestTile = nextTile = tile;
        LoadNeeds();
        characterColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1.0f);
    }

    public Character(Tile tile, Color color)
    {
        CurrTile = DestTile = nextTile = tile;
        characterColor = color;
        LoadNeeds();
    }

    /// A callback to trigger when character information changes (notably, the position).
    public event Action<Character> OnCharacterChanged;

    /// <summary>
    /// Returns a float representing the Character's X position, which can
    /// be part-way between two tiles during movement.
    /// </summary>
    public float X
    {
        get
        {
            if (nextTile == null)
            {
                return CurrTile.X;
            }

            return Mathf.Lerp(CurrTile.X, nextTile.X, movementPercentage);
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
            if (nextTile == null)
            {
                return CurrTile.Y;
            }

            return Mathf.Lerp(CurrTile.Y, nextTile.Y, movementPercentage);
        }
    }

    /// <summary>
    /// The tile the Character is considered to still be standing in.
    /// </summary>
    public Tile CurrTile
    {
        get
        {
            return currTile;
        }

        protected set
        {
            if (currTile != null)
            {
                currTile.Characters.Remove(this);
            }

            currTile = value;
            currTile.Characters.Add(this);
        }
    }

    /// Tile where job should be carried out.
    public Tile JobTile
    {
        get
        {
            if (jobTile == null && MyJob == null)
            {
                return null;
            }

            return jobTile ?? MyJob.tile;
        }
    }

    /// Our job, if any.
    public Job MyJob
    {
        get; protected set;
    }

    public bool IsSelected
    {
        get
        {
            return selected;
        }

        set
        {
            if (value == false)
            {
                VisualPath.Instance.RemoveVisualPoints(name);
            }

            selected = value;
        }
    }

    /// <summary>
    /// The Character's current goal tile (not necessarily the next one
    /// he or she will be entering). If we aren't moving, then destTile = currTile .
    /// </summary>
    private Tile DestTile
    {
        get
        {
            return destTile;
        }

        set
        {
            if (destTile != value)
            {
                destTile = value;
                pathAStar = null;   // If this is a new destination, then we need to invalidate pathfinding.
            }
        }
    }

    public IEnumerable<ContextMenuAction> GetContextMenuActions(ContextMenu contextMenu)
    {
        yield return new ContextMenuAction
        {
            Text = "Poke " + GetName(),
            RequireCharacterSelected = false,
            Action = (cm, c) => Debug.ULogChannel("Character", GetDescription())
        };
    }

    public void AbandonJob(bool intoWaitingQueue)
    {
        Debug.ULogChannel("Character", name + " abandoned their job.");
        if (MyJob == null)
        {
            return;
        }

        if (MyJob.IsNeed)
        {
            MyJob.OnJobStopped -= OnJobStopped;
            MyJob = null;
            return;
        }

        // Character does not have to move,
        // so destination tile is the one
        // he is standing on.
        nextTile = DestTile = CurrTile;

        // Drops the priority a level, to lowest.
        MyJob.DropPriority();

        if (intoWaitingQueue)
        {
            // If the job gets abandoned because of missing materials,
            // put it into the waiting queue.
            // Also create a callback for when an inventory gets created.
            // Lastly, remove the job from "MyJob".
            World.Current.jobWaitingQueue.Enqueue(MyJob);
            World.Current.OnInventoryCreated += OnInventoryCreated;
            MyJob.OnJobStopped -= OnJobStopped;
            MyJob = null;
        }
        else
        {
            // If the job gets abandoned because of pathing issues or something else,
            // just put it into the normal job queue and remove the job from "MyJob".
            World.Current.jobQueue.Enqueue(MyJob);
            MyJob.OnJobStopped -= OnJobStopped;
            MyJob = null;
        }
    }

    public void PrioritizeJob(Job job)
    {
        AbandonJob(false);
        World.Current.jobQueue.Remove(job);
        job.IsBeingWorked = true;

        /*Check if the character is carrying any materials and if they could be used for the new job,
        if the character is carrying materials but is not used in the new job, then drop them
        on the current tile for now.*/

        if (inventory != null && !job.inventoryRequirements.ContainsKey(inventory.objectType))
        {
            World.Current.inventoryManager.PlaceInventory(CurrTile, inventory);
            DumpExcessInventory();
        }

        MyJob = job;

        // Get our destination from the job.
        DestTile = MyJob.tile;

        // If the dest tile does not have neighbours that are walkable it's very likable that they can't be walked to.
        if (DestTile.GetNeighbours().Any((tile) => { return tile.MovementCost > 0; }) == false)
        {
            Debug.ULogChannel("Character", "No neighbouring floor tiles! Abandoning job.");
            AbandonJob(false);
            return;
        }

        MyJob.OnJobStopped += OnJobStopped;

        pathAStar = new Path_AStar(World.Current, CurrTile, DestTile);

        if (pathAStar != null && pathAStar.Length() == 0)
        {
            Debug.ULogChannel("Character", "Path_AStar returned no path to target job tile!");
            AbandonJob(false);
            return;
        }

        if (MyJob.adjacent)
        {
            IEnumerable<Tile> reversed = pathAStar.Reverse();
            reversed = reversed.Skip(1);
            pathAStar = new Path_AStar(new Queue<Tile>(reversed.Reverse()));
            DestTile = pathAStar.EndTile();
            jobTile = DestTile;
        }
        else
        {
            jobTile = MyJob.tile;
        }
    }

    /// Runs every "frame" while the simulation is not paused
    public void Update(float deltaTime)
    {
        Update_DoJob(deltaTime);

        Update_DoMovement(deltaTime);

        if (OnCharacterChanged != null)
        {
            OnCharacterChanged(this);
        }

        animation.Update(deltaTime);
    }

    #region IXmlSerializable implementation

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("name", name);
        writer.WriteAttributeString("X", CurrTile.X.ToString());
        writer.WriteAttributeString("Y", CurrTile.Y.ToString());
        string needString = string.Empty;
        foreach (Need n in needs)
        {
            int storeAmount = (int)(n.Amount * 10);
            needString = needString + n.needType + ";" + storeAmount.ToString() + ":";
        }

        writer.WriteAttributeString("needs", needString);
        writer.WriteAttributeString("r", characterColor.r.ToString());
        writer.WriteAttributeString("b", characterColor.b.ToString());
        writer.WriteAttributeString("g", characterColor.g.ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        if (reader.GetAttribute("needs") == null)
        {
            return;
        }

        string[] needListA = reader.GetAttribute("needs").Split(new char[] { ':' });
        foreach (string s in needListA)
        {
            string[] needListB = s.Split(new char[] { ';' });
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
                        Debug.ULogErrorChannel("Character", "Character.ReadXml() expected an int when deserializing needs");
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
        string needText = string.Empty;
        foreach (Need n in needs)
        {
            needText += "\n" + LocalizationTable.GetLocalization(n.localisationID, n.DisplayAmount);
        }

        return "A human astronaut." + needText;
    }

    public string GetHitPointString()
    {
        return "100/100";
    }

    public Color GetCharacterColor()
    {
        return characterColor;
    }

    public string GetJobDescription()
    {
        if (MyJob == null)
        {
            return "job_no_job_desc";
        }

        return MyJob.JobDescription;
    }
    #endregion

    private void LoadNeeds()
    {
        needs = new Need[World.Current.needPrototypes.Count];
        World.Current.needPrototypes.Values.CopyTo(needs, 0);
        for (int i = 0; i < World.Current.needPrototypes.Count; i++)
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
        {
            MyJob = new Job(null, need.RestoreNeedFurn.ObjectType, need.CompleteJobNorm, need.RestoreNeedTime, null, Job.JobPriority.High, false, true, false);
        }

        if (needPercent == 100 && need != null && need.CompleteOnFail)
        {
            MyJob = new Job(CurrTile, null, need.CompleteJobCrit, need.RestoreNeedTime * 10, null, Job.JobPriority.High, false, true, true);
        }

        // Get the first job on the queue.
        if (MyJob == null)
        {
            MyJob = World.Current.jobQueue.Dequeue();

            // Check if we got a job from the queue.
            if (MyJob == null)
            {
                Debug.ULogChannel("Character", name + " did not find a job.");
                MyJob = new Job(
                    CurrTile,
                    "Waiting",
                    null,
                    UnityEngine.Random.Range(0.1f, 0.5f),
                    null,
                    Job.JobPriority.Low,
                    false);
                MyJob.JobDescription = "job_waiting_desc";
            }
            else
            {
                if (MyJob.tile == null)
                {
                    Debug.ULogChannel("Character", name + " found a job.");
                }
                else
                {
                    Debug.ULogChannel("Character", name + " found a job at x " + MyJob.tile.X + " y " + MyJob.tile.Y + ".");
                }
            }
        }

        // Get our destination from the job.
        DestTile = MyJob.tile;

        // If the dest tile does not have neighbours that are walkable it's very likely that they can't be walked to
        if (DestTile != null)
        {
            if (DestTile.GetNeighbours().Any((tile) => { return tile.MovementCost > 0; }) == false)
            {
                Debug.ULogChannel("Character", "No neighbouring floor tiles! Abandoning job.");
                AbandonJob(false);
                return;
            }
        }

        MyJob.OnJobStopped += OnJobStopped;

        // Immediately check to see if the job tile is reachable.
        // NOTE: We might not be pathing to it right away (due to
        // requiring materials), but we still need to verify that the
        // final location can be reached.
        Profiler.BeginSample("PathGeneration");
        if (MyJob.IsNeed)
        {
            // This will calculate a path from curr to dest.
            pathAStar = new Path_AStar(World.Current, CurrTile, DestTile, need.RestoreNeedFurn.ObjectType, 0, false, true);
        }
        else
        {
            pathAStar = new Path_AStar(World.Current, CurrTile, DestTile);
        }

        Profiler.EndSample();

        if (pathAStar != null && pathAStar.Length() == 0)
        {
            Debug.ULogChannel("Character", "Path_AStar returned no path to target job tile!");
            AbandonJob(false);
            return;
        }

        if (MyJob.adjacent)
        {
            IEnumerable<Tile> reversed = pathAStar.Reverse();
            reversed = reversed.Skip(1);
            pathAStar = new Path_AStar(new Queue<Tile>(reversed.Reverse()));
            DestTile = pathAStar.EndTile();
            jobTile = DestTile;
        }
        else
        {
            jobTile = MyJob.tile;
        }

        MyJob.IsBeingWorked = true;
    }

    private void Update_DoJob(float deltaTime)
    {
        // Check if I already have a job.
        if (MyJob == null)
        {
            GetNewJob();

            // This should only be the case, if there was a pathing issue
            // or insufficent materials after getting the job.
            // In that case, just return early.
            if (MyJob == null)
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
                MyJob.DoWork(deltaTime);
            }
        }

        // Calculate needs.
        foreach (Need n in needs)
        {
            n.Update(deltaTime);
        }
    }

    /// <summary>
    /// Checks whether the current job has all the materials in place and if not instructs the working character to get the materials there first.
    /// Only ever returns true if all materials for the job are at the job location and thus signals to the calling code, that it can proceed with job execution.
    /// </summary>
    /// <returns></returns>
    private bool CheckForJobMaterials()
    {
        List<string> fulfillableInventoryRequirements = new List<string>();

        if (MyJob != null && MyJob.IsNeed && MyJob.Critical == false)
        {
            MyJob.tile = jobTile = new Path_AStar(World.Current, CurrTile, null, MyJob.JobObjectType, 0, false, true).EndTile();
        }

        if (MyJob == null || MyJob.MaterialNeedsMet())
        {
            // We can return early.
            return true;
        }
        else
        {
            fulfillableInventoryRequirements = FulfillableInventoryRequirements(MyJob);

            // If we somehow get here and fulfillableInventoryRequirements is empty then there is a problem!
            if (fulfillableInventoryRequirements == null || fulfillableInventoryRequirements.Count() == 0)
            {
                Debug.ULogChannel("Character", "CheckForJobMaterials: no fulfillable inventory requirements");
                AbandonJob(true);
                return false;
            }
        }

        // At this point we know that the job still needs materials and these needs are satisfiable.
        // First we check if we carry any materials the job wants by chance.
        if (inventory != null)
        {
            if (MyJob.AmountDesiredOfInventoryType(inventory) > 0)
            {
                // If so, deliver the goods.
                // Walk to the job tile, then drop off the stack into the job.
                if (CurrTile == JobTile)
                {
                    // We are at the job's site, so drop the inventory
                    World.Current.inventoryManager.PlaceInventory(MyJob, inventory);
                    MyJob.DoWork(0); // This will call all cbJobWorked callbacks, because even though
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
            if (CurrTile.Inventory != null &&
                MyJob.AmountDesiredOfInventoryType(CurrTile.Inventory) > 0 && !CurrTile.Inventory.locked &&
                (MyJob.canTakeFromStockpile || CurrTile.Furniture == null || CurrTile.Furniture.IsStockpile() == false))
            {
                // Pick up the stuff!
                World.Current.inventoryManager.PlaceInventory(
                    this,
                    CurrTile.Inventory,
                    MyJob.AmountDesiredOfInventoryType(CurrTile.Inventory));
            }
            else
            {
                // Walk towards a tile containing the required goods.
                if (CurrTile != nextTile)
                {
                    // We are still moving somewhere, so just bail out.
                    return false;
                }

                // Any chance we already have a path that leads to the items we want?
                // Check that we have an end tile and that it has content.
                // Check if contains the desired objectType�.
                if (WalkingToUsableInventory() && fulfillableInventoryRequirements.Contains(pathAStar.EndTile().Inventory.objectType))
                {
                    // We are already moving towards a tile that contains what we want!
                    // so....do nothing?
                    return false;
                }
                else
                {
                    Inventory desired = null;
                    Path_AStar newPath = null;
                    foreach (string itemType in fulfillableInventoryRequirements)
                    {
                        desired = MyJob.inventoryRequirements[itemType];
                        newPath = World.Current.inventoryManager.GetPathToClosestInventoryOfType(
                                             desired.objectType,
                                             CurrTile,
                                             desired.maxStackSize - desired.StackSize,
                                             MyJob.canTakeFromStockpile);

                        if (newPath == null || newPath.Length() < 1)
                        {
                            // Try the next requirement
                            Debug.ULogChannel("Character", "No tile contains objects of type '" + desired.objectType + "' to satisfy job requirements.");
                            continue;
                        }

                        // else, there is a valid path to an item that will satisfy the job
                        break;
                    }

                    if (newPath == null || newPath.Length() < 1)
                    {
                        // tried all requirements and found no path
                        Debug.ULogChannel("Character", "No reachable tile contains objects able to satisfy job requirements.");
                        AbandonJob(true);
                        return false;
                    }

                    Debug.ULogChannel("Character", "pathAStar returned with length of: " + newPath.Length());

                    DestTile = newPath.EndTile();

                    // Since we already have a path calculated, let's just save that.
                    pathAStar = newPath;

                    // Ignore first tile, because that's what we're already in.
                    nextTile = newPath.Dequeue();
                }

                // One way or the other, we are now on route to an object of the right type.
                return false;
            }
        }

        return false; // We can't continue until all materials are satisfied.
    }

    /// <summary>
    /// Fulfillable inventory requirements for job.
    /// </summary>
    /// <returns>A list of (string) objectTypes for job inventory requirements that can be met. Returns null if the job requires materials which do not exist on the map.</returns>
    private List<string> FulfillableInventoryRequirements(Job job)
    {
        List<string> fulfillableInventoryRequirements = new List<string>();

        foreach (Inventory inv in job.GetInventoryRequirementValues())
        {
            if (job.acceptsAny == false)
            {
                if (World.Current.inventoryManager.QuickCheck(inv.objectType) == false)
                {
                    // the job requires ALL inventory requirements to be met, and there is no source of a desired objectType
                    ///AbandonJob(true);
                    return null;
                }
                else
                {
                    fulfillableInventoryRequirements.Add(inv.objectType);
                }
            }
            else if (World.Current.inventoryManager.QuickCheck(inv.objectType))
            {
                // there is a source for a desired objectType that the job will accept
                fulfillableInventoryRequirements.Add(inv.objectType);
            }
        }

        return fulfillableInventoryRequirements;
    }

    private bool WalkingToUsableInventory()
    {
        bool destHasInventory = pathAStar != null && pathAStar.EndTile() != null && pathAStar.EndTile().Inventory != null;
        return destHasInventory &&
                !(pathAStar.EndTile().Furniture != null && (MyJob.canTakeFromStockpile == false && pathAStar.EndTile().Furniture.IsStockpile() == true));
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
        ////    Debug.ULogErrorChannel("Character", "Character tried to dump inventory into an invalid tile (maybe there's already something here). FIXME: Setting inventory to null and leaking for now");
        ////    // FIXME: For the sake of continuing on, we are still going to dump any
        ////    // reference to the current inventory, but this means we are "leaking"
        ////    // inventory.  This is permanently lost now.
        ////}

        inventory = null;
    }

    private void Update_DoMovement(float deltaTime)
    {
        if (CurrTile == DestTile)
        {
            // We're already were we want to be.
            pathAStar = null;
            IsWalking = false;
            VisualPath.Instance.RemoveVisualPoints(name);
            return;
        }

        if (nextTile == null || nextTile == CurrTile)
        {
            // Get the next tile from the pathfinder.
            if (pathAStar == null || pathAStar.Length() == 0)
            {
                // Generate a path to our destination.
                // This will calculate a path from curr to dest.
                pathAStar = new Path_AStar(World.Current, CurrTile, DestTile);
                if (pathAStar.Length() == 0)
                {
                    Debug.ULogErrorChannel("Character", "Path_AStar returned no path to destination!");
                    AbandonJob(false);
                    return;
                }

                // Let's ignore the first tile, because that's the tile we're currently in.
                nextTile = pathAStar.Dequeue();
            }

            if (IsSelected)
            {
                VisualPath.Instance.SetVisualPoints(name, pathAStar.GetList());
            }

            IsWalking = true;

            // Grab the next waypoint from the pathing system!
            nextTile = pathAStar.Dequeue();

            if (nextTile == CurrTile)
            {
                IsWalking = false;
            }
        }

        // Find character facing
        if (nextTile.X > CurrTile.X)
        {
            CharFacing = Facing.EAST;
        }
        else if (nextTile.X < CurrTile.X)
        {
            CharFacing = Facing.WEST;
        }
        else if (nextTile.Y > CurrTile.Y)
        {
            CharFacing = Facing.NORTH;
        }
        else
        {
            CharFacing = Facing.SOUTH;
        }

        // At this point we should have a valid nextTile to move to.
        // What's the total distance from point A to point B?
        // We are going to use Euclidean distance FOR NOW...
        // But when we do the pathfinding system, we'll likely
        // switch to something like Manhattan or Chebyshev distance
        float distToTravel = Mathf.Sqrt(
                                 Mathf.Pow(CurrTile.X - nextTile.X, 2) +
                                 Mathf.Pow(CurrTile.Y - nextTile.Y, 2));

        if (nextTile.IsEnterable() == Enterability.Never)
        {
            //// Most likely a wall got built, so we just need to reset our pathfinding information.
            //// FIXME: Ideally, when a wall gets spawned, we should invalidate our path immediately,
            ////            so that we don't waste a bunch of time walking towards a dead end.
            ////            To save CPU, maybe we can only check every so often?
            ////            Or maybe we should register a callback to the OnTileChanged event?
            //// Debug.ULogErrorChannel("FIXME", "A character was trying to enter an unwalkable tile.");
            nextTile = null;    // our next tile is a no-go
            pathAStar = null;   // clearly our pathfinding info is out of date.
            return;
        }
        else if (nextTile.IsEnterable() == Enterability.Soon)
        {
            // We can't enter the NOW, but we should be able to in the
            // future. This is likely a DOOR.
            // So we DON'T bail on our movement/path, but we do return
            // now and don't actually process the movement.
            return;
        }

        // How much distance can be travel this Update?
        float distThisFrame = speed / nextTile.MovementCost * deltaTime;

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

            CurrTile = nextTile;
            movementPercentage = 0;

            // FIXME?  Do we actually want to retain any overshot movement?
        }
    }

    private void OnJobStopped(Job j)
    {
        // Job completed (if non-repeating) or was cancelled.
        j.OnJobStopped -= OnJobStopped;

        if (j != MyJob)
        {
            Debug.ULogErrorChannel("Character", "Character being told about job that isn't his. You forgot to unregister something.");
            return;
        }

        MyJob = null;
    }

    private void OnInventoryCreated(Inventory inv)
    {
        // First remove the callback.
        World.Current.OnInventoryCreated -= OnInventoryCreated;

        // Get the relevant job and dequeue it from the waiting queue.
        Job job = World.Current.jobWaitingQueue.Dequeue();

        // Check if the initial job still exists.
        // It could have been deleted through the user
        // cancelling the job manually.
        if (job != null)
        {
            List<string> desired = FulfillableInventoryRequirements(job);

            // Check if the created inventory can fulfill the waiting job requirements.
            if (desired != null && desired.Contains(inv.objectType))
            {
                // If so, enqueue the job onto the (normal)
                // job queue.
                World.Current.jobQueue.Enqueue(job);
            }
            else
            {
                // If not, (re)enqueue the job onto the waiting queu
                // and also register a callback for the future.
                World.Current.jobWaitingQueue.Enqueue(job);
                World.Current.OnInventoryCreated += OnInventoryCreated;
            }
        }
    }
}
