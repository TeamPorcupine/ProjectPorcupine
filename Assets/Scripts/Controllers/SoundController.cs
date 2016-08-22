#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections;

public class SoundController
{
    float soundCooldown = 0;

    // Use this for initialization
    public SoundController(World world)
    {
        world.cbFurnitureCreated += OnFurnitureCreated;
        world.cbTileChanged += OnTileChanged;
    }
	
    // Update is called once per frame
    public void Update(float deltaTime)
    {
        soundCooldown -= deltaTime;
    }

    void OnTileChanged(Tile tile_data)
    {
        // FIXME

        if (soundCooldown > 0)
            return;

        AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        // FIXME
        if (soundCooldown > 0)
            return;
		
        AudioClip ac = Resources.Load<AudioClip>("Sounds/" + furn.objectType + "_OnCreated");

        if (ac == null)
        {
            // WTF?  What do we do?
            // Since there's no specific sound for whatever Furniture this is, just
            // use a default sound -- i.e. the Wall_OnCreated sound.
            ac = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
        }

        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }
}
