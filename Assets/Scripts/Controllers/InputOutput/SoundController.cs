#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using System.Linq;
using FMOD;
using UnityEngine;

public struct DriverInfo
{
    public int id;
    public System.Text.StringBuilder name;
    public System.Guid guid;
    public int systemRate;
    public SPEAKERMODE speakerMode;
    public int speakerModeChannels;

    public DriverInfo(int id)
    {
        this.id = id;
        this.name = new System.Text.StringBuilder(64);

        AudioManager.SoundSystem.getDriverInfo(id, this.name, 64, out this.guid, out this.systemRate, out this.speakerMode, out this.speakerModeChannels);
    }

    public override string ToString()
    {
        return id.ToString() + ", " + name.ToString();
    }
}

public class SoundController
{
    private Dictionary<SoundClip, float> cooldowns;
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
            world.OnTileTypeChanged += OnTileTypeChanged;
        }

        TimeManager.Instance.EveryFrame += Update;
        zero = GetVectorFrom(Vector3.zero);
        forward = GetVectorFrom(Vector3.forward);
        up = GetVectorFrom(Vector3.up);
        cooldowns = new Dictionary<SoundClip, float>();
    }

    public enum AudioChannel
    {
        master, UI, gameSounds, alerts, music
    }

    // Update is called once per frame
    public void Update(float deltaTime)
    {
        foreach (SoundClip key in cooldowns.Keys.ToList())
        {
            cooldowns[key] -= deltaTime;
            if (cooldowns[key] <= 0f)
            {
                cooldowns.Remove(key);
            }
        }
    }

    public void OnButtonSFX()
    {
        SoundClip clip = AudioManager.GetAudio("Sound", "MenuClick");
        if (cooldowns.ContainsKey(clip) && cooldowns[clip] > 0)
        {
            return;
        }

        cooldowns[clip] = 0.1f;
        PlaySound(clip.Get(), "UI");
    }

    public void OnFurnitureCreated(Furniture furniture)
    {
        SoundClip clip = AudioManager.GetAudio("Sound", furniture.Type + "_OnCreated");
        if (cooldowns.ContainsKey(clip) && cooldowns[clip] > 0)
        {
            return;
        }

        cooldowns[clip] = 0.1f;
        PlaySoundAt(clip.Get(), furniture.Tile, "gameSounds");
    }

    public void OnTileTypeChanged(Tile tileData)
    {
        SoundClip clip = AudioManager.GetAudio("Sound", "Floor_OnCreated");
        if (cooldowns.ContainsKey(clip) && cooldowns[clip] > 0)
        {
            return;
        }

        cooldowns[clip] = 0.1f;
        PlaySoundAt(clip.Get(), tileData, "gameSounds", 1);
    }

    /// <summary>
    /// Plays the clip at camera's location, on the channel group chanGroup. Pitch ranges from -freqRange to +freqRange in semitones.
    /// Volume ranges from -volRange to No change in decibels.
    /// </summary>
    /// <param name="clip">Sound to Play.</param>
    /// <param name="chanGroup">Chan group to play the sound on.</param>
    /// <param name="freqRange">Frequency range in semitones.</param>
    /// <param name="volRange">Volume range in decibels.</param>
    public void PlaySound(Sound clip, string chanGroup = "master", float freqRange = 0f, float volRange = 0f)
    {
        PlaySoundAt(clip, null, chanGroup, freqRange, volRange);
    }

    /// <summary>
    /// Plays the clip at tile's location, on the channel group chanGroup. Pitch ranges from -freqRange to +freqRange in semitones.
    /// Volume ranges from -volRange to No change in decibels.
    /// </summary>
    /// <param name="clip">Sound to Play.</param>
    /// <param name="tile">Tile's location to play at. If null, plays at camera's position.</param>
    /// <param name="chanGroup">Chan group to play the sound on.</param>
    /// <param name="freqRange">Frequency range in semitones.</param>
    /// <param name="volRange">Volume range in decibels.</param>
    public void PlaySoundAt(Sound clip, Tile tile, string chanGroup = "master", float freqRange = 0f, float volRange = 0f)
    {
        if (!AudioManager.channelGroups.ContainsKey(chanGroup))
        {
            chanGroup = "master";
        }

        ChannelGroup channelGroup = AudioManager.channelGroups[chanGroup];

        FMOD.System soundSystem = AudioManager.SoundSystem;
        Channel channel;
        soundSystem.playSound(clip, channelGroup, true, out channel);
        if (tile != null)
        {
            VECTOR tilePos = GetVectorFrom(tile);
            channel.set3DAttributes(ref tilePos, ref zero, ref zero);
        }

        if (!freqRange.AreEqual(0f))
        {
            float pitch = Mathf.Pow(1.059f, Random.Range(-freqRange, freqRange));
            channel.setPitch(pitch);
        }

        if (!volRange.AreEqual(0f))
        {
            float curVol;
            channel.getVolume(out curVol);
            float volChange = Random.Range(-volRange, 0f);
            channel.setVolume(curVol * DecibelsToVolume(volChange));
        }

        channel.set3DLevel(SettingsKeyHolder.LocationalSound ? 0.75f : 0f);
        channel.setPaused(false);
    }

    /// <summary>
    /// Sets the driver to the id provided.
    /// </summary>
    /// <param name="driverID"> Driver ID in question. </param>
    public void SetAudioDriver(int driverID)
    {
        if (driverID != GetCurrentAudioDriver())
        {
            AudioManager.SoundSystem.setDriver(driverID);
        }
    }

    /// <summary>
    /// Sets the driver to the driver with the string GUID provided.
    /// </summary>
    /// <param name="driverGUID"> Driver string GUID in question. </param>
    public void SetAudioDriver(string driverGUID)
    {
        for (int i = 0; i < GetDriverCount(); i++)
        {
            if (GetDriverInfo(i).guid.ToString() == driverGUID)
            {
                SetAudioDriver(i);
                break;
            }
        }
    }

    /// <summary>
    /// Sets the driver to the driver with the GUID provided.
    /// </summary>
    /// <param name="driverGUID"> Driver GUID in question. </param>
    public void SetAudioDriver(System.Guid driverGUID)
    {
        SetAudioDriver(driverGUID.ToString());
    }

    /// <summary>
    /// Returns the current audio device index.
    /// </summary>
    /// <returns> The index of the current audio device. </returns>
    public int GetCurrentAudioDriver()
    {
        int audioDriver;
        AudioManager.SoundSystem.getDriver(out audioDriver);
        return audioDriver;
    }

    /// <summary>
    /// Returns information on the current driver.
    /// </summary>
    /// <returns> Information covering index, name, GUID, and other information <see cref="DriverInfo"/>. </returns>
    public DriverInfo GetCurrentAudioDriverInfo()
    {
        int audioDriver;
        AudioManager.SoundSystem.getDriver(out audioDriver);
        return GetDriverInfo(audioDriver);
    }

    /// <summary>
    /// Gets driver information on the provided index.
    /// </summary>
    /// <param name="id"> The index in question. </param>
    /// <returns> Information covering index, name, GUID, and other information <see cref="DriverInfo"/>. </returns>
    public DriverInfo GetDriverInfo(int id)
    {
        return new DriverInfo(id);
    }

    /// <summary>
    /// Returns the total driver count.
    /// </summary>
    /// <returns> An integer representing the total count of all audio drivers. </returns>
    public int GetDriverCount()
    {
        int count;
        AudioManager.SoundSystem.getNumDrivers(out count);
        return count;
    }

    /// <summary>
    /// Sets the listener position.
    /// </summary>
    /// <param name="newPosition">New position for the listener.</param>
    public void SetListenerPosition(Vector3 newPosition)
    {
        SetListenerPosition(newPosition.x, newPosition.y, newPosition.z);
    }

    /// <summary>
    /// Sets the listener position.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="z">The z coordinate.</param>
    public void SetListenerPosition(float x, float y, float z)
    {
        VECTOR curLoc;
        curLoc.x = x;
        curLoc.y = y;
        curLoc.z = z * 2;
        AudioManager.SoundSystem.set3DListenerAttributes(0, ref curLoc, ref zero, ref forward, ref up);
        AudioManager.SoundSystem.update();
    }

    public void SetVolume(AudioChannel channel, float volume)
    {
        SetVolume(channel.ToString(), volume);
    }

    public void SetVolume(string channel, float volume)
    {
        if (AudioManager.channelGroups.ContainsKey(channel))
        {
            AudioManager.channelGroups[channel].setVolume(volume);
        }
        else
        {
            UnityDebugger.Debugger.LogWarning("Channel " + channel + "does not exist.");
        }
    }

    public float GetVolume(AudioChannel channel)
    {
        return GetVolume(channel.ToString());
    }

    public float GetVolume(string channel)
    {
        if (AudioManager.channelGroups.ContainsKey(channel))
        {
            float volume = 0;
            AudioManager.channelGroups[channel].getVolume(out volume);
            return volume;
        }
        else
        {
            UnityDebugger.Debugger.LogWarning("Channel " + channel + "does not exist.");
            return 0f;
        }
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

    private float DecibelsToVolume(float dB)
    {
        return Mathf.Pow(10.0f, 0.05f * dB);
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
