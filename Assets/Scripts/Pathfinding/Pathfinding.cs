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
using UnityEngine;

namespace ProjectPorcupine
{
    public static class Pathfinding
    {
        /// <summary>
        /// Delegate called to determine the distance from this tile to destination according to custom heuristics.
        /// </summary>
        /// <param name="tile">Tile to evalute.</param>
        public delegate float PathfindingHeuristic(Tile tile);

        /// <summary>
        /// Delegate called to determine if we've reached the goal.
        /// </summary>
        /// <param name="tile">Tile to evaluate.</param>
        public delegate bool GoalEvaluator(Tile tile);

        public static List<Tile> FindPath(Tile start, GoalEvaluator isGoal, PathfindingHeuristic costHeuristic)
        {
            Path_AStar resolver = new Path_AStar(World.Current, start, isGoal, costHeuristic);
            return resolver.GetList();
        }

        /// <summary>
        /// Finds the path to tile.
        /// </summary>
        /// <returns>The path to tile.</returns>
        /// <param name="start">Start tile.</param>
        /// <param name="end">Final tile.</param>
        /// <param name="adjacent">If set to <c>true</c> adjacent tiles can be targetted.</param>
        public static List<Tile> FindPathToTile(Tile start, Tile end, bool adjacent = false)
        {
            if (start == null || end == null)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalTileHeuristic(end, adjacent), ManhattanDistance(end));
            List<Tile> path = resolver.GetList();
            if (adjacent)
            {
                Debug.ULogChannel("Pathfinding", "Searched adjacent from: " + start.X + "," + start.Y + ", to: " + end.X + "," + end.Y + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            }
            else
            {
                Debug.ULogChannel("Pathfinding", "Searched from: " + start.X + "," + start.Y + ", to: " + end.X + "," + end.Y + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            }

            return path;
        }

        /// <summary>
        /// Finds the path to an inventory.
        /// </summary>
        /// <returns>The path to inventory.</returns>
        /// <param name="start">Start tile.</param>
        /// <param name="objectType">Object type of the inventory.</param>
        /// <param name="canTakeFromStockpile">If set to <c>true</c> can take from stockpile.</param>
        public static List<Tile> FindPathToInventory(Tile start, string objectType, bool canTakeFromStockpile = true)
        {
            if (start == null || objectType == null)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalInventoryHeuristic(objectType, canTakeFromStockpile), DijkstraDistance());
            List<Tile> path = resolver.GetList();
            Debug.ULogChannel("Pathfinding", "Searched from: " + start.X + "," + start.Y + ", for: " + objectType + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            return path;
        }

        /// <summary>
        /// Finds the path to furniture.
        /// </summary>
        /// <returns>The path to furniture.</returns>
        /// <param name="start">Start tile.</param>
        /// <param name="objectType">Object type of the furniture.</param>
        public static List<Tile> FindPathToFurniture(Tile start, string objectType)
        {
            if (start == null || objectType == null)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalFurnitureHeuristic(objectType), DijkstraDistance());
            List<Tile> path = resolver.GetList();
            Debug.ULogChannel("Pathfinding", "Searched from: " + start.X + "," + start.Y + ", for: " + objectType + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            return path;
        }

        /// <summary>
        /// Simple reusable goal heuristic. Will match for specific tiles or adjacent tiles.
        /// </summary>
        /// <param name="goalTile">Goal tile.</param>
        /// <param name="adjacent">If set to <c>true</c> adjacent tiles are matched.</param>
        public static GoalEvaluator GoalTileHeuristic(Tile goalTile, bool adjacent)
        {
            if (adjacent)
            {
                int minX = goalTile.X - 1;
                int maxX = goalTile.X + 1;
                int minY = goalTile.Y - 1;
                int maxY = goalTile.Y + 1;

                return tile => (
                    tile.X >= minX && tile.X <= maxX &&
                    tile.Y >= minY && tile.Y <= maxY);
            }
            else
            {
                return tile => goalTile == tile;
            }
        }

        public static PathfindingHeuristic DefaultDistanceHeuristic(Tile goalTile)
        {
            return ManhattanDistance(goalTile);
        }

        /// <summary>
        /// ManhattanDistance measurement.
        /// </summary>
        /// <param name="goalTile">Goal tile.</param>
        public static PathfindingHeuristic ManhattanDistance(Tile goalTile)
        {
            return tile => Mathf.Abs(tile.X - goalTile.X) + Mathf.Abs(tile.Y - goalTile.Y);
        }

        /// <summary>
        /// Evaluates if the goal is an inventory of the right type.
        /// </summary>
        /// <param name="objectType">Inventory's object type.</param>
        /// <param name="canTakeFromStockpile">If set to <c>true</c> can take from stockpile.</param>
        public static GoalEvaluator GoalInventoryHeuristic(string objectType, bool canTakeFromStockpile = true)
        {
            return current =>
            {
                // We don't have a POSITIONAL goal, we're just trying to find
                // some kind of inventory or furniture.  Have we reached it?
                if (current.Inventory != null && current.Inventory.Type == objectType && current.Inventory.Locked == false)
                {
                    // Type is correct and we are allowed to pick it up
                    if (canTakeFromStockpile || current.Furniture == null || current.Furniture.HasTypeTag("Storage") == false)
                    {
                        // Stockpile status is fine
                        return true;
                    }
                }

                return false;
            };
        }

        /// <summary>
        /// Evaluates if the goal is a furniture of the right type.
        /// </summary>
        /// <param name="objectType">Inventory's object type.</param>
        public static GoalEvaluator GoalFurnitureHeuristic(string objectType)
        {
            return current => current.Furniture != null && current.Furniture.Type == objectType;
        }

        /// <summary>
        /// Dijkstra's algorithm.
        /// </summary>
        public static PathfindingHeuristic DijkstraDistance()
        {
            return tile => 0f;
        }
    }
}