#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// The Manager that handles the loading and storing of audio from streamingAssets.
/// </summary>
public class AudioManager
{
    private static Dictionary<string, AudioClip> audioClips;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioManager"/> class.
    /// </summary>
    public AudioManager()
    {
        audioClips = new Dictionary<string, AudioClip>();
    }

    /// <summary>
    /// Creates a human readable string of the audioClips Dictionary.
    /// Used for debugging.
    /// </summary>
    /// <returns>String containing the information of audioClips.</returns>
    public static string GetAudioDictionaryString()
    {
        Dictionary<string, AudioClip> dictionary = audioClips;
        return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value.length).ToArray()) + "}";
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
    public static AudioClip GetAudio(string categoryName, string audioName)
    {
        AudioClip clip = new AudioClip();

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
        IEnumerable<WWW> audioFiles = LoadAudioFile(filesInDir, directoryPath);
        IEnumerator<WWW> e = audioFiles.GetEnumerator();
        while (e.MoveNext())
        {
            Debug.Log("Type: " + e.Current.GetType());
        }
    }

    private static IEnumerable<WWW> LoadAudioFile(string[] filesInDir, string directoryPath)
    {
        foreach (string file in filesInDir)
        {
            string audioCategory = new DirectoryInfo(directoryPath).Name;
            string filePath = new FileInfo(file).FullName;

            if (filePath.Contains(".xml") || filePath.Contains(".meta") || filePath.Contains(".db"))
            {
                continue;
            }

            WWW www = new WWW(@"file://" + filePath);

            yield return www;

            AudioClip clip = www.GetAudioClip(false, false);

            string filename = new FileInfo(filePath).Name;

            filename = audioCategory + "/" + filename;

            Debug.Log(filename + " Downloaded");

            audioClips[filename] = clip;
        }
    }
}
