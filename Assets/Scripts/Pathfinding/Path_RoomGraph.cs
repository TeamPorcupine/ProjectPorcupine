#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using ProjectPorcupine.Rooms;

public class Path_RoomGraph
{
    public Dictionary<Room, Path_Node<Room>> nodes;

    public Path_RoomGraph(World world)
    {
        UnityDebugger.Debugger.Log("Path_RoomGraph", "Entered Path_RoomGraph");

        // Loop through all rooms of the world
        // For each room, create a node
        nodes = new Dictionary<Room, Path_Node<Room>>();
        UnityDebugger.Debugger.Log("Path_RoomGraph", "There are " + world.RoomManager.Count + " Rooms");
        foreach (Room room in world.RoomManager)
        {
            Path_Node<Room> n = new Path_Node<Room>();
            n.data = room;
            nodes.Add(room, n);
        }

        UnityDebugger.Debugger.Log("Path_RoomGraph", "Created " + nodes.Count + " nodes.");

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
            UnityDebugger.Debugger.Log("Path_RoomGraph", "Room " + room.ID + " has edges to:");
            foreach (Path_Edge<Room> edge in nodes[room].edges)
            {
                UnityDebugger.Debugger.Log("Path_RoomGraph", "\tEdge connects to " + edge.node.data.ID);
            }
        }
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

        Dictionary<Tile, Room> neighbours = room.GetNeighbours();

        foreach (Tile tile in neighbours.Keys)
        {
            // We have found a different room to ourselves add an edge from us to them
            Path_Edge<Room> edge = new Path_Edge<Room>();
            edge.cost = 1;
            edge.tile = tile;
            edge.node = nodes[neighbours[tile]];
            edges.Add(edge);
        }

        node.edges = edges.ToArray();
    }

    private void GenerateEdgesOutside()
    {   
        List<Path_Edge<Room>> outsideEdges = new List<Path_Edge<Room>>();
        Room outsideRoom = World.Current.RoomManager.OutsideRoom;
        foreach (Room room in nodes.Keys)
        {
            if (room.IsOutsideRoom())
            {
                continue;
            }

            Path_Edge<Room>[] edges = nodes[room].edges;
            foreach (Path_Edge<Room> edge in edges)
            {
                if (edge.node.data.IsOutsideRoom())
                {
                    // Edge connects to the outside room
                    Path_Edge<Room> outsideEdge = new Path_Edge<Room>();

                    // The cost of going outside should be high as to avoid needlessly leaving the base
                    outsideEdge.tile = edge.tile;
                    outsideEdge.cost = 10f;
                    outsideEdge.node = nodes[room];
                    outsideEdges.Add(outsideEdge);
                }
            }
        }

        nodes[outsideRoom].edges = outsideEdges.ToArray();
    }
}