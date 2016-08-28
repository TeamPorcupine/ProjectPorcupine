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
using System.Linq;
using UnityEngine;

public class FurnitureSpriteController
{
    private Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    private Dictionary<Furniture, GameObject> powerStatusGameObjectMap;

    private World world;
    private GameObject furnnitureParent;

    // Use this for initialization.
    public FurnitureSpriteController(World currentWorld)
    {
        world = currentWorld;

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();
        powerStatusGameObjectMap = new Dictionary<Furniture, GameObject>();
        furnnitureParent = new GameObject("Furniture");

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.OnFurnitureCreated += OnFurnitureCreated;

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually.
        foreach (Furniture furn in world.furnitures)
        {
            OnFurnitureCreated(furn);
        }
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        // Create a visual GameObject linked to this data.
        // FIXME: Does not consider multi-tile objects nor rotated objects.
        // This creates a new GameObject and adds it to our scene.
        GameObject furn_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        furnitureGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.ObjectType + "_" + furn.Tile.X + "_" + furn.Tile.Y;
        furn_go.transform.position = new Vector3(furn.Tile.X + ((furn.Width - 1) / 2f), furn.Tile.Y + ((furn.Height - 1) / 2f), 0);
        furn_go.transform.SetParent(furnnitureParent.transform, true);

        // FIXME: This hardcoding is not ideal!
        if (furn.HasTypeTag("Door"))
        {
            // Check to see if we actually have a wall north/south, and if so
            // set the furniture verticalDoor flag to true.
            Tile northTile = world.GetTileAt(furn.Tile.X, furn.Tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.Tile.X, furn.Tile.Y - 1);

            if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
                northTile.Furniture.HasTypeTag("Wall") && southTile.Furniture.HasTypeTag("Wall"))
            {
                furn.VerticalDoor = true;
            }
        }

        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furn);
        sr.sortingLayerName = "Furniture";
        sr.color = furn.Tint;

        if (furn.PowerValue < 0)
        {
            GameObject power_go = new GameObject();
            powerStatusGameObjectMap.Add(furn, power_go);
            power_go.transform.parent = furn_go.transform;
            power_go.transform.position = furn_go.transform.position;

            SpriteRenderer powerSR = power_go.AddComponent<SpriteRenderer>();
            powerSR.sprite = GetPowerStatusSprite();
            powerSR.sortingLayerName = "Power";
            powerSR.color = PowerStatusColor();

            if (world.powerSystem.PowerLevel > 0)
            {
                power_go.SetActive(false);
            }
            else
            {
                power_go.SetActive(true);
            }
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        furn.Changed += OnFurnitureChanged;
        world.powerSystem.PowerLevelChanged += OnPowerStatusChange;
        furn.Removed += OnFurnitureRemoved;
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

        // Check for neighbours North, East, South, West.
        int x = furn.Tile.X;
        int y = furn.Tile.Y;

        spriteName += GetSuffixForNeighbour(furn, x, y + 1, "N");
        spriteName += GetSuffixForNeighbour(furn, x + 1, y, "E");
        spriteName += GetSuffixForNeighbour(furn, x, y - 1, "S");
        spriteName += GetSuffixForNeighbour(furn, x - 1, y, "W");

        // For example, if this object has all four neighbours of
        // the same type, then the string will look like:
        //       Wall_NESW
        return SpriteManager.current.GetSprite("Furniture", spriteName);
    }

    public Sprite GetSpriteForFurniture(string objectType)
    {
        Sprite s = SpriteManager.current.GetSprite("Furniture", objectType + (World.Current.furniturePrototypes[objectType].LinksToNeighbour ? "_" : string.Empty));

        return s;
    }

    private void OnFurnitureRemoved(Furniture furn)
    {
        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.ULogErrorChannel("FurnitureSpriteController", "OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        GameObject.Destroy(furn_go);
        furnitureGameObjectMap.Remove(furn);

        if (powerStatusGameObjectMap.ContainsKey(furn) == false)
        {
            return;
        }

        powerStatusGameObjectMap.Remove(furn);
    }

    private void OnFurnitureChanged(Furniture furn)
    {
        // Make sure the furniture's graphics are correct.
        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.ULogErrorChannel("FurnitureSpriteController", "OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];

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

    private void OnPowerStatusChange(IPowerRelated powerRelated)
    {
        Furniture furn = powerRelated as Furniture;
        if (furn == null)
        {
            return;
        }

        if (powerStatusGameObjectMap.ContainsKey(furn) == false)
        {
            return;
        }

        GameObject power_go = powerStatusGameObjectMap[furn];

        if (world.powerSystem.PowerLevel > 0)
        {
            power_go.SetActive(false);
        }
        else
        {
            power_go.SetActive(true);
        }

        power_go.GetComponent<SpriteRenderer>().color = PowerStatusColor();
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

    private Sprite GetPowerStatusSprite()
    {
        return SpriteManager.current.GetSprite("Power", "PowerIcon");
    }

    private Color PowerStatusColor()
    {
        if (world.powerSystem.PowerLevel > 0)
        {
            return Color.green;
        }
        else
        {
            return Color.red;
        }
    }
}
