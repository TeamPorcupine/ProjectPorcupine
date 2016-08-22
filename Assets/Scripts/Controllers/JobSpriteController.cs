#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections.Generic;

public class JobSpriteController
{

    // This bare-bones controller is mostly just going to piggyback
    // on FurnitureSpriteController because we don't yet fully know
    // what our job system is going to look like in the end.

    FurnitureSpriteController fsc;
    Dictionary<Job, GameObject> jobGameObjectMap;
    World world;
    GameObject jobParent;

    // Use this for initialization
    public JobSpriteController(World currentWorld, FurnitureSpriteController furnitureSpriteController)
    {
        world = currentWorld;
        jobGameObjectMap = new Dictionary<Job, GameObject>();

        fsc = furnitureSpriteController;
        world.jobQueue.cbJobCreated += OnJobCreated;
        jobParent = new GameObject("Jobs");
    }

    void OnJobCreated(Job job)
    {

        if (job.jobObjectType == null && job.jobTileType == TileType.Empty)
        {
            // This job doesn't really have an associated sprite with it, so no need to render.
            return;
        }

        // FIXME: We can only do furniture-building jobs.

        // TODO: Sprite


        if (jobGameObjectMap.ContainsKey(job))
        {
            //Logger.LogError("OnJobCreated for a jobGO that already exists -- most likely a job being RE-QUEUED, as opposed to created.");
            return;
        }

        GameObject job_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        jobGameObjectMap.Add(job, job_go);

        job_go.name = "JOB_" + job.jobObjectType + "_" + job.tile.X + "_" + job.tile.Y;
        job_go.transform.SetParent(jobParent.transform, true);

        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        if (job.jobTileType != TileType.Empty)
        {
            //This job is for building a tile
            //For now, the only tile that could be is the floor, so just show a floor sprite
            //until the graphics system for tiles is fleshed out further

            job_go.transform.position = new Vector3(job.tile.X, job.tile.Y, 0);
            sr.sprite = SpriteManager.current.GetSprite("Tile", "Empty");
        }
        else
        {
            //This is a normal furniture job.
            job_go.transform.position = new Vector3(job.tile.X + ((job.furniturePrototype.Width - 1) / 2f), job.tile.Y + ((job.furniturePrototype.Height - 1) / 2f), 0);
            sr.sprite = fsc.GetSpriteForFurniture (job.jobObjectType);
        }
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        sr.sortingLayerName = "Jobs";

        // FIXME: This hardcoding is not ideal!  <== Understatement
        if (job.jobObjectType == "Door")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees

            Tile northTile = world.GetTileAt(job.tile.X, job.tile.Y + 1);
            Tile southTile = world.GetTileAt(job.tile.X, job.tile.Y - 1);

            if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
            northTile.furniture.objectType.Contains("Wall") && southTile.furniture.objectType.Contains("Wall"))
            {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }


        job.cbJobCompleted += OnJobEnded;
        job.cbJobStopped += OnJobEnded;
    }

    void OnJobEnded(Job job)
    {
        // This executes whether a job was COMPLETED or CANCELLED

        // FIXME: We can only do furniture-building jobs.

        GameObject job_go = jobGameObjectMap[job];

        job.cbJobCompleted -= OnJobEnded;
        job.cbJobStopped -= OnJobEnded;

        GameObject.Destroy(job_go);

    }



}
