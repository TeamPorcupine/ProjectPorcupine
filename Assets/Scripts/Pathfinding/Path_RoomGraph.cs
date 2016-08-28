#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using UnityEngine;

public class Path_RoomGraph
{
    public Dictionary<Room, Path_Node<Room>> nodes;

    public Path_RoomGraph(World world)
    {
        Debug.ULogChannel("Path_RoomGraph", "Entered Path_RoomGraph");

        // Loop through all rooms of the world
        // For each room, create a node
        nodes = new Dictionary<Room, Path_Node<Room>>();

        foreach (Room room in world.rooms)
        {
            Path_Node<Room> n = new Path_Node<Room>();
            n.data = room;
            nodes.Add(room, n);
        }

        Debug.ULogChannel("Path_RoomGraph", "Created " + nodes.Count + " nodes.");

        // Now loop through all nodes again
        // Create edges for neighbours
        foreach (Room room in nodes.Keys)
        {
            if (room.IsOutsideRoom() == false)
            {
                GenerateEdgesByRoom(room);
            }
        }

        GenerateEdgesOutside();

        foreach (Room room in nodes.Keys)
        {
            Debug.ULogChannel("Path_RoomGraph", "Room " + room.ID + " has edges to:");
            Debug.ULogChannel("Path_RoomGraph", "\tRoom " + room.ID + " has " + nodes[room].edges.Length + " edges");
            foreach (Path_Edge<Room> edge in nodes[room].edges)
            {
                Debug.ULogChannel("Path_RoomGraph", "\t\tEdge connects to " + edge.node.data.ID);
            }
        }
    }

    public void RegenerateGraph()
    {
        nodes.Clear();
        foreach (Room room in World.Current.rooms)
        {
            Path_Node<Room> n = new Path_Node<Room>();
            n.data = room;
            nodes.Add(room, n);
        }

        foreach (Room room in nodes.Keys)
        {
            if (room.IsOutsideRoom() == false)
            {
                GenerateEdgesByRoom(room);
            }
        }

        GenerateEdgesOutside();

        foreach (Room room in nodes.Keys)
        {
            Debug.ULogChannel("Path_RoomGraph", "Room " + room.ID + " has edges to:");
            Debug.ULogChannel("Path_RoomGraph", "\tRoom " + room.ID + " has " + nodes[room].edges.Length + " edges");
            foreach (Path_Edge<Room> edge in nodes[room].edges)
            {
                Debug.ULogChannel("Path_RoomGraph", "\t\tEdge connects to " + edge.node.data.ID);
            }
        }
    }

    public Room[] RoomConnections(Tile tile)
    {
        List<Room> rooms = new List<Room>();
        if (tile == null)
        {
            return null;
        }

        foreach (Path_Node<Room> node in nodes.Values)
        {
            foreach (Path_Edge<Room> edge in node.edges)
            {
                if (edge.tile == tile)
                {
                    rooms.Add(edge.node.data);
                }
            }
        }

        return rooms.ToArray();
    }

    private void GenerateEdgesByRoom(Room room)
    {
        if (room.IsOutsideRoom())
        {
            // We don't want to loop over all the tiles in the outside room as this would be expensive
            return;
        }

        Path_Node<Room> node = nodes[room];
        List<Path_Edge<Room>> edges = new List<Path_Edge<Room>>();

        List<Tile> exits = room.exits;

        foreach (Tile tile in exits)
        {
            // Loop over the exits to find a different room
            Tile[] neighbours = tile.GetNeighbours();
            foreach (Tile tileNeigbour in neighbours)
            {
                if (tileNeigbour == null || tileNeigbour.Room == null)
                {
                    continue;
                }

                // We have found a room
                if (tileNeigbour.Room != room)
                {
                    // We have found a different room to ourselves add an edge from us to them
                    Path_Edge<Room> edge = new Path_Edge<Room>();
                    edge.tile = tile;
                    edge.cost = 1;
                    edge.node = nodes[tileNeigbour.Room];
                    edges.Add(edge);
                }
            }
        }

        node.edges = edges.ToArray();
    }

    private void GenerateEdgesOutside()
    {   
        List<Path_Edge<Room>> outsideEdges = new List<Path_Edge<Room>>();
        Room outsideRoom = null;
        foreach (Room room in nodes.Keys)
        {
            if (room.IsOutsideRoom())
            {
                outsideRoom = room;
                continue;
            }

            Path_Edge<Room>[] edges = nodes[room].edges;
            foreach (Path_Edge<Room> edge in edges)
            {
                if (edge.node.data.IsOutsideRoom())
                {
                    // Edge connects to the outside room
                    Path_Edge<Room> outsideEdge = new Path_Edge<Room>();
                    outsideEdge.tile = edge.tile;
                    outsideRoom.exits.Add(edge.tile);
                    outsideEdge.cost = 1;
                    outsideEdge.node = nodes[room];
                    outsideEdges.Add(outsideEdge);
                }
            }
        }

        nodes[outsideRoom].edges = outsideEdges.ToArray();
    }
}