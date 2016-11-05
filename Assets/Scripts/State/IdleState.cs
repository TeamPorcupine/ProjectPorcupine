#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using ProjectPorcupine.Pathfinding;
using System;
using System.Collections.Generic;
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
                Tile startTile = character.CurrTile;
                Tile endTile = null;
                List<Tile> path = null;

                // Get a random Int 0,1,2,3 for representing the 4 directions.
                int direction = Int32.Parse(Math.Truncate(Random.value * 4).ToString());

                switch (direction)
                {
                    case 0:
                        endTile = World.Current.GetTileAt(startTile.X + 1, startTile.Y, startTile.Z);
                        break;
                    case 1:
                        endTile = World.Current.GetTileAt(startTile.X, startTile.Y + 1, startTile.Z);
                        break;
                    case 2:
                        endTile = World.Current.GetTileAt(startTile.X - 1, startTile.Y, startTile.Z);
                        break;
                    case 3:
                        endTile = World.Current.GetTileAt(startTile.X, startTile.Y - 1, startTile.Z);
                        break;
                }

                if (endTile != null)
                {
                  path = Pathfinder.FindPathToTile(startTile, endTile);
                }

            Pathfinder.GoalEvaluator GTE = Pathfinder.GoalTileEvaluator(endTile, true);

            timeSpentIdle += deltaTime;
            if (timeSpentIdle >= totalIdleTime)
            {
                // We are done. Execute move then look for work.
              character.SetState(new MoveState(character, GTE, path));
            }
        }
    }
}