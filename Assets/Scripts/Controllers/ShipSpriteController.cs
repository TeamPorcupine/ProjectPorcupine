#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSpriteController : BaseSpriteController<Ship>
{
    public ShipSpriteController(World world) : base(world, "Ships")
    {
    }

    protected override void OnCreated(Ship ship)
    {
        GameObject ship_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(ship, ship_go);

        ship_go.name = "Ship";
        //// ship_go.transform.position = new Vector3(c.X, c.Y, c.Z);
        ship_go.transform.SetParent(objectParent.transform, true);

        SpriteRenderer sr = ship_go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        sr.sprite = SpriteManager.current.GetSprite("Ships", ship.ShipType);

        ship.ShipChanged += OnChanged;
    }

    protected override void OnChanged(Ship ship)
    { 
    }

    protected override void OnRemoved(Ship ship)
    {   
    }
}
