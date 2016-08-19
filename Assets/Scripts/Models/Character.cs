//=======================================================================
// Copyright Martin "quill18" Glaude 2015-2016.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

/// <summary>
/// A Character is an entity on the map that can move between tiles and,
/// for now, grabs jobs from the work queue and performs this.
/// Later, the Character class will likely be refactored (possibly with
/// sub-classes or interfaces) to support friendly workers, enemies, etc...
/// </summary>
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
            if (_nextTile == null)
                return CurrTile.X;

            return Mathf.Lerp(CurrTile.X, _nextTile.X, _movementPercentage);
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
            if (_nextTile == null)
                return CurrTile.Y;

            return Mathf.Lerp(CurrTile.Y, _nextTile.Y, _movementPercentage);
        }
    }

    /// <summary>
    /// The tile the Character is considered to still be standing in.
    /// </summary>
    public Tile CurrTile
    {
        get { return _currTile; }

        protected set
        {
            if (_currTile != null)
            {
                _currTile.Characters.Remove(this);
            }

            _currTile = value;
            _currTile.Characters.Add(this);
        }
    }

    private Tile _currTile;



    /// <summary>
    /// The Character's current goal tile (not necessarily the next one
    /// he or she will be entering). If we aren't moving, then destTile = currTile
    /// </summary>
    private Tile DestTile
    {
        get { return _destTile; }
        set
        {
            if (_destTile != value)
            {
                _destTile = value;
                _pathAStar = null;	// If this is a new destination, then we need to invalidate pathfinding.
            }
        }
    }

    private Tile _destTile;

    /// The next tile in the pathfinding sequence (the one we are about to enter).
    private Tile _nextTile;

    /// Goes from 0 to 1 as we move from CurrTile to NextTile
    private float _movementPercentage;

    /// Holds the path to reach DestTile.
    private Path_AStar _pathAStar;

    /// Tiles per second
    private float _speed = 5f;

    /// A callback to trigger when character information changes (notably, the position)
    public event Action<Character> CharacterChanged;

    /// Our job, if any.
    private Job _myJob;

    // The item we are carrying (not gear/equipment)
    public Inventory Inventory { get; set; }

    /// Use only for serialization
    public Character()
    {

    }

    public Character(Tile tile)
    {
        CurrTile = DestTile = _nextTile = tile;
    }


    private void GetNewJob()
    {
        // Get the first job on the queue.
        _myJob = World.Current.JobQueue.Dequeue();

        if (_myJob == null)
        {
            _myJob = new Job(CurrTile,
                "Waiting",
                null,
                UnityEngine.Random.Range(0.1f, 0.5f),
                null,
                false);
        }

        // Get our destination from the job
        DestTile = _myJob.Tile;

        _myJob.JobStopped += OnJobStopped;

        // Immediately check to see if the job tile is reachable.
        // NOTE: We might not be pathing to it right away (due to 
        // requiring materials), but we still need to verify that the
        // final location can be reached.
        Profiler.BeginSample("PathGeneration");
        _pathAStar = new Path_AStar(World.Current, CurrTile, DestTile);	// This will calculate a path from curr to dest.
        Profiler.EndSample();
        if (_pathAStar.Length() == 0)
        {
            Debug.LogError("Path_AStar returned no path to target job tile!");
            AbandonJob();
            DestTile = CurrTile;
        }
    }

    private void Update_DoJob(float deltaTime)
    {
        // Do I have a job?
        if (_myJob == null)
        {
            GetNewJob();
        }

        if (CheckForJobMaterials()) //make sure all materials are in place
        {
            // If we get here, then the job has all the material that it needs.
            // Lets make sure that our destination tile is the job site tile.
            DestTile = _myJob.Tile;

            // Are we there yet?
            if (CurrTile == _myJob.Tile)
            {
                // We are at the correct tile for our job, so 
                // execute the job's "DoWork", which is mostly
                // going to countdown jobTime and potentially
                // call its "Job Complete" callback.
                _myJob.DoWork(deltaTime);
            }
        }
    }

    /// <summary>
    /// Checks weather the current job has all the materials in place and if not instructs the working character to get the materials there first.
    /// Only ever returns true if all materials for the job are at the job location and thus signals to the calling code, that it can proceed with job execution.
    /// </summary>
    /// <returns></returns>
    private bool CheckForJobMaterials()
    {  
        if (_myJob.HasAllMaterial())
            return true; //we can return early

        // At this point we know, that the job still needs materials.
        // First we check if we carry any materials the job wants by chance.
        if (Inventory != null)
        {
            if (_myJob.DesiresInventoryType(Inventory) > 0)
            {
                // If so, deliver the goods.
                // Walk to the job tile, then drop off the stack into the job.
                if (CurrTile == _myJob.Tile)
                {
                    // We are at the job's site, so drop the inventory
                    World.Current.InventoryManager.PlaceInventory(_myJob, Inventory);
                    _myJob.DoWork(0); // This will call all cbJobWorked callbacks, because even though
                                     // we aren't progressing, it might want to do something with the fact
                                     // that the requirements are being met.

                    //at this point we should dump anything in our inventory
                    DumpExcessInventory();
                  
                }
                else
                {
                    // We still need to walk to the job site.
                    DestTile = _myJob.Tile;
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
            Debug.Log("Standing on Tile check");
            if (CurrTile.Inventory != null &&
                _myJob.DesiresInventoryType(CurrTile.Inventory) > 0 &&
                (_myJob.canTakeFromStockpile || CurrTile.Furniture == null || CurrTile.Furniture.IsStockpile() == false))
            {
                // Pick up the stuff!
                Debug.Log("Pick up the stuff");

                World.Current.InventoryManager.PlaceInventory(
                    this,
                    CurrTile.Inventory,
                    _myJob.DesiresInventoryType(CurrTile.Inventory)
                );

            }
            else
            {
                // Walk towards a tile containing the required goods.
                Debug.Log("Walk to the stuff");
                Debug.Log(_myJob.canTakeFromStockpile);


                // Find the first thing in the Job that isn't satisfied.
                Inventory desired = _myJob.GetFirstDesiredInventory();

                if (CurrTile != _nextTile)
                {
                    // We are still moving somewhere, so just bail out.
                    return false;
                }

                // Any chance we already have a path that leads to the items we want?
                if (_pathAStar != null && _pathAStar.EndTile() != null && _pathAStar.EndTile().Inventory != null && (_pathAStar.EndTile().Furniture != null && !(_myJob.canTakeFromStockpile == false && _pathAStar.EndTile().Furniture.IsStockpile() == true)) && _pathAStar.EndTile().Inventory.objectType == desired.objectType)
                {
                    // We are already moving towards a tile that contains what we want!
                    // so....do nothing?
                }
                else
                {
                    Path_AStar newPath = World.Current.InventoryManager.GetPathToClosestInventoryOfType(
                                             desired.objectType,
                                             CurrTile,
                                             desired.maxStackSize - desired.StackSize,
                                             _myJob.canTakeFromStockpile
                                         );

                    if (newPath == null || newPath.Length() < 1)
                    {
                        //Debug.Log("pathAStar is null and we have no path to object of type: " + desired.objectType);
                        // Cancel the job, since we have no way to get any raw materials!
                        Debug.Log("No tile contains objects of type '" + desired.objectType + "' to satisfy job requirements.");
                        AbandonJob();
                        return false;
                    }

                    Debug.Log("pathAStar returned with length of: " + newPath.Length());                    

                    DestTile = newPath.EndTile();

                    // Since we already have a path calculated, let's just save that.
                    _pathAStar = newPath;

                    // Ignore first tile, because that's what we're already in.
                    _nextTile = newPath.Dequeue();
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

        //if (World.current.inventoryManager.PlaceInventory(CurrTile, inventory) == false)
        //{
        //    Debug.LogError("Character tried to dump inventory into an invalid tile (maybe there's already something here). FIXME: Setting inventory to null and leaking for now");
        //    // FIXME: For the sake of continuing on, we are still going to dump any
        //    // reference to the current inventory, but this means we are "leaking"
        //    // inventory.  This is permanently lost now.
        //}

        Inventory = null;
    }

    public void AbandonJob()
    {
        _nextTile = DestTile = CurrTile;
        World.Current.JobQueue.Enqueue(_myJob);
        _myJob.JobStopped -= OnJobStopped;
        _myJob = null;
    }

    private void Update_DoMovement(float deltaTime)
    {
        if (CurrTile == DestTile)
        {
            _pathAStar = null;
            return;	// We're already were we want to be.
        }

        // currTile = The tile I am currently in (and may be in the process of leaving)
        // nextTile = The tile I am currently entering
        // destTile = Our final destination -- we never walk here directly, but instead use it for the pathfinding

        if (_nextTile == null || _nextTile == CurrTile)
        {
            // Get the next tile from the pathfinder.
            if (_pathAStar == null || _pathAStar.Length() == 0)
            {
                // Generate a path to our destination
                _pathAStar = new Path_AStar(World.Current, CurrTile, DestTile);	// This will calculate a path from curr to dest.
                if (_pathAStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar returned no path to destination!");
                    AbandonJob();
                    return;
                }

                // Let's ignore the first tile, because that's the tile we're currently in.
                _nextTile = _pathAStar.Dequeue();

            }


            // Grab the next waypoint from the pathing system!
            _nextTile = _pathAStar.Dequeue();

            if (_nextTile == CurrTile)
            {
                //Debug.LogError("Update_DoMovement - nextTile is currTile?");
            }
        }

        // At this point we should have a valid nextTile to move to.

        // What's the total distance from point A to point B?
        // We are going to use Euclidean distance FOR NOW...
        // But when we do the pathfinding system, we'll likely
        // switch to something like Manhattan or Chebyshev distance
        float distToTravel = Mathf.Sqrt(
                                 Mathf.Pow(CurrTile.X - _nextTile.X, 2) +
                                 Mathf.Pow(CurrTile.Y - _nextTile.Y, 2)
                             );

        if (_nextTile.IsEnterable() == Enterability.Never)
        {
            // Most likely a wall got built, so we just need to reset our pathfinding information.
            // FIXME: Ideally, when a wall gets spawned, we should invalidate our path immediately,
            //		  so that we don't waste a bunch of time walking towards a dead end.
            //		  To save CPU, maybe we can only check every so often?
            //		  Or maybe we should register a callback to the OnTileChanged event?
            // Debug.LogError("FIXME: A character was trying to enter an unwalkable tile.");
            _nextTile = null;	// our next tile is a no-go
            _pathAStar = null;	// clearly our pathfinding info is out of date.
            return;
        }
        else if (_nextTile.IsEnterable() == Enterability.Soon)
        {
            // We can't enter the NOW, but we should be able to in the
            // future. This is likely a DOOR.
            // So we DON'T bail on our movement/path, but we do return
            // now and don't actually process the movement.
            return;
        }

        // How much distance can be travel this Update?
        float distThisFrame = _speed / _nextTile.movementCost * deltaTime;

        // How much is that in terms of percentage to our destination?
        float percThisFrame = distThisFrame / distToTravel;

        // Add that to overall percentage travelled.
        _movementPercentage += percThisFrame;

        if (_movementPercentage >= 1)
        {
            // We have reached our destination

            // TODO: Get the next tile from the pathfinding system.
            //       If there are no more tiles, then we have TRULY
            //       reached our destination.

            CurrTile = _nextTile;
            _movementPercentage = 0;
            // FIXME?  Do we actually want to retain any overshot movement?
        }


    }

    /// Runs every "frame" while the simulation is not paused
    public void Update(float deltaTime)
    {
        Update_DoJob(deltaTime);

        Update_DoMovement(deltaTime);

        if (CharacterChanged != null)
            CharacterChanged(this);

    }

    private void OnJobStopped(Job j)
    {
        // Job completed (if non-repeating) or was cancelled.

        j.JobStopped -= OnJobStopped;

        if (j != _myJob)
        {
            Debug.LogError("Character being told about job that isn't his. You forgot to unregister something.");
            return;
        }

        _myJob = null;
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
    }

    public void ReadXml(XmlReader reader)
    {
    }

    #endregion

    #region ISelectableInterface implementation

    public string GetName()
    {
        return "Sally S. Smith";
    }

    public string GetDescription()
    {
        return "A human astronaut. She is currently depressed because her friend was ejected out of an airlock.";
    }

    public string GetHitPointString()
    {
        return "100/100";
    }

    #endregion
}
