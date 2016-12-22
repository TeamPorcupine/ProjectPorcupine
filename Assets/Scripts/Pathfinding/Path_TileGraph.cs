#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;

public class Path_TileGraph
{
    // This class constructs a simple path-finding compatible graph
    // of our world.  Each tile is a node. Each WALKABLE neighbour
    // from a tile is linked via an edge connection.
    public Dictionary<Tile, Path_Node<Tile>> nodes;

    public Path_TileGraph(World world)
    {
        UnityDebugger.Debugger.Log("Path_TileGraph", "Entered Path_TileGraph");

       /*
        * Loop through all tiles of the world
        * For each tile, create a node
        *  Do we create nodes for non-floor tiles?  NO!
        *  Do we create nodes for tiles that are completely unwalkable (i.e. walls)?  NO!
        */

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                for (int z = 0; z < world.Depth; z++)
                {
                    Tile t = world.GetTileAt(x, y, z);

                    ////if(t.movementCost > 0) {    // Tiles with a move cost of 0 are unwalkable
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    nodes.Add(t, n);
                }
            }
        }

        UnityDebugger.Debugger.Log("Path_TileGraph", "Created " + nodes.Count + " nodes.");

        // Now loop through all nodes again
        // Create edges for neighbours
        foreach (Tile t in nodes.Keys)
        {
            GenerateEdgesByTile(t);
        }
    }
        
    public void RegenerateGraphAtTile(Tile changedTile)
    {
        if (changedTile == null)
        {
            return;
        }

        GenerateEdgesByTile(changedTile);
        foreach (Tile tile in changedTile.GetNeighbours(true))
        {
            GenerateEdgesByTile(tile);
        }
    }

    private void GenerateEdgesByTile(Tile t)
    {
        if (t == null)
        {
            return;
        }

        Path_Node<Tile> n = nodes[t];
        List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();

        // Get a list of neighbours for the tile
        Tile[] neighbours = t.GetNeighbours(true, true);

        // NOTE: Some of the array spots could be null.
        // If neighbour is walkable, create an edge to the relevant node.
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] != null && neighbours[i].PathfindingCost > 0 && t.IsClippingCorner(neighbours[i]) == false)
            {
                // This neighbour exists, is walkable, and doesn't requiring clipping a corner --> so create an edge.
                Path_Edge<Tile> e = new Path_Edge<Tile>();
                e.cost = neighbours[i].PathfindingCost;
                e.node = nodes[neighbours[i]];

                // Add the edge to our temporary (and growable!) list
                edges.Add(e);
            }
        }

        n.edges = edges.ToArray();
    }
}
