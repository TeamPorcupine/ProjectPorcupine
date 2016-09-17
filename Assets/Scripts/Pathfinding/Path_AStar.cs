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
using Priority_Queue;
using UnityEngine;

public class Path_AStar
{
    private Queue<Tile> path;

    public Path_AStar(Queue<Tile> path)
    {
        if (path == null || !path.Any())
        {
            Debug.ULogWarningChannel("Path_AStar", "Created path with no tiles, is this intended?");
        }

        this.path = path;
    }

    public Path_AStar(World world, Tile tileStart, Tile tileEnd, string type = null, int desiredAmount = 0, bool canTakeFromStockpile = false, bool lookingForFurn = false, bool adjacentOkay = false)
    {
        // if tileEnd is null, then we are simply scanning for the nearest Type.
        // We can do this by ignoring the heuristic component of AStar, which basically
        // just turns this into an over-engineered Dijkstra's algo

        // Check to see if we have a valid tile graph
        if (world.tileGraph == null)
        {
            world.tileGraph = new Path_TileGraph(world);
        }

        // Check to see if we have a valid tile graph
        if (world.roomGraph == null)
        {
            world.roomGraph = new Path_RoomGraph(world);
        }

        // A dictionary of all valid, walkable nodes.
        Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;

        // Make sure our start/end tiles are in the list of nodes!
        if (nodes.ContainsKey(tileStart) == false)
        {
            Debug.ULogErrorChannel("Path_AStar", "The starting tile isn't in the list of nodes!");

            return;
        }

        Path_Node<Tile> start = nodes[tileStart];

        // if tileEnd is null, then we are simply looking for an inventory object
        // so just set goal to null.
        Path_Node<Tile> goal = null;
        List<Path_Node<Tile>> neighbors = new List<Path_Node<Tile>>();
        if (tileEnd != null)
        {
            if (nodes.ContainsKey(tileEnd) == false)
            {
                Debug.ULogErrorChannel("Path_AStar", "The ending tile isn't in the list of nodes!");
                return;
            }

            if (adjacentOkay)
            {
                foreach (Tile tile in tileEnd.GetNeighbours())
                {
                    neighbors.Add(nodes[tile]);
                }
            }

            goal = nodes[tileEnd];
        }

        /*
         * Mostly following this pseusocode:
         * https://en.wikipedia.org/wiki/A*_search_algorithm
         */
        HashSet<Path_Node<Tile>> closedSet = new HashSet<Path_Node<Tile>>();

        /*
         * List<Path_Node<Tile>> openSet = new List<Path_Node<Tile>>();
         *        openSet.Add( start );
         */

        PathfindingPriorityQueue<Path_Node<Tile>> openSet = new PathfindingPriorityQueue<Path_Node<Tile>>();
        openSet.Enqueue(start, 0);

        Dictionary<Path_Node<Tile>, Path_Node<Tile>> came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();
        g_score[start] = 0;

        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        f_score[start] = Heuristic_cost_estimate(start, goal);

        while (openSet.Count > 0)
        {
            Path_Node<Tile> current = openSet.Dequeue();

            // If we have a POSITIONAL goal, check to see if we are there.
            if (goal != null)
            {
                if (current == goal || (adjacentOkay && neighbors.Contains(current)))
                {
                    Reconstruct_path(came_From, current);
                    return;
                }
            }
            else
            {
                // We don't have a POSITIONAL goal, we're just trying to find
                // some kind of inventory or furniture.  Have we reached it?
                if (current.data.Inventory != null && current.data.Inventory.Type == type && lookingForFurn == false && current.data.Inventory.Locked == false)
                {
                    // Type is correct and we are allowed to pick it up
                    if (canTakeFromStockpile || current.data.Furniture == null || current.data.Furniture.HasTypeTag("Storage") == false)
                    {
                        // Stockpile status is fine
                        Reconstruct_path(came_From, current);
                        return;
                    }
                }

                if (current.data.Furniture != null && current.data.Furniture.Type == type && lookingForFurn)
                {
                    // Type is correct
                    Reconstruct_path(came_From, current);
                    return;
                }
            }

            closedSet.Add(current);

            foreach (Path_Edge<Tile> edge_neighbor in current.edges)
            {
                Path_Node<Tile> neighbor = edge_neighbor.node;

                if (closedSet.Contains(neighbor))
                {
                    continue; // ignore this already completed neighbor
                }

                float pathfinding_cost_to_neighbor = neighbor.data.PathfindingCost * Dist_between(current, neighbor);

                float tentative_g_score = g_score[current] + pathfinding_cost_to_neighbor;

                if (openSet.Contains(neighbor) && tentative_g_score >= g_score[neighbor])
                {
                    continue;
                }

                came_From[neighbor] = current;
                g_score[neighbor] = tentative_g_score;
                f_score[neighbor] = g_score[neighbor] + Heuristic_cost_estimate(neighbor, goal);

                openSet.EnqueueOrUpdate(neighbor, f_score[neighbor]);
            } // foreach neighbour
        } // while

        // If we reached here, it means that we've burned through the entire
        // openSet without ever reaching a point where current == goal.
        // This happens when there is no path from start to goal
        // (so there's a wall or missing floor or something).

        // We don't have a failure state, maybe? It's just that the
        // path list will be null.
    }

    public Tile Dequeue()
    {
        if (path == null)
        {
            Debug.ULogErrorChannel("Path_AStar", "Attempting to dequeue from an null path.");
            return null;
        }

        if (path.Count <= 0)
        {
            Debug.ULogErrorChannel("Path_AStar", "Path queue is zero or less elements long.");
            return null;
        }

        return path.Dequeue();
    }

    public int Length()
    {
        if (path == null)
        {
            return 0;
        }

        return path.Count;
    }

    public Tile EndTile()
    {
        if (path == null || path.Count == 0)
        {
            Debug.ULogChannel("Path_AStar", "Path is null or empty.");
            return null;
        }

        return path.Last();
    }

    public IEnumerable<Tile> Reverse()
    {
        return path == null ? null : path.Reverse();
    }

    public List<Tile> GetList()
    {
        return path.ToList();
    }

    private float Heuristic_cost_estimate(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        if (b == null)
        {
            // We have no fixed destination (i.e. probably looking for an inventory item)
            // so just return 0 for the cost estimate (i.e. all directions as just as good)
            return 0f;
        }

        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2));
    }

    private float Dist_between(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        // We can make assumptions because we know we're working
        // on a grid at this point.

        // Hori/Vert neighbours have a distance of 1
        if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1f;
        }

        // Diag neighbours have a distance of 1.41421356237
        if (Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1.41421356237f;
        }

        // Otherwise, do the actual math.
        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2));
    }

    private void Reconstruct_path(
        Dictionary<Path_Node<Tile>, Path_Node<Tile>> came_From,
        Path_Node<Tile> current)
    {
        // So at this point, current IS the goal.
        // So what we want to do is walk backwards through the Came_From
        // map, until we reach the "end" of that map...which will be
        // our starting node!
        Queue<Tile> total_path = new Queue<Tile>();
        total_path.Enqueue(current.data); // This "final" step is the path is the goal!

        while (came_From.ContainsKey(current))
        {
            /*    Came_From is a map, where the
            *    key => value relation is real saying
            *    some_node => we_got_there_from_this_node
            */

            current = came_From[current];
            total_path.Enqueue(current.data);
        }

        // At this point, total_path is a queue that is running
        // backwards from the END tile to the START tile, so let's reverse it.
        path = new Queue<Tile>(total_path.Reverse());
    }
}
