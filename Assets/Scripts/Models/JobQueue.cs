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
using UnityEngine;

public class JobQueue
{
    private SortedList<Job.JobPriority, Job> jobQueue;
    private Dictionary<string, List<Job>> jobsWaitingForInventory;

    public JobQueue()
    {
        jobQueue = new SortedList<Job.JobPriority, Job>(new DuplicateKeyComparer<Job.JobPriority>(true));
        jobsWaitingForInventory = new Dictionary<string, List<Job>>();

        World.Current.inventoryManager.InventoryCreated += ReevaluateWaitingQueue;
    }

    public event Action<Job> OnJobCreated;

    public bool IsEmpty()
    {
        return jobQueue.Count == 0;
    }

    // Returns the job count in the queue.
    // (Necessary, since jobQueue is private).
    public int GetCount()
    {
        return jobQueue.Count;
    }

    public void Enqueue(Job job)
    {
        Debug.ULogChannel("FSM", "Enqueue({0})", job.JobObjectType);
        if (job.JobTime < 0)
        {
            // Job has a negative job time, so it's not actually
            // supposed to be queued up.  Just insta-complete it.
            job.DoWork(0);
            return;
        }

        // TODO: We really should check `job.tile.HasWalkableNeighbours(true)` here, but we aren't registered for events on neighboring tiles.
        if (job.IsRequiredInventoriesAvailable() == false)
        {
            string missing = job.acceptsAny ? "*" : job.GetFirstDesiredInventory().Type;
            Debug.ULogChannel("FSM", " - missingInventory {0}", missing);
            if (jobsWaitingForInventory.ContainsKey(missing) == false)
            {
                jobsWaitingForInventory[missing] = new List<Job>();
            }

            jobsWaitingForInventory[missing].Add(job);
        }
        else
        {
            Debug.ULogChannel("FSM", " - job ok");
            jobQueue.Add(job.Priority, job);
        }


        if (OnJobCreated != null)
        {
            OnJobCreated(job);
        }
    }

    public Job Dequeue()
    {
        if (jobQueue.Count == 0)
        {
            return null;
        }

        Job job = jobQueue.Values[0];
        jobQueue.RemoveAt(0);
        return job;
    }

    /// <summary>
    /// Search for a job that can be performed by the specified character. Tests that the job can be reached and there is enough inventory to complete it, somewhere.
    /// </summary>
    public Job GetJob(Character character)
    {
        Debug.ULogChannel("FSM", "GetJob() (Queue size: {0})", jobQueue.Count);
        if (jobQueue.Count == 0)
        {
            return null;
        }

        // This makes a large assumption that we are the only one accessing the queue right now
        for (int i = 0; i < jobQueue.Count; i++)
        {
            Job job = jobQueue.Values[i];

            // TODO: This is a simplistic version and needs to be expanded.
            // If we can get all material and we can walk to the tile, the job is workable.
            if (job.IsRequiredInventoriesAvailable() && job.tile.HasWalkableNeighbours())
            {
                jobQueue.RemoveAt(i);
                return job;
            }

            Debug.ULogChannel("FSM", " - job failed requirements, test the next.");
        }

        return null;
    }

    public void Remove(Job job)
    {
        if (jobQueue.ContainsValue(job))
        {
            jobQueue.RemoveAt(jobQueue.IndexOfValue(job));
        }
        else
        {
            foreach (string inventoryType in jobsWaitingForInventory.Keys)
            {
                if (jobsWaitingForInventory[inventoryType].Contains(job))
                {
                    jobsWaitingForInventory[inventoryType].Remove(job);
                }
            }
        }
    }

    /// <summary>
    /// Returns an IEnumerable for every job, including jobs that are in the waiting state.
    /// </summary>
    public IEnumerable<Job> PeekAllJobs()
    {
        foreach (Job job in jobQueue.Values)
        {
            yield return job;
        }

        foreach (string inventoryType in jobsWaitingForInventory.Keys)
        {
            foreach (Job job in jobsWaitingForInventory[inventoryType])
            {
                yield return job;
            }
        }
    }

    private void ReevaluateWaitingQueue(Inventory inv)
    {
        Debug.ULogChannel("FSM", "ReevaluateWaitingQueue() new resource: {0}, count: {1}", inv.Type, inv.StackSize);
        // In case we are alerted about something we don't care about
        if (jobsWaitingForInventory.ContainsKey(inv.Type) == false || jobsWaitingForInventory[inv.Type].Count == 0)
        {
            return;
        }

        // Get the current list of jbos
        List<Job> jobs = jobsWaitingForInventory[inv.Type];
        // Replace it with an empty list
        jobsWaitingForInventory[inv.Type] = new List<Job>();

        foreach (Job job in jobs)
        {
            // Enqueue will put them in the new waiting list we created if they still have unmet needs
            Enqueue(job);
        }

        // Do the same thing for the AnyMaterial jobs
        jobs = jobsWaitingForInventory["*"];
        jobsWaitingForInventory["*"] = new List<Job>();

        foreach (Job job in jobs)
        {
            Enqueue(job);
        }
    }
}
