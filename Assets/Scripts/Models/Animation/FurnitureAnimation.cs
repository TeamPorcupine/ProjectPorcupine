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
    /// <summary>
    /// Animations for furniture. Can have several "states" that can be switched using SetState.
    /// </summary>
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

        /// <summary>
        /// Set the animation state. Will only have an effect if stateName is different from current animation stateName.
        /// </summary>
        public void SetState(string stateName)
        {
            if (animations.ContainsKey(stateName) == false)
            {
                Debug.ULogErrorChannel("Animation", "SetState tries to set " + stateName + " which doesn't exist.");
                return;
            }

            if (stateName != currentAnimationState)
            {
                currentAnimationState = stateName;
                currentAnimation = animations[currentAnimationState];
                currentAnimation.Play();
                ShowSprite(currentAnimation.CurrentFrameName);
            }            
        }

        /// <summary>
        /// Get spritename from the current animation.
        /// </summary>
        public string GetSpriteName()
        {            
            return currentAnimation.CurrentFrameName;
        }

        /// <summary>
        /// Add animation to furniture. First animation added will be default for sprites e.g. ghost image when placing furniture.
        /// </summary>
        public void AddAnimation(string state, List<string> spriteNames, float fps, bool looping)
        {
            animations.Add(state, new SpritenameAnimation(state, spriteNames.ToArray(), 1 / fps, looping));

            // set default state to first state entered - most likely "idle"
            if (string.IsNullOrEmpty(currentAnimationState))
            {
                currentAnimationState = state;
                currentAnimation = animations[currentAnimationState];
                prevFrameIndex = 0;
            }
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