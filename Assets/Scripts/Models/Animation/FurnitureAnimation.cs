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

namespace Animation
{
    public class FurnitureAnimation
    {
        // current shown frame
        private int prevFrameIndex;

        // Holds the actual animation sprites from the spritesheet
        private Sprite[] sprites;

        // Collection of animations
        private Dictionary<string, SpritenameAnimation> animations;
        private SpritenameAnimation currentAnimation;
        private string currentAnimationState;
        
        public FurnitureAnimation()
        {            
            animations = new Dictionary<string, SpritenameAnimation>();
        }

        public SpriteRenderer Renderer { get; set; }

        public FurnitureAnimation Clone()
        {
            FurnitureAnimation newFA = new FurnitureAnimation();
            newFA.sprites = sprites;
            newFA.animations = new Dictionary<string, SpritenameAnimation>();

            foreach (KeyValuePair<string, SpritenameAnimation> entry in animations)
            {
                newFA.animations.Add(entry.Key, (SpritenameAnimation)entry.Value.Clone());
            }

            newFA.currentAnimationState = currentAnimationState;
            newFA.currentAnimation = newFA.animations[currentAnimationState];
            newFA.prevFrameIndex = 0;
            return newFA;
        }

        public void Update(float deltaTime)
        {
            currentAnimation.Update(deltaTime);

            if (prevFrameIndex != currentAnimation.CurrentFrame)
            {
                ShowSprite(currentAnimation.CurrentFrameName);
                prevFrameIndex = currentAnimation.CurrentFrame;
            }            
        }
        
        public void SetState(string stateName)
        {
            if (animations.ContainsKey(stateName) == false)
            {
                Debug.ULogErrorChannel("Animation", "SetState tries to set " + stateName + " which doesn't exist.");
                return;
            }

            if (stateName != currentAnimationState)
            {
                Debug.ULogChannel("ani", "setstate " + stateName);
                currentAnimationState = stateName;
                currentAnimation = animations[currentAnimationState];
                currentAnimation.Play();
                ShowSprite(currentAnimation.CurrentFrameName);
            }            
        }

        public void AddAnimation(string state, string spriteBase, string frames, float fps, bool looping)
        {
            string[] framesArr = frames.Split(',');
            List<string> spriteNames = new List<string>();
            for (int i = 0; i < framesArr.Length; i++)
            {
                spriteNames.Add(string.Concat(spriteBase, framesArr[i]));
            }

            animations.Add(state, new SpritenameAnimation(state, spriteNames.ToArray(), 1 / fps, looping));

            currentAnimationState = state;
            currentAnimation = animations[currentAnimationState];
            prevFrameIndex = 0;
        }

        private void ShowSprite(string spriteName)
        {
            if (Renderer != null)
            {
                Renderer.sprite = SpriteManager.current.GetSprite("Furniture", spriteName);
            }
        }
    }
}