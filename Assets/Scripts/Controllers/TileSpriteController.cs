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

public class TileSpriteController
{

    Dictionary<Tile, GameObject> tileGameObjectMap;

    World world;

    // Use this for initialization
    public TileSpriteController(World currnetWorld)
    {
        world = currnetWorld;
    }

    public void Render() {
        GameObject tileParent = new GameObject("Tiles");

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create a GameObject for each of our tiles, so they show visually. (and redunt reduntantly)
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                // Get the tile data
                Tile tile_data = world.GetTileAt(x, y);

                // This creates a new GameObject and adds it to our scene.
                GameObject tile_go = new GameObject();

                // Add our tile/GO pair to the dictionary.
                tileGameObjectMap.Add(tile_data, tile_go);

                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_go.transform.SetParent(tileParent.transform, true);

                // Add a Sprite Renderer
                // Add a default sprite for empty tiles.
                SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteManager.current.GetSprite("Tile", "Empty");
                sr.sortingLayerName = "Tiles";

                OnTileChanged(tile_data);
            }
        }

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.cbTileChanged += OnTileChanged;
    }

    // THIS IS AN EXAMPLE -- NOT CURRENTLY USED (and probably out of date)
    void DestroyAllTileGameObjects()
    {
        // This function might get called when we are changing floors/levels.
        // We need to destroy all visual **GameObjects** -- but not the actual tile data!

        while (tileGameObjectMap.Count > 0)
        {
            Tile tile_data = tileGameObjectMap.Keys.First();
            GameObject tile_go = tileGameObjectMap[tile_data];

            // Remove the pair from the map
            tileGameObjectMap.Remove(tile_data);

            // Unregister the callback!
            tile_data.cbTileChanged -= OnTileChanged;

            // Destroy the visual GameObject
            GameObject.Destroy(tile_go);
        }

        // Presumably, after this function gets called, we'd be calling another
        // function to build all the GameObjects for the tiles on the new floor/level
    }

    // This function should be called automatically whenever a tile's data gets changed.
    void OnTileChanged(Tile tile_data)
    {

        if (tileGameObjectMap.ContainsKey(tile_data) == false)
        {
            Debug.LogError("tileGameObjectMap doesn't contain the tile_data -- did you forget to add the tile to the dictionary? Or maybe forget to unregister a callback?");
            return;
        }

        GameObject tile_go = tileGameObjectMap[tile_data];

        if (tile_go == null)
        {
            Debug.LogError("tileGameObjectMap's returned GameObject is null -- did you forget to add the tile to the dictionary? Or maybe forget to unregister a callback?");
            return;
        }

        if (tile_data.Type == TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = SpriteManager.current.GetSprite("Tile", "Floor");
        }
        else if (tile_data.Type == TileType.Ladder)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = SpriteManager.current.GetSprite("Tile", "Ladder");
        }
        else if (tile_data.Type == TileType.Empty)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = SpriteManager.current.GetSprite("Tile", "Empty");
        }
        else
        {
            Debug.LogError("OnTileChanged - Unrecognized tile type.");
        }


    }



}
