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
using System;
using MoonSharp.Interpreter;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Linq;

[MoonSharpUserData]
public class TileType : IXmlSerializable {

    // Just two util functions to not break every link to TileType.(Empty | Floor)
    // TODO: Maybe cache the empty and floor tiletypes.
    public static TileType Empty { get { return tileTypeDictionary["Empty"]; } }

    public static TileType Floor { get { return tileTypeDictionary["Floor"]; } }

    // A private dictionary for storing all TileTypes
    private static Dictionary<string, TileType> tileTypeDictionary = new Dictionary<string, TileType>();

    private static Dictionary<TileType, Job> tileTypeBuildJobPrototypes = new Dictionary<TileType, Job>();

    public string Type { get; protected set; }

    public string Name { get; protected set; }

    public string Description { get; protected set; }
    
    public float BaseMovementCost { get; protected set; }

    // TODO!
    public bool LinksToNeighbours { get; protected set; }

    // Standard movement cost calculation (lua function).
    public string MovementCostLua { get; protected set; }
    
    public string CanBuildHereLua { get; protected set; }
    
    public string LocalizationCode { get; protected set; }

    public string UnlocalizedDescription { get; protected set; }

    private TileType()
    {
        // Default lua method names
        CanBuildHereLua = "CanBuildHere_Standard";
    }

    // Will this even be needed?
    private TileType(string name, string description, float baseMovementCost)
    {
        this.Name = name;
        this.Description = description;
        this.BaseMovementCost = baseMovementCost;

        // Add this to the dictionary of all tileTypes
        tileTypeDictionary[name] = this;
    }

    /// <summary>
    /// Gets the TileType with the type, null if not found!
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static TileType GetTileType(string type)
    {
        if (tileTypeDictionary.ContainsKey(type))
        {
            return tileTypeDictionary[type];
        }
        else
        {
            Debug.ULogWarningChannel("TileType", "No tile type " + type + " found!");

            return null;
        }
    }

    /// <summary>
    /// Returns an array of all the currently registerd TileTypes.
    /// </summary>
    /// <returns></returns>
    public static TileType[] GetTileTypes()
    {
        return tileTypeDictionary.Values.ToArray();
    }

    /// <summary>
    /// Gets the construction job prototype for this tileType.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Job GetConstructionJobPrototype(TileType type)
    {
        if (tileTypeBuildJobPrototypes.ContainsKey(type))
        {
            return tileTypeBuildJobPrototypes[type].Clone();
        }

        Debug.ULogWarningChannel("TileType", "TileType job prototype for " + type + " did not exits!");

        return null;
    }
    
    /// <summary>
    /// Loads all TileType definitions in Data\ and Data\Mods
    /// </summary>
    public static void LoadTileTypes()
    {
        // Load lua code
        string luaPath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        string luaFilePath = System.IO.Path.Combine(luaPath, "Tile.lua");

        LuaUtilities.LoadScriptFromFile(luaFilePath);

        // Load all mod defined lua code
        foreach (DirectoryInfo mod in WorldController.Instance.modsManager.GetMods())
        {
            foreach (FileInfo file in mod.GetFiles("Tiles.lua"))
            {
                Debug.ULogChannel("TileType", "Loading mod " + mod.Name + " TileType definitions!");

                LuaUtilities.LoadScriptFromFile(file.FullName);
            }
        }

        // Load TileType xml definitions
        string dataPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        string xmlPath = System.IO.Path.Combine(dataPath, "Tiles.xml");
        string xmlText = System.IO.File.ReadAllText(xmlPath);
        
        readTileTypesFromXml(xmlText);

        // Load all mod defined TileType definitions
        foreach (DirectoryInfo mod in WorldController.Instance.modsManager.GetMods())
        {
            foreach (FileInfo file in mod.GetFiles("Tiles.xml"))
            {
                Debug.ULogChannel("TileType", "Loading mod " + mod.Name + " TileType definitions!");

                xmlText = System.IO.File.ReadAllText(file.FullName);

                readTileTypesFromXml(xmlText);
            }
        }
    }

    // Overrides ToString() to be able to do localizaton in character
    public override string ToString()
    {
        return Type;
    }

    private static void readTileTypesFromXml(string xmlText)
    {
        XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

        if (reader.ReadToDescendant("Tiles"))
        {
            if (reader.ReadToDescendant("Tile"))
            {
                do
                {

                    TileType type = new TileType();
                    
                    type.ReadXml(reader);

                    tileTypeDictionary[type.Type] = type;

                    Debug.ULogChannel("TileType", "Read tileType: " + type.Name + "!");

                } while (reader.ReadToNextSibling("Tile"));
            }
            else
            {
                Debug.ULogErrorChannel("TileType", "The tiletypes definition file doesn't have any 'Tile' elements.");
            }
        }
        else
        {
            Debug.ULogErrorChannel("TileType", "Did not find a 'Tiles' element in the prototype definition file.");
        }
    }
    
    #region IXmlSerializable implementation 

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader_parent)
    {
        Type = reader_parent.GetAttribute("tileType");

        XmlReader reader = reader_parent.ReadSubtree();

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
                case "LinksToNeighbours":
                    reader.Read();
                    LinksToNeighbours = reader.ReadContentAsBoolean();
                    break;
                case "BuildingJob":

                    float jobTime = float.Parse(reader.GetAttribute("jobTime"));

                    List<Inventory> invs = new List<Inventory>();

                    XmlReader invs_reader = reader.ReadSubtree();

                    while (invs_reader.Read())
                    {
                        if (invs_reader.Name == "Inventory")
                        {
                            // Found an inventory requirement, so add it to the list!
                            invs.Add(new Inventory(
                                    invs_reader.GetAttribute("objectType"),
                                    int.Parse(invs_reader.GetAttribute("amount")),
                                    0));
                        }
                    }
                    
                    Job j = new Job(
                        null,
                        this,
                        Tile.ChangeTileTypeJobComplete,
                        jobTime,
                        invs.ToArray(),
                        Job.JobPriority.High,
                        false,
                        true);
                    j.JobDescription = "job_build_floor_" + this;

                    tileTypeBuildJobPrototypes[this] = j;

                    break;
                case "MovementCost":
                    string movementCostAttribute = reader.GetAttribute("FunctionName");
                    if (movementCostAttribute != null)
                    {
                        MovementCostLua = movementCostAttribute;
                    }
                    Debug.Log("MovmentCostLua: " + MovementCostLua);
                    break;
                case "CanPlaceHere":
                    string canPlaceHereAttribute = reader.GetAttribute("FunctionName");
                    if (canPlaceHereAttribute != null)
                    {
                        CanBuildHereLua = canPlaceHereAttribute;
                    }
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

    #endregion
}
