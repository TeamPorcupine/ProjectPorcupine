#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using System.Linq;
using ProjectPorcupine.Pathfinding;
using UnityEngine;

namespace ProjectPorcupine.State
{
    public class MoveState : State
    {
        private Pathfinder.GoalEvaluator hasReachedDestination;
        private List<Tile> path;

        private float movementPercentage;
        private float distToTravel;

        private Tile nextTile;

        public MoveState(Character character, Pathfinder.GoalEvaluator goalEvaluator, List<Tile> path, State nextState = null)
            : base("Move", character, nextState)
        {
            hasReachedDestination = goalEvaluator;
            this.path = path;

            DebugLog("created with path length: {0}", path.Count);
        }

        public override void Update(float deltaTime)
        {
            if (nextTile.IsEnterable() == Enterability.Soon)
            {
                // We can't enter the NOW, but we should be able to in the
                // future. This is likely a DOOR.
                // So we DON'T bail on our movement/path, but we do return
                // now and don't actually process the movement.
                return;
            }

            // How much distance can be travel this Update?
            float distThisFrame = character.MovementSpeed / nextTile.MovementCost * deltaTime;

            // How much is that in terms of percentage to our destination?
            float percThisFrame = distThisFrame / distToTravel;

            // Add that to overall percentage travelled.
            movementPercentage += percThisFrame;

            if (movementPercentage >= 1f)
            {
                // We have reached the next tile
                character.CurrTile = nextTile;
                character.CurrTile.OnEnter();

                float overshotMovement = Mathf.Clamp01(movementPercentage - 1f);
                movementPercentage = 0f;

                // Arrived at the destination or run out of path.
                if (hasReachedDestination(character.CurrTile) || path.Count == 0)
                {
                    Finished();
                    return;
                }

                AdvanceNextTile();

                distToTravel = Mathf.Sqrt(
                    Mathf.Pow(character.CurrTile.X - nextTile.X, 2) +
                    Mathf.Pow(character.CurrTile.Y - nextTile.Y, 2) +
                    Mathf.Pow(character.CurrTile.Z - nextTile.Z, 2));

                if (nextTile.IsEnterable() == Enterability.Yes)
                {
                    movementPercentage = overshotMovement;
                }
                else if (nextTile.IsEnterable() == Enterability.Never)
                {
                    // Most likely a wall got built, so we just need to reset our pathfinding information.
                    // FIXME: Ideally, when a wall gets spawned, we should invalidate our path immediately,
                    //            so that we don't waste a bunch of time walking towards a dead end.
                    //            To save CPU, maybe we can only check every so often?
                    //            Or maybe we should register a callback to the OnTileChanged event?
                    // UnityDebugger.Debugger.LogErrorChannel("FIXME", "A character was trying to enter an unwalkable tile.");

                    // Should the character show that he is surprised to find a wall?
                    Finished();
                    return;
                }
            }

            character.TileOffset = new Vector3(
                (nextTile.X - character.CurrTile.X) * movementPercentage,
                (nextTile.Y - character.CurrTile.Y) * movementPercentage,
                (nextTile.Z - character.CurrTile.Z) * movementPercentage);
        }

        public override void Enter()
        {
            base.Enter();

            character.IsWalking = true;

            if (character.IsSelected)
            {
                VisualPath.Instance.SetVisualPoints(character.name, new List<Tile>(path));
            }

            if (path == null || path.Count == 0)
            {
                Finished();
                return;
            }

            // The starting tile might be included, so we need to get rid of it
            while (path[0].Equals(character.CurrTile))
            {
                path.RemoveAt(0);

                if (path.Count == 0)
                {
                    DebugLog(" - Ran out of path to walk");

                    // We've either arrived, or we need to find a new path to the target
                    Finished();
                    return;
                }
            }

            AdvanceNextTile();

            distToTravel = Mathf.Sqrt(
                Mathf.Pow(character.CurrTile.X - nextTile.X, 2) +
                Mathf.Pow(character.CurrTile.Y - nextTile.Y, 2));
        }

        public override void Exit()
        {
            base.Exit();

            character.IsWalking = false;

            VisualPath.Instance.RemoveVisualPoints(character.name);
        }

        public override void Interrupt()
        {
            if (path != null)
            {
                Tile goal = path.Last();
                if (goal.Inventory != null)
                {
                    goal.Inventory.ReleaseClaim(character);
                }
            }

            base.Interrupt();
        }

        private void AdvanceNextTile()
        {
            nextTile = path[0];
            path.RemoveAt(0);

            character.FaceTile(nextTile);
        }
    }
}
