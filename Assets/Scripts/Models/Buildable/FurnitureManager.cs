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
using System.Xml;
using UnityEngine;

public class FurnitureManager : IEnumerable<Furniture>
{
    private List<Furniture> furnitures;

    // A temporary list of all visible furniture. Gets updated when camera moves.
    private List<Furniture> furnituresVisible;

    // A temporary list of all invisible furniture. Gets updated when camera moves.
    private List<Furniture> furnituresInvisible;

    /// <summary>
    /// Initializes a new instance of the <see cref="FurnitureManager"/> class.
    /// </summary>
    public FurnitureManager()
    {
        furnitures = new List<Furniture>();
        furnituresVisible = new List<Furniture>();
        furnituresInvisible = new List<Furniture>();
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
            Debug.ULogErrorChannel("World", "furniturePrototypes doesn't contain a proto for key: " + type);
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
        furnituresVisible.Add(furniture);

        // Do we need to recalculate our rooms?
        if (doRoomFloodFill && furniture.RoomEnclosure)
        {
            World.Current.RoomManager.DoRoomFloodFill(furniture.Tile, true);
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

        // FIXME: I don't like having to manually and explicitly set
        // flags that prevent conflicts. It's too easy to forget to set/clear them!
        job.tile.PendingBuildJob = null;
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
    /// Reuturns a list of furniture using the given filter function.
    /// </summary>
    /// <returns>A list of furnitures.</returns>
    /// <param name="filterFunc">The filter function.</param>
    public List<Furniture> Find(Func<Furniture, bool> filterFunc)
    {
        return furnitures.Where(filterFunc).ToList();
    }

    /// <summary>
    /// Calls the furnitures update function on every frame.
    /// The list needs to be copied temporarily in case furnitures are added or removed during the update.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void TickEveryFrame(float deltaTime)
    {
        List<Furniture> tempFurnituresVisible = new List<Furniture>(furnituresVisible);
        foreach (Furniture furniture in tempFurnituresVisible)
        {
            furniture.EveryFrameUpdate(deltaTime);
        }
    }

    /// <summary>
    /// Calls the furnitures update function on a fixed frequency.
    /// The list needs to be copied temporarily in case furnitures are added or removed during the update.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void TickFixedFrequency(float deltaTime)
    {
        // TODO: Further optimization could divide eventFurnitures in multiple lists
        //       and update one of the lists each frame.
        //       FixedFrequencyUpdate on invisible furniture could also be even slower.

        // Update furniture outside of the camera view
        List<Furniture> tempFurnituresInvisible = new List<Furniture>(furnituresInvisible);
        foreach (Furniture furniture in tempFurnituresInvisible)
        {
            furniture.EveryFrameUpdate(deltaTime);
        }

        // Update all furniture with EventActions
        List<Furniture> tempFurnitures = new List<Furniture>(furnitures);
        foreach (Furniture furniture in tempFurnitures)
        {
            furniture.FixedFrequencyUpdate(deltaTime);
        }
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

    /// <summary>
    /// Notify world that the camera moved, so we can check which entities are visible to the camera.
    /// The invisible enities can be updated less frequent for better performance.
    /// </summary>
    public void OnCameraMoved(Bounds cameraBounds)
    {        
        // Expand bounds to include tiles on the edge where the centre isn't inside the bounds
        cameraBounds.Expand(1);

        foreach (Furniture furn in furnitures)
        {
            // Multitile furniture base tile is bottom left - so add width and height 
            Bounds furnitureBounds = new Bounds(
                new Vector3(furn.Tile.X - 0.5f + (furn.Width / 2), furn.Tile.Y - 0.5f + (furn.Height / 2), 0),
                new Vector3(furn.Width, furn.Height));

            if (cameraBounds.Intersects(furnitureBounds))
            {
                if (furnituresInvisible.Contains(furn))
                {
                    furnituresInvisible.Remove(furn);
                    furnituresVisible.Add(furn);
                }
            }
            else
            {
                if (furnituresVisible.Contains(furn))
                {
                    furnituresVisible.Remove(furn);
                    furnituresInvisible.Add(furn);
                }
            }            
        }
    }

    /// <summary>
    /// Writes the furniture to the XML.
    /// </summary>
    /// <param name="writer">The Xml Writer.</param>
    public void WriteXml(XmlWriter writer)
    {
        foreach (Furniture furn in furnitures)
        {
            writer.WriteStartElement("Furniture");
            furn.WriteXml(writer);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Called when a furniture is removed so that it can be deleted from the list.
    /// </summary>
    /// <param name="furnitures">The furniture being removed.</param>
    private void OnRemoved(Furniture furniture)
    {
        furnitures.Remove(furniture);

        if (furnituresInvisible.Contains(furniture))
        {
            furnituresInvisible.Remove(furniture);            
        }
        else if (furnituresVisible.Contains(furniture))
        {
            furnituresVisible.Remove(furniture);
        }
    }
}
