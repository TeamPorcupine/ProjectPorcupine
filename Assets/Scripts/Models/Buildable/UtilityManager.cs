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
using Newtonsoft.Json.Linq;

public class UtilityManager : IEnumerable<Utility>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityManager"/> class.
    /// </summary>
    public UtilityManager()
    {
        Utilities = new List<Utility>();
    }

    /// <summary>
    /// Occurs when a utility is created.
    /// </summary>
    public event Action<Utility> Created;

    public List<Utility> Utilities { get; private set; }

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
            UnityDebugger.Debugger.LogError("World", "PrototypeManager.Utility doesn't contain a proto for key: " + type);
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
    /// When a construction job is completed, place the utility.
    /// </summary>
    /// <param name="job">The completed job.</param>
    public void ConstructJobCompleted(Job job)
    {
        PlaceUtility(job.Type, job.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        job.tile.PendingBuildJobs.Remove(job);
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

    public JToken ToJson()
    {
        JArray utilitiesJson = new JArray();
        foreach (Utility utility in Utilities)
        {
            utilitiesJson.Add(utility.ToJSon());
        }

        return utilitiesJson;
    }

    public void FromJson(JToken utilitiesToken)
    {
        JArray utilitiesJArray = (JArray)utilitiesToken;

        foreach (JToken utilityToken in utilitiesJArray)
        {
            int x = (int)utilityToken["X"];
            int y = (int)utilityToken["Y"];
            int z = (int)utilityToken["Z"];
            string type = (string)utilityToken["Type"];
            Utility utility = PlaceUtility(type, World.Current.GetTileAt(x, y, z), true);
            utility.FromJson(utilityToken);
        }

        foreach (Utility utility in Utilities)
        {
            utility.UpdateGrid(utility);
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
