#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using UnityEngine;

public class UtilitySpriteController : BaseSpriteController<Utility>
{
    // Use this for initialization
    public UtilitySpriteController(World world) : base(world, "Utility")
    {
        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.UtilityManager.Created += OnCreated;

        // Go through any EXISTING utility (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually.
        foreach (Utility utility in world.UtilityManager)
        {
            OnCreated(utility);
        }
    }

    public override void RemoveAll()
    {
        world.UtilityManager.Created -= OnCreated;

        foreach (Utility utility in world.UtilityManager)
        {
            utility.Changed -= OnChanged;
            utility.Removed -= OnRemoved;
        }

        base.RemoveAll();
    }

    public Sprite GetSpriteForUtility(string objectType)
    {
        Utility proto = PrototypeManager.Utility.Get(objectType);
        Sprite sprite = SpriteManager.GetSprite("Utility", objectType + (proto.LinksToNeighbour ? "_" : string.Empty));

        return sprite;
    }

    public Sprite GetSpriteForUtility(Utility util)
    {
        string spriteName = util.GetSpriteName();

        if (util.LinksToNeighbour == false)
        {
            return SpriteManager.GetSprite("Utility", spriteName);
        }

        // Otherwise, the sprite name is more complicated.
        spriteName += "_";

        // Check for neighbours North, East, South, West, Northeast, Southeast, Southwest, Northwest
        int x = util.Tile.X;
        int y = util.Tile.Y;
        string suffix = string.Empty;

        suffix += GetSuffixForNeighbour(util, x, y + 1, util.Tile.Z, "N");
        suffix += GetSuffixForNeighbour(util, x + 1, y, util.Tile.Z, "E");
        suffix += GetSuffixForNeighbour(util, x, y - 1, util.Tile.Z, "S");
        suffix += GetSuffixForNeighbour(util, x - 1, y, util.Tile.Z, "W");

        // For example, if this object has all eight neighbours of
        // the same type, then the string will look like:
        //       Wall_NESWneseswnw
        return SpriteManager.GetSprite("Utility", spriteName + suffix);
    }

    protected override void OnCreated(Utility utility)
    {
        GameObject util_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(utility, util_go);

        util_go.name = utility.Type + "_" + utility.Tile.X + "_" + utility.Tile.Y;
        util_go.transform.position = new Vector3(utility.Tile.X, utility.Tile.Y, utility.Tile.Z);
        util_go.transform.SetParent(objectParent.transform, true);

        SpriteRenderer spriteRenderer = util_go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSpriteForUtility(utility);
        spriteRenderer.sortingLayerName = "Utility";
        spriteRenderer.color = utility.Tint;

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        utility.Changed += OnChanged;
        utility.Removed += OnRemoved;
    }

    protected override void OnChanged(Utility util)
    {
        // Make sure the utility's graphics are correct.
        if (objectGameObjectMap.ContainsKey(util) == false)
        {
            UnityDebugger.Debugger.LogError("UtilitySpriteController", "OnUtilityChanged -- trying to change visuals for utility not in our map.");
            return;
        }

        GameObject util_go = objectGameObjectMap[util];

        util_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForUtility(util);
        util_go.GetComponent<SpriteRenderer>().color = util.Tint;
    }
        
    protected override void OnRemoved(Utility util)
    {
        if (objectGameObjectMap.ContainsKey(util) == false)
        {
            UnityDebugger.Debugger.LogError("UtilitySpriteController", "OnUtilityRemoved -- trying to change visuals for utility not in our map.");
            return;
        }

        util.Changed -= OnChanged;
        util.Removed -= OnRemoved;
        GameObject util_go = objectGameObjectMap[util];
        objectGameObjectMap.Remove(util);
        GameObject.Destroy(util_go);
    }

    private string GetSuffixForNeighbour(Utility util, int x, int y, int z, string suffix)
    {
        Tile tile = world.GetTileAt(x, y, z);
        if (tile != null && tile.Utilities != null && tile.Utilities.ContainsKey(util.Type))
         {
             return suffix;
         }

        return string.Empty;
    }
}
