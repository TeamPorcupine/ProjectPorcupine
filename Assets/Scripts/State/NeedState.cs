#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

namespace ProjectPorcupine.State
{
    public class NeedState : State
    {
        public NeedState(Character character, State nextState = null)
            : base("Need", character, nextState)
        {
        }

        public override void Update(float deltaTime)
        {
            float needPercent = 0f;
            Need biggestNeed = null;

            foreach (Need need in character.Needs)
            {
                need.Update(deltaTime);
            }

            // At this point we want to do something about the need, but we let the current state finish first
            if (needPercent > 50 && needPercent < 100 && biggestNeed.RestoreNeedFurn != null)
            {
                if (World.Current.FurnitureManager.CountWithType(biggestNeed.RestoreNeedFurn.Type) > 0)
                {
                    Job job = new Job(null, biggestNeed.RestoreNeedFurn.Type, biggestNeed.CompleteJobNorm, biggestNeed.RestoreNeedTime, null, Job.JobPriority.High, false, true, false);
                    character.QueueState(new JobState(character, job));
                }
            }

            // We must do something immediately, drop what we are doing.
            if (needPercent == 100 && biggestNeed != null && biggestNeed.CompleteOnFail)
            {
                Job job = new Job(character.CurrTile, null, biggestNeed.CompleteJobCrit, biggestNeed.RestoreNeedTime * 10, null, Job.JobPriority.High, false, true, true);
                character.InterruptState();
                character.ClearStateQueue();
                character.SetState(new JobState(character, job));
            }
        }
    }
}