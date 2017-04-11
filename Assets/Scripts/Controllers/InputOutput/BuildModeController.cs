#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using ProjectPorcupine.OrderActions;
using ProjectPorcupine.Rooms;

public enum BuildMode
{
    FLOOR,
    ROOMBEHAVIOR,
    FURNITURE,
    UTILITY,
    DECONSTRUCT,
    MINE
}

public class BuildModeController
{
    public BuildMode buildMode = BuildMode.FLOOR;
    public string buildModeType;

    private MouseController mouseController;
    private TileType buildModeTile = TileType.Floor;

    private bool useCratedObject;

    public BuildModeController()
    {
        Instance = this;
        CurrentPreviewRotation = 0f;
        KeyboardManager.Instance.RegisterInputAction("RotateFurnitureLeft", KeyboardMappedInputType.KeyUp, RotateFurnitireLeft);
        KeyboardManager.Instance.RegisterInputAction("RotateFurnitureRight", KeyboardMappedInputType.KeyUp, RotateFurnitireRight);
    }

    public static BuildModeController Instance { get; protected set; }

    // The rotation applied to the object.
    public float CurrentPreviewRotation { get; private set; }

    // Use this for initialization
    public void SetMouseController(MouseController currentMouseController)
    {
        mouseController = currentMouseController;
    }

    public bool IsObjectDraggable()
    {
        if (buildMode == BuildMode.FLOOR || buildMode == BuildMode.DECONSTRUCT || buildMode == BuildMode.UTILITY || buildMode == BuildMode.MINE)
        {
            // floors are draggable
            return true;
        }

        if (buildMode == BuildMode.ROOMBEHAVIOR)
        {
            // Room Behaviors are not draggable
            return false;
        }

        Furniture proto = PrototypeManager.Furniture.Get(buildModeType);

        return proto.DragType != "single";
    }

    public string GetFloorTile()
    {
        return buildModeTile.ToString();
    }

    public void SetModeBuildTile(TileType type)
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = type;

        mouseController.StartBuildMode();
    }

    public void SetMode_DesignateRoomBehavior(string type)
    {
        buildMode = BuildMode.ROOMBEHAVIOR;
        buildModeType = type;
        mouseController.StartBuildMode();
    }

    public void SetMode_BuildFurniture(string type, bool useCratedObject = false)
    {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        buildMode = BuildMode.FURNITURE;
        buildModeType = type;
        this.useCratedObject = useCratedObject;
        CurrentPreviewRotation = 0f;
        mouseController.StartBuildMode();
    }

    public void SetMode_BuildUtility(string type)
    {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        buildMode = BuildMode.UTILITY;
        buildModeType = type;
        mouseController.StartBuildMode();
    }

    public void SetMode_Deconstruct()
    {
        buildMode = BuildMode.DECONSTRUCT;
        mouseController.StartBuildMode();
    }

    public void SetMode_Mine()
    {
        buildMode = BuildMode.MINE;
        mouseController.StartBuildMode();
    }

    public void DoBuild(Tile tile)
    {
        if (buildMode == BuildMode.ROOMBEHAVIOR)
        {
            string roomBehaviorType = buildModeType;

            if (tile.Room != null && WorldController.Instance.World.IsRoomBehaviorValidForRoom(roomBehaviorType, tile.Room))
            {
                RoomBehavior proto = PrototypeManager.RoomBehavior.Get(roomBehaviorType);
                tile.Room.DesignateRoomBehavior(proto.Clone());
            }
        }
        else if (buildMode == BuildMode.FURNITURE)
        {
            // Create the Furniture and assign it to the tile
            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function!
            string furnitureType = buildModeType;

            if (
                World.Current.FurnitureManager.IsPlacementValid(furnitureType, tile, CurrentPreviewRotation) &&
                World.Current.FurnitureManager.IsWorkSpotClear(furnitureType, tile) &&
                DoesFurnitureBuildJobOverlapExistingBuildJob(tile, furnitureType, CurrentPreviewRotation) == false)
            {
                // This tile position is valid for this furniture

                // Check if there is existing furniture in this tile. If so delete it.
                if (tile.Furniture != null)
                {
                    tile.Furniture.SetDeconstructJob();
                }

                // Create a job for it to be build
                Job job;

                Furniture toBuildProto = PrototypeManager.Furniture.Get(furnitureType);
                OrderAction orderAction = toBuildProto.GetOrderAction<Build>();
                if (orderAction != null)
                {
                    job = orderAction.CreateJob(tile, furnitureType);
                    if (useCratedObject)
                    {
                        // We want to use a crated furniture, so set requested items to crated version.
                        job.RequestedItems.Clear();
                        job.RequestedItems.Add(this.buildModeType, new ProjectPorcupine.Jobs.RequestedItem(this.buildModeType, 1));
                        useCratedObject = false;
                    }

                    // this is here so OrderAction can be used for utility as well as furniture
                    job.OnJobCompleted += (theJob) => World.Current.FurnitureManager.ConstructJobCompleted(theJob);
                }
                else
                {
                    UnityDebugger.Debugger.LogError("BuildModeController", "There is no furniture job prototype for '" + furnitureType + "'");
                    job = new Job(tile, furnitureType, World.Current.FurnitureManager.ConstructJobCompleted, 0.1f, null, Job.JobPriority.High);
                    job.adjacent = true;
                    job.Description = "job_build_" + furnitureType + "_desc";
                }

                Furniture furnitureToBuild = PrototypeManager.Furniture.Get(furnitureType).Clone();
                furnitureToBuild.SetRotation(CurrentPreviewRotation);
                job.buildablePrototype = furnitureToBuild;

                // Add the job to the queue or build immediately if in Dev mode
                if (SettingsKeyHolder.DeveloperMode)
                {
                    World.Current.FurnitureManager.PlaceFurniture(furnitureToBuild, job.tile);
                }
                else
                {
                    for (int x_off = tile.X; x_off < (tile.X + job.buildablePrototype.Width); x_off++)
                    {
                        for (int y_off = tile.Y; y_off < (tile.Y + job.buildablePrototype.Height); y_off++)
                        {
                            // FIXME: I don't like having to manually and explicitly set
                            // flags that prevent conflicts. It's too easy to forget to set/clear them!
                            Tile offsetTile = World.Current.GetTileAt(x_off, y_off, tile.Z);
                            HashSet<Job> pendingBuildJobs = WorldController.Instance.World.GetTileAt(x_off, y_off, tile.Z).PendingBuildJobs;
                            if (pendingBuildJobs != null)
                            {
                                // if the existing buildJobs furniture is replaceable by the current furnitureType,
                                // we can pretend it does not overlap with the new build

                                // We should only have 1 furniture building job per tile, so this should return that job and only that job
                                IEnumerable<Job> pendingFurnitureJob = pendingBuildJobs.Where(pendingJob => pendingJob.buildablePrototype.GetType() == typeof(Furniture));
                                if (pendingFurnitureJob.Count() == 1)
                                {
                                    pendingFurnitureJob.Single().CancelJob();
                                }
                            }

                            offsetTile.PendingBuildJobs.Add(job);
                            job.OnJobStopped += (theJob) => offsetTile.PendingBuildJobs.Remove(job);
                        }
                    }

                    World.Current.jobQueue.Enqueue(job);

                    // Let our workspot tile know it is reserved for us
                    World.Current.ReserveTileAsWorkSpot((Furniture)job.buildablePrototype, job.tile);
                }
            }
        }
        else if (buildMode == BuildMode.UTILITY)
        {
            // Create the Furniture and assign it to the tile
            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function!
            string utilityType = buildModeType;
            if (
                World.Current.UtilityManager.IsPlacementValid(utilityType, tile) &&
                DoesSameUtilityTypeAlreadyExist(utilityType, tile) == false &&
                DoesUtilityBuildJobOverlapExistingBuildJob(utilityType, tile) == false)
            {
                // This tile position is valid for this furniture

                // Create a job for it to be build
                Job job;

                Utility toBuildProto = PrototypeManager.Utility.Get(utilityType);
                OrderAction orderAction = toBuildProto.GetOrderAction<Build>();
                if (orderAction != null)
                {
                    job = orderAction.CreateJob(tile, utilityType);

                    // this is here so OrderAction can be used for utility as well as furniture
                    job.OnJobCompleted += (theJob) => World.Current.UtilityManager.ConstructJobCompleted(theJob);
                }
                else
                {
                    UnityDebugger.Debugger.LogError("BuildModeController", "There is no furniture job prototype for '" + utilityType + "'");
                    job = new Job(tile, utilityType, World.Current.UtilityManager.ConstructJobCompleted, 0.1f, null, Job.JobPriority.High);
                    job.Description = "job_build_" + utilityType + "_desc";
                }

                job.buildablePrototype = PrototypeManager.Utility.Get(utilityType);

                // Add the job to the queue or build immediately if in dev mode
                if (SettingsKeyHolder.DeveloperMode)
                {
                    World.Current.UtilityManager.PlaceUtility(job.Type, job.tile, true);
                }
                else
                {
                    // FIXME: I don't like having to manually and explicitly set
                    // flags that preven conflicts. It's too easy to forget to set/clear them!
                    Tile offsetTile = World.Current.GetTileAt(tile.X, tile.Y, tile.Z);
                    offsetTile.PendingBuildJobs.Add(job);
                    job.OnJobStopped += (theJob) => offsetTile.PendingBuildJobs.Remove(job);

                    World.Current.jobQueue.Enqueue(job);
                }
            }
        }
        else if (buildMode == BuildMode.FLOOR)
        {
            // We are in tile-changing mode.
            ////t.Type = buildModeTile;

            TileType tileType = buildModeTile;

            if (
                tile.Type != tileType &&
                tile.Furniture == null &&
                tile.PendingBuildJobs.Count == 0 &&
                tileType.CanBuildHere(tile))
            {
                // This tile position is valid tile type

                // Create a job for it to be build
                Job buildingJob = tileType.BuildingJob;

                buildingJob.tile = tile;

                // Add the job to the queue or build immediately if in Dev mode
                if (SettingsKeyHolder.DeveloperMode)
                {
                    buildingJob.tile.SetTileType(buildingJob.JobTileType);
                }
                else
                {
                    buildingJob.OnJobStopped += (theJob) => theJob.tile.PendingBuildJobs.Remove(theJob);

                    WorldController.Instance.World.jobQueue.Enqueue(buildingJob);
                }
            }
        }
        else if (buildMode == BuildMode.DECONSTRUCT)
        {
            bool canDeconstructAll = SettingsKeyHolder.DeveloperMode;

            if (tile.Furniture != null && (canDeconstructAll || tile.Furniture.HasTypeTag("Non-deconstructible") == false))
            {
                // check if this is a WALL neighbouring a pressured and pressureless environment, and if so, bail
                if (IsTilePartOfPressuredRoom(tile))
                {
                    return;
                }

                tile.Furniture.SetDeconstructJob();
            }
            else if (tile.PendingBuildJobs != null && tile.PendingBuildJobs.Count > 0)
            {
                tile.PendingBuildJobs.Last().CancelJob();
            }
            else if (tile.Utilities.Count > 0)
            {
                tile.Utilities.Last().Value.SetDeconstructJob();
            }
        }
        else if (buildMode == BuildMode.MINE)
        {
            if (tile.Furniture != null)
            {
                Job existingMineJob;
                bool hasMineJob = tile.Furniture.Jobs.HasJobWithPredicate(x => x.OrderName == typeof(Mine).Name, out existingMineJob);
                if (!hasMineJob)
                {
                    OrderAction mineAction = tile.Furniture.GetOrderAction<Mine>();
                    if (mineAction != null)
                    {
                        // check if this is a WALL neighbouring a pressured and pressureless environment, and if so, bail
                        if (IsTilePartOfPressuredRoom(tile))
                        {
                            return;
                        }

                        Job job = mineAction.CreateJob(tile, null);
                        if (SettingsKeyHolder.DeveloperMode)
                        {
                            // complete job right away, needs buildable
                            job.buildable = tile.Furniture;
                            job.DoWork(0);
                        }
                        else
                        {
                            tile.Furniture.Jobs.Add(job);
                        }
                    }
                }
            }
        }
        else
        {
            UnityDebugger.Debugger.LogError("BuildModeController", "UNIMPLEMENTED BUILD MODE");
        }
    }

    public bool DoesFurnitureBuildJobOverlapExistingBuildJob(Tile t, string furnitureType, float rotation = 0)
    {
        Furniture furnitureToBuild = PrototypeManager.Furniture.Get(furnitureType).Clone();
        furnitureToBuild.SetRotation(rotation);

        for (int x_off = t.X; x_off < (t.X + furnitureToBuild.Width); x_off++)
        {
            for (int y_off = t.Y; y_off < (t.Y + furnitureToBuild.Height); y_off++)
            {
                HashSet<Job> pendingBuildJobs = WorldController.Instance.World.GetTileAt(x_off, y_off, t.Z).PendingBuildJobs;
                if (pendingBuildJobs != null)
                {
                    // if the existing buildJobs furniture is replaceable by the current furnitureType,
                    // we can pretend it does not overlap with the new build

                    // We should only have 1 furniture building job per tile, so this should return that job and only that job
                    IEnumerable<Job> pendingFurnitureJob = pendingBuildJobs.Where(job => job.buildablePrototype.GetType() == typeof(Furniture));
                    if (pendingFurnitureJob.Count() == 1)
                    {
                        return !furnitureToBuild.ReplaceableFurniture.Any(pendingFurnitureJob.Single().buildablePrototype.HasTypeTag);
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Does the utility build job overlap an existing utility build job of the same type.
    /// </summary>
    /// <returns><c>true</c>, if utility build job overlaps an existing utility build job of the same type, <c>false</c> otherwise.</returns>
    /// <param name="utilityType">Utility type to check for.</param>
    /// <param name="tile">Tile to check for pending buildjobs.</param>
    public bool DoesUtilityBuildJobOverlapExistingBuildJob(string utilityType, Tile tile)
    {
        HashSet<Job> pendingBuildJobs = tile.PendingBuildJobs;
        if (pendingBuildJobs != null)
        {
            IEnumerable<Job> pendingUtilityJob = pendingBuildJobs.Where(job => job.buildablePrototype.GetType() == typeof(Utility));
            if (pendingUtilityJob.Count() > 0)
            {
                return pendingUtilityJob.Any(job => ((Utility)job.buildablePrototype).Type == utilityType);
            }
        }

        return false;
    }

    public bool DoesSameUtilityTypeAlreadyExist(string type, Tile tile)
    {
        Utility proto = PrototypeManager.Utility.Get(type);
        return tile.Utilities.ContainsKey(proto.Type);
    }

    private bool IsTilePartOfPressuredRoom(Tile tile)
    {
        // check if this is a WALL neighbouring a pressured and pressureless environment, and if so, bail
        if (tile.Furniture.HasTypeTag("Wall"))
        {
            Tile[] neighbors = tile.GetNeighbours(); // diagOkay??
            int pressuredNeighbors = 0;
            int vacuumNeighbors = 0;
            foreach (Tile neighbor in neighbors)
            {
                if (neighbor != null && neighbor.Room != null)
                {
                    if (neighbor.Room.IsOutsideRoom() || MathUtilities.IsZero(neighbor.Room.Atmosphere.GetGasAmount()))
                    {
                        vacuumNeighbors++;
                    }
                    else
                    {
                        pressuredNeighbors++;
                    }
                }
            }

            if (vacuumNeighbors > 0 && pressuredNeighbors > 0)
            {
                UnityDebugger.Debugger.Log("BuildModeController", "Someone tried to deconstruct a wall between a pressurized room and vacuum!");
                return true;
            }
        }

        return false;
    }

    // Rotate the preview furniture to the left.
    private void RotateFurnitireLeft()
    {
        if (buildMode == BuildMode.FURNITURE && PrototypeManager.Furniture.Get(buildModeType).CanRotate)
        {
            CurrentPreviewRotation = (CurrentPreviewRotation + 90) % 360;
        }
    }

    // Rotate the preview furniture to the right.
    private void RotateFurnitireRight()
    {
        if (buildMode == BuildMode.FURNITURE && PrototypeManager.Furniture.Get(buildModeType).CanRotate)
        {
            CurrentPreviewRotation = (CurrentPreviewRotation - 90) % 360;
        }
    }
}
