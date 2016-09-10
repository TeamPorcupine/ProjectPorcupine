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
using ProjectPorcupine.Pathfinding;

namespace ProjectPorcupine.State
{
    [System.Diagnostics.DebuggerDisplay("JobState: {job}")]
    public class JobState : State
    {
        private bool jobFinished = false;

        public JobState(Character character, Job job, State nextState = null)
            : base("Job", character, nextState)
        {
            this.Job = job;

            job.OnJobCompleted += OnJobCompleted;
            job.OnJobStopped += OnJobStopped;
            job.IsBeingWorked = true;

            FSMLog("created {0}", job.GetName());
        }

        public Job Job { get; private set; }

        public override void Update(float deltaTime)
        {
            if (jobFinished)
            {
                FSMLog(" - Update called on a finished job");
                Finished();
                return;
            }

            // If we are lacking material, then go deliver materials
            if (Job.MaterialNeedsMet() == false)
            {
                FSMLog(" - Next action: Haul material");
                character.SetState(new HaulState(character, Job, this));
            }
            else if (Job.IsTileAtJobSite(character.CurrTile) == false)
            {
                FSMLog(" - Next action: Go to job");
                List<Tile> path = Pathfinder.FindPathToTile(character.CurrTile, Job.tile, Job.adjacent);
                if (path != null && path.Count > 0)
                {
                    character.SetState(new MoveState(character, Job.IsTileAtJobSite, path, this));
                }
                else
                {
                    Interrupt();
                }
            }
            else
            {
                FSMLog(" - Next action: Work");
                Job.DoWork(deltaTime);
            }
        }

        public override void Interrupt()
        {
            // If we still have a reference to a job, then someone else is stealing the state and we should put it back on the queue.
            if (Job != null)
            {
                AbandonJob();
            }

            base.Interrupt();
        }

        private void AbandonJob()
        {
            FSMLog(" - Job abandoned!");
            Debug.ULogChannel("Character", character.GetName() + " abandoned their job.");

            Job.OnJobStopped -= OnJobStopped;
            Job.IsBeingWorked = false;

            // Tell anyone else who cares that it was cancelled
            Job.CancelJob();

            if (Job.IsNeed)
            {
                return;
            }

            // Drops the priority a level.
            Job.DropPriority();

            // If the job gets abandoned because of pathing issues or something else, just return it to the queue
            World.Current.jobQueue.Enqueue(Job);
        }

        private void OnJobStopped(Job stoppedJob)
        {
            FSMLog(" - Job stopped");

            jobFinished = true;

            // Job completed (if non-repeating) or was cancelled.
            stoppedJob.OnJobCompleted -= OnJobCompleted;
            stoppedJob.OnJobStopped -= OnJobStopped;
            Job.IsBeingWorked = false;

            if (Job != stoppedJob)
            {
                Debug.ULogErrorChannel("Character", "Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }
        }

        private void OnJobCompleted(Job finishedJob)
        {
            FSMLog(" - Job finished");

            jobFinished = true;

            finishedJob.OnJobCompleted -= OnJobCompleted;
            finishedJob.OnJobStopped -= OnJobStopped;

            if (Job != finishedJob)
            {
                Debug.ULogErrorChannel("Character", "Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }
        }
    }
}
