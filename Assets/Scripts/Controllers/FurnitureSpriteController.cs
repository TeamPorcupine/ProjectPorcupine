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

    public void OnFurnitureCreated(Furniture furniture)
    {
        // Create a visual GameObject linked to this data.
        // FIXME: Does not consider multi-tile objects nor rotated objects.
        // This creates a new GameObject and adds it to our scene.
        GameObject furn_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        furnitureGameObjectMap.Add(furniture, furn_go);

        furn_go.name = furniture.ObjectType + "_" + furniture.Tile.X + "_" + furniture.Tile.Y;
        furn_go.transform.position = new Vector3(furniture.Tile.X + ((furniture.Width - 1) / 2f), furniture.Tile.Y + ((furniture.Height - 1) / 2f), 0);
        furn_go.transform.SetParent(furnnitureParent.transform, true);

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
        furniture.Changed += OnFurnitureChanged;
        furniture.IsOperatingChanged += OnIsOperatingChanged;
        furniture.Removed += OnFurnitureRemoved;
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

    private void OnFurnitureRemoved(Furniture furniture)
    {
        if (furnitureGameObjectMap.ContainsKey(furniture) == false)
        {
            Debug.ULogErrorChannel("FurnitureSpriteController", "OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furniture];
        GameObject.Destroy(furn_go);
        furnitureGameObjectMap.Remove(furniture);

        if (powerStatusGameObjectMap.ContainsKey(furniture) == false)
        {
            return;
        }

        powerStatusGameObjectMap.Remove(furniture);

        furniture.Changed -= OnFurnitureChanged;
        furniture.IsOperatingChanged -= OnIsOperatingChanged;
        furniture.Removed -= OnFurnitureRemoved;
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

    private Sprite GetPowerStatusSprite()
    {
        return SpriteManager.current.GetSprite("Power", "PowerIcon");
    }
}
