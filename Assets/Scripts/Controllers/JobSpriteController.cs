using UnityEngine;
using System.Collections.Generic;

public class JobSpriteController : MonoBehaviour
{

    // This bare-bones controller is mostly just going to piggyback
    // on FurnitureSpriteController because we don't yet fully know
    // what our job system is going to look like in the end.

    FurnitureSpriteController fsc;
    Dictionary<Job, GameObject> jobGameObjectMap;

    // Use this for initialization
    void Start()
    {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
        
        WorldController.Instance.world.JobQueue.JobCreated += OnJobCreated;
    }

    void OnJobCreated(Job job)
    {

        if (job.JobObjectType == null && job.JobTileType == TileType.Empty)
        {
            // This job doesn't really have an associated sprite with it, so no need to render.
            return;
        }

        // FIXME: We can only do furniture-building jobs.

        // TODO: Sprite


        if (jobGameObjectMap.ContainsKey(job))
        {
            //Debug.LogError("OnJobCreated for a jobGO that already exists -- most likely a job being RE-QUEUED, as opposed to created.");
            return;
        }

        GameObject job_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        jobGameObjectMap.Add(job, job_go);

        job_go.name = "JOB_" + job.JobObjectType + "_" + job.Tile.X + "_" + job.Tile.Y;
        job_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        if (job.JobTileType != TileType.Empty)
        {
            //This job is for building a tile
            //For now, the only tile that could be is the floor, so just show a floor sprite
            //until the graphics system for tiles is fleshed out further

            job_go.transform.position = new Vector3(job.Tile.X, job.Tile.Y, 0);
            sr.sprite = SpriteManager.current.GetSprite("Tile", "Empty");
        }
        else
        {
            //This is a normal furniture job.
            job_go.transform.position = new Vector3(job.Tile.X + ((job.FurniturePrototype.Width - 1) / 2f), job.Tile.Y + ((job.FurniturePrototype.Height - 1) / 2f), 0);
            sr.sprite = fsc.GetSpriteForFurniture (job.JobObjectType);
        }
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        sr.sortingLayerName = "Jobs";

        // FIXME: This hardcoding is not ideal!  <== Understatement
        if (job.JobObjectType == "Door")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees

            Tile northTile = World.Current.GetTileAt(job.Tile.X, job.Tile.Y + 1);
            Tile southTile = World.Current.GetTileAt(job.Tile.X, job.Tile.Y - 1);

            if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
            northTile.Furniture.ObjectType.Contains("Wall") && southTile.Furniture.ObjectType.Contains("Wall"))
            {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }


        job.JobCompleted += OnJobEnded;
        job.JobStopped += OnJobEnded;
    }

    void OnJobEnded(Job job)
    {
        // This executes whether a job was COMPLETED or CANCELLED

        // FIXME: We can only do furniture-building jobs.

        GameObject job_go = jobGameObjectMap[job];

        job.JobCompleted -= OnJobEnded;
        job.JobStopped -= OnJobEnded;

        Destroy(job_go);

    }



}
