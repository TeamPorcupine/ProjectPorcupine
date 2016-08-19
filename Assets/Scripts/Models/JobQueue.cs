﻿//=======================================================================
// Copyright Martin "quill18" Glaude 2015-2016.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections.Generic;
using System;

public class JobQueue
{
    private Queue<Job> _jobQueue;

    public event Action<Job> JobCreated;

    public JobQueue()
    {
        _jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        //Debug.Log("Adding job to queue. Existing queue size: " + jobQueue.Count);
        if (j.JobTime < 0)
        {
            // Job has a negative job time, so it's not actually
            // supposed to be queued up.  Just insta-complete it.
            j.DoWork(0);
            return;
        }

        _jobQueue.Enqueue(j);

        if (JobCreated != null)
        {
            JobCreated(j);
        }
    }

    public Job Dequeue()
    {
        if (_jobQueue.Count == 0)
            return null;

        return _jobQueue.Dequeue();
    }

    public void Remove(Job j)
    {
        // TODO: Check docs to see if there's a less memory/swappy solution
        List<Job> jobs = new List<Job>(_jobQueue);

        if (jobs.Contains(j) == false)
        {
            //Debug.LogError("Trying to remove a job that doesn't exist on the queue.");
            // Most likely, this job wasn't on the queue because a character was working it!
            return;
        }

        jobs.Remove(j);
        _jobQueue = new Queue<Job>(jobs);
    }
}