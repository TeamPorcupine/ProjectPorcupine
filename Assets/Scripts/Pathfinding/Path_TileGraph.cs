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


public class Path_TileGraph
{

    // This class constructs a simple path-finding compatible graph
    // of our world.  Each tile is a node. Each WALKABLE neighbour
    // from a tile is linked via an edge connection.

    public Dictionary<Tile, Path_Node<Tile>> nodes;

    public Path_TileGraph(World world)
    {

        Logger.Log("Path_TileGraph");

        // Loop through all tiles of the world
        // For each tile, create a node
        //  Do we create nodes for non-floor tiles?  NO!
        //  Do we create nodes for tiles that are completely unwalkable (i.e. walls)?  NO!

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {

                Tile t = world.GetTileAt(x, y);

                //if(t.movementCost > 0) {	// Tiles with a move cost of 0 are unwalkable
                Path_Node<Tile> n = new Path_Node<Tile>();
                n.data = t;
                nodes.Add(t, n);
                //}

            }
        }

        Logger.Log("Path_TileGraph: Created " + nodes.Count + " nodes.");


        // Now loop through all nodes again
        // Create edges for neighbours


        foreach (Tile t in nodes.Keys)
        {
            GenerateEdgesByTile(t);
        }


    }

    void GenerateEdgesByTile(Tile t)
    {
        Path_Node<Tile> n = nodes[t];
        List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();
        // Get a list of neighbours for the tile
        Tile[] neighbours = t.GetNeighbours(true);
        // NOTE: Some of the array spots could be null.
        // If neighbour is walkable, create an edge to the relevant node.
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] != null && neighbours[i].movementCost > 0 && IsClippingCorner(t, neighbours[i]) == false)
            {
                // This neighbour exists, is walkable, and doesn't requiring clipping a corner --> so create an edge.
                Path_Edge<Tile> e = new Path_Edge<Tile>();
                e.cost = neighbours[i].movementCost;
                e.node = nodes[neighbours[i]];
                // Add the edge to our temporary (and growable!) list
                edges.Add(e);
            }
        }
        n.edges = edges.ToArray();
    }

    public void RegenerateGraphAtTile(Tile changedTile)
    {
        GenerateEdgesByTile(changedTile);
        foreach (Tile tile in changedTile.GetNeighbours(true))
        {
            GenerateEdgesByTile(tile);
        }
    }

    bool IsClippingCorner(Tile curr, Tile neigh)
    {
        // If the movement from curr to neigh is diagonal (e.g. N-E)
        // Then check to make sure we aren't clipping (e.g. N and E are both walkable)

        int dX = curr.X - neigh.X;
        int dY = curr.Y - neigh.Y;

        if (Mathf.Abs(dX) + Mathf.Abs(dY) == 2)
        {
            // We are diagonal

            if (World.current.GetTileAt(curr.X - dX, curr.Y).movementCost == 0)
            {
                // East or West is unwalkable, therefore this would be a clipped movement.
                return true;
            }

            if (World.current.GetTileAt(curr.X, curr.Y - dY).movementCost == 0)
            {
                // North or South is unwalkable, therefore this would be a clipped movement.
                return true;
            }

            // If we reach here, we are diagonal, but not clipping
        }

        // If we are here, we are either not clipping, or not diagonal
        return false;
    }

}
