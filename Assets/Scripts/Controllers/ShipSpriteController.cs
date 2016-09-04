using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSpriteController : BaseSpriteController<Ship> {

	// Use this for initialization
	void Start () {
        ShipManager shipManager = World.Current.shipManager;
        shipManager.ShipCreated += OnCreated;
        shipManager.ShipRemoved += OnRemoved;
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    public ShipSpriteController(World world) : base(world, "Ships")
    {
        
    }

    protected override void OnCreated(Ship ship)
    {
        GameObject ship_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        objectGameObjectMap.Add(ship, ship_go);

        ship_go.name = "Ship";
        // ship_go.transform.position = new Vector3(c.X, c.Y, c.Z);
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
