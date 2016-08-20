﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum BuildMode
{
    FLOOR,
    FURNITURE,
    DECONSTRUCT
}

public class BuildModeController : MonoBehaviour
{

    public BuildMode buildMode = BuildMode.FLOOR;
    TileType buildModeTile = TileType.Floor;
    public string buildModeObjectType;



    // Use this for initialization
    void Start()
    {


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

        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }

    public void SetMode_Bulldoze()
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = TileType.Empty;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }

    public void SetMode_BuildFurniture(string objectType)
    {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        buildMode = BuildMode.FURNITURE;
        buildModeObjectType = objectType;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }

    public void SetMode_Deconstruct()
    {
        buildMode = BuildMode.DECONSTRUCT;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
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
                DoesBuildJobOverlapExistingBuildJob(t,furnitureType) == false)
            {
                // This tile position is valid for this furniture

                // Check if there is existing furniture in this tile. If so delete it.
                // TODO Possibly return resources. Will the Deconstruct() method handle that? If so what will happen if resources drop ontop of new non-passable structure.
                if (t.furniture != null)
                {
                    t.furniture.Deconstruct();
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
                    Logger.LogError("There is no furniture job prototype for '" + furnitureType + "'");
                    j = new Job(t, furnitureType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null,Job.JobPriority.High);
                }

                j.furniturePrototype = WorldController.Instance.world.furniturePrototypes[furnitureType];

                for (int x_off = t.X; x_off < (t.X + WorldController.Instance.world.furniturePrototypes[furnitureType].Width); x_off++)
                {
                    for (int y_off = t.Y; y_off < (t.Y + WorldController.Instance.world.furniturePrototypes[furnitureType].Height); y_off++)
                    {
                        // FIXME: I don't like having to manually and explicitly set
                        // flags that preven conflicts. It's too easy to forget to set/clear them!
                        Tile offsetTile = WorldController.Instance.world.GetTileAt(x_off,y_off);
                        offsetTile.pendingBuildJob = j;
                        j.cbJobStopped += (theJob) =>
                            {
                                offsetTile.pendingBuildJob = null;
                            };
                    }
                }

                // Add the job to the queue
                WorldController.Instance.world.jobQueue.Enqueue(j);

            }

        }
        else if (buildMode == BuildMode.FLOOR)
        {
            // We are in tile-changing mode.
            //t.Type = buildModeTile;

            TileType tileType = buildModeTile;

            if ( 
                t.Type != tileType && 
                t.furniture == null &&
                t.pendingBuildJob == null)
            {
                // This tile position is valid til type

                // Create a job for it to be build

                Job j = new Job(t,
                    tileType, 
                    Tile.ChangeTileTypeJobComplete, 
                    0.1f, 
                    null,
                    Job.JobPriority.High, 
                    false,
                    true);


                // FIXME: I don't like having to manually and explicitly set
                // flags that preven conflicts. It's too easy to forget to set/clear them!
                t.pendingBuildJob = j;
                j.cbJobStopped += (theJob) =>
                {
                    theJob.tile.pendingBuildJob = null;
                };

                // Add the job to the queue
                WorldController.Instance.world.jobQueue.Enqueue(j);

            }

        }
        else if (buildMode == BuildMode.DECONSTRUCT)
        {
            // TODO
            if (t.furniture != null)
            {
                t.furniture.Deconstruct();
            }

        }
        else
        {
            Logger.LogError("UNIMPLMENTED BUILD MODE");
        }

    }

    public bool DoesBuildJobOverlapExistingBuildJob(Tile t, string furnitureType)
    {
        for (int x_off = t.X; x_off < (t.X + WorldController.Instance.world.furniturePrototypes[furnitureType].Width); x_off++)
        {
            for (int y_off = t.Y; y_off < (t.Y + WorldController.Instance.world.furniturePrototypes[furnitureType].Height); y_off++)
            {
                if (WorldController.Instance.world.GetTileAt(x_off, y_off).pendingBuildJob != null)
                {
                    return true;
                }
            }
        }

        return false;
    }


}
