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
        world.FurnitureManager.Created += OnFurnitureCreated;
        world.OnTileChanged += OnTileChanged;

        TimeManager.Instance.EveryFrame += Update;
    }
    
    // Update is called once per frame
    public void Update(float deltaTime)
    {
        soundCooldown -= deltaTime;
    }

    public void OnFurnitureCreated(Furniture furniture)
    {
        // FIXME
        if (soundCooldown > 0)
        {
            return;
        }

        AudioClip ac = AudioManager.GetAudio("Sound", furniture.Type + "_OnCreated");
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

        if (tileData.ForceTileUpdate)
        {  
            AudioClip ac = AudioManager.GetAudio("Sound", "Floor_OnCreated");
            AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
            soundCooldown = 0.1f;
        }
    }
}
