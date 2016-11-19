#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Xml;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class QuestReward
{
    private static System.Random rand = new System.Random();

    public string Description { get; set; }

    public string OnRewardLuaFunction { get; set; }

    public Parameter Parameters { get; set; }

    public bool IsCollected { get; set; }

    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        Description = reader_parent.GetAttribute("Description");
        OnRewardLuaFunction = reader_parent.GetAttribute("OnRewardLuaFunction");

        XmlReader reader = reader_parent.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Params":
                    Parameters = Parameter.ReadXml(reader);
                    break;
            }
        }
    }

    /// <summary>
    /// Will be used to find a tile near the center of the map that does not currently have any invetory or furniture.
    /// </summary>
    /// <param name="attempts"> The amount of attemps the function will try to find an empty tile.</param>
    /// <returns> An empty tile near the center of the map.</returns>
    public Tile GetEmptyTileNearCenter(int attempts)
    {
        Tile current = World.Current.GetCenterTile();

        while (attempts > 0)
        {
            // make sure that we have an existing tile
            if (current != null)
            {
                // make sure that we have a tile with no funiture
                if (current.Furniture == null)
                {
                    // if theres no inventory here then we consider this tile empty.
                    if (current.Inventory == null)
                    {
                        break;
                    }

                    // if the stack size of this inventory is zero then we consider it empty.
                    if (current.Inventory.StackSize == 0)
                    {
                        break;
                    }
                }
            }

            /// TODO: search in a spiral around the center tile or something less likely to fail.
            current = World.Current.GetTileAt(current.X + rand.Next(-1, 2), current.Y + rand.Next(-1, 2), current.Z);
            attempts--;
        }

        // if we ran out of attempts then we haven't found an empty tile so return null
        if (attempts <= 0)
        {
            return null;
        }

        Debug.ULogChannel("Quests", string.Format("Found empty tile at X:{0} Y:{1} Z:{2}", current.X, current.Y, current.Z));

        return current;
    }
}