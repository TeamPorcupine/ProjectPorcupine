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
using FMOD;

/// <summary>
/// The Manager that handles the loading and storing of audio from streamingAssets.
/// </summary>
public class AudioManager
{
    public static FMOD.System SoundSystem;

    // Channel Groups
    public static Dictionary<string, ChannelGroup> channelGroups;

    public static ChannelGroup master;

    private static Dictionary<string, SoundClip> audioClips;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioManager"/> class.
    /// </summary>
    public AudioManager()
    {
        channelGroups = new Dictionary<string, ChannelGroup>();
        Factory.System_Create(out SoundSystem);
        SoundSystem.setDSPBufferSize(1024, 10);
        SoundSystem.init(32, INITFLAGS.NORMAL, (IntPtr)0);
        SoundSystem.getMasterChannelGroup(out master);
        DSPConnection throwaway;
        channelGroups.Add("UI", null);
        channelGroups.Add("gameSounds", null);
        channelGroups.Add("alerts", null);
        channelGroups.Add("music", null);
        foreach (string key in channelGroups.Keys.ToArray())
        {
            ChannelGroup chanGroup;
            SoundSystem.createChannelGroup(key, out chanGroup);
            master.addGroup(chanGroup, true, out throwaway);
            channelGroups[key] = chanGroup;
        }

        channelGroups.Add("master", master);

        SoundSystem.set3DSettings(1f, 1f, .25f);
        audioClips = new Dictionary<string, SoundClip>();
    }

    /// <summary>
    /// Creates a human readable string of the audioClips Dictionary.
    /// Used for debugging.
    /// </summary>
    /// <returns>String containing the information of audioClips.</returns>
    public static string GetAudioDictionaryString()
    {
        Dictionary<string, SoundClip> dictionary = audioClips;

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
    /// <param name="audioName">The name of the SoundClip.</param>
    /// <returns>SoundClip from the specified category with the specified name.</returns>
    public static SoundClip GetAudio(string categoryName, string audioName)
    {
        SoundClip clip;

        string audioNameAndCategory = categoryName + "/" + audioName;

        if (audioClips.ContainsKey(audioNameAndCategory))
        {
            clip = audioClips[audioNameAndCategory];
        }
        else
        {
            try
            {
                UnityDebugger.Debugger.LogWarning("AudioManager", "No audio available called: " + audioNameAndCategory);
                clip = audioClips["Sound/Error"];
            }
            catch
            {
                throw new FileNotFoundException("Sound/Error.ogg not found");
            }
        }

        return clip;
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
        SoundSystem.close();

        // This will also release master, so we don't have to call master.release();
        foreach (string key in channelGroups.Keys)
        {
            channelGroups[key].release();
        }

        SoundSystem.release();

        SoundSystem = null;
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

            Sound clip;
            SoundSystem.createSound(filePath, MODE._3D, out clip);
            string filename = Path.GetFileNameWithoutExtension(filePath);

            // If the filename contains a period, it is part of a sequence, remove everything after the period,
            // and it will be handled appropriately.
            if (filename.Contains("."))
            {
                filename = filename.Substring(0, filename.IndexOf("."));
            }

            filename = audioCategory + "/" + filename;

            if (audioClips.ContainsKey(filename))
            {
                audioClips[filename].Add(clip);
            }
            else
            {
                audioClips[filename] = new SoundClip(clip);
            }
        }
    }
}
