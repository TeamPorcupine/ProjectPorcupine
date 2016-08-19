﻿//=======================================================================
// Copyright Martin "quill18" Glaude 2015.
//		http://quill18.com
//=======================================================================

using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpriteController : MonoBehaviour
{

    Dictionary<Furniture, GameObject> furnitureGameObjectMap;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Use this for initialization
    void Start()
    {
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.FurnitureCreated += OnFurnitureCreated;

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually
        foreach (Furniture furn in world.Furnitures)
        {
            OnFurnitureCreated(furn);
        }
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        //Debug.Log("OnFurnitureCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject furn_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        furnitureGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.ObjectType + "_" + furn.Tile.X + "_" + furn.Tile.Y;
        furn_go.transform.position = new Vector3(furn.Tile.X + ((furn.Width - 1) / 2f), furn.Tile.Y + ((furn.Height - 1) / 2f), 0);
        furn_go.transform.SetParent(this.transform, true);

        // FIXME: This hardcoding is not ideal!
        if (furn.ObjectType == "Door" || furn.ObjectType == "Airlock")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees

            Tile northTile = world.GetTileAt(furn.Tile.X, furn.Tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.Tile.X, furn.Tile.Y - 1);

            if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
            northTile.Furniture.ObjectType.Contains("Wall") && southTile.Furniture.ObjectType.Contains("Wall"))
            {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }



        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furn);
        sr.sortingLayerName = "Furniture";
        sr.color = furn.tint;

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        furn.OnChanged += OnFurnitureChanged;
        furn.OnRemoved += OnFurnitureRemoved;

    }

    void OnFurnitureRemoved(Furniture furn)
    {
        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        Destroy(furn_go);
        furnitureGameObjectMap.Remove(furn);
    }

    void OnFurnitureChanged(Furniture furn)
    {
        //Debug.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        //Debug.Log(furn_go);
        //Debug.Log(furn_go.GetComponent<SpriteRenderer>());

        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        furn_go.GetComponent<SpriteRenderer>().color = furn.tint;

    }




    public Sprite GetSpriteForFurniture(Furniture furn)
    {
        string spriteName = furn.ObjectType;

        if (furn.LinksToNeighbour == false)
        {

            // If this is a DOOR, let's check OPENNESS and update the sprite.
            // FIXME: All this hardcoding needs to be generalized later.
            if (furn.ObjectType == "Door")
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
                //Debug.Log(spriteName);
            }
            if (furn.ObjectType == "Airlock")
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
                //Debug.Log(spriteName);
            }

            /*if(furnitureSprites.ContainsKey(spriteName) == false) {
				Debug.Log("furnitureSprites has no definition for: " + spriteName);
				return null;
			}
*/

            return SpriteManager.current.GetSprite("Furniture", spriteName); // furnitureSprites[spriteName];
        }

        // Otherwise, the sprite name is more complicated.

        spriteName = furn.ObjectType + "_";

        // Check for neighbours North, East, South, West

        int x = furn.Tile.X;
        int y = furn.Tile.Y;

        Tile t;

        t = world.GetTileAt(x, y + 1);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
        {
            spriteName += "N";
        }
        t = world.GetTileAt(x + 1, y);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
        {
            spriteName += "E";
        }
        t = world.GetTileAt(x, y - 1);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
        {
            spriteName += "S";
        }
        t = world.GetTileAt(x - 1, y);
        if (t != null && t.Furniture != null && t.Furniture.ObjectType == furn.ObjectType)
        {
            spriteName += "W";
        }

        // For example, if this object has all four neighbours of
        // the same type, then the string will look like:
        //       Wall_NESW

/*		if(furnitureSprites.ContainsKey(spriteName) == false) {
			Debug.LogError("GetSpriteForInstalledObject -- No sprites with name: " + spriteName);
			return null;
		}
*/

        return SpriteManager.current.GetSprite("Furniture", spriteName); //furnitureSprites[spriteName];

    }


    public Sprite GetSpriteForFurniture(string objectType)
    {
        Sprite s = SpriteManager.current.GetSprite("Furniture", objectType);

        if (s == null)
        {
            s = SpriteManager.current.GetSprite("Furniture", objectType + "_");
        }

        return s;
    }
}
