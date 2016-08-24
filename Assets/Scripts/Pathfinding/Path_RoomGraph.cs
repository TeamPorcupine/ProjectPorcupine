#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections.Generic;


public class Path_RoomGraph
{

    // This class constructs a simple path-finding compatible graph
    // of our world.  Each tile is a node. Each WALKABLE neighbour
    // from a tile is linked via an edge connection.

    public Dictionary<Tile, Path_Node<Room>> nodes;

    public Path_RoomGraph(World world)
    {

        Debug.ULogChannel("Path_RoomGraph","Entered Path_RoomGraph");

        // Loop through all rooms of the world
        // For each room, create a node

        nodes = new Dictionary<Room, Path_Node<Room>>();

        foreach (Room r in world.rooms)
        {
            Path_Node<Room> n = new Path_Node<Room>();
            n.data = r;
            nodes.Add(r, n);
        }

        Debug.ULogChannel("Path_RoomGraph", "Created " + nodes.Count + " nodes.");


        // Now loop through all nodes again
        // Create edges for neighbours


        foreach (Room r in nodes.Keys)
        {
            if (r.IsOutsideRoom == false)
            {
                GenerateEdgesByRoom(r);
            }
        }
        GenerateOutsideEdges(world.GetOutsideRoom)
    }

    void GenerateEdgesByRoom(Room r)
    {
        if (r.IsOutsideRoom)
        {
            // We don't want to loop over all the tiles in the outside room as this would be expensive
            return;
        }
        Path_Node<Room> n = nodes[r];
        List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();
        List<Path_Edge<Tile>> outsideEdges = new List<Path_Edge<Tile>>();

        List<Tile> exits = r.exits;

        foreach (Tile t in exits)
        {
            // Loop over the exits to find a different room
            Tile[] neighbours = t.GetNeighbours();
            foreach (Tile t2 in neighbours)
            {
                if (t2 != null && t2.Room != null)
                {
                    continue;
                }

                // We have found a room

                if (t2.Room != r)
                {
                    // We have found a different room to ourselves add an edge from us to them
                    Path_Edge<Tile> edge = new Path_Edge<Tile>();
                    edge.cost = 1;
                    edge.node = t2.Room;
                    edges.Add(edge);
                }
            }
        }
        AddOutsideEdges(outsideEdges);

        n.edges = edges.ToArray();
    }

    void GenerateOutsideEdges(Room outside)
    {   
        foreach (Room r in nodes.Keys)
        {
            if (r.IsOutsideRoom)
            {
                Path_Node<Room> outsideRoom = nodes[r];
                continue;
            }


            Path_Edge<Tile>[] edges = nodes[r].edges;
            foreach (Path_Edge<Tile> edge in edges)
            {
                if (edge.node.IsOutsideRoom)
                {
                    // Edge connects to the outside room
                    Path_Edge<Tile> outsieEdge = new Path_Edge<Tile>();
                    outsieEdge.cost = 1;
                    outsieEdge.node = r;
                    edges.Add(outsieEdge);
                }
            }

        }
        outsideRoom.edges = edges.ToArray();
    }

    private void AddOutsideEdges(List<Path_Edge<Tile>> outsideEdges)
    {
        Path_Node<Room> outside = nodes[r];
        // Current outside edges
        // Path_Edge[] current_outside_edges = outside.edges;

        int[] current_outside_edges;
        outside.edges = outsideEdges.ToArray();
    }


    public void RegenerateGraph()
    {
        foreach (Room r in nodes.Keys)
        {
            GenerateEdgesByRoom(r);
        }
    }
}
