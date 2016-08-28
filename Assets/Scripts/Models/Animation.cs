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
using UnityEngine;

/// <summary>
/// Animation class.
/// </summary>
public class Animation
{    
    // index in spritesheet
    public int CurrentIndex;
    public bool FlipX;

    // frame of animation. 0 to frames.length
    private int currentFrame;
    private int frameCount;
    
    // delay between frames in seconds
    private float delay;
    private bool finished;
    private bool loops;
    
    // easier to debug with a name. Can be removed
    private string name;
    private int[] frames;
    private float timer = 0;

    public Animation(string name, int[] frames, float delay = .5f, bool loops = true, bool flipX = false)
    {
        this.name = name;
        this.frames = frames;
        this.delay = delay;
        this.loops = loops;
        this.FlipX = flipX;

        frameCount = this.frames.Length;
        currentFrame = 0;
        CurrentIndex = this.frames[currentFrame];
    }

    public void Play()
    {
        finished = false;
        currentFrame = 0;
        CurrentIndex = frames[currentFrame];
    }

    public void SetFrame(int frame)
    {
        if (frame > frameCount)
        {
            Debug.ULogErrorChannel("Animation", "SetFrame frame " + frame + " int is larger than array length " + frameCount + ".");
        }

        finished = false;
        currentFrame = frame;
        CurrentIndex = frames[currentFrame];
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
            if (currentFrame >= frameCount - 1)
            {
                if (loops)
                {
                    currentFrame = 0;
                }
                else
                {
                    finished = true;
                }
            }
            else
            {
                currentFrame++;
            }

            CurrentIndex = frames[currentFrame];
        }
        
        // TODO: if we need a callback after finished animation - put it here
    }
}
