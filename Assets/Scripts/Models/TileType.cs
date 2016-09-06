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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class TileType : IXmlSerializable, IEquatable<TileType>
{
    private static readonly string ULogChanelName = "TileType";
    private static readonly string TilesDescriptionFileName = "Tiles.xml";
    private static readonly string TilesScriptFileName = "Tiles.lua";

    // A private dictionary for storing all TileTypes
    private static readonly Dictionary<string, TileType> TileTypes = new Dictionary<string, TileType>();

    private static TileType empty;
    private static TileType floor;

    private Job buildingJob;

    private TileType()
    {
        PathfindingModifier = 0.0f;
        PathfindingWeight = 1.0f;
    }

    public static TileType Empty
    {
        get { return empty ?? (empty = TileTypes["Empty"]); }
    }

    public static TileType Floor
    {
        get { return floor ?? (floor = TileTypes["Floor"]); }
    }

    /// <summary>
    /// All currently registerd TileTypes.
    /// </summary>
    public static TileType[] LoadedTileTypes
    {
        get
        {
            return TileTypes.Values.ToArray();
        }
    }

    /// <summary>
    /// Unique TileType identifier.
    /// </summary>
    public string Type { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public float BaseMovementCost { get; private set; }

    /// <summary>
    /// Gets or sets the TileType's pathfinding weight which is multiplied into the Tile's final PathfindingCost.
    /// </summary>
    public float PathfindingWeight { get; set; }

    /// <summary>
    /// Gets or sets the TileType's pathfinding modifier which is added into the Tile's final PathfindingCost.
    /// </summary>
    public float PathfindingModifier { get; set; }

    public bool LinksToNeighbours { get; private set; }

    public string CanBuildHereLua { get; private set; }

    public string LocalizationCode { get; private set; }

    public string UnlocalizedDescription { get; private set; }

    /// <summary>
    /// Gets clone of construction job prototype for this tileType.
    /// </summary>
    public Job BuildingJob
    {
        get { return buildingJob.Clone(); }
    }

    /// <summary>
    /// Gets the TileType with the type, null if not found.
    /// </summary>
    public static TileType GetTileType(string type)
    {
        if (TileTypes.ContainsKey(type))
        {
            return TileTypes[type];
        }

        Debug.ULogWarningChannel(ULogChanelName, "No TileType with Type = {0} found.", type);
        return null;
    }

    /// <summary>
    /// Loads all TileType definitions in Data\ and Data\Mods.
    /// </summary>
    public static void LoadTileTypes()
    {
        // Load lua code
        string luaPath = Path.Combine(Application.streamingAssetsPath, "LUA");
        string luaFilePath = Path.Combine(luaPath, TilesScriptFileName);

        LuaUtilities.LoadScriptFromFile(luaFilePath);

        // Load all mod defined lua code
        foreach (DirectoryInfo mod in WorldController.Instance.modsManager.GetMods())
        {
            foreach (FileInfo file in mod.GetFiles(TilesScriptFileName))
            {
                Debug.ULogChannel(ULogChanelName, "Loading Tile LUA scripts from mod: {0}", mod.Name);
                LuaUtilities.LoadScriptFromFile(file.FullName);
            }
        }

        // Load TileType xml definitions
        string dataPath = Path.Combine(Application.streamingAssetsPath, "Data");
        string xmlPath = Path.Combine(dataPath, TilesDescriptionFileName);
        string xmlText = File.ReadAllText(xmlPath);

        ReadTileTypesFromXml(xmlText);

        // Load all mod defined TileType definitions
        foreach (DirectoryInfo mod in WorldController.Instance.modsManager.GetMods())
        {
            foreach (FileInfo file in mod.GetFiles(TilesDescriptionFileName))
            {
                Debug.ULogChannel(ULogChanelName, "Loading TileType definitions from mod: {0}", mod.Name);
                xmlText = File.ReadAllText(file.FullName);
                ReadTileTypesFromXml(xmlText);
            }
        }
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

    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader parentReader)
    {
        Type = parentReader.GetAttribute("tileType");

        XmlReader reader = parentReader.ReadSubtree();
        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Name":
                    reader.Read();
                    Name = reader.ReadContentAsString();
                    break;
                case "Description":
                    reader.Read();
                    Description = reader.ReadContentAsString();
                    break;
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

    public void WriteXml(XmlWriter writer)
    {
        throw new NotSupportedException();
    }

    private static void ReadTileTypesFromXml(string xmlText)
    {
        XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));
        if (reader.ReadToDescendant("Tiles"))
        {
            if (reader.ReadToDescendant("Tile"))
            {
                do
                {
                    TileType tileType = new TileType();
                    tileType.ReadXml(reader);
                    TileTypes[tileType.Type] = tileType;

                    Debug.ULogChannel(ULogChanelName, "Read tileType: " + tileType.Name + "!");
                }
                while (reader.ReadToNextSibling("Tile"));
            }
            else
            {
                Debug.ULogErrorChannel(ULogChanelName, "The tiletypes definition file doesn't have any 'Tile' elements.");
            }
        }
        else
        {
            Debug.ULogErrorChannel(ULogChanelName, "Did not find a 'Tiles' element in the prototype definition file.");
        }
    }

    private void ReadBuildingJob(XmlReader parentReader)
    {
        string jobTime = parentReader.GetAttribute("jobTime");
        float jobTimeValue;
        if (float.TryParse(jobTime, out jobTimeValue) == false)
        {
            Debug.ULogErrorChannel(ULogChanelName, "Could not load jobTime for TyleType: {0} -- jobTime readed {1}", Type, jobTime);
            return;
        }

        List<Inventory> inventoryRequirements = new List<Inventory>();
        XmlReader inventoryReader = parentReader.ReadSubtree();
        while (inventoryReader.Read())
        {
            if (inventoryReader.Name != "Inventory")
            {
                continue;
            }

            // Found an inventory requirement, so add it to the list!
            int amount;
            string objectType = inventoryReader.GetAttribute("objectType");
            if (int.TryParse(inventoryReader.GetAttribute("amount"), out amount))
            {
                inventoryRequirements.Add(new Inventory(objectType, amount));
            }
            else
            {
                Debug.ULogErrorChannel(ULogChanelName, "Could not load Inventory item for TyleType: {0}", Type);
            }
        }

        buildingJob = new Job(
            null,
            this,
            Tile.ChangeTileTypeJobComplete,
            jobTimeValue,
            inventoryRequirements.ToArray(),
            Job.JobPriority.High,
            false,
            true)
        {
            JobDescription = "job_build_floor_" + this
        };
    }
}
