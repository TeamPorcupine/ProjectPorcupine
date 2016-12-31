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
using ProjectPorcupine.Buildable.Components;
using UnityEngine;

public class FurnitureSpriteController : BaseSpriteController<Furniture>
{
    private const float IndicatorOffset = 0.25f;
    private const float IndicatorScale = 0.5f;

    private Dictionary<Furniture, FurnitureChildObjects> childObjectMap;

    private Dictionary<BuildableComponent.Requirements, Vector3> statusIndicatorOffsets;
    
    // Use this for initialization
    public FurnitureSpriteController(World world) : base(world, "Furniture")
    {
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        childObjectMap = new Dictionary<Furniture, FurnitureChildObjects>();

        statusIndicatorOffsets = new Dictionary<BuildableComponent.Requirements, Vector3>();
        statusIndicatorOffsets[BuildableComponent.Requirements.Power] = new Vector3(IndicatorOffset, -IndicatorOffset, 0);
        statusIndicatorOffsets[BuildableComponent.Requirements.Fluid] = new Vector3(-IndicatorOffset, -IndicatorOffset, 0);
        statusIndicatorOffsets[BuildableComponent.Requirements.Gas] = new Vector3(IndicatorOffset, IndicatorOffset, 0);
        statusIndicatorOffsets[BuildableComponent.Requirements.Production] = new Vector3(-IndicatorOffset, IndicatorOffset, 0);

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

        foreach (FurnitureChildObjects childObjects in childObjectMap.Values)
        {
            childObjects.Destroy();
        }

        childObjectMap.Clear();
        base.RemoveAll();
    }

    public Sprite GetSpriteForFurniture(string type)
    {
        Furniture proto = PrototypeManager.Furniture.Get(type);
        Sprite s = SpriteManager.GetSprite("Furniture", proto.GetDefaultSpriteName());

        return s;
    }

    public Sprite GetSpriteForFurniture(Furniture furniture)
    {
        bool explicitSpriteUsed;
        string spriteName = furniture.GetSpriteName(out explicitSpriteUsed);

        if (string.IsNullOrEmpty(furniture.LinksToNeighbour) || explicitSpriteUsed)
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

    public Sprite GetOverlaySpriteForFurniture(Furniture furniture)
    {
        Sprite overlaySprite = null;
        string spriteName = furniture.GetOverlaySpriteName();
        if (!string.IsNullOrEmpty(spriteName))
        {
            overlaySprite = SpriteManager.GetSprite("Furniture", spriteName);
        }

        return overlaySprite;
    }

    protected override void OnCreated(Furniture furniture)
    {
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

        FurnitureChildObjects childObjects = new FurnitureChildObjects();
        childObjectMap.Add(furniture, childObjects);

        childObjects.Overlay = new GameObject();
        childObjects.Overlay.transform.parent = furn_go.transform;
        childObjects.Overlay.transform.position = furn_go.transform.position;
        SpriteRenderer spriteRendererOverlay = childObjects.Overlay.AddComponent<SpriteRenderer>();
        Sprite overlaySprite = GetOverlaySpriteForFurniture(furniture);
        if (overlaySprite != null)
        {
            spriteRendererOverlay.sprite = overlaySprite;
            spriteRendererOverlay.sortingLayerName = "Furniture";
            spriteRendererOverlay.sortingOrder = Mathf.RoundToInt(furn_go.transform.position.y * -1) + 1;
        }

        // indicators (power, fluid, ...)
        BuildableComponent.Requirements furnReq = furniture.GetPossibleRequirements();
        foreach (BuildableComponent.Requirements req in Enum.GetValues(typeof(BuildableComponent.Requirements)))
        {
            if (req != BuildableComponent.Requirements.None && (furnReq & req) == req)
            {
                GameObject indicator = new GameObject();
                indicator.transform.parent = furn_go.transform;
                indicator.transform.localScale = new Vector3(IndicatorScale, IndicatorScale, IndicatorScale);
                indicator.transform.position = furn_go.transform.position + statusIndicatorOffsets[req];

                SpriteRenderer powerSpriteRenderer = indicator.AddComponent<SpriteRenderer>();
                powerSpriteRenderer.sprite = GetStatusIndicatorSprite(req);
                powerSpriteRenderer.sortingLayerName = "Power";
                powerSpriteRenderer.color = Color.red;

                childObjects.AddStatus(req, indicator);
            }
        }

        UpdateIconObjectsVisibility(furniture, childObjects);
        
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
            UnityDebugger.Debugger.LogError("FurnitureSpriteController", "OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
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

        Sprite overlaySprite = GetOverlaySpriteForFurniture(furn);
        if (overlaySprite != null)
        {
            childObjectMap[furn].Overlay.GetComponent<SpriteRenderer>().sprite = overlaySprite;
        }
    }
        
    protected override void OnRemoved(Furniture furn)
    {
        if (objectGameObjectMap.ContainsKey(furn) == false)
        {
            UnityDebugger.Debugger.LogError("FurnitureSpriteController", "OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        furn.Changed -= OnChanged;
        furn.Removed -= OnRemoved;
        furn.IsOperatingChanged -= OnIsOperatingChanged;
        GameObject furn_go = objectGameObjectMap[furn];
        objectGameObjectMap.Remove(furn);
        GameObject.Destroy(furn_go);

        if (childObjectMap.ContainsKey(furn) == false)
        {
            return;
        }

        childObjectMap.Remove(furn);
    }
        
    private void OnIsOperatingChanged(Furniture furniture)
    {
        if (furniture == null)
        {
            return;
        }

        if (childObjectMap.ContainsKey(furniture) == false)
        {
            return;
        }

        UpdateIconObjectsVisibility(furniture, childObjectMap[furniture]);
    }

    private void UpdateIconObjectsVisibility(Furniture furniture, FurnitureChildObjects statuses)
    {
        if (statuses.StatusIndicators != null && statuses.StatusIndicators.Count > 0)
        {
            foreach (BuildableComponent.Requirements req in Enum.GetValues(typeof(BuildableComponent.Requirements)).Cast<BuildableComponent.Requirements>()
                .Where(x => x != BuildableComponent.Requirements.None && statuses.StatusIndicators.ContainsKey(x)))
            {                
                if ((furniture.Requirements & req) == 0)
                {
                    statuses.StatusIndicators[req].SetActive(false);
                }
                else
                {
                    statuses.StatusIndicators[req].SetActive(true);
                }                
            }
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

    private Sprite GetStatusIndicatorSprite(BuildableComponent.Requirements oneIdicator)
    {
        return SpriteManager.GetSprite("Icon", string.Format("{0}Indicator", oneIdicator.ToString()));
    }

    public class FurnitureChildObjects
    {
        public GameObject Overlay { get; set; }

        public Dictionary<BuildableComponent.Requirements, GameObject> StatusIndicators { get; set; }
        
        public void AddStatus(BuildableComponent.Requirements requirements, GameObject gameObj)
        {
            if (StatusIndicators == null)
            {
                StatusIndicators = new Dictionary<BuildableComponent.Requirements, GameObject>();
            }

            StatusIndicators[requirements] = gameObj;
        }
        
        public void Destroy()
        {
            GameObject.Destroy(Overlay);
            if (StatusIndicators != null)
            {
                foreach (GameObject status in StatusIndicators.Values)
                {
                    GameObject.Destroy(status);
                }
            }
        }
    }
}
