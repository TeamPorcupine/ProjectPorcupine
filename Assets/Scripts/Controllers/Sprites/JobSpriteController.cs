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
    private UtilitySpriteController usc;

    // Use this for initialization
    public JobSpriteController(World world, FurnitureSpriteController furnitureSpriteController, UtilitySpriteController utilitySpriteController)
        : base(world, "Jobs")
    {
        fsc = furnitureSpriteController;
        usc = utilitySpriteController;
        world.jobQueue.OnJobCreated += OnCreated;

        foreach (Job job in world.jobQueue.PeekAllJobs())
        {
            OnCreated(job);
        }

        foreach (Character character in world.CharacterManager)
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

        foreach (Job job in world.jobQueue.PeekAllJobs())
        {
            job.OnJobCompleted -= OnRemoved;
            job.OnJobStopped -= OnRemoved;
        }

        foreach (Character character in world.CharacterManager)
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
        if (job.JobTileType == null && job.Type == null)
        {
            // This job doesn't really have an associated sprite with it, so no need to render.
            return;
        }

        if (objectGameObjectMap.ContainsKey(job))
        {
            return;
        }

        GameObject job_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(job, job_go);

        job_go.name = "JOB_" + job.Type + "_" + job.tile.X + "_" + job.tile.Y + "_" + job.tile.Z;
        job_go.transform.SetParent(objectParent.transform, true);

        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        if (job.JobTileType != null)
        {
            // This job is for building a tile.
            // For now, the only tile that could be is the floor, so just show a floor sprite
            // until the graphics system for tiles is fleshed out further.
            job_go.transform.position = job.tile.Vector3;
            sr.sprite = SpriteManager.GetSprite("Tile", "Solid");
            sr.color = new Color32(128, 255, 128, 192);
        }
        else if (job.Description.Contains("deconstruct"))
        {
            sr.sprite = SpriteManager.GetSprite("UI", "CursorCircle");
            sr.color = Color.red;
            job_go.transform.position = job.tile.Vector3;
        }
        else if (job.Description.Contains("mine"))
        {
            sr.sprite = SpriteManager.GetSprite("UI", "MiningIcon");
            sr.color = new Color(1, 1, 1, 0.25f);
            job_go.transform.position = job.tile.Vector3;
        }
        else
        {
            // If we get this far we need a buildable prototype, bail if we don't have one
            if (job.buildablePrototype == null)
            {
                return;
            }

            // This is a normal furniture job.
            if (job.buildablePrototype.GetType().ToString() == "Furniture")
            {
                Furniture furnitureToBuild = (Furniture)job.buildablePrototype;
                sr.sprite = fsc.GetSpriteForFurniture(job.Type);
                job_go.transform.position = job.tile.Vector3 + ImageUtils.SpritePivotOffset(sr.sprite, furnitureToBuild.Rotation);
                job_go.transform.Rotate(0, 0, furnitureToBuild.Rotation);
            }
            else if (job.buildablePrototype.GetType().ToString() == "Utility")
            {
                sr.sprite = usc.GetSpriteForUtility(job.Type);
                job_go.transform.position = job.tile.Vector3 + ImageUtils.SpritePivotOffset(sr.sprite);
            }

            sr.color = new Color32(128, 255, 128, 64);
        }

        sr.sortingLayerName = "Jobs";

        // FIXME: This hardcoding is not ideal!  <== Understatement
        if (job.Type == "Door")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees
            Tile northTile = world.GetTileAt(job.tile.X, job.tile.Y + 1, job.tile.Z);
            Tile southTile = world.GetTileAt(job.tile.X, job.tile.Y - 1, job.tile.Z);

            if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
                northTile.Furniture.HasTypeTag("Wall") && southTile.Furniture.HasTypeTag("Wall"))
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
