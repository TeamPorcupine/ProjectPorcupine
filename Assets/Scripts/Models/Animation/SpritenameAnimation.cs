﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// Animation class.
    /// </summary>
    public class SpritenameAnimation
    {
        // index in spritesheet
        public string CurrentFrameName;
        public bool FlipX;

        // frame of animation. 0 to frames.length
        public int CurrentFrame;
        private int frameCount;

        // delay between frames in seconds
        private float delay;
        private bool finished;
        private bool loops;

        // easier to debug with a name. Can be removed
        private string name;
        private string[] frames;
        private float timer = 0;

        public SpritenameAnimation(string name, string[] frames, float delay = .5f, bool loops = true, bool flipX = false)
        {
            this.name = name;
            this.frames = frames;
            this.delay = delay;
            this.loops = loops;
            this.FlipX = flipX;

            frameCount = this.frames.Length;
            CurrentFrame = 0;
            CurrentFrameName = this.frames[CurrentFrame];
        }

        public void Play()
        {
            finished = false;
            CurrentFrame = 0;
            CurrentFrameName = frames[CurrentFrame];
        }

        public void SetFrame(int frame)
        {
            if (frame > frameCount)
            {
                Debug.ULogErrorChannel("Animation", "SetFrame frame " + frame + " int is larger than array length " + frameCount + ".");
            }

            finished = false;
            CurrentFrame = frame;
            CurrentFrameName = frames[CurrentFrame];
        }

        public void Update(float deltaTime)
        {
            if (finished)
            {
                return;
            }

            timer += deltaTime;

            // Is it time to switch to next frame?
            if (timer >= delay)
            {
                timer = 0;

                // do we need to loop?
                if (CurrentFrame >= frameCount - 1)
                {
                    if (loops)
                    {
                        CurrentFrame = 0;
                    }
                    else
                    {
                        finished = true;
                    }
                }
                else
                {
                    CurrentFrame++;
                }

                CurrentFrameName = frames[CurrentFrame];
            }

            // TODO: if we need a callback after finished animation - put it here
        }
    }
}