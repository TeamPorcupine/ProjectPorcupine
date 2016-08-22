#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpriteController : MonoBehaviour
{

    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    Dictionary<Furniture, GameObject> powerStatusGameObjectMap;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Use this for initialization
    void Start()
    {
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();
        powerStatusGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.cbFurnitureCreated += OnFurnitureCreated;

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually
        foreach (Furniture furn in world.furnitures)
        {
            OnFurnitureCreated(furn);
        }
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        //Logger.Log("OnFurnitureCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject furn_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        furnitureGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X + ((furn.Width - 1) / 2f), furn.tile.Y + ((furn.Height - 1) / 2f), 0);
        furn_go.transform.SetParent(this.transform, true);

        // FIXME: This hardcoding is not ideal!
        if (furn.objectType == "Door" || furn.objectType == "Airlock")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees

            Tile northTile = world.GetTileAt(furn.tile.X, furn.tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.tile.X, furn.tile.Y - 1);

            if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
            northTile.furniture.objectType.Contains("Wall") && southTile.furniture.objectType.Contains("Wall"))
            {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }



        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furn);
        sr.sortingLayerName = "Furniture";
        sr.color = furn.tint;

        if (furn.powerValue < 0) 
        {
            GameObject power_go = new GameObject();
            powerStatusGameObjectMap.Add(furn, power_go);
            power_go.transform.parent = furn_go.transform;
            power_go.transform.position = furn_go.transform.position;

            SpriteRenderer powerSR = power_go.AddComponent<SpriteRenderer>();
            powerSR.sprite = GetPowerStatusSprite();
            powerSR.sortingLayerName = "Power";
            powerSR.color = PowerStatusColor();

            if (world.powerSystem.PowerLevel > 0) {
                power_go.SetActive(false);
            }
            else {
                power_go.SetActive(true);
            }

        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        furn.cbOnChanged += OnFurnitureChanged;
        world.powerSystem.PowerLevelChanged += OnPowerStatusChange;
        furn.cbOnRemoved += OnFurnitureRemoved;

    }

    void OnFurnitureRemoved(Furniture furn)
    {
        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Logger.LogError("OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        Destroy(furn_go);
        furnitureGameObjectMap.Remove(furn);

        if (powerStatusGameObjectMap.ContainsKey(furn) == false)
            return;

        powerStatusGameObjectMap.Remove(furn);
    }

    void OnFurnitureChanged(Furniture furn)
    {
        //Logger.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Logger.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];

        // FIXME: This hardcoding is not ideal!
        if (furn.objectType == "Door" || furn.objectType == "Airlock")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees

            Tile northTile = world.GetTileAt(furn.tile.X, furn.tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.tile.X, furn.tile.Y - 1);
            Tile eastTile = world.GetTileAt(furn.tile.X + 1, furn.tile.Y);
            Tile westTile = world.GetTileAt(furn.tile.X - 1, furn.tile.Y);

            if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
            northTile.furniture.objectType.Contains("Wall") && southTile.furniture.objectType.Contains("Wall"))
            {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (eastTile != null && westTile != null && eastTile.furniture != null && westTile.furniture != null &&
            eastTile.furniture.objectType.Contains("Wall") && westTile.furniture.objectType.Contains("Wall"))
            {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        furn_go.GetComponent<SpriteRenderer>().color = furn.tint;                   

    }

    void OnPowerStatusChange(Furniture furn) 
    {
        if (powerStatusGameObjectMap.ContainsKey(furn) == false)
            return;

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


    public Sprite GetSpriteForFurniture(Furniture furn)
    {
        string spriteName = furn.objectType;

        if (furn.linksToNeighbour == false)
        {

            // If this is a DOOR, let's check OPENNESS and update the sprite.
            // FIXME: All this hardcoding needs to be generalized later.
            if (furn.objectType == "Door")
            {
                if (furn.GetParameter("openness") < 0.1f)
                {
                    // Door is closed
                    spriteName = "Door";
                }
                else if (furn.GetParameter("openness") < 0.5f)
                {
                    // Door is a bit open
                    spriteName = "Door_openness_1";
                }
                else if (furn.GetParameter("openness") < 0.9f)
                {
                    // Door is a lot open
                    spriteName = "Door_openness_2";
                }
                else
                {
                    // Door is a fully open
                    spriteName = "Door_openness_3";
                }
                //Logger.Log(spriteName);
            }
            if (furn.objectType == "Airlock")
            {
                if (furn.GetParameter("openness") < 0.1f)
                {
                    // Airlock is closed
                    spriteName = "Airlock";
                }
                else if (furn.GetParameter("openness") < 0.5f)
                {
                    // Airlock is a bit open
                    spriteName = "Airlock_openness_1";
                }
                else if (furn.GetParameter("openness") < 0.9f)
                {
                    // Airlock is a lot open
                    spriteName = "Airlock_openness_2";
                }
                else
                {
                    // Airlock is a fully open
                    spriteName = "Airlock_openness_3";
                }
                //Logger.Log(spriteName);
            }

            /*if(furnitureSprites.ContainsKey(spriteName) == false) {
				Logger.Log("furnitureSprites has no definition for: " + spriteName);
				return null;
			}
*/

            return SpriteManager.current.GetSprite("Furniture", spriteName); // furnitureSprites[spriteName];
        }

        // Otherwise, the sprite name is more complicated.

        spriteName = furn.objectType + "_";

        // Check for neighbours North, East, South, West

        int x = furn.tile.X;
        int y = furn.tile.Y;

        Tile t;

        t = world.GetTileAt(x, y + 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "N";
        }
        t = world.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "E";
        }
        t = world.GetTileAt(x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "S";
        }
        t = world.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "W";
        }

        // For example, if this object has all four neighbours of
        // the same type, then the string will look like:
        //       Wall_NESW

/*		if(furnitureSprites.ContainsKey(spriteName) == false) {
			Logger.LogError("GetSpriteForInstalledObject -- No sprites with name: " + spriteName);
			return null;
		}
*/

        return SpriteManager.current.GetSprite("Furniture", spriteName); //furnitureSprites[spriteName];

    }

    Sprite GetPowerStatusSprite() 
    {
        return SpriteManager.current.GetSprite("Power", "PowerIcon");
    }

    Color PowerStatusColor() 
    {
        if (world.powerSystem.PowerLevel > 0)
            return Color.green;
        else
            return Color.red;
    }

    public Sprite GetSpriteForFurniture(string objectType)
    {
        Sprite s = SpriteManager.current.GetSprite("Furniture", objectType + (World.current.furniturePrototypes[objectType].linksToNeighbour ? "_" : ""));
        
        return s;
    }
}
