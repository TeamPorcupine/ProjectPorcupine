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
        //private Furniture furniture;
        public SpriteRenderer Renderer {get;set;}

        // current shown frame
        private int prevFrameIndex;

        // Holds the actual animation sprites from the spritesheet
        private Sprite[] sprites;

        // Collection of animations
        private Dictionary<string, SpritenameAnimation> animations;
        private SpritenameAnimation currentAnimation;
        private string currentAnimationName;

        public FurnitureAnimation()
        {
            //SpriteManager.current.GetSprite("Character", "IdleBack"),      
            animations = new Dictionary<string, SpritenameAnimation>();
        }

        public void SetSprites(Sprite[] s)
        {
            sprites = s;

            // make sure that every sprite has correct filtermode
            foreach (Sprite sprite in sprites)
            {
                sprite.texture.filterMode = FilterMode.Point;
            }
            /*
            animations.Add(AnimationType.HELMET_IDLE_NORTH, new FrameAnimation("in", new int[] { 0 }, 0.7f, false, false));
            animations.Add(AnimationType.HELMET_IDLE_EAST, new FrameAnimation("ie", new int[] { 1 }, 0.7f, false, false));
            animations.Add(AnimationType.HELMET_IDLE_SOUTH, new FrameAnimation("is", new int[] { 2 }, 0.7f, false, false));
            animations.Add(AnimationType.HELMET_IDLE_WEST, new FrameAnimation("iw", new int[] { 1 }, 0.7f, false, true));

            animations.Add(AnimationType.HELMET_WALK_NORTH, new FrameAnimation("wn", new int[] { 3, 4 }, 0.7f));
            animations.Add(AnimationType.HELMET_WALK_EAST, new FrameAnimation("we", new int[] { 5, 6 }, 0.7f));
            animations.Add(AnimationType.HELMET_WALK_SOUTH, new FrameAnimation("ws", new int[] { 7, 8 }, 0.7f));
            animations.Add(AnimationType.HELMET_WALK_WEST, new FrameAnimation("ww", new int[] { 5, 6 }, 0.7f, true, true));

            animations.Add(AnimationType.NOHELMET_IDLE_NORTH, new FrameAnimation("in", new int[] { 9 }, 0.2f, false, false));
            animations.Add(AnimationType.NOHELMET_IDLE_EAST, new FrameAnimation("ie", new int[] { 10 }, 0.2f, false, false));
            animations.Add(AnimationType.NOHELMET_IDLE_SOUTH, new FrameAnimation("is", new int[] { 11 }, 0.2f, false, false));
            animations.Add(AnimationType.NOHELMET_IDLE_WEST, new FrameAnimation("iw", new int[] { 10 }, 0.2f, false, true));

            animations.Add(AnimationType.NOHELMET_WALK_NORTH, new FrameAnimation("wn", new int[] { 12, 13 }, 0.2f));
            animations.Add(AnimationType.NOHELMET_WALK_EAST, new FrameAnimation("we", new int[] { 14, 15 }, 0.2f));
            animations.Add(AnimationType.NOHELMET_WALK_SOUTH, new FrameAnimation("ws", new int[] { 16, 17 }, 0.2f));
            animations.Add(AnimationType.NOHELMET_WALK_WEST, new FrameAnimation("ww", new int[] { 14, 15 }, 0.2f, true, true));
           
            currentAnimationType = AnimationType.HELMET_IDLE_SOUTH;
            currentAnimation = animations[currentAnimationType]; */
            prevFrameIndex = 0;
        }

        public void Update(float deltaTime)
        {
            /*
            int newAnimation = (int)character.CharFacing;
            if (character.IsWalking)
            {
                newAnimation += 10;
            }
      
            if (character.CurrTile.GetGasPressure("O2") >= 0.15f)
            {
                newAnimation += 100; // Remove helmet
            }

            // check if we need to switch animations
            if (newAnimation != (int)currentAnimationType)
            {
                currentAnimationType = (AnimationType)newAnimation;
                currentAnimation = animations[currentAnimationType];
                if (currentAnimation.FlipX == true && renderer.flipX == false)
                {
                    renderer.flipX = true;
                }
                else if (currentAnimation.FlipX == false && renderer.flipX == true)
                {
                    renderer.flipX = false;
                }
            }
            */
            Debug.ULogChannel("ani", "update " + currentAnimationName);
            currentAnimation.Update(deltaTime);

            if (prevFrameIndex != currentAnimation.CurrentFrame)
            {
                ShowSprite(currentAnimation.CurrentFrame);
                prevFrameIndex = currentAnimation.CurrentFrame;
            }

        }

        private void ShowSprite(int s)
        {
            if (Renderer != null)
            {
                Renderer.sprite = sprites[s];
            }            
        }

        public void AddAnimation(string name, string spriteBase, string frames, bool looping)
        {
            string[] framesArr = frames.Split(',');
            List<string> spriteNames = new List<string>();
            for (int i = 0; i < framesArr.Length - 1; i++)
            {
                spriteNames.Add(string.Concat(spriteBase, framesArr[i]));
            }
            animations.Add(name, new SpritenameAnimation(name, spriteNames.ToArray(), 0.5f, looping, false));

            currentAnimationName = name;
            currentAnimation = animations["name"];
            prevFrameIndex = 0;
        }
        public void AddAnimation(string name, string spriteBase, string frames, bool looping, string conditionParam,
                                 float conditionValue, string progressParam, float progressMin, float progressMax)
        {
            string[] framesArr = frames.Split(',');
            List<string> spriteNames = new List<string>();
            for (int i = 0; i < framesArr.Length - 1; i++)
            {
                spriteNames.Add(string.Concat(spriteBase, framesArr[i]));                
            }
            animations.Add(name, new SpritenameAnimation(name, spriteNames.ToArray(), 0.5f, true, false));

            currentAnimationName = name;
            currentAnimation = animations["name"];
            prevFrameIndex = 0;
        }
    }
}