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

        World.Current.inventoryManager.InventoryCreated += ReevaluateWaitingQueue;

        PrototypeManager.SchedulerEvent.Add(
            "JobQueue_ReevaluateReachability",
            new Scheduler.ScheduledEvent(
                "JobQueue_ReevaluateReachability",
                (evt) => ReevaluateReachability()
            )
        );

        Scheduler.Scheduler.Current.ScheduleEvent("JobQueue_ReevaluateReachability", 60f, true);
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

        // If the job requres material but there is nothing available, store it in jobsWaitingForInventory
        if (job.inventoryRequirements.Count > 0 && job.GetFirstFulfillableInventoryRequirement() == null)
        {
            string missing = job.acceptsAny ? "*" : job.GetFirstDesiredInventory().Type;
            Debug.ULogChannel("FSM", " - missingInventory {0}", missing);
            if (jobsWaitingForInventory.ContainsKey(missing) == false)
            {
                jobsWaitingForInventory[missing] = new List<Job>();
            }

            jobsWaitingForInventory[missing].Add(job);
        }
        else if (job.tile != null && job.tile.IsReachableFromAnyNeighbor(true) == false)
        {
            Debug.ULogChannel("FSM", " - Job can't be reached");
            unreachableJobs.Enqueue(job);
        }
        else
        {
            Debug.ULogChannel("FSM", " - {0}", job.acceptsAny ? "Any" : "All");
            foreach (Inventory inventory in job.inventoryRequirements.Values)
            {
                Debug.ULogChannel("FSM", "   - {0} {1}", inventory.MaxStackSize - inventory.StackSize, inventory.ObjectType);
            }
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
            if (job.IsRequiredInventoriesAvailable() && job.tile.IsReachableFromAnyNeighbor(true))
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

        List<Job> waitingJobs = null;

        // Check that there is a job waiting for this inventory.
        if (jobsWaitingForInventory.ContainsKey(inv.ObjectType) && jobsWaitingForInventory[inv.ObjectType].Count > 0)
        {
            // Get the current list of jobs
            waitingJobs = jobsWaitingForInventory[inv.ObjectType];

            // Replace it with an empty list
            jobsWaitingForInventory[inv.ObjectType] = new List<Job>();

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

    private void ReevaluateReachability()
    {
        Debug.ULogChannel("FSM", " - Reevaluate reachability of {0} jobs", unreachableJobs.Count);
        Queue<Job> jobsToReevaluate = unreachableJobs;
        unreachableJobs = new Queue<Job>();

        foreach (Job job in jobsToReevaluate)
        {
            Enqueue(job);
        }
    }
}
