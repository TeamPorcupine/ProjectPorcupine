#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
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

            DebugLog("created {0}", job.Type ?? "Unnamed Job");
        }

        public Job Job { get; private set; }

        public override void Update(float deltaTime)
        {
            if (jobFinished)
            {
                DebugLog(" - Update called on a finished job");
                Finished();
                return;
            }

            // If we are lacking material, then go deliver materials
            if (Job.MaterialNeedsMet() == false)
            {
                if (Job.IsRequiredInventoriesAvailable() == false)
                {
                    AbandonJob();
                    Finished();
                    return;
                }

                DebugLog(" - Next action: Haul material");
                character.SetState(new HaulState(character, Job, this));
            }
            else if (Job.IsTileAtJobSite(character.CurrTile) == false)
            {
                DebugLog(" - Next action: Go to job");
                List<Tile> path = Pathfinder.FindPathToTile(character.CurrTile, Job.tile, Job.adjacent);
                if (path != null && path.Count > 0)
                {
                    character.SetState(new MoveState(character, Job.IsTileAtJobSite, path, this));
                }
                else
                {
                    // Add character to the list of characters unable to reach the job.
                    if (!World.Current.jobQueue.CharacterCantReachHelper(Job, character))
                    {
                        Job.CharsCantReach.Add(character);
                    }
                   
                    Interrupt();
                }
            }
            else
            {
                DebugLog(" - Next action: Work");

                if (Job.tile != character.CurrTile)
                {
                    // We aren't standing on the job spot itself, so make sure to face it.
                    character.FaceTile(Job.tile);
                }

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
            DebugLog(" - Job abandoned!");
            UnityDebugger.Debugger.Log("Character", character.GetName() + " abandoned their job.");

            Job.OnJobCompleted -= OnJobCompleted;
            Job.OnJobStopped -= OnJobStopped;
            Job.IsBeingWorked = false;

            // Tell anyone else who cares that it was cancelled
            Job.CancelJob();

            if (Job.IsNeed)
            {
                return;
            }

            // If the job gets abandoned because of pathing issues or something else, just return it to the queue
            World.Current.jobQueue.Enqueue(Job);

            // Tell the player that we need a new task.
            character.SetState(null);
        }

        private void OnJobStopped(Job stoppedJob)
        {
            DebugLog(" - Job stopped");

            jobFinished = true;

            // Job completed (if non-repeating) or was cancelled.
            stoppedJob.OnJobCompleted -= OnJobCompleted;
            stoppedJob.OnJobStopped -= OnJobStopped;
            Job.IsBeingWorked = false;

            if (Job != stoppedJob)
            {
                UnityDebugger.Debugger.LogError("Character", "Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }
        }

        private void OnJobCompleted(Job finishedJob)
        {
            // Finish job, unless it repeats, in which case continue as if nothing happened.
            if (finishedJob.IsRepeating == false)
            {
                DebugLog(" - Job finished");

                jobFinished = true;

                finishedJob.OnJobCompleted -= OnJobCompleted;
                finishedJob.OnJobStopped -= OnJobStopped;

                if (Job != finishedJob)
                {
                    UnityDebugger.Debugger.LogError("Character", "Character being told about job that isn't his. You forgot to unregister something.");
                    return;
                }
            }
        }
    }
}
