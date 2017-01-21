#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Xml;
using MoonSharp.Interpreter;
using ProjectPorcupine.Jobs;

[MoonSharpUserData]
public class TileType : IPrototypable, IEquatable<TileType>
{
    private static readonly string ULogChanelName = "TileType";

    private Job buildingJob;

    /// <summary>
    /// Initializes a new instance of the <see cref="TileType"/> class.
    /// </summary>
    public TileType()
    {
        PathfindingModifier = 0.0f;
        PathfindingWeight = 1.0f;
    }

    /// <summary>
    /// Gets the empty tile type prototype.
    /// </summary>
    /// <value>The empty tile type.</value>
    public static TileType Empty
    {
        get { return PrototypeManager.TileType.Get("empty"); }
    }

    /// <summary>
    /// Gets the floor tile type prototype.
    /// </summary>
    /// <value>The floor tile type.</value>
    public static TileType Floor
    {
        get { return PrototypeManager.TileType.Get("floor"); }
    }

    /// <summary>
    /// Unique TileType identifier.
    /// </summary>
    /// <value>The tile type.</value>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the base movement cost.
    /// </summary>
    /// <value>The base movement cost.</value>
    public float BaseMovementCost { get; private set; }

    /// <summary>
    /// Gets or sets the TileType's pathfinding weight which is multiplied into the Tile's final PathfindingCost.
    /// </summary>
    /// <value>The pathfinding weight.</value>
    public float PathfindingWeight { get; private set; }

    /// <summary>
    /// Gets or sets the TileType's pathfinding modifier which is added into the Tile's final PathfindingCost.
    /// </summary>
    /// <value>The pathfinding modifier.</value>
    public float PathfindingModifier { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="TileType"/> links to neighbours.
    /// </summary>
    /// <value><c>true</c> if links to neighbours; otherwise, <c>false</c>.</value>
    public bool LinksToNeighbours { get; private set; }

    /// <summary>
    /// Gets the lua function that can be called to determine if this instance can build here lua.
    /// </summary>
    /// <value>The name of the lua function.</value>
    public string CanBuildHereLua { get; private set; }

    /// <summary>
    /// Gets the localization code.
    /// </summary>
    /// <value>The localization code.</value>
    public string LocalizationCode { get; private set; }

    /// <summary>
    /// Gets the localized description.
    /// </summary>
    /// <value>The localized description.</value>
    public string UnlocalizedDescription { get; private set; }

    /// <summary>
    /// Gets a clone of construction job prototype for this tileType.
    /// </summary>
    /// <value>The building job.</value>
    public Job BuildingJob
    {
        get { return buildingJob.Clone(); }
    }

    public static bool operator ==(TileType left, TileType right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TileType left, TileType right)
    {
        return !Equals(left, right);
    }

    public bool Equals(TileType other)
    {
        return string.Equals(Type, other.Type);
    }

    public override bool Equals(object obj)
    {
        return Equals((TileType)obj);
    }

    public override int GetHashCode()
    {
        return Type != null ? Type.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return Type;
    }

    /// <summary>
    /// Determines whether this tile type is allowed to be built on the given tile.
    /// </summary>
    /// <returns><c>true</c> if the ile type is allowed to be built on the given tile; otherwise, <c>false</c>.</returns>
    /// <param name="tile">The tile to build on.</param>
    public bool CanBuildHere(Tile tile)
    {
        if (CanBuildHereLua == null)
        {
            return true;
        }

        DynValue value = FunctionsManager.TileType.Call(CanBuildHereLua, tile);
        if (value != null)
        {
            return value.Boolean;
        }

        UnityDebugger.Debugger.Log("Lua", "Found no lua function " + CanBuildHereLua);
        return false;
    }

    /// <summary>
    /// Reads the prototype from the specified XML reader.
    /// </summary>
    /// <param name="parentReader">The XML reader to read from.</param>
    public void ReadXmlPrototype(XmlReader parentReader)
    {
        Type = parentReader.GetAttribute("type");

        XmlReader reader = parentReader.ReadSubtree();
        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "BaseMovementCost":
                    reader.Read();
                    BaseMovementCost = reader.ReadContentAsFloat();
                    break;
                case "PathfindingModifier":
                    reader.Read();
                    PathfindingModifier = reader.ReadContentAsFloat();
                    break;
                case "PathfindingWeight":
                    reader.Read();
                    PathfindingWeight = reader.ReadContentAsFloat();
                    break;
                case "LinksToNeighbours":
                    reader.Read();
                    LinksToNeighbours = reader.ReadContentAsBoolean();
                    break;
                case "BuildingJob":
                    ReadBuildingJob(reader);
                    break;
                case "CanPlaceHere":
                    CanBuildHereLua = reader.GetAttribute("functionName");
                    break;
                case "LocalizationCode":
                    reader.Read();
                    LocalizationCode = reader.ReadContentAsString();
                    break;
                case "UnlocalizedDescription":
                    reader.Read();
                    UnlocalizedDescription = reader.ReadContentAsString();
                    break;
            }
        }
    }

    /// <summary>
    /// Reads the building job.
    /// </summary>
    /// <param name="parentReader">Parent reader.</param>
    private void ReadBuildingJob(XmlReader parentReader)
    {
        string jobTime = parentReader.GetAttribute("jobTime");
        float jobTimeValue;
        if (float.TryParse(jobTime, out jobTimeValue) == false)
        {
            UnityDebugger.Debugger.LogErrorFormat(ULogChanelName, "Could not load jobTime for TyleType: {0} -- jobTime readed {1}", Type, jobTime);
            return;
        }

        List<RequestedItem> requiredItems = new List<RequestedItem>();
        XmlReader inventoryReader = parentReader.ReadSubtree();

        while (inventoryReader.Read())
        {
            if (inventoryReader.Name != "Inventory")
            {
                continue;
            }

            // Found an inventory requirement, so add it to the list!
            int amount;
            string type = inventoryReader.GetAttribute("type");
            if (int.TryParse(inventoryReader.GetAttribute("amount"), out amount))
            {
                requiredItems.Add(new RequestedItem(type, amount));
            }
            else
            {
                UnityDebugger.Debugger.LogErrorFormat(ULogChanelName, "Could not load Inventory item for TyleType: {0}", Type);
            }
        }

        buildingJob = new Job(
            null,
            this,
            Tile.ChangeTileTypeJobComplete,
            jobTimeValue,
            requiredItems.ToArray(),
            Job.JobPriority.High,
            false,
            true)
        {
            Description = "job_build_floor_" + this
        };
    }
}
