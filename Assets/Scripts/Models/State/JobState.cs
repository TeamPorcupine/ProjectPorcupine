using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectPorcupine.State
{
    [System.Diagnostics.DebuggerDisplay("JobState: {job}")]
    public class JobState : State
    {
        public Job job { get; private set; }

        public JobState(Character character, Job job, State nextState = null)
            : base("Job", character, nextState)
        {
            this.job = job;

            job.OnJobStopped += OnJobStopped;
            job.IsBeingWorked = true;

            FSMLog("created {0}", job.GetName());
        }

        public override void Update(float deltaTime)
        {
            if (IsAtJobSite())
            {
                DropOffInventory();

                job.DoWork(deltaTime);
                return;
            }

            if (job.IsNeed && NeedToWalkToFurniture())
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
            if (job != null)
            {
                AbandonJob();
            }

            base.Interrupt();
        }

        private void DropOffInventory()
        {
            if (character.inventory != null)
            {
                if (job.AmountDesiredOfInventoryType(character.inventory.Type) > 0)
                {
                    FSMLog(" - Dropping off inventory");
                    World.Current.inventoryManager.PlaceInventory(job, character.inventory);
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
            if (job.tile != null)
            {
                if (job.IsTileAtJobSite(character.CurrTile))
                {
                    return false;
                }
                else
                {
                    path = Pathfinding.FindPath(character.CurrTile, job.IsTileAtJobSite, Pathfinding.DefaultDistanceHeuristic(job.tile));
                }
            }
            else
            {
                path = Pathfinding.FindPathToFurniture(character.CurrTile, job.JobObjectType);    
            }

            if (path == null || path.Count == 0)
            {
                // We can't find a furniture of the right type so abandon job, exit this state and stop processing
                AbandonJob();
                Finished();
                return true;
            }

            job.tile = path.Last();
            if (job.IsTileAtJobSite(character.CurrTile))
            {
                // We are already there!
                return false;
            }

            FSMLog(" - Need to walk to furniture");

            // Let's go there
            MoveState moveState = new MoveState(character, job.IsTileAtJobSite, path, this);
            character.SetState(moveState);
            return true;
        }

        // Returns true if everything has been handled
        private bool NeedToFindMoreMaterial()
        {
            // Checks if material is already delivered to the job site
            if (job.MaterialNeedsMet())
            {
                return false;
            }

            // Check if character is carrying something that is needed
            if (character.inventory != null)
            {
                int amountNeeded = job.AmountDesiredOfInventoryType(character.inventory);
                // 0 means that they don't want that inventory and we need to dump!
                if (amountNeeded == 0)
                {
                    DumpExcessInventory();
                    // Restart method as a stop gap measure instead of hauling
                    return NeedToFindMoreMaterial();
                }

                // We don't have enough, but we can carry enough
                if (amountNeeded > character.inventory.StackSize && character.inventory.MaxStackSize >= amountNeeded)
                {
                    return FetchInventory(character.inventory.Type, amountNeeded - character.inventory.StackSize);
                }

                // We don't have enough, but we need to deliver what we have
                return false;
            }

            // At this point we know we don't carry anything, so we can grab anything required
            // TODO: GetFirstFulfillableInventoryRequirement doesn't do the stockpile and lock check.
            Inventory neededInventory = job.GetFirstFulfillableInventoryRequirement();
            if (neededInventory == null)
            {
                AbandonJob();
                Finished();
                return true;
            }

            return FetchInventory(neededInventory.Type, neededInventory.StackSize);
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
            List<Tile> path = World.Current.inventoryManager.GetPathToClosestInventoryOfType(objectType, character.CurrTile, amount, job.canTakeFromStockpile);
            if (path == null || path.Count == 0)
            {
                AbandonJob();
                Finished();
                return true;
            }

            FSMLog(" - Need to fetch {0} {1}", amount, objectType);

            Tile destinationTile = path.Last();
            Pathfinding.GoalEvaluator isGoal = tile => destinationTile.Equals(tile);
            MoveState moveState = new MoveState(character, isGoal, path, this);
            character.SetState(moveState);
            return true;
        }

        private bool IsAtJobSite()
        {
            if (job.IsTileAtJobSite(character.CurrTile))
            {
                return true;
            }

            // Find our way to the jobsite
            List<Tile> path = Pathfinding.FindPath(character.CurrTile, job.IsTileAtJobSite, Pathfinding.DefaultDistanceHeuristic(job.tile));
            if (path == null || path.Count == 0)
            {
                AbandonJob();
                Finished();
                return false;
            }

            MoveState moveState = new MoveState(character, job.IsTileAtJobSite, path, this);
            character.SetState(moveState);
            return false;
        }

        private void AbandonJob()
        {
            FSMLog(" - Job abandoned!");
            Debug.ULogChannel("Character", character.GetName() + " abandoned their job.");

            job.OnJobStopped -= OnJobStopped;
            job.IsBeingWorked = false;

            // Tell anyone else who cares that it was cancelled
            job.CancelJob();

            if (job.IsNeed)
            {
                return;
            }

            // Drops the priority a level.
            job.DropPriority();

            // If the job gets abandoned because of pathing issues or something else, just return it to the queue
            World.Current.jobQueue.Enqueue(job);
        }

        private void OnJobStopped(Job stoppedJob)
        {
            FSMLog(" - Job stopped!");
            // Job completed (if non-repeating) or was cancelled.
            stoppedJob.OnJobStopped -= OnJobStopped;
            job.IsBeingWorked = false;

            if (job != stoppedJob)
            {
                Debug.ULogErrorChannel("Character", "Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }

            Finished();
        }
    }
}

