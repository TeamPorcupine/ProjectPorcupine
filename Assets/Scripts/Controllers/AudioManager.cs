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

public class AudioManager
{
    private static Dictionary<string, AudioClip> audioClips;

    public AudioManager()
    {
        audioClips = new Dictionary<string, AudioClip>();
    }

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

            Debug.Log("before - " + www.error);

            yield return www;

            Debug.Log("After - " + www.error);

            AudioClip clip = www.GetAudioClip(false, false);

            Debug.Log(clip.name + "Downloaded");

            string filename = new FileInfo(filePath).Name;

            audioClips[filename] = clip;

            // filename = audioCategory + "/" + filename;

        }
    }

    public static string GetDebugString(Dictionary<string, AudioClip> dictionary)
    {
        return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value.length).ToArray()) + "}";
    }

    public static AudioClip GetAudio(string categoryName, string audioName)
    {
        AudioClip clip = new AudioClip();

        //string audioNameAndCategory = categoryName + "/" + audioName;

        if (audioClips.ContainsKey(audioName))
        {
            clip = audioClips[audioName];
        }
        else
        {
            try
            {
                clip = audioClips["Sound/Error.wav"];
            }
            catch
            {
                string str = GetDebugString(audioClips);
                throw new UnityException(str);
            }


           }


        return clip;
    }

}
