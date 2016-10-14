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

public class UtilityManager : IEnumerable<Utility>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityManager"/> class.
    /// </summary>
    public UtilityManager()
    {
        Utilities = new List<Utility>();
    }

    public List<Utility> Utilities { get; private set; }

    /// <summary>
    /// Occurs when a utility is created.
    /// </summary>
    public event Action<Utility> Created;

    /// <summary>
    /// Creates a utility with the given type and places it at the given tile.
    /// </summary>
    /// <returns>The utility.</returns>
    /// <param name="type">The type of the utility.</param>
    /// <param name="tile">The tile to place the utility at.</param>
    /// <param name="skipGridUpdate">If set to <c>true</c> don't build the network, in which case the network will need explictly built.</param>
    public Utility PlaceUtility(string type, Tile tile, bool skipGridUpdate = false)
    {
        if (PrototypeManager.Utility.Has(type) == false)
        {
            Debug.ULogErrorChannel("World", "PrototypeManager.Utility doesn't contain a proto for key: " + type);
            return null;
        }

        Utility utility = PrototypeManager.Utility.Get(type);

        return PlaceUtility(utility, tile, skipGridUpdate);
    }

    /// <summary>
    /// Places the given utility prototype at the given tile.
    /// </summary>
    /// <returns>The utility.</returns>
    /// <param name="prototype">The utility prototype.</param>
    /// <param name="tile">The tile to place the utility at.</param>
    /// <param name="skipGridUpdate">If set to <c>true</c> don't build the network, in which case the network will need explictly built.</param>
    public Utility PlaceUtility(Utility prototype, Tile tile, bool skipGridUpdate = false)
    {
        Utility utility = Utility.PlaceInstance(prototype, tile, skipGridUpdate);

        if (utility == null)
        {
            // Failed to place object -- most likely there was already something there.
            return null;
        }

        utility.Removed += OnRemoved;
        Utilities.Add(utility);

        if (Created != null)
        {
            Created(utility);
        }

        return utility;
    }

    /// <summary>
    /// Calls the utility update function on every frame.
    /// The list needs to be copied temporarily in case furnitures are added or removed during the update.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void TickEveryFrame(float deltaTime)
    {
        List<Utility> tempUtilities = Utilities;
        foreach (Utility utility in tempUtilities)
        {
            utility.EveryFrameUpdate(deltaTime);
        }
    }

    /// <summary>
    /// Calls the furnitures update function on every frame.
    /// The list needs to be copied temporarily in case furnitures are added or removed during the update.
    /// </summary>
    /// <param name="deltaTime">Delta time.</param>
    public void TickFixedFrequency(float deltaTime)
    {
        List<Utility> tempUtilities = Utilities;
        foreach (Utility utility in tempUtilities)
        {
            utility.FixedFrequencyUpdate(deltaTime);
        }
    }

    /// <summary>
    /// When a construction job is completed, place the utility.
    /// </summary>
    /// <param name="job">The completed job.</param>
    public void ConstructJobCompleted(Job job)
    {
        PlaceUtility(job.JobObjectType, job.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        job.tile.PendingBuildJob = null;
    }

    /// <summary>
    /// Determines whether the placement of a utility with the given type at the given tile is valid.
    /// </summary>
    /// <returns><c>true</c> if the placement is valid; otherwise, <c>false</c>.</returns>
    /// <param name="type">The utility type.</param>
    /// <param name="tile">The tile where the utility will be placed.</param>
    public bool IsPlacementValid(string type, Tile tile)
    {
        return PrototypeManager.Utility.Get(type).IsValidPosition(tile);
    }

    /// <summary>
    /// Gets the utilities enumerator.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator GetEnumerator()
    {
        return Utilities.GetEnumerator();
    }

    /// <summary>
    /// Gets each utility.
    /// </summary>
    /// <returns>Each utility.</returns>
    IEnumerator<Utility> IEnumerable<Utility>.GetEnumerator()
    {
        foreach (Utility utility in Utilities)
        {
            yield return utility;
        }
    }

    /// <summary>
    /// Writes the utilities to the XML.
    /// </summary>
    /// <param name="writer">The Xml Writer.</param>
    public void WriteXml(XmlWriter writer)
    {
        foreach (Utility utility in Utilities)
        {
            writer.WriteStartElement("Utility");
            utility.WriteXml(writer);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Called when a utility is removed so that it can be deleted from the list.
    /// </summary>
    /// <param name="utility">The utility being removed.</param>
    private void OnRemoved(Utility utility)
    {
        Utilities.Remove(utility);
    }
}
