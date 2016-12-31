#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UnityEngine;

[MoonSharpUserData]
public class FurnitureManager : IEnumerable<Furniture>
{
    private List<Furniture> furnitures;

    /// <summary>
    /// Initializes a new instance of the <see cref="FurnitureManager"/> class.
    /// </summary>
    public FurnitureManager()
    {
        furnitures = new List<Furniture>();
    }

    /// <summary>
    /// Occurs when a Furniture is created.
    /// </summary>
    public event Action<Furniture> Created;

    /// <summary>
    /// Creates a furniture with the given type and places it at the given tile.
    /// </summary>
    /// <returns>The furniture.</returns>
    /// <param name="type">The type of the furniture.</param>
    /// <param name="tile">The tile to place the furniture at.</param>
    /// <param name="doRoomFloodFill">If set to <c>true</c> do room flood fill.</param>
    /// /// <param name="rotation">The rotation applied to te furniture.</param>
    public Furniture PlaceFurniture(string type, Tile tile, bool doRoomFloodFill = true, float rotation = 0f)
    {
        if (PrototypeManager.Furniture.Has(type) == false)
        {
            UnityDebugger.Debugger.LogError("World", "furniturePrototypes doesn't contain a proto for key: " + type);
            return null;
        }

        Furniture furn = PrototypeManager.Furniture.Get(type).Clone();
        furn.SetRotation(rotation);

        return PlaceFurniture(furn, tile, doRoomFloodFill);
    }

    /// <summary>
    /// Places the given furniture prototype at the given tile.
    /// </summary>
    /// <returns>The furniture.</returns>
    /// <param name="prototype">The furniture prototype.</param>
    /// <param name="tile">The tile to place the furniture at.</param>
    /// <param name="doRoomFloodFill">If set to <c>true</c> do room flood fill.</param>
    public Furniture PlaceFurniture(Furniture prototype, Tile tile, bool doRoomFloodFill = true)
    {
        Furniture furniture = Furniture.PlaceInstance(prototype, tile);

        if (furniture == null)
        {
            // Failed to place object -- most likely there was already something there.
            return null;
        }

        furniture.Removed += OnRemoved;

        furnitures.Add(furniture);
        if (furniture.RequiresFastUpdate)
        {
            TimeManager.Instance.RegisterFastUpdate(furniture);
        }

        if (furniture.RequiresSlowUpdate)
        {
            TimeManager.Instance.RegisterSlowUpdate(furniture);
        }

        // Do we need to recalculate our rooms/reachability for other jobs?
        if (doRoomFloodFill && furniture.RoomEnclosure)
        {
            World.Current.RoomManager.DoRoomFloodFill(furniture.Tile, true);
            World.Current.jobQueue.ReevaluateReachability();
        }

        if (Created != null)
        {
            Created(furniture);
        }

        return furniture;
    }

    /// <summary>
    /// When a construction job is completed, place the furniture.
    /// </summary>
    /// <param name="job">The completed job.</param>
    public void ConstructJobCompleted(Job job)
    {
        Furniture furn = (Furniture)job.buildablePrototype;

        // Let our workspot tile know it is no longer reserved for us
        World.Current.UnreserveTileAsWorkSpot(furn, job.tile);

        PlaceFurniture(furn, job.tile);
    }

    /// <summary>
    /// Determines whether the placement of a furniture with the given type at the given tile is valid.
    /// </summary>
    /// <returns><c>true</c> if the placement is valid; otherwise, <c>false</c>.</returns>
    /// <param name="type">The furniture type.</param>
    /// <param name="tile">The tile where the furniture will be placed.</param>
    /// <param name="rotation">The rotation applied to the furniture.</param>
    public bool IsPlacementValid(string type, Tile tile, float rotation = 0f)
    {
        Furniture furn = PrototypeManager.Furniture.Get(type).Clone();
        furn.SetRotation(rotation);
        return furn.IsValidPosition(tile);
    }

    /// <summary>
    /// Determines whether the work spot of the furniture with the given type at the given tile is clear.
    /// </summary>
    /// <returns><c>true</c> if the work spot at the give tile is clear; otherwise, <c>false</c>.</returns>
    /// <param name="furnitureType">Furniture type.</param>
    /// <param name="tile">The tile we want to check.</param>
    public bool IsWorkSpotClear(string type, Tile tile)
    {
        Furniture proto = PrototypeManager.Furniture.Get(type);

        // If the workspot is internal, we don't care about furniture blocking it, this will be stopped or allowed
        //      elsewhere depending on if the furniture being placed can replace the furniture already in this tile.
        if (proto.Jobs.WorkSpotIsInternal())
        {
            return true;
        }

        if (proto.Jobs != null && World.Current.GetTileAt((int)(tile.X + proto.Jobs.WorkSpotOffset.x), (int)(tile.Y + proto.Jobs.WorkSpotOffset.y), (int)tile.Z).Furniture != null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the amount of furniture with the given type.
    /// </summary>
    /// <returns>The amount of furniture with the given type.</returns>
    /// <param name="type">The furniture type.</param>
    public int CountWithType(string type)
    {
        return furnitures.Count(f => f.Type == type);
    }

    /// <summary>
    /// Returns a list of furniture using the given filter function.
    /// </summary>
    /// <returns>A list of furnitures.</returns>
    /// <param name="filterFunc">The filter function.</param>
    public List<Furniture> Find(Func<Furniture, bool> filterFunc)
    {
        return furnitures.Where(filterFunc).ToList();
    }

    /// <summary>
    /// Gets the furnitures enumerator.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator GetEnumerator()
    {
        return furnitures.GetEnumerator();
    }

    /// <summary>
    /// Gets each furniture.
    /// </summary>
    /// <returns>Each furniture.</returns>
    IEnumerator<Furniture> IEnumerable<Furniture>.GetEnumerator()
    {
        foreach (Furniture furniture in furnitures)
        {
            yield return furniture;
        }
    }

    public JToken ToJson()
    {
        JArray furnituresJson = new JArray();
        foreach (Furniture furniture in furnitures)
        {
            furnituresJson.Add(furniture.ToJSon());
        }

        return furnituresJson;
    }

    public void FromJson(JToken furnituresToken)
    {
        JArray furnituresJArray = (JArray)furnituresToken;

        foreach (JToken furnitureToken in furnituresJArray)
        {
            int x = (int)furnitureToken["X"];
            int y = (int)furnitureToken["Y"];
            int z = (int)furnitureToken["Z"];
            string type = (string)furnitureToken["Type"];
            float rotation = (float)furnitureToken["Rotation"];
            Furniture furniture = PlaceFurniture(type, World.Current.GetTileAt(x, y, z), false, rotation);
            furniture.FromJson(furnitureToken);
        }
    }

    /// <summary>
    /// Called when a furniture is removed so that it can be deleted from the list.
    /// </summary>
    /// <param name="furnitures">The furniture being removed.</param>
    private void OnRemoved(Furniture furniture)
    {
        furnitures.Remove(furniture);
        TimeManager.Instance.UnregisterFastUpdate(furniture);
        TimeManager.Instance.UnregisterSlowUpdate(furniture);

        // Movement to jobs might have been opened, let's move jobs back into the queue to be re-evaluated.
        World.Current.jobQueue.ReevaluateReachability();
    }
}
