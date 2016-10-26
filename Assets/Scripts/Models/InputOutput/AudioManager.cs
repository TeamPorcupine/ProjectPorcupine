#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// The Manager that handles the loading and storing of audio from streamingAssets.
/// </summary>
public class AudioManager
{
    private static Dictionary<string, FMOD.Sound> audioClips;

    public static FMOD.System SoundSystem;

    // Channel Groups
    public static Dictionary<string, FMOD.ChannelGroup> channelGroups;

    // TODO: These should probably have a property to access them and possibly be put in a dictionary for easier access
    public static FMOD.ChannelGroup master;
    public static FMOD.ChannelGroup UI;
    public static FMOD.ChannelGroup gameSounds;
    public static FMOD.ChannelGroup alerts;
    public static FMOD.ChannelGroup music;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioManager"/> class.
    /// </summary>
    public AudioManager()
    {
        channelGroups = new Dictionary<string, FMOD.ChannelGroup>();
        FMOD.RESULT res = FMOD.Factory.System_Create(out SoundSystem);
        SoundSystem.setDSPBufferSize(1024, 10);
        SoundSystem.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)0);
        SoundSystem.getMasterChannelGroup(out master);
        FMOD.DSPConnection throwaway;
        channelGroups.Add("UI", null);
        channelGroups.Add("gameSounds", null);
        channelGroups.Add("alerts", null);
        channelGroups.Add("music", null);
//        SoundSystem.createChannelGroup("UI", out UI);
//        SoundSystem.createChannelGroup("building", out gameSounds);
//        SoundSystem.createChannelGroup("alerts", out alerts);
//        SoundSystem.createChannelGroup("music", out music);
//        master.addGroup(UI, true, out throwaway);
//        master.addGroup(gameSounds, true, out throwaway);
        foreach (string key in channelGroups.Keys.ToArray())
        {
            FMOD.ChannelGroup chanGroup;
            SoundSystem.createChannelGroup(key, out chanGroup);
//            FMOD.DSPConnection throwaway;
            master.addGroup(chanGroup, true, out throwaway);
            channelGroups[key] = chanGroup;
        }
        channelGroups.Add("master", master);

        SoundSystem.set3DSettings(1f, 1f, 0.1f);
        audioClips = new Dictionary<string, FMOD.Sound>();
//        master.setVolume(.5f);
//        UI.setVolume(8f);
    }

    /// <summary>
    /// Creates a human readable string of the audioClips Dictionary.
    /// Used for debugging.
    /// </summary>
    /// <returns>String containing the information of audioClips.</returns>
    public static string GetAudioDictionaryString()
    {
        Dictionary<string, FMOD.Sound> dictionary = audioClips;

        return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value.ToString()).ToArray()) + "}";
    }

    /// <summary>
    /// Gets an AudioCLip from the specified category, with the specified name.
    /// Throws a LogWarning and returns an Error sound if the AudioClip does not exist.
    /// </summary>
    /// <param name="categoryName">
    /// The category that the AudioClip is in. Usually the same as the 
    /// directory that the audio file was load from.
    /// </param>
    /// <param name="audioName">The name of the AudioClip.</param>
    /// <returns>AudioClip form the specified category with the specified name.</returns>
    public static FMOD.Sound GetAudio(string categoryName, string audioName)
    {
        FMOD.Sound clip;

        string audioNameAndCategory = categoryName + "/" + audioName + ".ogg";

        if (audioClips.ContainsKey(audioNameAndCategory))
        {
            clip = audioClips[audioNameAndCategory];
        }
        else
        {
            try
            {
                Debug.LogWarning("No audio available called: " + audioNameAndCategory);
                clip = audioClips["Sound/Error.ogg"];
            }
            catch
            {
                throw new FileNotFoundException("Sound/Error.ogg not found");
            }
        }

        return clip;
    }

    public static List<FMOD.Sound> GetAudioSequence(string categoryName, string audioName)
    {
        List<FMOD.Sound> clipSequence;
        string audioNameAndCategory = categoryName + "/" + audioName;
        clipSequence = audioClips.Where(entry => entry.Key.StartsWith(audioNameAndCategory)).Select(kvPair => kvPair.Value).ToList();
        return clipSequence;

    }

    /// <summary>
    /// Loads all the audio files from the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path of the directory you want to load the audio files from.</param>
    public static void LoadAudioFiles(string directoryPath)
    {
        string[] subDirectories = Directory.GetDirectories(directoryPath);
        foreach (string subDirectory in subDirectories)
        {
            LoadAudioFiles(subDirectory);
        }

        string[] filesInDir = Directory.GetFiles(directoryPath);
        LoadAudioFile(filesInDir, directoryPath);
    }

    public static void Destroy()
    {
        SoundSystem.release();
        audioClips = null;
    }

    private static void LoadAudioFile(string[] filesInDir, string directoryPath)
    {
        foreach (string file in filesInDir)
        {
            string audioCategory = new DirectoryInfo(directoryPath).Name;
            string filePath = new FileInfo(file).FullName;

            if (filePath.Contains(".xml") || filePath.Contains(".meta") || filePath.Contains(".db"))
            {
                continue;
            }

            FMOD.Sound clip;
            SoundSystem.createSound(filePath, FMOD.MODE._3D, out clip);
            string filename = new FileInfo(filePath).Name;
            filename = audioCategory + "/" + filename;

            Debug.Log(filename + " Downloaded");


            audioClips[filename] = clip;
        }
    }
}
