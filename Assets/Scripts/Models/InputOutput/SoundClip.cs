#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections.Generic;
using FMOD;
using UnityEngine;

public class SoundClip
{
    private readonly List<Sound> clips;
    private int place;

    public SoundClip()
    {
        clips = new List<Sound>();
        place = 0;
        SequenceType = Sequence.RANDOM;
    }

    public SoundClip(Sound clip) : this()
    {
        clips.Add(clip);
    }

    public enum Sequence
    {
        RANDOM, SERIAL
    }

    public Sequence SequenceType { get; set; }

    public void Add(Sound clip)
    {
        clips.Add(clip);
    }

    /// <summary>
    /// Either get the only FMOD.Sound in this SoundClip or get the next in the sequence according to GroupType.
    /// </summary>
    public Sound Get()
    {
        if (clips == null || clips.Count == 0)
        {
            UnityDebugger.Debugger.LogError("Audio", "Attempting to access an empty SoundClip.");
            return null;
        }

        if (clips.Count == 1)
        {
            return clips[0];
        }

        if (SequenceType == Sequence.RANDOM)
        {
            return clips[Random.Range(0, clips.Count)];
        }
        else
        {
            if (place >= clips.Count)
            {
                place = 0;
            }

            return clips[place++];
        }
    }
}
