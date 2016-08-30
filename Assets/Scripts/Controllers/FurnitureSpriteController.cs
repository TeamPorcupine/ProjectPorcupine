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

public class FurnitureSpriteController : BaseSpriteController<Furniture>
{
    private Dictionary<Furniture, GameObject> powerStatusGameObjectMap;

    // Use this for initialization
    public FurnitureSpriteController(World world) : base(world, "Furniture")
    {
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        powerStatusGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.OnFurnitureCreated += OnCreated;

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually.
        foreach (Furniture furn in world.furnitures)
        {
            OnCreated(furn);
        }
    }

    public override void RemoveAll()
    {
        world.OnFurnitureCreated -= OnCreated;

        foreach (Furniture furn in world.furnitures)
        {
            furn.Changed -= OnChanged;
            furn.Removed -= OnRemoved;
            furn.IsOperatingChanged -= OnIsOperatingChanged;
        }

        foreach (Furniture furn in powerStatusGameObjectMap.Keys)
        {
            GameObject.Destroy(powerStatusGameObjectMap[furn]);
        }
            
        powerStatusGameObjectMap.Clear();
        base.RemoveAll();
    }

    public Sprite GetSpriteForFurniture(string objectType)
    {
        Furniture proto = PrototypeManager.Furniture.GetPrototype(objectType);
        Sprite s = SpriteManager.current.GetSprite("Furniture", objectType + (proto.LinksToNeighbour ? "_" : string.Empty));

        return s;
    }

    public Sprite GetSpriteForFurniture(Furniture furn)
    {
        string spriteName = furn.GetSpriteName();

        if (furn.LinksToNeighbour == false)
        {
            return SpriteManager.current.GetSprite("Furniture", spriteName);
        }

        // Otherwise, the sprite name is more complicated.
        spriteName += "_";

        // Check for neighbours North, East, South, West, Northeast, Southeast, Southwest, Northwest
        int x = furn.Tile.X;
        int y = furn.Tile.Y;
        string suffix = string.Empty;

        suffix += GetSuffixForNeighbour(furn, x, y + 1, "N");
        suffix += GetSuffixForNeighbour(furn, x + 1, y, "E");
        suffix += GetSuffixForNeighbour(furn, x, y - 1, "S");
        suffix += GetSuffixForNeighbour(furn, x - 1, y, "W");

        // Now we check if we have the neighbours in the cardinal directions next to the respective diagonals
        // because pure diagonal checking would leave us with diagonal walls and stockpiles, which make no sense.
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "E", furn, x + 1, y + 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "E", furn, x + 1, y - 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "W", furn, x - 1, y - 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "W", furn, x - 1, y + 1);

        // For example, if this object has all eight neighbours of
        // the same type, then the string will look like:
        //       Wall_NESWneseswnw
        return SpriteManager.current.GetSprite("Furniture", spriteName + suffix);
    }

    protected override void OnCreated(Furniture furniture)
    {
        // FIXME: Does not consider multi-tile objects nor rotated objects
        GameObject furn_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(furniture, furn_go);

        furn_go.name = furniture.ObjectType + "_" + furniture.Tile.X + "_" + furniture.Tile.Y;
        furn_go.transform.position = new Vector3(furniture.Tile.X + ((furniture.Width - 1) / 2f), furniture.Tile.Y + ((furniture.Height - 1) / 2f), 0);
        furn_go.transform.SetParent(objectParent.transform, true);

        // FIXME: This hardcoding is not ideal!
        if (furniture.HasTypeTag("Door"))
        {
            // Check to see if we actually have a wall north/south, and if so
            // set the furniture verticalDoor flag to true.
            Tile northTile = world.GetTileAt(furniture.Tile.X, furniture.Tile.Y + 1);
            Tile southTile = world.GetTileAt(furniture.Tile.X, furniture.Tile.Y - 1);

            if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
                northTile.Furniture.HasTypeTag("Wall") && southTile.Furniture.HasTypeTag("Wall"))
            {
                furniture.VerticalDoor = true;
            }
        }

        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furniture);
        sr.sortingLayerName = "Furniture";
        sr.color = furniture.Tint;

        if (furniture.PowerConnection != null && furniture.PowerConnection.IsPowerConsumer)
        {
            GameObject powerGameObject = new GameObject();
            powerStatusGameObjectMap.Add(furniture, powerGameObject);
            powerGameObject.transform.parent = furn_go.transform;
            powerGameObject.transform.position = furn_go.transform.position;

            SpriteRenderer powerSpriteRenderer = powerGameObject.AddComponent<SpriteRenderer>();
            powerSpriteRenderer.sprite = GetPowerStatusSprite();
            powerSpriteRenderer.sortingLayerName = "Power";
            powerSpriteRenderer.color = Color.red;

            if (furniture.IsOperating)
            {
                powerGameObject.SetActive(false);
            }
            else
            {
                powerGameObject.SetActive(true);
            }
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        furniture.Changed += OnChanged;
        furniture.Removed += OnRemoved;
        furniture.IsOperatingChanged += OnIsOperatingChanged;
    }

    protected override void OnChanged(Furniture furn)
    {
        // Make sure the furniture's graphics are correct.
        if (objectGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.ULogErrorChannel("FurnitureSpriteController", "OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = objectGameObjectMap[furn];

        if (furn.HasTypeTag("Door"))
        {
            // Check to see if we actually have a wall north/south, and if so
            // set the furniture verticalDoor flag to true.
            Tile northTile = world.GetTileAt(furn.Tile.X, furn.Tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.Tile.X, furn.Tile.Y - 1);
            Tile eastTile = world.GetTileAt(furn.Tile.X + 1, furn.Tile.Y);
            Tile westTile = world.GetTileAt(furn.Tile.X - 1, furn.Tile.Y);

            if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
                northTile.Furniture.HasTypeTag("Wall") && southTile.Furniture.HasTypeTag("Wall"))
            {
                furn.VerticalDoor = true;
            }
            else if (eastTile != null && westTile != null && eastTile.Furniture != null && westTile.Furniture != null &&
                eastTile.Furniture.HasTypeTag("Wall") && westTile.Furniture.HasTypeTag("Wall"))
            {
                furn.VerticalDoor = false;
            }
        }

        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        furn_go.GetComponent<SpriteRenderer>().color = furn.Tint;
    }
        
    protected override void OnRemoved(Furniture furn)
    {
        if (objectGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.ULogErrorChannel("FurnitureSpriteController", "OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        furn.Changed -= OnChanged;
        furn.Removed -= OnRemoved;
        furn.IsOperatingChanged -= OnIsOperatingChanged;
        GameObject furn_go = objectGameObjectMap[furn];
        objectGameObjectMap.Remove(furn);
        GameObject.Destroy(furn_go);

        if (powerStatusGameObjectMap.ContainsKey(furn) == false)
        {
            return;
        }

        powerStatusGameObjectMap.Remove(furn);
    }
        
    private void OnIsOperatingChanged(Furniture furniture)
    {
        if (furniture == null)
        {
            return;
        }

        if (powerStatusGameObjectMap.ContainsKey(furniture) == false)
        {
            return;
        }

        GameObject powerGameObject = powerStatusGameObjectMap[furniture];
        if (furniture.IsOperating)
        {
            powerGameObject.SetActive(false);
        }
        else
        {
            powerGameObject.SetActive(true);
        }
    }

    private string GetSuffixForNeighbour(Furniture furn, int x, int y, string suffix)
    {
         Tile t = world.GetTileAt(x, y);
         if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
         {
             return suffix;
         }

        return string.Empty;
    }

    private string GetSuffixForDiagonalNeighbour(string suffix, string coord1, string coord2, Furniture furn, int x, int y)
    {
        if (suffix.Contains(coord1) && suffix.Contains(coord2))
        {
            return GetSuffixForNeighbour(furn, x, y, coord1.ToLower() + coord2.ToLower());
        }

        return string.Empty;
    }

    private Sprite GetPowerStatusSprite()
    {
        return SpriteManager.current.GetSprite("Power", "PowerIcon");
    }
}
