#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;

public class SoundController
{
    private float soundCooldown = 0;

    // Use this for initialization
    public SoundController(World world)
    {
        world.OnFurnitureCreated += OnFurnitureCreated;
        world.OnTileChanged += OnTileChanged;
    }
    
    // Update is called once per frame
    public void Update(float deltaTime)
    {
        soundCooldown -= deltaTime;
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        // FIXME
        if (soundCooldown > 0)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>("Sounds/" + furn.ObjectType + "_OnCreated");

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

    private void OnTileChanged(Tile tileData)
    {
        // FIXME
        if (soundCooldown > 0)
        {
            return;
        }

        AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }
}
