#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : BaseSpriteController<Job>
{
    // This bare-bones controller is mostly just going to piggyback
    // on FurnitureSpriteController because we don't yet fully know
    // what our job system is going to look like in the end.
    private FurnitureSpriteController fsc;

    // Use this for initialization
    public JobSpriteController(World world, FurnitureSpriteController furnitureSpriteController) : base(world, "Jobs")
    {
        fsc = furnitureSpriteController;
        world.jobQueue.OnJobCreated += OnCreated;

        foreach (Job job in world.jobQueue.PeekJobs())
        {
            OnCreated(job);
        }

        foreach (Job job in world.jobWaitingQueue.PeekJobs())
        {
            OnCreated(job);
        }

        foreach (Character character in world.characters)
        {
            if (character.MyJob != null)
            {
                OnCreated(character.MyJob);
            }
        }
    }

    public override void RemoveAll()
    {
        world.jobQueue.OnJobCreated -= OnCreated;

        foreach (Job job in world.jobQueue.PeekJobs())
        {
            job.OnJobCompleted -= OnRemoved;
            job.OnJobStopped -= OnRemoved;
        }

        foreach (Job job in world.jobWaitingQueue.PeekJobs())
        {
            job.OnJobCompleted -= OnRemoved;
            job.OnJobStopped -= OnRemoved;
        }

        foreach (Character character in world.characters)
        {
            if (character.MyJob != null)
            {
                character.MyJob.OnJobCompleted -= OnRemoved;
                character.MyJob.OnJobStopped -= OnRemoved;
            }
        }

        base.RemoveAll();
    }

    protected override void OnCreated(Job job)
    {
        if (job.JobObjectType == null && job.JobTileType == null)
        {
            // This job doesn't really have an associated sprite with it, so no need to render.
            return;
        }

        // FIXME: We can only do furniture-building jobs.
        // TODO: Sprite
        if (objectGameObjectMap.ContainsKey(job))
        {
            return;
        }

        GameObject job_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(job, job_go);

        job_go.name = "JOB_" + job.JobObjectType + "_" + job.tile.X + "_" + job.tile.Y;
        job_go.transform.SetParent(objectParent.transform, true);

        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        if (job.JobTileType != null)
        {
            // This job is for building a tile.
            // For now, the only tile that could be is the floor, so just show a floor sprite
            // until the graphics system for tiles is fleshed out further.
            job_go.transform.position = new Vector3(job.tile.X, job.tile.Y, 0);
            sr.sprite = SpriteManager.current.GetSprite("Tile", "Solid");
        }
        else
        {
            // This is a normal furniture job.
            job_go.transform.position = new Vector3(job.tile.X + ((job.furniturePrototype.Width - 1) / 2f), job.tile.Y + ((job.furniturePrototype.Height - 1) / 2f), 0);
            sr.sprite = fsc.GetSpriteForFurniture(job.JobObjectType);
        }

        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        sr.sortingLayerName = "Jobs";

        // FIXME: This hardcoding is not ideal!  <== Understatement
        if (job.JobObjectType == "Door")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees
            Tile northTile = world.GetTileAt(job.tile.X, job.tile.Y + 1);
            Tile southTile = world.GetTileAt(job.tile.X, job.tile.Y - 1);

            if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
            northTile.Furniture.ObjectType.Contains("Wall") && southTile.Furniture.ObjectType.Contains("Wall"))
            {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
            
        job.OnJobCompleted += OnRemoved;
        job.OnJobStopped += OnRemoved;
    }

    protected override void OnChanged(Job job) 
    { 
    }

    protected override void OnRemoved(Job job)
    {
        // This executes whether a job was COMPLETED or CANCELLED
        job.OnJobCompleted -= OnRemoved;
        job.OnJobStopped -= OnRemoved;

        GameObject job_go = objectGameObjectMap[job];
        objectGameObjectMap.Remove(job);
        GameObject.Destroy(job_go);
    }
}
