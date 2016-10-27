#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using FMOD;
using UnityEngine;

public class SoundController
{
    private float soundCooldown = 0;

    private VECTOR up;
    private VECTOR forward;
    private VECTOR zero;
    private VECTOR ignore;

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
 
        Sound clip = AudioManager.GetAudio("Sound", "MenuClick").Get();
        PlaySound(clip, "UI");
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

        PlaySoundAt(AudioManager.GetAudio("Sound", furniture.Type + "_OnCreated").Get(), furniture.Tile, "gameSounds");
    
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
            PlaySoundAt(AudioManager.GetAudio("Sound", "Floor_OnCreated").Get(), tileData, "gameSounds", 1);
            soundCooldown = 0.1f;
        }
    }

    public void PlaySound(Sound clip, string chanGroup = "master", float freqRange = 0f, float volRange = 0f)
    {
        if (!AudioManager.channelGroups.ContainsKey(chanGroup))
        {
            chanGroup = "master";
        }
        ChannelGroup channelGroup = AudioManager.channelGroups[chanGroup];

        FMOD.System SoundSystem = AudioManager.SoundSystem;
        Channel Channel;
        SoundSystem.playSound(clip, channelGroup, true, out Channel);
        if (!freqRange.AreEqual(0f))
        {
            float pitch = Mathf.Pow(1.059f, (Random.Range(-freqRange, freqRange)));
            Channel.setPitch(pitch);
        }
        Channel.setPaused(false);
    }

    public void PlaySoundAt(Sound clip, Tile tile, string chanGroup = "master", float freqRange = 0f, float volRange = 0f)
    {
        if (!AudioManager.channelGroups.ContainsKey(chanGroup))
        {
            chanGroup = "master";
        }
        ChannelGroup channelGroup = AudioManager.channelGroups[chanGroup];

        FMOD.System SoundSystem = AudioManager.SoundSystem;
        Channel Channel;
        SoundSystem.playSound(clip, channelGroup, true, out Channel);
        VECTOR tilePos = GetVectorFrom(tile);
        Channel.set3DAttributes(ref tilePos, ref zero, ref zero);
        if (!freqRange.AreEqual(0f))
        {
            float pitch = Mathf.Pow(1.059f, (Random.Range(-freqRange, freqRange)));
            Channel.setPitch(pitch);
        }

        if (!volRange.AreEqual(0f))
        {
            float curVol;
            Channel.getVolume(out curVol);
            float volChange = Random.Range(-volRange, 0f);
            Channel.setVolume(curVol * dBToVolume(volChange));
        }

        Channel.setPaused(false);
    }

    private VECTOR GetVectorFrom(Vector3 vector)
    {
        VECTOR fmodVector;
        fmodVector.x = vector.x;
        fmodVector.y = vector.y;
        fmodVector.z = vector.z;
        return fmodVector;
    }

    private VECTOR GetVectorFrom(Tile tile)
    {
        VECTOR fmodVector;
        fmodVector.x = tile.X;
        fmodVector.y = tile.Y;
        fmodVector.z = tile.Z * 2;
        return fmodVector;
    }

    private float dBToVolume(float dB)
    {
        return Mathf.Pow(10.0f, 0.05f * dB);
    }

    public void SetListenerPosition(Vector3 newPosition)
    {
        SetListenerPosition(newPosition.x, newPosition.y, newPosition.z);
    }

    public void SetListenerPosition(float x, float y, float z)
    {
        VECTOR curLoc;
        curLoc.x = x;
        curLoc.y = y;
        curLoc.z = z;
        AudioManager.SoundSystem.set3DListenerAttributes(0, ref curLoc, ref zero, ref forward, ref up);
        AudioManager.SoundSystem.update();
    }

    private Vector3 GetUnityVector(VECTOR vector)
    {
        Vector3 unityVector;
        unityVector.x = vector.x;
        unityVector.y = vector.y;
        unityVector.z = vector.z;

        return unityVector;
    }
}
