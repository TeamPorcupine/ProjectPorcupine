//=======================================================================
// Copyright Martin "quill18" Glaude 2015-2016.
//		http://quill18.com
//=======================================================================

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

    public void Enqueue(Job j)
    {
        //Debug.Log("Adding job to queue. Existing queue size: " + jobQueue.Count);
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
            return;
        }
        jobQueue.RemoveAt(jobQueue.IndexOfValue(j));
    }

}
