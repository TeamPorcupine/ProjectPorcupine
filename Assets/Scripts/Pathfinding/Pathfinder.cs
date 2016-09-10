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

namespace ProjectPorcupine.Pathfinding
{
    public static class Pathfinder
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

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalTileEvaluator(end, adjacent), ManhattanDistance(end));
            List<Tile> path = resolver.GetList();
            if (adjacent)
            {
                DebugLogIf(path.Count > 0, "FindPathToTile adjacent from: {0, to: {1}, found {2} [Length: {3}", start, end, path.Last(), path.Count);
//                DebugLogIf(path.Count > 0, "Searched adjacent from: " + start.X + "," + start.Y + ", to: " + end.X + "," + end.Y + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            }
            else
            {
                DebugLogIf(path.Count > 0, "FindPathToTile from: " + start.X + "," + start.Y + ", to: " + end.X + "," + end.Y + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            }
            DebugLogIf(path == null, "Failed to find path to tile {0}", start);

            return path;
        }

        public static List<Tile> FindPathToInventory(Tile start, string[] objectTypes, bool canTakeFromStockpile = true)
        {
            if (start == null || objectTypes == null || objectTypes.Length == 0)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalInventoryEvaluator(objectTypes, canTakeFromStockpile), DijkstraDistance());
            List<Tile> path = resolver.GetList();
            if (path.Count > 0)
                DebugLog("FindPathToInventory from: " + start.X + "," + start.Y + ", for: " + string.Join(",", objectTypes) + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            DebugLogIf(path == null, "Failed to find path to inventory of type {0}", string.Join(",", objectTypes));
            return path;
        }

        /// <summary>
        /// Finds the path to an inventory.
        /// </summary>
        public static List<Tile> FindPathToInventory(Tile start, string objectType, bool canTakeFromStockpile = true)
        {
            if (start == null || objectType == null)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalInventoryEvaluator(objectType, canTakeFromStockpile), DijkstraDistance());
            List<Tile> path = resolver.GetList();
            if (path.Count > 0)
                DebugLog("FindPathToInventory from: " + start.X + "," + start.Y + ", for: " + objectType + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            DebugLogIf(path == null, "Failed to find path to inventory of type {0}", objectType);
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

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalFurnitureEvaluator(objectType), DijkstraDistance());
            List<Tile> path = resolver.GetList();
            DebugLogIf(path.Count > 0, "FindPathToFurniture from: " + start.X + "," + start.Y + ", for: " + objectType + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            DebugLogIf(path == null, "Failed to find path to furniture of type {0}", objectType);
            return path;
        }

        public static List<Tile> FindPathToDumpInventory(Tile start, string objectType, int amount)
        {
            if (start == null || objectType == null || amount <= 0)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalCanFitInventoryEvaluator(objectType, amount), DijkstraDistance());
            List<Tile> path = resolver.GetList();
            DebugLogIf(path.Count > 0, "FindPathToDumpInventory from: " + start.X + "," + start.Y + ", for: " + objectType + " found: " + path.Last().X + "," + path.Last().Y + " [Length: " + path.Count + "]");
            DebugLogIf(path == null, "Failed to find path to furniture of type {0}", objectType);
            return path;
        }

        /// <summary>
        /// Simple reusable goal heuristic. Will match for specific tiles or adjacent tiles.
        /// </summary>
        /// <param name="goalTile">Goal tile.</param>
        /// <param name="adjacent">If set to <c>true</c> adjacent tiles are matched.</param>
        public static GoalEvaluator GoalTileEvaluator(Tile goalTile, bool adjacent)
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

        public static GoalEvaluator GoalInventoryEvaluator(string[] objectTypes, bool canTakeFromStockpile = true)
        {
            return tile => InventoryManager.InventoryCanBePickedUp(tile.Inventory, canTakeFromStockpile) && objectTypes.Contains(tile.Inventory.Type);
        }

        /// <summary>
        /// Evaluates if the goal is an inventory of the right type.
        /// </summary>
        /// <param name="objectType">Inventory's object type.</param>
        /// <param name="canTakeFromStockpile">If set to <c>true</c> can take from stockpile.</param>
        public static GoalEvaluator GoalInventoryEvaluator(string objectType, bool canTakeFromStockpile = true)
        {
            return tile => InventoryManager.InventoryCanBePickedUp(tile.Inventory, canTakeFromStockpile) && objectType == tile.Inventory.Type;
        }

        /// <summary>
        /// Evaluates if the goal is a furniture of the right type.
        /// </summary>
        /// <param name="objectType">Inventory's object type.</param>
        public static GoalEvaluator GoalFurnitureEvaluator(string objectType)
        {
            return current => current.Furniture != null && current.Furniture.Type == objectType;
        }

        /// <summary>
        /// Evaluates if it is an appropriate place to dump objectType of amount
        /// </summary>
        public static GoalEvaluator GoalCanFitInventoryEvaluator(string objectType, int amount)
        {
            return tile => tile.Type == TileType.Floor && (
                tile.Inventory == null ||
                tile.Inventory.Type == objectType && (tile.Inventory.StackSize + amount) <= tile.Inventory.MaxStackSize);
        }

        /// <summary>
        /// Dijkstra's algorithm.
        /// </summary>
        public static PathfindingHeuristic DijkstraDistance()
        {
            return tile => 0f;
        }

        private static void DebugLog(string message, params object[] par)
        {
            Debug.ULogChannel("Pathfinding", message, par);
        }

        private static void DebugLogIf(bool condition, string message, params object[] par)
        {
            if (condition)
            {
                DebugLog(message, par);
            }
        }
    }
}
