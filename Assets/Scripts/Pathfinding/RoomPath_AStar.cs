#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
using ProjectPorcupine.Rooms;


#endregion
using System.Collections.Generic;
using System.Linq;
using ProjectPorcupine.Pathfinding;
using UnityEngine;

public class RoomPath_AStar
{
    /// This will contain the built path.
    private Queue<Room> path;

    public RoomPath_AStar(Queue<Room> path)
    {
        if (path == null || !path.Any())
        {
            Debug.ULogWarningChannel("Path_AStar", "Created path with no tiles, is this intended?");
        }

        this.path = path;
    }

    public RoomPath_AStar(World world, Room roomStart, Pathfinder.RoomGoalEvaluator isGoal, Pathfinder.RoomPathfindingHeuristic costEstimate)
    {
        float startTime = Time.realtimeSinceStartup;

        // Set path to empty Queue so that there always is something to check count on
        path = new Queue<Room>();

        // if tileEnd is null, then we are simply scanning for the nearest objectType.
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
        Dictionary<Room, Path_Node<Room>> nodes = world.roomGraph.nodes;

        // Make sure our start/end tiles are in the list of nodes!
        if (nodes.ContainsKey(roomStart) == false)
        {
            Debug.ULogErrorChannel("Path_AStar", "The starting tile isn't in the list of nodes!");

            return;
        }

        Path_Node<Room> start = nodes[roomStart];

        /*
         * Mostly following this pseusocode:
         * https://en.wikipedia.org/wiki/A*_search_algorithm
         */
        HashSet<Path_Node<Room>> closedSet = new HashSet<Path_Node<Room>>();

        /*
         * List<Path_Node<Tile>> openSet = new List<Path_Node<Tile>>();
         *        openSet.Add( start );
         */

        PathfindingPriorityQueue<Path_Node<Room>> openSet = new PathfindingPriorityQueue<Path_Node<Room>>();
        openSet.Enqueue(start, 0);

        Dictionary<Path_Node<Room>, Path_Node<Room>> came_From = new Dictionary<Path_Node<Room>, Path_Node<Room>>();

        Dictionary<Path_Node<Room>, float> g_score = new Dictionary<Path_Node<Room>, float>();
        g_score[start] = 0;

        Dictionary<Path_Node<Room>, float> f_score = new Dictionary<Path_Node<Room>, float>();
        f_score[start] = costEstimate(start.data);

        while (openSet.Count > 0)
        {
            Path_Node<Room> current = openSet.Dequeue();

            // Check to see if we are there.
            if (isGoal(current.data))
            {
                Duration = Time.realtimeSinceStartup - startTime;
                Reconstruct_path(came_From, current);
                return;
            }

            closedSet.Add(current);

            foreach (Path_Edge<Room> edge_neighbor in current.edges)
            {
                Path_Node<Room> neighbor = edge_neighbor.node;

                if (closedSet.Contains(neighbor))
                {
                    continue; // ignore this already completed neighbor
                }

                float pathfinding_cost_to_neighbor = neighbor.data.TileCount;

                float tentative_g_score = g_score[current] + pathfinding_cost_to_neighbor;

                if (openSet.Contains(neighbor) && tentative_g_score >= g_score[neighbor])
                {
                    continue;
                }

                came_From[neighbor] = current;
                g_score[neighbor] = tentative_g_score;
                f_score[neighbor] = g_score[neighbor] + costEstimate(neighbor.data);

                openSet.EnqueueOrUpdate(neighbor, f_score[neighbor]);
            } // foreach neighbour
        } // while

        // If we reached here, it means that we've burned through the entire
        // openSet without ever reaching a point where current == goal.
        // This happens when there is no path from start to goal
        // (so there's a wall or missing floor or something).

        // We don't have a failure state, maybe? It's just that the
        // path list will be null.
        Duration = Time.realtimeSinceStartup - startTime;
    }

    /// Contains the time it took to find the path
    public float Duration { get; private set; }

    public Room Dequeue()
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

    public Room EndRoom()
    {
        if (path == null || path.Count == 0)
        {
            Debug.ULogChannel("Path_AStar", "Path is null or empty.");
            return null;
        }

        return path.Last();
    }

    public IEnumerable<Room> Reverse()
    {
        return path == null ? null : path.Reverse();
    }

    public List<Room> GetList()
    {
        return path.ToList();
    }

    public Queue<Room> GetQueue()
    {
        return path;
    }

    private float Heuristic_cost_estimate(Path_Node<Room> a, Path_Node<Room> b)
    {
        if (b == null)
        {
            // We have no fixed destination (i.e. probably looking for an inventory item)
            // so just return 0 for the cost estimate (i.e. all directions as just as good)
            return 0f;
        }

        // How do we even measure the distance between the rooms?
        // Center tile would work, but calculating a rooms center tile will be expensive, and will need to be redone when room changes
        // We could approximate it, finding lowest and highest x and y of tiles and average, but that seems pretty expensive too.
        // For now, let's just always return 0.
        return 0f;
//        return Mathf.Sqrt(
//            Mathf.Pow(a.data.X - b.data.X, 2) +
//            Mathf.Pow(a.data.Y - b.data.Y, 2) +
//            Mathf.Pow(a.data.Z - b.data.Z, 2));
    }


    // FOr now we're not using this, but let's leave it here while I think on it.
//    private float Dist_between(Path_Node<Room> a, Path_Node<Room> b)
//    {
//        // We can make assumptions because we know we're working
//        // on a grid at this point.
//
//        // Hori/Vert neighbours have a distance of 1
//        if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1 && a.data.Z == b.data.Z)
//        {
//            return 1f;
//        }
//
//        // Diag neighbours have a distance of 1.41421356237
//        if (Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1 && a.data.Z == b.data.Z)
//        {
//            return 1.41421356237f;
//        }
//
//        // Up/Down neighbors have a distance of 1
//        if (a.data.X == b.data.X && a.data.Y == b.data.Y && Mathf.Abs(a.data.Z - b.data.Z) == 1)
//        {
//            return 1f;
//        }
//
//        // Otherwise, do the actual math.
//        return Mathf.Sqrt(
//            Mathf.Pow(a.data.X - b.data.X, 2) +
//            Mathf.Pow(a.data.Y - b.data.Y, 2) +
//            Mathf.Pow(a.data.Z - b.data.Z, 2));
//    }

    private void Reconstruct_path(
        Dictionary<Path_Node<Room>, Path_Node<Room>> came_From,
        Path_Node<Room> current)
    {
        // So at this point, current IS the goal.
        // So what we want to do is walk backwards through the Came_From
        // map, until we reach the "end" of that map...which will be
        // our starting node!
        Queue<Room> total_path = new Queue<Room>();
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
        path = new Queue<Room>(total_path.Reverse());
    }
}
