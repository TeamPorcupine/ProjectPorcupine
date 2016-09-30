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
using MoonSharp.Interpreter;
using ProjectPorcupine.Rooms;
using UnityEngine;

public enum BuildMode
{
    FLOOR,
    ROOMBEHAVIOR,
    FURNITURE,
    UTILITY,
    DECONSTRUCT
}

public class BuildModeController
{
    public BuildMode buildMode = BuildMode.FLOOR;
    public string buildModeType;

    private MouseController mouseController;
    private TileType buildModeTile = TileType.Floor;

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
        if (buildMode == BuildMode.FLOOR || buildMode == BuildMode.DECONSTRUCT || buildMode == BuildMode.UTILITY)
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

    public void SetMode_BuildFurniture(string type)
    {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        buildMode = BuildMode.FURNITURE;
        buildModeType = type;
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
                DoesBuildJobOverlapExistingBuildJob(tile, furnitureType, CurrentPreviewRotation) == false)
            {
                // This tile position is valid for this furniture

                // Check if there is existing furniture in this tile. If so delete it.
                if (tile.Furniture != null)
                {
                    tile.Furniture.SetDeconstructJob();
                }

                // Create a job for it to be build
                Job job;

                if (PrototypeManager.FurnitureConstructJob.Has(furnitureType))
                {
                    // Make a clone of the job prototype
                    job = PrototypeManager.FurnitureConstructJob.Get(furnitureType).Clone();

                    // Assign the correct tile.
                    job.tile = tile;
                }
                else
                {
                    Debug.ULogErrorChannel("BuildModeController", "There is no furniture job prototype for '" + furnitureType + "'");
                    job = new Job(tile, furnitureType, World.Current.FurnitureManager.ConstructJobCompleted, 0.1f, null, Job.JobPriority.High);
                    job.adjacent = true;
                    job.JobDescription = "job_build_" + furnitureType + "_desc";
                }

                Furniture furnituteToBuild = PrototypeManager.Furniture.Get(furnitureType).Clone();
                furnituteToBuild.SetRotation(CurrentPreviewRotation);
                job.buildablePrototype = furnituteToBuild;

                // Add the job to the queue or build immediately if in Dev mode
                if (Settings.GetSetting("DialogBoxSettings_developerModeToggle", false))
                {
                    World.Current.FurnitureManager.PlaceFurniture(furnituteToBuild, job.tile);
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
                            offsetTile.PendingBuildJob = job;
                            job.OnJobStopped += (theJob) => offsetTile.PendingBuildJob = null;
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

            // TODO: Reimplement this later: DoesBuildJobOverlapExistingBuildJob(t, furnitureType) == false)
            if ( 
                World.Current.UtilityManager.IsPlacementValid(utilityType, tile)  &&
                DoesSameUtilityTypeAlreadyExist(tile, utilityType) == false)
            {
                // This tile position is valid for this furniture

                // Create a job for it to be build
                Job job;

                if (PrototypeManager.UtilityConstructJob.Has(utilityType))
                {
                    // Make a clone of the job prototype
                    job = PrototypeManager.UtilityConstructJob.Get(utilityType).Clone();

                    // Assign the correct tile.
                    job.tile = tile;
                }
                else
                {
                    Debug.ULogErrorChannel("BuildModeController", "There is no furniture job prototype for '" + utilityType + "'");
                    job = new Job(tile, utilityType, World.Current.UtilityManager.ConstructJobCompleted, 0.1f, null, Job.JobPriority.High);
                    job.JobDescription = "job_build_" + utilityType + "_desc";
                }

                job.buildablePrototype = PrototypeManager.Utility.Get(utilityType);

                // Add the job to the queue or build immediately if in dev mode
                if (Settings.GetSetting("DialogBoxSettings_developerModeToggle", false))
                {
                    World.Current.UtilityManager.PlaceUtility(job.JobObjectType, job.tile);
                }
                else
                {
                    // FIXME: I don't like having to manually and explicitly set
                    // flags that preven conflicts. It's too easy to forget to set/clear them!
                    Tile offsetTile = World.Current.GetTileAt(tile.X, tile.Y, tile.Z);
                    offsetTile.PendingBuildJob = job;
                    job.OnJobStopped += (theJob) => offsetTile.PendingBuildJob = null;

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
                tile.PendingBuildJob == null &&
                tileType.CanBuildHere(tile))
            {
                // This tile position is valid tile type

                // Create a job for it to be build
                Job buildingJob = tileType.BuildingJob;

                buildingJob.tile = tile;

                // Add the job to the queue or build immediately if in Dev mode
                if (Settings.GetSetting("DialogBoxSettings_developerModeToggle", false))
                {
                    buildingJob.tile.Type = buildingJob.JobTileType;
                }
                else
                {
                    // FIXME: I don't like having to manually and explicitly set
                    // flags that prevent conflicts. It's too easy to forget to set/clear them!
                    tile.PendingBuildJob = buildingJob;
                    buildingJob.OnJobStopped += (theJob) => theJob.tile.PendingBuildJob = null;

                    WorldController.Instance.World.jobQueue.Enqueue(buildingJob);
                }
            }
        }
        else if (buildMode == BuildMode.DECONSTRUCT)
        {
            // TODO
            bool canDeconstructAll = Settings.GetSetting("DialogBoxSettings_developerModeToggle", false);
            if (tile.Furniture != null && (canDeconstructAll || tile.Furniture.HasTypeTag("Non-deconstructible") == false))
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
                            if (neighbor.Room.IsOutsideRoom() || MathUtilities.IsZero(neighbor.Room.GetTotalGasPressure()))
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
                        Debug.ULogChannel("BuildModeController", "Someone tried to deconstruct a wall between a pressurized room and vacuum!");
                        return;
                    }
                }

                tile.Furniture.SetDeconstructJob();
            }
            else if (tile.PendingBuildJob != null)
            {
                tile.PendingBuildJob.CancelJob();
            }
            else if (tile.Utilities.Count > 0)
            {
                tile.Utilities.Last().Value.SetDeconstructJob();
            }
        }
        else
        {
            Debug.ULogErrorChannel("BuildModeController", "UNIMPLEMENTED BUILD MODE");
        }
    }

    public bool DoesBuildJobOverlapExistingBuildJob(Tile t, string furnitureType, float rotation = 0)
    {
        Furniture furnitureToBuild = PrototypeManager.Furniture.Get(furnitureType).Clone();
        furnitureToBuild.SetRotation(rotation);

        for (int x_off = t.X; x_off < (t.X + furnitureToBuild.Width); x_off++)
        {
            for (int y_off = t.Y; y_off < (t.Y + furnitureToBuild.Height); y_off++)
            {
                Job pendingBuildJob = WorldController.Instance.World.GetTileAt(x_off, y_off, t.Z).PendingBuildJob;
                if (pendingBuildJob != null)
                {
                    // if the existing buildJobs furniture is replaceable by the current furnitureType,
                    // we can pretend it does not overlap with the new build
                    return !furnitureToBuild.ReplaceableFurniture.Any(pendingBuildJob.buildablePrototype.HasTypeTag);
                }
            }
        }

        return false;
    }

    public bool DoesSameUtilityTypeAlreadyExist(Tile tile, string furnitureType)
    {
        Utility proto = PrototypeManager.Utility.Get(furnitureType);
        return tile.Utilities.ContainsKey(proto.Name);
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
