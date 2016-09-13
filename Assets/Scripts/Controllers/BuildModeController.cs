#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;

public enum BuildMode
{
    FLOOR,
    FURNITURE,
    DECONSTRUCT
}

public class BuildModeController
{
    public BuildMode buildMode = BuildMode.FLOOR;
    public string buildModeType;

    private MouseController mouseController;
    private TileType buildModeTile = TileType.Floor;

    // Use this for initialization
    public void SetMouseController(MouseController currentMouseController)
    {
        mouseController = currentMouseController;
    }

    public bool IsObjectDraggable()
    {
        if (buildMode == BuildMode.FLOOR || buildMode == BuildMode.DECONSTRUCT)
        {
            // floors are draggable
            return true;
        }

        Furniture proto = PrototypeManager.Furniture.Get(buildModeType);

        return proto.Width == 1 && proto.Height == 1;
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
    
    public void SetMode_BuildFurniture(string type)
    {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        buildMode = BuildMode.FURNITURE;
        buildModeType = type;
        mouseController.StartBuildMode();
    }

    public void SetMode_Deconstruct()
    {
        buildMode = BuildMode.DECONSTRUCT;
        mouseController.StartBuildMode();
    }

    public void DoPathfindingTest()
    {
        WorldController.Instance.World.SetupPathfindingExample();
    }

    public void DoBuild(Tile t)
    {
        if (buildMode == BuildMode.FURNITURE)
        {
            // Create the Furniture and assign it to the tile
            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function!
            string furnitureType = buildModeType;

            if ( 
                WorldController.Instance.World.IsFurniturePlacementValid(furnitureType, t) &&
                DoesBuildJobOverlapExistingBuildJob(t, furnitureType) == false)
            {
                // This tile position is valid for this furniture

                // Check if there is existing furniture in this tile. If so delete it.
                // TODO Possibly return resources. Will the Deconstruct() method handle that? If so what will happen if resources drop ontop of new non-passable structure.
                if (t.Furniture != null)
                {
                    t.Furniture.Deconstruct();
                }

                // Create a job for it to be build
                Job j;

                if (PrototypeManager.FurnitureJob.Has(furnitureType))
                {
                    // Make a clone of the job prototype
                    j = PrototypeManager.FurnitureJob.Get(furnitureType).Clone();

                    // Assign the correct tile.
                    j.tile = t;
                }
                else
                {
                    Debug.ULogErrorChannel("BuildModeController", "There is no furniture job prototype for '" + furnitureType + "'");
                    j = new Job(t, furnitureType, FunctionsManager.JobComplete_FurnitureBuilding, 0.1f, null, Job.JobPriority.High);
                    j.JobDescription = "job_build_" + furnitureType + "_desc";
                }

                j.furniturePrototype = PrototypeManager.Furniture.Get(furnitureType);

                // Add the job to the queue or build immediately if in Dev mode
                if (Settings.GetSetting("DialogBoxSettings_developerModeToggle", false))
                {
                    WorldController.Instance.World.PlaceFurniture(j.JobObjectType, j.tile);
                }
                else
                {
                    for (int x_off = t.X; x_off < (t.X + j.furniturePrototype.Width); x_off++)
                    {
                        for (int y_off = t.Y; y_off < (t.Y + j.furniturePrototype.Height); y_off++)
                        {
                            // FIXME: I don't like having to manually and explicitly set
                            // flags that prevent conflicts. It's too easy to forget to set/clear them!
                            Tile offsetTile = WorldController.Instance.World.GetTileAt(x_off, y_off, t.Z);
                            offsetTile.PendingBuildJob = j;
                            j.OnJobStopped += (theJob) =>
                                {
                                    offsetTile.PendingBuildJob = null;
                                };
                        }
                    }

                    WorldController.Instance.World.jobQueue.Enqueue(j);
                }
            }
        }
        else if (buildMode == BuildMode.FLOOR)
        {
            // We are in tile-changing mode.
            ////t.Type = buildModeTile;

            TileType tileType = buildModeTile;

            if ( 
                t.Type != tileType && 
                t.Furniture == null &&
                t.PendingBuildJob == null &&
                tileType.CanBuildHere(t))
            {
                // This tile position is valid tile type

                // Create a job for it to be build
                Job buildingJob = tileType.BuildingJob;
                
                buildingJob.tile = t;

                // Add the job to the queue or build immediately if in Dev mode
                if (Settings.GetSetting("DialogBoxSettings_developerModeToggle", false))
                {
                    buildingJob.tile.Type = buildingJob.JobTileType;
                }
                else
                {
                    // FIXME: I don't like having to manually and explicitly set
                    // flags that prevent conflicts. It's too easy to forget to set/clear them!
                    t.PendingBuildJob = buildingJob;
                    buildingJob.OnJobStopped += (theJob) => theJob.tile.PendingBuildJob = null;

                    WorldController.Instance.World.jobQueue.Enqueue(buildingJob);
                }
            }
        }
        else if (buildMode == BuildMode.DECONSTRUCT)
        {
            // TODO
            bool canDeconstructAll = Settings.GetSetting("DialogBoxSettings_developerModeToggle", false);
            if (t.Furniture != null && (canDeconstructAll || t.Furniture.HasTypeTag("Non-deconstructible") == false))
            {
                // check if this is a WALL neighbouring a pressured and pressureless environment, and if so, bail
                if (t.Furniture.HasTypeTag("Wall"))
                {
                    Tile[] neighbors = t.GetNeighbours(); // diagOkay??
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

                t.Furniture.Deconstruct();
            }
            else if (t.PendingBuildJob != null)
            {
                t.PendingBuildJob.CancelJob();
            }
        }
        else
        {
            Debug.ULogErrorChannel("BuildModeController", "UNIMPLEMENTED BUILD MODE");
        }
    }

    public bool DoesBuildJobOverlapExistingBuildJob(Tile t, string furnitureType)
    {
        Furniture proto = PrototypeManager.Furniture.Get(furnitureType);

        for (int x_off = t.X; x_off < (t.X + proto.Width); x_off++)
        {
            for (int y_off = t.Y; y_off < (t.Y + proto.Height); y_off++)
            {
                Job pendingBuildJob = WorldController.Instance.World.GetTileAt(x_off, y_off, t.Z).PendingBuildJob;
                if (pendingBuildJob != null)
                {
                    // if the existing buildJobs furniture is replaceable by the current furnitureType,
                    // we can pretend it does not overlap with the new build
                    return !proto.ReplaceableFurniture.Any(pendingBuildJob.furniturePrototype.HasTypeTag);
                }
            }
        }

        return false;
    }

    // Use this for initialization
    private void Start()
    {
    }
}
