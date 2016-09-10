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

namespace ProjectPorcupine.State
{
    [System.Diagnostics.DebuggerDisplay("JobState: {job}")]
    public class JobState : State
    {
        public JobState(Character character, Job job, State nextState = null)
            : base("Job", character, nextState)
        {
            this.Job = job;

            job.OnJobStopped += OnJobStopped;
            job.IsBeingWorked = true;

            FSMLog("created {0}", job.GetName());
        }

        public Job Job { get; private set; }

        public override void Update(float deltaTime)
        {
            if (IsAtJobSite())
            {
                if (character.CurrTile.Equals(Job.tile) == false)
                {
                    character.FaceTile(Job.tile);
                }

                DropOffInventory();

                if (Job.MaterialNeedsMet())
                {
//                    FSMLog(" - DoWork({0}) (Left: {1})", deltaTime, Job.JobTime);
                    Job.DoWork(deltaTime);
                    return;
                }
            }

            if (Job.IsNeed && NeedToWalkToFurniture())
            {
                return;
            }

            TryPickupInventoryFromCurrentTile();

            if (NeedToFindMoreMaterial())
            {
                return;
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

        public override void Enter()
        {
            FSMLog(" - Enter");
            base.Enter();
        }

        public override void Exit()
        {
            FSMLog(" - Exit");
            base.Exit();
        }

        private void DropOffInventory()
        {
            if (character.inventory != null)
            {
                if (job.AmountDesiredOfInventoryType(character.inventory.Type) > 0)
                {
                    FSMLog(" - Dropping off {0} {1}", character.inventory.StackSize, character.inventory.ObjectType);
                    World.Current.inventoryManager.PlaceInventory(Job, character.inventory);

                    // Ping the machine
                    Job.DoWork(0);
                }
                else
                {
                    DumpExcessInventory();
                }
            }
        }

        // Returns true if everything has been handled
        private bool NeedToWalkToFurniture()
        {
            List<Tile> path = null;

            // We know where we need to go, but we aren't there
            if (Job.tile != null)
            {
                if (Job.IsTileAtJobSite(character.CurrTile))
                {
                    return false;
                }
                else
                {
                    path = Pathfinding.FindPath(character.CurrTile, Job.IsTileAtJobSite, Pathfinding.DefaultDistanceHeuristic(Job.tile));
                }
            }
            else
            {
                path = Pathfinding.FindPathToFurniture(character.CurrTile, Job.JobObjectType);
            }

            if (path == null || path.Count == 0)
            {
                // We can't find a furniture of the right type so abandon job, exit this state and stop processing
                AbandonJob();
                Finished();
                return true;
            }

            Job.tile = path.Last();
            if (Job.IsTileAtJobSite(character.CurrTile))
            {
                // We are already there!
                return false;
            }

            FSMLog(" - Need to walk to furniture");

            // Let's go there
            MoveState moveState = new MoveState(character, Job.IsTileAtJobSite, path, this);
            character.SetState(moveState);
            return true;
        }

        // Returns true if everything has been handled
        private bool NeedToFindMoreMaterial()
        {
            // Checks if material is already delivered to the job site
            if (Job.MaterialNeedsMet())
            {
                return false;
            }

            // Check if character is carrying something that is needed
            if (character.inventory != null)
            {
                int amountNeeded = Job.AmountDesiredOfInventoryType(character.inventory);

                // 0 means that they don't want that inventory and we need to dump!
                if (amountNeeded == 0)
                {
                    DumpExcessInventory();

                    // Restart method as a stop gap measure instead of hauling
                    // TODO: Change this when DumpExcessInventory() creates a hauling job
                    return NeedToFindMoreMaterial();
                }

                // We don't have enough, but we can carry enough
                if (amountNeeded > character.inventory.StackSize && character.inventory.MaxStackSize > amountNeeded)
                {
                    return FetchInventory(character.inventory.Type, amountNeeded - character.inventory.StackSize);
                }

                // We don't have enough, but we need to deliver what we have
                return false;
            }

            // At this point we know we don't carry anything, so we can grab anything required
            // TODO: GetFirstFulfillableInventoryRequirement doesn't do the stockpile and lock check.
            Inventory neededInventory = Job.GetFirstFulfillableInventoryRequirement();
            if (neededInventory == null)
            {
                AbandonJob();
                Finished();
                return true;
            }

            // MaxStackSize is how many items the job want in total and StackSize is how many are already delivered
            return FetchInventory(neededInventory.ObjectType, neededInventory.MaxStackSize - neededInventory.StackSize);
        }

        private bool TryPickupInventoryFromCurrentTile()
        {
            // Check that there is something on the tile, and that we are allowed to pick it up
            bool allowedToPickUp = job.canTakeFromStockpile || character.CurrTile.Furniture == null || character.CurrTile.Furniture.HasTypeTag("Storage") == false;
            if (character.CurrTile.Inventory != null && character.CurrTile.Inventory.Locked == false && allowedToPickUp)
            {
                Inventory tileInventory = character.CurrTile.Inventory;

                // If we carry something else, we ignore it
                if (character.inventory != null && character.inventory.Type != tileInventory.Type)
                {
                    return false;
                }

                int amountNeeded = job.AmountDesiredOfInventoryType(tileInventory.Type);
                if (amountNeeded > 0)
                {
                    int roomInInventory = character.inventory == null ? tileInventory.MaxStackSize : tileInventory.MaxStackSize - character.inventory.StackSize;
                    int amountToPickup = Math.Min(amountNeeded, roomInInventory);

                    FSMLog(" - Picking up resources");

                    World.Current.inventoryManager.PlaceInventory(
                        character,
                        tileInventory,
                        amountToPickup);

                    return true;
                }
            }

            return false;
        }

        private void DumpExcessInventory()
        {
            FSMLog(" - Dumping excess inventory");

            // TODO: Should haul this away
            character.inventory = null;
        }

        private bool FetchInventory(string objectType, int amount)
        {
            FSMLog(" - Need to fetch {0} {1}", amount, objectType);

            List<Tile> path = World.Current.inventoryManager.GetPathToClosestInventoryOfType(objectType, character.CurrTile, amount, Job.canTakeFromStockpile);
            if (path == null || path.Count == 0)
            {
                return false;
            }

            Tile destinationTile = path.Last();
            Pathfinding.GoalEvaluator isGoal = tile => destinationTile.Equals(tile);
            MoveState moveState = new MoveState(character, isGoal, path, this);
            character.SetState(moveState);
            return true;
        }

        private bool IsAtJobSite()
        {
            if (Job.IsTileAtJobSite(character.CurrTile))
            {
                return true;
            }

            FSMLog(" - IsAtJobSite: Must walk to job site");
            // Find our way to the jobsite
            List<Tile> path = Pathfinding.FindPath(character.CurrTile, Job.IsTileAtJobSite, Pathfinding.DefaultDistanceHeuristic(Job.tile));
            if (path == null || path.Count == 0)
            {
                AbandonJob();
                Finished();
                return false;
            }

            MoveState moveState = new MoveState(character, Job.IsTileAtJobSite, path, this);
            character.SetState(moveState);
            return false;
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

            // Job completed (if non-repeating) or was cancelled.
            stoppedJob.OnJobCompleted -= OnJobCompleted;
            stoppedJob.OnJobStopped -= OnJobStopped;
            Job.IsBeingWorked = false;

            if (Job != stoppedJob)
            {
                Debug.ULogErrorChannel("Character", "Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }

            Finished();
        }

        private void OnJobCompleted(Job finishedJob)
        {
            FSMLog(" - Job finished");

            finishedJob.OnJobCompleted -= OnJobCompleted;
            finishedJob.OnJobStopped -= OnJobStopped;

            if (Job != finishedJob)
            {
                Debug.ULogErrorChannel("Character", "Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }

            Finished();
        }
    }
}
