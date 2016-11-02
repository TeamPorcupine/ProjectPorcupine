#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

public class Path_Edge<T>
{
    // Cost to traverse this edge (i.e. cost to ENTER the tile)
    public Tile tile;

    public float cost;

    public Path_Node<T> node;
}
