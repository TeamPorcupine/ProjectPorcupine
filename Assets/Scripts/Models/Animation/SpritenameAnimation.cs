#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;

namespace Animation
{
    /// <summary>
    /// Animation class.
    /// </summary>
    public class SpritenameAnimation
    {
        public bool FlipX;
        public bool ValueBased;

        private int frameCount;
                
        // delay between frames in seconds
        private float delay;
        private bool finished;
        private bool loops;
        private string[] frames;
        private float timer = 0;
        
        public SpritenameAnimation(string state, string[] frames, float delay, bool loops = true, bool flipX = false, bool valueBased = false)
        {
            this.State = state;
            this.frames = frames;
            this.delay = delay;
            this.loops = loops;
            this.FlipX = flipX;
            this.ValueBased = valueBased;

            frameCount = this.frames.Length;
            CurrentFrame = 0;
            CurrentFrameName = this.frames[CurrentFrame];
        }

        // current frames value
        public string CurrentFrameName { get; set; }

        // frame of animation. 0 to frames.length
        public int CurrentFrame { get; set; }

        public string State { get; set; }

        public SpritenameAnimation Clone()
        {
            SpritenameAnimation returnFA = new SpritenameAnimation(State, frames, delay, loops, FlipX, ValueBased);
            return returnFA;
        }
        
        public void Play()
        {
            finished = false;
            CurrentFrame = 0;
            CurrentFrameName = frames[CurrentFrame];
        }

        public void SetFrame(int frame)
        {
            if (frame >= frameCount)
            {
                UnityDebugger.Debugger.LogError("Animation", "SetFrame frame " + frame + " int is larger than array length " + frameCount + ".");
                return;
            }

            finished = false;
            CurrentFrame = frame;
            CurrentFrameName = frames[CurrentFrame];            
        }

        public void SetProgressValue(float percent)
        {
            int frame = (int)Math.Round((frameCount - 1) * percent, 0);
            if (frame == CurrentFrame)
            {
                return;
            }

            SetFrame(frame);
        }

        public void Update(float deltaTime)
        {
            if (finished || ValueBased)
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