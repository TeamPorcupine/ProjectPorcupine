#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
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
    public string buildModeObjectType;

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

        Furniture proto = WorldController.Instance.world.furniturePrototypes[buildModeObjectType];

        return proto.Width == 1 && proto.Height == 1;
    }

    public void SetMode_BuildFloor()
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = TileType.Floor;

        mouseController.StartBuildMode();
    }

    public void SetMode_BuildLadder()
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = TileType.Ladder;

        mouseController.StartBuildMode();
    }

    public void SetMode_Bulldoze()
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = TileType.Empty;
        mouseController.StartBuildMode();
    }

    public void SetMode_BuildFurniture(string objectType)
    {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        buildMode = BuildMode.FURNITURE;
        buildModeObjectType = objectType;
        mouseController.StartBuildMode();
    }

    public void SetMode_Deconstruct()
    {
        buildMode = BuildMode.DECONSTRUCT;
        mouseController.StartBuildMode();
    }

    public void DoPathfindingTest()
    {
        WorldController.Instance.world.SetupPathfindingExample();
    }

    public void DoBuild(Tile t)
    {
        if (buildMode == BuildMode.FURNITURE)
        {
            // Create the Furniture and assign it to the tile
            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function!
            string furnitureType = buildModeObjectType;

            if ( 
                WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t) &&
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

                if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey(furnitureType))
                {
                    // Make a clone of the job prototype
                    j = WorldController.Instance.world.furnitureJobPrototypes[furnitureType].Clone();

                    // Assign the correct tile.
                    j.tile = t;
                }
                else
                {
                    Debug.LogError("There is no furniture job prototype for '" + furnitureType + "'");
                    j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null, Job.JobPriority.High);
                }

                j.furniturePrototype = WorldController.Instance.world.furniturePrototypes[furnitureType];

                for (int x_off = t.X; x_off < (t.X + WorldController.Instance.world.furniturePrototypes[furnitureType].Width); x_off++)
                {
                    for (int y_off = t.Y; y_off < (t.Y + WorldController.Instance.world.furniturePrototypes[furnitureType].Height); y_off++)
                    {
                        // FIXME: I don't like having to manually and explicitly set
                        // flags that preven conflicts. It's too easy to forget to set/clear them!
                        Tile offsetTile = WorldController.Instance.world.GetTileAt(x_off,y_off);
                        offsetTile.PendingBuildJob = j;
                        j.cbJobStopped += (theJob) =>
                            {
                                offsetTile.PendingBuildJob = null;
                            };
                    }
                }

                // Add the job to the queue
                if (WorldController.Instance.devMode)
                {
                    WorldController.Instance.world.PlaceFurniture(j.jobObjectType, j.tile);
                }
                else
                {
                    WorldController.Instance.world.jobQueue.Enqueue(j);
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
                CanBuildTileTypeHere(t, tileType))
            {
                // This tile position is valid tile type

                // Create a job for it to be build
                Job j = new Job(
                    t,
                    tileType, 
                    Tile.ChangeTileTypeJobComplete, 
                    0.1f, 
                    null,
                    Job.JobPriority.High, 
                    false,
                    true);

                // FIXME: I don't like having to manually and explicitly set
                // flags that preven conflicts. It's too easy to forget to set/clear them!
                t.PendingBuildJob = j;
                j.cbJobStopped += (theJob) =>
                {
                    theJob.tile.PendingBuildJob = null;
                };

                // Add the job to the queue
                if (WorldController.Instance.devMode)
                {
                    j.tile.Type = j.jobTileType;
                }
                else
                {
                    WorldController.Instance.world.jobQueue.Enqueue(j);
                }
            }
        }
        else if (buildMode == BuildMode.DECONSTRUCT)
        {
            // TODO
            if (t.Furniture != null)
            {
                t.Furniture.Deconstruct();
            }
            else if (t.PendingBuildJob != null)
            {
                t.PendingBuildJob.CancelJob();
            }
        }
        else
        {
            Debug.LogError("UNIMPLEMENTED BUILD MODE");
        }
    }

    // Checks whether the given floor type is allowed to be built on the tile.
    // TODO Export this kind of check to an XML/LUA file for easier modding of floor types.
    private bool CanBuildTileTypeHere(Tile t, TileType tileType)
    {
        switch(tileType) {
            case TileType.Empty:
                return true;
            case TileType.Floor:
                return true;
            case TileType.Ladder:
                return t.Room.IsOutsideRoom();
            default:
                return true;
        }
    }

    public bool DoesBuildJobOverlapExistingBuildJob(Tile t, string furnitureType)
    {
        for (int x_off = t.X; x_off < (t.X + WorldController.Instance.world.furniturePrototypes[furnitureType].Width); x_off++)
        {
            for (int y_off = t.Y; y_off < (t.Y + WorldController.Instance.world.furniturePrototypes[furnitureType].Height); y_off++)
            {
                if (WorldController.Instance.world.GetTileAt(x_off, y_off).PendingBuildJob != null)
                {
                    return true;
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
