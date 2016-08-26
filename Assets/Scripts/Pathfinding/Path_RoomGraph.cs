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

    public Dictionary<Room, Path_Node<Room>> nodes;

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
            if (r.IsOutsideRoom() == false)
            {
                GenerateEdgesByRoom(r);
            }
        }
        GenerateEdgesOutside();

        foreach (Room r in nodes.Keys)
        {
            Debug.ULogChannel("Path_RoomGraph", "Room " + r.ID + " has edges to:");
            Debug.ULogChannel("Path_RoomGraph", "Room " + r.ID + " has " + nodes[r].edges.Length + " edges");
            foreach (Path_Edge<Room> edge in nodes[r].edges)
            {
                Debug.ULogChannel("Path_RoomGraph", "edge " + edge.node.data.ID);
            }

        }

    }

    void GenerateEdgesByRoom(Room r)
    {
        if (r.IsOutsideRoom())
        {
            // We don't want to loop over all the tiles in the outside room as this would be expensive
            return;
        }
        Path_Node<Room> n = nodes[r];
        List<Path_Edge<Room>> edges = new List<Path_Edge<Room>>();


        List<Tile> exits = r.exits;

        foreach (Tile t in exits)
        {
            // Loop over the exits to find a different room
            Tile[] neighbours = t.GetNeighbours();
            foreach (Tile t2 in neighbours)
            {
                if (t2 == null || t2.Room == null)
                {
                    continue;
                }

                // We have found a room

                if (t2.Room != r)
                {
                    // We have found a different room to ourselves add an edge from us to them
                    Path_Edge<Room> edge = new Path_Edge<Room>();
                    edge.tile = t;
                    edge.cost = 1;
                    edge.node = nodes[t2.Room];
                    edges.Add(edge);
                }
            }
        }

        n.edges = edges.ToArray();
    }

    void GenerateEdgesOutside()
    {   
        List<Path_Edge<Room>> outsideEdges = new List<Path_Edge<Room>>();
        Room outsideRoom = null;
        foreach (Room r in nodes.Keys)
        {
            if (r.IsOutsideRoom())
            {
                outsideRoom = r;
                continue;
            }


            Path_Edge<Room>[] edges = nodes[r].edges;
            foreach (Path_Edge<Room> edge in edges)
            {
                if (edge.node.data.IsOutsideRoom())
                {
                    // Edge connects to the outside room
                    Path_Edge<Room> outsideEdge = new Path_Edge<Room>();
                    outsideEdge.tile = edge.tile;
                    outsideRoom.exits.Add(edge.tile);
                    outsideEdge.cost = 1;
                    outsideEdge.node = nodes[r];
                    outsideEdges.Add(outsideEdge);
                }
            }
        }
        nodes[outsideRoom].edges = outsideEdges.ToArray();
    }


    public void RegenerateGraph()
    {
        foreach (Room r in nodes.Keys)
        {
            GenerateEdgesByRoom(r);
        }
        GenerateEdgesOutside();
    }

    public rooms[] RoomConnections(Tile t)
    {
        List<Room> rooms = new List<Room>;
        if(t == null)
        {
            return null;
        }
        foreach(Path_Node<Room> node in this.nodes)
        {
            foreach(Path_Edge<Room> edge in node.edges)
            {
                if(edge.tile == t)
                {
                    rooms.Add(edge.node.data);

                }
            }
        }
        return rooms.ToArray;
    }
}
