#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

public class ShipSpriteController : BaseSpriteController<Ship>
{
    public ShipSpriteController(World world) : base(world, "Ships")
    {
        world.ShipManager.ShipCreated += OnCreated;
        world.ShipManager.ShipRemoved += OnRemoved;
    }

    protected override void OnCreated(Ship ship)
    {
        UnityDebugger.Debugger.Log("Ships", "Ship created: " + ship.Type);

        GameObject ship_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(ship, ship_go);

        ship_go.name = "Ship";
        ship_go.transform.SetParent(objectParent.transform, true);

        SpriteRenderer sr = ship_go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Characters";
        sr.sprite = SpriteManager.GetSprite("Ship", ship.Type);

        ship.ShipChanged += OnChanged;
    }

    protected override void OnChanged(Ship ship)
    {
        GameObject ship_go = objectGameObjectMap[ship];
        ship_go.transform.position = new Vector3(ship.Position.x, ship.Position.y, 0);

        if (ship.State == ShipState.UNWRAPPED)
        {
            ship_go.GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            ship_go.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    protected override void OnRemoved(Ship ship)
    {
        GameObject ship_go = objectGameObjectMap[ship];
        GameObject.Destroy(ship_go);
        objectGameObjectMap.Remove(ship);
    }
}
