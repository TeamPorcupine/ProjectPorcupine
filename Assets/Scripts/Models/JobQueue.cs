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
using System;

public class JobQueue
{
    SortedList<Job.JobPriority, Job> jobQueue;

    public event Action<Job> cbJobCreated;

    public JobQueue()
    {
        jobQueue = new SortedList<Job.JobPriority, Job>(new DuplicateKeyComparer<Job.JobPriority>(true));
    }

    public bool IsEmpty()
    {
        return jobQueue.Count == 0;
    }

    // Returns the job count in the queue.
    // (Necessary, since jobQueue is private.)
    public int GetCount()
    {
        return jobQueue.Count;
    }

    public void Enqueue(Job j)
    {
        //Logger.Log("Adding job to queue. Existing queue size: " + jobQueue.Count);
        if (j.jobTime < 0)
        {
            // Job has a negative job time, so it's not actually
            // supposed to be queued up.  Just insta-complete it.
            j.DoWork(0);
            return;
        }

        jobQueue.Add(j.jobPriority,j);

        if (cbJobCreated != null)
        {
            cbJobCreated(j);
        }
    }

    public Job Dequeue()
    {
        if (jobQueue.Count == 0)
            return null;

        Job job = jobQueue.Values[0];
        jobQueue.RemoveAt(0);
        return job;
    }

    public void Remove(Job j)
    {
        if (jobQueue.ContainsValue(j)==false)
        {
            //Logger.LogError("Trying to remove a job that doesn't exist on the queue.");
            // Most likely, this job wasn't on the queue because a character was working it!
            return;
        }
        jobQueue.RemoveAt(jobQueue.IndexOfValue(j));
    }

    public IEnumerable<Job> PeekJobs()
    {
        // For debugging only. For the real thing we want to return something safer (like preformatted strings.).
        foreach (Job job in jobQueue.Values)
        {
            yield return job;
        }
    }
}
