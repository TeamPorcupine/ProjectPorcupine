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
using ProjectPorcupine.Jobs;

public class JobQueue
{
    private SortedList<Job.JobPriority, Job> jobQueue;
    private Dictionary<string, List<Job>> jobsWaitingForInventory;
    private Queue<Job> unreachableJobs;

    public JobQueue()
    {
        jobQueue = new SortedList<Job.JobPriority, Job>(new DuplicateKeyComparer<Job.JobPriority>(true));
        jobsWaitingForInventory = new Dictionary<string, List<Job>>();
        unreachableJobs = new Queue<Job>();

        World.Current.InventoryManager.InventoryCreated += ReevaluateWaitingQueue;
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

    /// <summary>
    /// Add a job to the JobQueue.
    /// </summary>
    /// <param name="job">The job to be inserted into the Queue.</param>
    public void Enqueue(Job job)
    {
        DebugLog("Enqueue({0})", job.Type);
        if (job.JobTime < 0)
        {
            // Job has a negative job time, so it's not actually
            // supposed to be queued up.  Just insta-complete it.
            job.DoWork(0);
            return;
        }

        // If the job requires material but there is nothing available, store it in jobsWaitingForInventory
        if (job.RequestedItems.Count > 0 && job.GetFirstFulfillableInventoryRequirement() == null)
        {
            string missing = job.acceptsAny ? "*" : job.GetFirstDesiredItem().Type;
            DebugLog(" - missingInventory {0}", missing);
            if (jobsWaitingForInventory.ContainsKey(missing) == false)
            {
                jobsWaitingForInventory[missing] = new List<Job>();
            }

            jobsWaitingForInventory[missing].Add(job);
        }
        else if ((job.tile != null && job.tile.IsReachableFromAnyNeighbor(true) == false) ||
            job.CharsCantReach.Count == World.Current.CharacterManager.characters.Count)
        {
            // No one can reach the job.
            DebugLog("JobQueue", "- Job can't be reached");
            unreachableJobs.Enqueue(job);
        }
        else
        {
            DebugLog(" - {0}", job.acceptsAny ? "Any" : "All");
            foreach (RequestedItem item in job.RequestedItems.Values)
            {
                DebugLog("   - {0} Min: {1}, Max: {2}", item.Type, item.MinAmountRequested, item.MaxAmountRequested);
            }

            DebugLog(" - job ok");

            jobQueue.Add(job.Priority, job);       
        }

        if (OnJobCreated != null)
        {
            OnJobCreated(job);
        }
    }

    /// <summary>
    /// Returns the first job from the JobQueue.
    /// </summary>
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
        DebugLog("{0} GetJob() (Queue size: {1})", character.GetName(), jobQueue.Count);
        if (jobQueue.Count == 0)
        {
            return null;
        }

        // This makes a large assumption that we are the only one accessing the queue right now
        for (int i = 0; i < jobQueue.Count; i++)
        {
            Job job = jobQueue.Values[i];
            jobQueue.RemoveAt(i);

            // TODO: This is a simplistic version and needs to be expanded.
            // If we can get all material and we can walk to the tile, the job is workable.
            if (job.IsRequiredInventoriesAvailable() && job.tile.IsReachableFromAnyNeighbor(true))
            {
                if (CharacterCantReachHelper(job, character))
                {
                    UnityDebugger.Debugger.LogError("JobQueue", "Character could not find a path to the job site.");
                    ReInsertHelper(job);
                    continue;
                }
                else if ((job.RequestedItems.Count > 0) && !job.CanGetToInventory(character))
                {
                    job.AddCharCantReach(character);
                    UnityDebugger.Debugger.LogError("JobQueue", "Character could not find a path to any inventory available.");
                    ReInsertHelper(job);
                    continue;
                }

                return job;
            }

            DebugLog(" - job failed requirements, test the next.");
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

    /// <summary>
    /// Call this whenever a furniture gets changed or removed that might effect the reachability of an object.
    /// </summary>
    public void ReevaluateReachability()
    {
        // TODO: Should this be an event on the furniture object?
        DebugLog(" - Reevaluate reachability of {0} jobs", unreachableJobs.Count);
        Queue<Job> jobsToReevaluate = unreachableJobs;
        unreachableJobs = new Queue<Job>();

        foreach (Job job in jobsToReevaluate)
        {
            job.ClearCharCantReach();
            Enqueue(job);
        }
    }

    /// <summary>
    /// Returns true if the character is already in the list of characters unable to reach the job.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="character"></param>
    /// <returns></returns>
    public bool CharacterCantReachHelper(Job job, Character character)
    {
        if (job.CharsCantReach != null)
        {
            foreach (Character charTemp in job.CharsCantReach)
            {
                if (charTemp == character)
                {
                    return true;
                }
            }   
        }

        return false;
    }

    public void ReevaluateWaitingQueue(Inventory inv)
    {
        DebugLog("ReevaluateWaitingQueue() new resource: {0}, count: {1}", inv.Type, inv.StackSize);

        List<Job> waitingJobs = null;

        // Check that there is a job waiting for this inventory.
        if (jobsWaitingForInventory.ContainsKey(inv.Type) && jobsWaitingForInventory[inv.Type].Count > 0)
        {
            // Get the current list of jobs
            waitingJobs = jobsWaitingForInventory[inv.Type];

            // Replace it with an empty list
            jobsWaitingForInventory[inv.Type] = new List<Job>();

            foreach (Job job in waitingJobs)
            {
                // Enqueue will put them in the new waiting list we created if they still have unmet needs
                Enqueue(job);
            }
        }

        // Do the same thing for the AnyMaterial jobs
        if (jobsWaitingForInventory.ContainsKey("*"))
        {
            waitingJobs = jobsWaitingForInventory["*"];
            jobsWaitingForInventory["*"] = new List<Job>();

            foreach (Job job in waitingJobs)
            {
                Enqueue(job);
            }
        }
    }

    private void ReInsertHelper(Job job)
    {
        jobQueue.Reverse();
        Enqueue(job);
        jobQueue.Reverse();
    }

    [System.Diagnostics.Conditional("FSM_DEBUG_LOG")]
    private void DebugLog(string message, params object[] par)
    {
        UnityDebugger.Debugger.LogFormat("FSM", message, par);
    }
}
