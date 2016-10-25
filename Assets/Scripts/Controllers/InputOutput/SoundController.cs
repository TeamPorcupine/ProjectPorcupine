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

    private FMOD.VECTOR up;
    private FMOD.VECTOR forward;
    private FMOD.VECTOR zero;


    // Use this for initialization
    public SoundController(World world = null)
    {
        if (world != null)
        {
            world.FurnitureManager.Created += OnFurnitureCreated;
            world.OnTileChanged += OnTileChanged;
        }

        TimeManager.Instance.EveryFrame += Update;

        zero = GetVectorFrom(Vector3.zero);
        forward = GetVectorFrom(Vector3.forward);
        up = GetVectorFrom(Vector3.up);

    }
    
    // Update is called once per frame
    public void Update(float deltaTime)
    {
        soundCooldown -= deltaTime;
    }

    public void OnButtonSFX()
    {
        // FIXME
        if (soundCooldown > 0)
        {
             return;
        }
 
        FMOD.Sound clip = AudioManager.GetAudio("Sound", "MenuClick");
        FMOD.System SoundSystem = AudioManager.SoundSystem;
        FMOD.Channel Channel;
        SoundSystem.playSound(clip, null, true, out Channel);
        Channel.setVolume(1f);
        Channel.setPaused(false);
        //            
        soundCooldown = 0.1f;
    }

    public void OnFurnitureCreated(Furniture furniture)
    {
        // FIXME
        if (soundCooldown > 0)
        {
            return;
        }

        PlaySoundAt(AudioManager.GetAudio("Sound", furniture.Type + "_OnCreated"), furniture.Tile);
    
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
            PlaySoundAt(AudioManager.GetAudio("Sound", "Floor_OnCreated"), tileData, 2);
//            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            soundCooldown = 0.1f;
        }
    }

    public void PlaySound(FMOD.Sound clip)
    {
        FMOD.System SoundSystem = AudioManager.SoundSystem;
        FMOD.Channel Channel;
        SoundSystem.playSound(clip, null, true, out Channel);
        Channel.setVolume(1f);
        Channel.setPaused(false);
    }

    public void PlaySoundAt(FMOD.Sound clip, Tile tile, float freqRange = 0f)
    {
        FMOD.System SoundSystem = AudioManager.SoundSystem;
        FMOD.Channel Channel;
        FMOD.VECTOR curLoc;
        curLoc.x = Camera.main.transform.position.x;
        curLoc.y = Camera.main.transform.position.y;
        curLoc.z = WorldController.Instance.cameraController.CurrentLayer;


        SoundSystem.set3DListenerAttributes(0, ref curLoc, ref zero, ref forward, ref up);
//        SoundSystem.set3DListenerAttributes(0, ref tilePos, ref zero, ref forward, ref up);
        SoundSystem.playSound(clip, null, true, out Channel);
        Channel.setVolume(1f);
        Channel.set3DMinMaxDistance(Camera.main.orthographicSize / 2, 1000);
        FMOD.VECTOR tilePos = GetVectorFrom(tile);
        Channel.set3DAttributes(ref tilePos, ref zero, ref zero);
        float pitch = Mathf.Pow(1.059f, (Random.Range(-freqRange, freqRange)));
        Channel.setPitch(pitch);
        Channel.setPaused(false);
    }

    private FMOD.VECTOR GetVectorFrom(Vector3 vector)
    {
        FMOD.VECTOR fmodVector;
        fmodVector.x = vector.x;
        fmodVector.y = vector.y;
        fmodVector.z = vector.z;
        return fmodVector;
    }

    private FMOD.VECTOR GetVectorFrom(Tile tile)
    {
        FMOD.VECTOR fmodVector;
        fmodVector.x = tile.X;
        fmodVector.y = tile.Y;
        fmodVector.z = tile.Z * 2;
        return fmodVector;
    }

}
