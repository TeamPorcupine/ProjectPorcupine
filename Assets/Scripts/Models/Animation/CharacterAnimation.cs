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

// these enums should make it easy to add 10 if walking and add 100 if helmet is off
namespace Animation
{
    public enum AnimationType
    {
        HELMET_IDLE_NORTH = 0,
        HELMET_IDLE_EAST = 1,
        HELMET_IDLE_SOUTH = 2,
        HELMET_IDLE_WEST = 3,

        HELMET_WALK_NORTH = 10,
        HELMET_WALK_EAST = 11,
        HELMET_WALK_SOUTH = 12,
        HELMET_WALK_WEST = 13,

        NOHELMET_IDLE_NORTH = 100,
        NOHELMET_IDLE_EAST = 101,
        NOHELMET_IDLE_SOUTH = 102,
        NOHELMET_IDLE_WEST = 103,

        NOHELMET_WALK_NORTH = 110,
        NOHELMET_WALK_EAST = 111,
        NOHELMET_WALK_SOUTH = 112,
        NOHELMET_WALK_WEST = 113
    }

    /// <summary>
    /// CharacterAnimation gets reference to character, and should be able to 
    /// figure out which animation to play, by looking at character Facing and IsMoving.
    /// </summary>
    public class CharacterAnimation
    {
        private Character character;
        private SpriteRenderer renderer;

        // current shown frame
        private int prevFrameIndex;

        // Holds the actual animation sprites from the spritesheet
        private Sprite[] sprites;

        // Collection of animations
        private Dictionary<AnimationType, FrameAnimation> animations;
        private FrameAnimation currentAnimation;
        private AnimationType currentAnimationType;

        private float lastCharYPosition = 0;

        public CharacterAnimation(Character c, SpriteRenderer r)
        {
            character = c;
            renderer = r;

            Sprite[] sprites =
                {
                    SpriteManager.current.GetSprite("Character", "IdleBack"),
                    SpriteManager.current.GetSprite("Character", "IdleSide"),
                    SpriteManager.current.GetSprite("Character", "IdleFront"),
                    SpriteManager.current.GetSprite("Character", "WalkBack1"),
                    SpriteManager.current.GetSprite("Character", "WalkBack2"),
                    SpriteManager.current.GetSprite("Character", "WalkSide1"),
                    SpriteManager.current.GetSprite("Character", "WalkSide2"),
                    SpriteManager.current.GetSprite("Character", "WalkFront1"),
                    SpriteManager.current.GetSprite("Character", "WalkFront2"),

                    SpriteManager.current.GetSprite("Character", "2IdleBack"),
                    SpriteManager.current.GetSprite("Character", "2IdleSide"),
                    SpriteManager.current.GetSprite("Character", "2IdleFront"),
                    SpriteManager.current.GetSprite("Character", "2WalkBack1"),
                    SpriteManager.current.GetSprite("Character", "2WalkBack2"),
                    SpriteManager.current.GetSprite("Character", "2WalkSide1"),
                    SpriteManager.current.GetSprite("Character", "2WalkSide2"),
                    SpriteManager.current.GetSprite("Character", "2WalkFront1"),
                    SpriteManager.current.GetSprite("Character", "2WalkFront2")
                };
            SetSprites(sprites);
        }

        public int CurrentSortingOrder { get; private set; }

        public void SetSprites(Sprite[] s)
        {
            sprites = s;
            
            animations = new Dictionary<AnimationType, FrameAnimation>();

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
            currentAnimation = animations[currentAnimationType];
            prevFrameIndex = 0;
        }

        public void Update(float deltaTime)
        {
            int newAnimation = (int)character.CharFacing;
            if (character.IsWalking)
            {
                newAnimation += 10;
            }

            // TODO: What is the acceptable amount of O2 gas pressure? 
            // Using .15 from Need.cs for now.        
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

            currentAnimation.Update(deltaTime);

            if (prevFrameIndex != currentAnimation.CurrentIndex)
            {
                ShowSprite(currentAnimation.CurrentIndex);
                prevFrameIndex = currentAnimation.CurrentIndex;
            }

            // if there was change in Y position, update the sorting order
            if (lastCharYPosition - character.Y != 0f)
            {
                CurrentSortingOrder = Mathf.RoundToInt(character.Y * 100f) * -1;
                renderer.sortingOrder = CurrentSortingOrder;
            }

            lastCharYPosition = character.Y;
        }

        private void ShowSprite(int s)
        {
            renderer.sprite = sprites[s];
        }
    }
}