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
        world.FurnitureManager.Created += OnCreated;

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually.
        foreach (Furniture furniture in world.FurnitureManager)
        {
            OnCreated(furniture);
        }
    }

    public override void RemoveAll()
    {
        world.FurnitureManager.Created -= OnCreated;

        foreach (Furniture furniture in world.FurnitureManager)
        {
            furniture.Changed -= OnChanged;
            furniture.Removed -= OnRemoved;
            furniture.IsOperatingChanged -= OnIsOperatingChanged;
        }

        foreach (Furniture furniture in powerStatusGameObjectMap.Keys)
        {
            GameObject.Destroy(powerStatusGameObjectMap[furniture]);
        }
            
        powerStatusGameObjectMap.Clear();
        base.RemoveAll();
    }

    public Sprite GetSpriteForFurniture(string type)
    {
        Furniture proto = PrototypeManager.Furniture.Get(type);
        string spriteName = proto.GetSpriteName();
        Sprite s = SpriteManager.GetSprite("Furniture", spriteName + (proto.LinksToNeighbour != string.Empty && !proto.OnlyUseDefaultSpriteName ? "_" : string.Empty));

        return s;
    }

    public Sprite GetSpriteForFurniture(Furniture furniture)
    {
        string spriteName = furniture.GetSpriteName();

        if (furniture.LinksToNeighbour == string.Empty || furniture.OnlyUseDefaultSpriteName)
        {
            return SpriteManager.GetSprite("Furniture", spriteName);
        }

        // Otherwise, the sprite name is more complicated.
        spriteName += "_";

        // Check for neighbours North, East, South, West, Northeast, Southeast, Southwest, Northwest
        int x = furniture.Tile.X;
        int y = furniture.Tile.Y;
        string suffix = string.Empty;

        suffix += GetSuffixForNeighbour(furniture, x, y + 1, furniture.Tile.Z, "N");
        suffix += GetSuffixForNeighbour(furniture, x + 1, y, furniture.Tile.Z, "E");
        suffix += GetSuffixForNeighbour(furniture, x, y - 1, furniture.Tile.Z, "S");
        suffix += GetSuffixForNeighbour(furniture, x - 1, y, furniture.Tile.Z, "W");

        // Now we check if we have the neighbours in the cardinal directions next to the respective diagonals
        // because pure diagonal checking would leave us with diagonal walls and stockpiles, which make no sense.
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "E", furniture, x + 1, y + 1, furniture.Tile.Z);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "E", furniture, x + 1, y - 1, furniture.Tile.Z);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "W", furniture, x - 1, y - 1, furniture.Tile.Z);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "W", furniture, x - 1, y + 1, furniture.Tile.Z);

        // For example, if this object has all eight neighbours of
        // the same type, then the string will look like:
        //       Wall_NESWneseswnw
        return SpriteManager.GetSprite("Furniture", spriteName + suffix);
    }

    protected override void OnCreated(Furniture furniture)
    {
        // FIXME: Does not consider rotated objects
        GameObject furn_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(furniture, furn_go);

        // FIXME: This hardcoding is not ideal!
        if (furniture.HasTypeTag("Door"))
        {
            // Check to see if we actually have a wall north/south, and if so
            // set the furniture verticalDoor flag to true.
            Tile northTile = world.GetTileAt(furniture.Tile.X, furniture.Tile.Y + 1, furniture.Tile.Z);
            Tile southTile = world.GetTileAt(furniture.Tile.X, furniture.Tile.Y - 1, furniture.Tile.Z);

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

        furn_go.name = furniture.Type + "_" + furniture.Tile.X + "_" + furniture.Tile.Y;
        furn_go.transform.position = furniture.Tile.Vector3 + ImageUtils.SpritePivotOffset(sr.sprite, furniture.Rotation);
        furn_go.transform.Rotate(0, 0, furniture.Rotation);
        furn_go.transform.SetParent(objectParent.transform, true);

        sr.sortingOrder = Mathf.RoundToInt(furn_go.transform.position.y * -1);

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

        if (furniture.Animation != null)
        { 
            furniture.Animation.Renderer = sr;
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
            Tile northTile = world.GetTileAt(furn.Tile.X, furn.Tile.Y + 1, furn.Tile.Z);
            Tile southTile = world.GetTileAt(furn.Tile.X, furn.Tile.Y - 1, furn.Tile.Z);
            Tile eastTile = world.GetTileAt(furn.Tile.X + 1, furn.Tile.Y, furn.Tile.Z);
            Tile westTile = world.GetTileAt(furn.Tile.X - 1, furn.Tile.Y, furn.Tile.Z);

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

        // don't change sprites on furniture with animations
        if (furn.Animation != null)
        {
            furn.Animation.OnFurnitureChanged();
            return;
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

    private string GetSuffixForNeighbour(Furniture furn, int x, int y, int z, string suffix)
    {
         Tile t = world.GetTileAt(x, y, z);
         if (t != null && t.Furniture != null && t.Furniture.LinksToNeighbour == furn.LinksToNeighbour)
         {
             return suffix;
         }

        return string.Empty;
    }

    private string GetSuffixForDiagonalNeighbour(string suffix, string coord1, string coord2, Furniture furn, int x, int y, int z)
    {
        if (suffix.Contains(coord1) && suffix.Contains(coord2))
        {
            return GetSuffixForNeighbour(furn, x, y, z, coord1.ToLower() + coord2.ToLower());
        }

        return string.Empty;
    }

    private Sprite GetPowerStatusSprite()
    {
        return SpriteManager.GetSprite("Power", "PowerIcon");
    }
}
