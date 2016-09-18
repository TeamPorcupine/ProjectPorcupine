#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class FurnitureJobs
{
    private Furniture furniture;
    private List<Job> activeJobs;
    private List<Job> pausedJobs;

    /// <summary>
    /// Initializes a new instance of the <see cref="FurnitureJobs"/> class.
    /// </summary>
    /// <param name="furn">The current furniture.</param>
    public FurnitureJobs(Furniture furn)
    {
        furniture = furn;
        activeJobs = new List<Job>();
        pausedJobs = new List<Job>();

        SpawnSpotOffset = Vector2.zero;
        WorkSpotOffset = Vector2.zero;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FurnitureJobs"/> class by copying some of the values from another instance.
    /// </summary>
    /// <param name="furn">The current furniture.</param>
    /// <param name="other">The prototype furniture.</param>
    public FurnitureJobs(Furniture furn, Furniture other)
    {
        furniture = furn;
        activeJobs = new List<Job>();
        pausedJobs = new List<Job>();

        SpawnSpotOffset = other.Jobs.SpawnSpotOffset;
        WorkSpotOffset = other.Jobs.WorkSpotOffset;
    }

    /// <summary>
    /// Gets the spot offset where the Character will stand when he is using the furniture. This is relative to the bottom
    /// left tile of the sprite. This can be outside of the actual furniture.
    /// </summary>
    /// <value>The spot offset where the Character will stand when he uses the furniture.</value>
    public Vector2 WorkSpotOffset { get; private set; }

    /// <summary>
    /// Gets the spot offset where inventory is spawn when a Job is done with this machine.
    /// </summary>
    /// <value>The spawn spot offset.</value>
    public Vector2 SpawnSpotOffset { get; private set; }

    /// <summary>
    /// Gets the active <see cref="Job"/> with the specified index.
    /// </summary>
    /// <param name="i">The index.</param>
    public Job this[int i]
    {
        get
        {
            return activeJobs[i];
        }
    }

    /// <summary>
    /// Gets the tile that is used to do a job.
    /// </summary>
    /// <returns>Tile that is used for jobs.</returns>
    public Tile GetWorkSpotTile()
    {
        return World.Current.GetTileAt(furniture.Tile.X + (int)WorkSpotOffset.x, furniture.Tile.Y + (int)WorkSpotOffset.y, furniture.Tile.Z);
    }

    /// <summary>
    /// Gets the tile that is used to spawn new objects (i.e. Inventory, Character).
    /// </summary>
    /// <returns>Tile that is used to spawn objects (i.e. Inventory, Character).</returns>
    public Tile GetSpawnSpotTile()
    {
        return World.Current.GetTileAt(furniture.Tile.X + (int)SpawnSpotOffset.x, furniture.Tile.Y + (int)SpawnSpotOffset.y, furniture.Tile.Z);
    }

    /// <summary>
    /// Reads the job work spot offset from the xml.
    /// </summary>
    /// <param name="reader">The Xml Reader.</param>
    public void ReadWorkSpotOffset(XmlReader reader)
    {
        WorkSpotOffset = new Vector2(
            int.Parse(reader.GetAttribute("X")),
            int.Parse(reader.GetAttribute("Y")));
    }

    /// <summary>
    /// Reads the spawn spot offset from the xml.
    /// </summary>
    /// <param name="reader">The Xml Reader.</param>
    public void ReadSpawnSpotOffset(XmlReader reader)
    {
        SpawnSpotOffset = new Vector2(
            int.Parse(reader.GetAttribute("X")),
            int.Parse(reader.GetAttribute("Y")));
    }

    /// <summary>
    /// How many active jobs are linked to this furniture.
    /// </summary>
    /// <returns>The number of jobs linked to this furniture.</returns>
    public int Count()
    {
        return activeJobs.Count;
    }

    /// <summary>
    /// Checks for first furniture job with specific condition.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="job">Job fulfilling predicate.</param>
    /// <returns>True if there is job with predicate.</returns>
    public bool HasJobWithPredicate(Func<Job, bool> predicate, out Job job)
    {
        job = activeJobs.FirstOrDefault(predicate);
        return job != null;
    }

    /// <summary>
    /// Link a job to the current furniture.
    /// </summary>
    /// <param name="job">The job that you want to link to the furniture.</param>
    public void Add(Job job)
    {
        if (furniture.isBeingDestroyed)
        {
            return;
        }
        job.buildable = furniture;
        activeJobs.Add(job);
        job.OnJobStopped += OnJobStopped;
        World.Current.jobQueue.Enqueue(job);
    }

    /// <summary>
    /// Cancel all the active jobs linked to the current furniture.
    /// </summary>
    public void CancelAll()
    {
        Job[] jobsArray = activeJobs.ToArray();
        foreach (Job job in jobsArray)
        {
            job.CancelJob();
        }
    }

    /// <summary>
    /// Resumes all the paused jobs linked to the current furniture.
    /// </summary>
    /// TODO: Refactor this when the new job system is implemented
    public void ResumeAll()
    {
        if (pausedJobs.Count > 0)
        {
            Job[] jobsArray = pausedJobs.ToArray();
            foreach (Job job in jobsArray)
            {
                Add(job);
                pausedJobs.Remove(job);
            }
        }
    }

    /// <summary>
    /// Pauses all the active jobs linked to the current furniture.
    /// </summary>
    /// TODO: Refactor this when the new job system is implemented
    public void PauseAll()
    {
        if (activeJobs.Count > 0)
        {
            Job[] jobsArray = activeJobs.ToArray();
            foreach (Job job in jobsArray)
            {
                pausedJobs.Add(job);
                job.CancelJob();
            }
        }
    }

    /// <summary>
    /// Remove the specified job. It removes the link to the furniture and the event.
    /// </summary>
    /// <param name="job">The job to remove.</param>
    private void Remove(Job job)
    {
        job.OnJobStopped -= OnJobStopped;
        activeJobs.Remove(job);
        job.buildable = null;
    }

    /// <summary>
    /// Removes all the active jobs.
    /// </summary>
    private void RemoveAll()
    {
        Job[] jobsArray = activeJobs.ToArray();
        foreach (Job job in jobsArray)
        {
            Remove(job);
        }
    }

    /// <summary>
    /// Called when a job stops to remove the job from the active jobs.
    /// </summary>
    /// <param name="job">The stopped job.</param>
    private void OnJobStopped(Job job)
    {
        Remove(job);
    }
}
