#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class MusicController : MonoBehaviour
{
    AudioSource backgroundMusic;

	void Awake ()
    {
        backgroundMusic = GetComponent<AudioSource>();
        AudioClip ac = Resources.Load<AudioClip>("Music/SpaceTheme");
        backgroundMusic.clip = ac;
        backgroundMusic.loop = true;
        backgroundMusic.Play();

	}
}
