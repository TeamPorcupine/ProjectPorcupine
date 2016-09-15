#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using System;
using System.IO;
using NVorbis;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    MemoryStream stream;
    VorbisReader vorbis;
    AudioSource backgroundMusic;
    public string musicTitle = "SpaceTheme";

    void OnEnable()
    {
        backgroundMusic = GetComponent<AudioSource>();
        AudioSettings.outputSampleRate = 44100;
        AudioClip ac = LoadMusic(musicTitle);
        backgroundMusic.clip = ac;
        backgroundMusic.loop = true;
        backgroundMusic.Play();

    }

    private AudioClip LoadMusic(string title)
    {
        string filePath = Application.streamingAssetsPath;
        filePath = Path.Combine(filePath, "Music");
        filePath = Path.Combine(filePath, title + ".ogg");

        byte[] dataAsByteArray = File.ReadAllBytes(filePath);
        stream = new MemoryStream(dataAsByteArray);
        vorbis = new VorbisReader(stream, true);

        int samplecount = (int)(vorbis.SampleRate * vorbis.TotalTime.TotalSeconds);

        return AudioClip.Create(title, samplecount, vorbis.Channels, vorbis.SampleRate, false, true, OnAudioRead, OnAudioSetPosition);
    }

    void OnAudioRead(float[] data)
    {
        var f = new float[data.Length];
        vorbis.ReadSamples(f, 0, data.Length);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = f[i];
        }
    }

    void OnAudioSetPosition(int newPosition)
    {
        vorbis.DecodedTime = new TimeSpan(0);
    }
}
