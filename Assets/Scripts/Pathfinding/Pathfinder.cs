﻿#region License
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
        public delegate float PathfindingHeuristic(Tile tile);

        /// <summary>
        /// Delegate called to determine if we've reached the goal.
        /// </summary>
        public delegate bool GoalEvaluator(Tile tile);

        public static List<Tile> FindPath(Tile start, GoalEvaluator isGoal, PathfindingHeuristic costHeuristic)
        {
            Path_AStar resolver = new Path_AStar(World.Current, start, isGoal, costHeuristic);
            return resolver.GetList();
        }

        /// <summary>
        /// Finds the path to tile.
        /// </summary>
        public static List<Tile> FindPathToTile(Tile start, Tile end, bool adjacent = false)
        {
            if (start == null || end == null)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalTileEvaluator(end, adjacent), ManhattanDistance(end));
            List<Tile> path = resolver.GetList();

            DebugLogIf(path.Count > 0, "FindPathToTile{4} from: {0}, to: {1}, found {2} [Length: {3}, took: {5}ms]", start, end, path.LastOrDefault(), path.Count, adjacent ? " adjacent" : string.Empty, (int)(resolver.Duration * 1000));

            return path;
        }

        /// <summary>
        /// Finds the path to any inventory with type in <paramref name="types"/>
        /// </summary>
        public static List<Tile> FindPathToInventory(Tile start, string[] types, bool canTakeFromStockpile = true)
        {
            if (start == null || types == null || types.Length == 0)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalInventoryEvaluator(types, canTakeFromStockpile), DijkstraDistance());
            List<Tile> path = resolver.GetList();

            DebugLogIf(path.Count > 0, "FindPathToInventory from: {0}, to: {1}, found {2} [Length: {3}, took: {4}ms]", start, string.Join(",", types), path.LastOrDefault(), path.Count, (int)(resolver.Duration * 1000));

            return path;
        }

        /// <summary>
        /// Finds the path to an inventory of type <paramref name="type"/>.
        /// </summary>
        public static List<Tile> FindPathToInventory(Tile start, string type, bool canTakeFromStockpile = true)
        {
            if (start == null || type == null)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalInventoryEvaluator(type, canTakeFromStockpile), DijkstraDistance());
            List<Tile> path = resolver.GetList();

            DebugLogIf(path.Count > 0, "FindPathToInventory from: {0}, to: {1}, found {2} [Length: {3}, took: {4}ms]", start, type, path.LastOrDefault(), path.Count, (int)(resolver.Duration * 1000));

            return path;
        }

        /// <summary>
        /// Finds the path to furniture of type <paramref name="type"/>.
        /// </summary>
        public static List<Tile> FindPathToFurniture(Tile start, string type)
        {
            if (start == null || type == null)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalFurnitureEvaluator(type), DijkstraDistance());
            List<Tile> path = resolver.GetList();

            DebugLogIf(path.Count > 0, "FindPathToFurniture from: {0}, to: {1}, found {2} [Length: {3}, took: {4}ms]", start, type, path.LastOrDefault(), path.Count, (int)(resolver.Duration * 1000));

            return path;
        }

        /// <summary>
        /// Finds the path to a nearby tile where inventory of type <paramref name="type"/> can be dumped.
        /// </summary>
        public static List<Tile> FindPathToDumpInventory(Tile start, string type, int amount)
        {
            if (start == null || type == null || amount <= 0)
            {
                return null;
            }

            Path_AStar resolver = new Path_AStar(World.Current, start, GoalCanFitInventoryEvaluator(type, amount), DijkstraDistance());
            List<Tile> path = resolver.GetList();

            DebugLogIf(path.Count > 0, "FindPathToDumpInventory from: {0}, to: {1}, found {2} [Length: {3}, took: {4}ms]", start, type, path.LastOrDefault(), path.Count, (int)(resolver.Duration * 1000));

            return path;
        }

        /// <summary>
        /// A good choice for a quick route to the target.
        /// </summary>
        public static PathfindingHeuristic DefaultDistanceHeuristic(Tile goalTile)
        {
            return ManhattanDistance(goalTile);
        }

        /// <summary>
        /// ManhattanDistance measurement.
        /// </summary>
        public static PathfindingHeuristic ManhattanDistance(Tile goalTile)
        {
            return tile => Mathf.Abs(tile.X - goalTile.X) + Mathf.Abs(tile.Y - goalTile.Y);
        }

        /// <summary>
        /// Dijkstra's algorithm. Searches in an ever expanding circle from the start position.
        /// </summary>
        public static PathfindingHeuristic DijkstraDistance()
        {
            return tile => 0f;
        }

        /// <summary>
        /// Simple reusable goal heuristic. Will match for specific tiles or adjacent tiles.
        /// </summary>
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
                    tile.Y >= minY && tile.Y <= maxY &&
                    goalTile.IsClippingCorner(tile) == false);
            }
            else
            {
                return tile => goalTile == tile;
            }
        }

        /// <summary>
        /// Evaluates if the goal is an inventory of any of the types in <paramref name="types"/>.
        /// </summary>
        public static GoalEvaluator GoalInventoryEvaluator(string[] types, bool canTakeFromStockpile = true)
        {
            return tile => tile.Inventory != null && tile.Inventory.CanBePickedUp(canTakeFromStockpile) && types.Contains(tile.Inventory.Type);
        }

        /// <summary>
        /// Evaluates if the goal is an inventory of type <paramref name="type"/>.
        /// </summary>
        public static GoalEvaluator GoalInventoryEvaluator(string type, bool canTakeFromStockpile = true)
        {
            return tile => tile.Inventory != null && tile.Inventory.CanBePickedUp(canTakeFromStockpile) && type == tile.Inventory.Type;
        }

        /// <summary>
        /// Evaluates if the goal is a furniture of type <paramref name="type"/>.
        /// </summary>
        public static GoalEvaluator GoalFurnitureEvaluator(string type)
        {
            return current => current.Furniture != null && current.Furniture.Type == type;
        }

        /// <summary>
        /// Evaluates if it is an appropriate place to dump inventory of type <paramref name="type"/> and <paramref name="amount"/>.
        /// </summary>
        public static GoalEvaluator GoalCanFitInventoryEvaluator(string type, int amount)
        {
            return tile => tile.Type == TileType.Floor && (
                tile.Inventory == null ||
                (tile.Inventory.Type == type && (tile.Inventory.StackSize + amount) <= tile.Inventory.MaxStackSize));
        }

        [System.Diagnostics.Conditional("PATHFINDER_DEBUG_LOG")]
        private static void DebugLog(string message, params object[] par)
        {
            Debug.ULogChannel("Pathfinding", message, par);
        }

        [System.Diagnostics.Conditional("PATHFINDER_DEBUG_LOG")]
        private static void DebugLogIf(bool condition, string message, params object[] par)
        {
            if (condition)
            {
                DebugLog(message, par);
            }
        }
    }
}
