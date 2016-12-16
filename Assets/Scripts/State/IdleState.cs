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
using ProjectPorcupine.Pathfinding;
using Random = UnityEngine.Random;

namespace ProjectPorcupine.State
{
    [System.Diagnostics.DebuggerDisplay("Idle: ")]
    public class IdleState : State
    {
        private float totalIdleTime;
        private float timeSpentIdle;

        public IdleState(Character character, State nextState = null)
            : base("Idle", character, nextState)
        {
            timeSpentIdle = 0f;
            totalIdleTime = Random.Range(0.2f, 2.0f);
        }

        public override void Update(float deltaTime)
        {
            // Moves character in a random direction while idle.
            timeSpentIdle += deltaTime;
            if (timeSpentIdle >= totalIdleTime)
            {
                Tile[] neighbors = character.CurrTile.GetNeighbours();
                Tile endTile = neighbors[Random.Range(0, 4)];
                List<Tile> path = new List<Tile>() { character.CurrTile, endTile };

                if (endTile.MovementCost != 0)
                {
                    // See if the desired tile is walkable, then go there if we can.
                    character.SetState(new MoveState(character, Pathfinder.GoalTileEvaluator(endTile, true), path));
                }
                else
                {
                    // If the tile is unwalkable, just get a new state.
                    character.SetState(null);
                }
            }
        }
    }
}
