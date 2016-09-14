using System.Collections.Generic;
using System.Linq;
using ProjectPorcupine.Pathfinding;
using UnityEngine;

namespace ProjectPorcupine.State
{
    enum HaulAction {
        DumpMaterial,
        FindMaterial,
        PickupMaterial,
        DeliverMaterial,
        DropOffmaterial}
;

    public class HaulState: State
    {
        private Job Job;
        private bool noMoreMaterialFound = false;

        public HaulState(Character character, Job job, State nextState = null)
            : base("Haul", character, nextState)
        {
            Job = job;
        }

        public override void Update(float deltaTime)
        {
            List<Tile> path = null;
            HaulAction nextAction = NextAction();

            FSMLog(" - next action: {0}", nextAction);

            switch (nextAction)
            {
                case HaulAction.DumpMaterial:
                    character.SetState(new DumpState(character, this));
                    break;

                case HaulAction.FindMaterial:
                    // Find material somewhere
                    string[] inventoryTypes = character.inventory != null ?
                        new string[] { character.inventory.Type } :
                        Job.inventoryRequirements.Keys.ToArray();

                    path = World.Current.inventoryManager.GetPathToClosestInventoryOfType(inventoryTypes, character.CurrTile, Job.canTakeFromStockpile);
                    if (path != null && path.Count > 0)
                    {
                        character.SetState(new MoveState(character, Pathfinder.GoalTileEvaluator(path.Last(), false), path, this));
                    }
                    else if (character.inventory == null)
                    {
                        // The character has no inventory and can't find anything to haul.
                        FSMLog(" - Nothing to haul");
                        Finished();
                    }
                    else
                    {
                        noMoreMaterialFound = true;
                    }
                    break;

                case HaulAction.PickupMaterial:
                    Inventory tileInventory = character.CurrTile.Inventory;
                    int amountCarried = character.inventory != null ? character.inventory.StackSize : 0;
                    int amount = Mathf.Min(Job.AmountDesiredOfInventoryType(tileInventory.Type) - amountCarried, tileInventory.StackSize);
                    FSMLog(" - Picked up {0} {1}", amount, tileInventory.Type);
                    World.Current.inventoryManager.PlaceInventory(character, tileInventory, amount);
                    break;

                case HaulAction.DeliverMaterial:
                    path = Pathfinder.FindPath(character.CurrTile, Job.IsTileAtJobSite, Pathfinder.DefaultDistanceHeuristic(Job.tile));
                    if (path != null && path.Count > 0)
                    {
                        character.SetState(new MoveState(character, Pathfinder.GoalTileEvaluator(path.Last(), false), path, this));
                    }
                    else
                    {
                        character.InterruptState();
                    }
                    break;

                case HaulAction.DropOffmaterial:
                    FSMLog(" - Delivering {0} {1}", character.inventory.StackSize, character.inventory.Type);
                    World.Current.inventoryManager.PlaceInventory(Job, character);

                    // Ping the Job system
                    Job.DoWork(0);

                    Finished();
                    break;
            }
        }

        private HaulAction NextAction()
        {
            Inventory tileInventory = character.CurrTile.Inventory;
            bool jobWantsTileInventory = InventoryManager.InventoryCanBePickedUp(tileInventory, Job.canTakeFromStockpile) &&
                                         Job.AmountDesiredOfInventoryType(tileInventory) > 0;

            if (noMoreMaterialFound && character.inventory != null)
            {
                return Job.IsTileAtJobSite(character.CurrTile) ? HaulAction.DropOffmaterial : HaulAction.DeliverMaterial;
            }
            else if (character.inventory != null && Job.AmountDesiredOfInventoryType(character.inventory) == 0)
            {
                return HaulAction.DumpMaterial;
            }
            else if (character.inventory == null)
            {
                return jobWantsTileInventory ? HaulAction.PickupMaterial : HaulAction.FindMaterial;
            }
            else
            {
                int amountWanted = Job.AmountDesiredOfInventoryType(character.inventory);
                int currentlyCarrying = character.inventory.StackSize;
                int spaceAvailable = character.inventory.MaxStackSize - currentlyCarrying;

                // Already carrying it
                if (amountWanted <= currentlyCarrying || spaceAvailable == 0)
                {
                    return Job.IsTileAtJobSite(character.CurrTile) ? HaulAction.DropOffmaterial : HaulAction.DeliverMaterial;
                }
                // Can carry more and want more
                else
                {
                    return jobWantsTileInventory ? HaulAction.PickupMaterial : HaulAction.FindMaterial;
                }
            }
        }
    }
}

