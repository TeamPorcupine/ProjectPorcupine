#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
using System.Collections.Generic;


#endregion
using System.Collections;
using UnityEngine;

public class SoundController
{
    private float soundCooldown = 0;

    private FMOD.VECTOR up;
    private FMOD.VECTOR forward;
    private FMOD.VECTOR zero;
    private FMOD.VECTOR ignore;

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

        PlaySoundAt(AudioManager.GetAudio("Sound", furniture.Type + "_OnCreated"), furniture.Tile, "gameSounds");
    
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
//            List<FMOD.Sound> sequence = AudioManager.GetAudioSequence("Sound", "Floor_OnCreated");

//            PlaySoundAt(sequence[Random.Range(0, sequence.Count)], tileData);
            PlaySoundAt(AudioManager.GetAudio("Sound", "Floor_OnCreated"), tileData, "gameSounds", 1);
//            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            soundCooldown = 0.1f;
        }
    }

    public void PlaySound(FMOD.Sound clip, string chanGroup = "master", float freqRange = 0f, float volRange = 0f)
    {
        if (!AudioManager.channelGroups.ContainsKey(chanGroup))
        {
            chanGroup = "master";
        }
        FMOD.ChannelGroup channelGroup = AudioManager.channelGroups[chanGroup];
//        if (chanGroup == null)
//        {
//            chanGroup = AudioManager.master;
//        }

        FMOD.System SoundSystem = AudioManager.SoundSystem;
        FMOD.Channel Channel;
        SoundSystem.playSound(clip, channelGroup, true, out Channel);
        if (!freqRange.AreEqual(0f))
        {
            float pitch = Mathf.Pow(1.059f, (Random.Range(-freqRange, freqRange)));
            Channel.setPitch(pitch);
        }
        Channel.setPaused(false);
    }

    public void PlaySoundAt(FMOD.Sound clip, Tile tile, string chanGroup = "master", float freqRange = 0f, float volRange = 0f)
    {
        if (!AudioManager.channelGroups.ContainsKey(chanGroup))
        {
            chanGroup = "master";
        }
        FMOD.ChannelGroup channelGroup = AudioManager.channelGroups[chanGroup];
//        if (chanGroup == null)
//        {
//            chanGroup = AudioManager.channelGroups["master"];
//        }

        FMOD.System SoundSystem = AudioManager.SoundSystem;
        FMOD.Channel Channel;
        SoundSystem.playSound(clip, channelGroup, true, out Channel);
//        Channel.setVolume(1f);
//        Channel.set3DMinMaxDistance(Camera.main.orthographicSize / 2, 1000);
        FMOD.VECTOR tilePos = GetVectorFrom(tile);
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
        //        FMOD.VECTOR prevLoc;
        FMOD.VECTOR curLoc;
        curLoc.x = x;
        curLoc.y = y;
        curLoc.z = z;
        //        AudioManager.SoundSystem.get3DListenerAttributes(0, out prevLoc, out ignore, out ignore, out ignore);
        //        FMOD.VECTOR velocity = GetVectorFrom((GetUnityVector(curLoc) - GetUnityVector(prevLoc)) / (Time.deltaTime*5));
        AudioManager.SoundSystem.set3DListenerAttributes(0, ref curLoc, ref zero, ref forward, ref up);
        AudioManager.SoundSystem.update();
    }

//    public void SetListenerPosition(Vector3 newPosition)
//    {
//        //        FMOD.VECTOR prevLoc;
//        FMOD.VECTOR curLoc;
//        curLoc.x = newPosition.x;
//        curLoc.y = newPosition.y;
//        curLoc.z = newPosition.z;
//        //        AudioManager.SoundSystem.get3DListenerAttributes(0, out prevLoc, out ignore, out ignore, out ignore);
//        //        FMOD.VECTOR velocity = GetVectorFrom((GetUnityVector(curLoc) - GetUnityVector(prevLoc)) / (Time.deltaTime*5));
//        AudioManager.SoundSystem.set3DListenerAttributes(0, ref curLoc, ref zero, ref forward, ref up);
//        AudioManager.SoundSystem.update();
//    }

    private Vector3 GetUnityVector(FMOD.VECTOR vector)
    {
        Vector3 unityVector;
        unityVector.x = vector.x;
        unityVector.y = vector.y;
        unityVector.z = vector.z;

        return unityVector;
    }
}
