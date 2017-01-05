#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

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
        
        // Collection of animations
        private Dictionary<AnimationType, SpritenameAnimation> animations;
        private SpritenameAnimation currentAnimation;
        private AnimationType currentAnimationType;

        private float lastCharYPosition = 0;

        public CharacterAnimation(Character c, SpriteRenderer r)
        {
            character = c;
            renderer = r;

            SetSprites();
        }

        public int CurrentSortingOrder { get; private set; }
        
        /// <summary>
        /// Sets sortingOrder on the character and returns the value.
        /// </summary>
        public int SetAndGetSortOrder()
        {
            // if there was change in Y position, update the sorting order
            if (lastCharYPosition - character.Y != 0f)
            {
                CurrentSortingOrder = Mathf.RoundToInt(character.Y * 100f) * -1;
                renderer.sortingOrder = CurrentSortingOrder;
            }

            lastCharYPosition = character.Y;
            return renderer.sortingOrder;
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

                // Make sure we update the sprite
                prevFrameIndex = -1;

                if (currentAnimation.FlipX == true && renderer.flipX == false)
                {
                    renderer.flipX = true;
                }
                else if (currentAnimation.FlipX == false && renderer.flipX == true)
                {
                    renderer.flipX = false;
                }
            }

            currentAnimation.Update(deltaTime * character.MovementSpeed);

            if (prevFrameIndex != currentAnimation.CurrentFrame)
            {
                ShowSprite(currentAnimation.CurrentFrameName);
                prevFrameIndex = currentAnimation.CurrentFrame;
            }
        }

        private void SetSprites()
        {
            animations = new Dictionary<AnimationType, SpritenameAnimation>();

            animations.Add(AnimationType.HELMET_IDLE_NORTH, new SpritenameAnimation(AnimationType.HELMET_IDLE_NORTH.ToString(), new string[] { "IdleBack" }, 0.7f, false));
            animations.Add(AnimationType.HELMET_IDLE_EAST, new SpritenameAnimation(AnimationType.HELMET_IDLE_EAST.ToString(), new string[] { "IdleSide" }, 0.7f, false));
            animations.Add(AnimationType.HELMET_IDLE_SOUTH, new SpritenameAnimation(AnimationType.HELMET_IDLE_SOUTH.ToString(), new string[] { "IdleFront" }, 0.7f, false));
            animations.Add(AnimationType.HELMET_IDLE_WEST, new SpritenameAnimation(AnimationType.HELMET_IDLE_WEST.ToString(), new string[] { "IdleSide" }, 0.7f, false, true));

            animations.Add(AnimationType.HELMET_WALK_NORTH, new SpritenameAnimation(AnimationType.HELMET_WALK_NORTH.ToString(), new string[] { "WalkBack1", "WalkBack2" }, 0.7f));
            animations.Add(AnimationType.HELMET_WALK_EAST, new SpritenameAnimation(AnimationType.HELMET_WALK_EAST.ToString(), new string[] { "WalkSide1", "WalkSide2" }, 0.7f));
            animations.Add(AnimationType.HELMET_WALK_SOUTH, new SpritenameAnimation(AnimationType.HELMET_WALK_SOUTH.ToString(), new string[] { "WalkFront1", "WalkFront2" }, 0.7f));
            animations.Add(AnimationType.HELMET_WALK_WEST, new SpritenameAnimation(AnimationType.HELMET_WALK_WEST.ToString(), new string[] { "WalkSide1", "WalkSide2" }, 0.7f, true, true));

            animations.Add(AnimationType.NOHELMET_IDLE_NORTH, new SpritenameAnimation(AnimationType.NOHELMET_IDLE_NORTH.ToString(), new string[] { "2IdleBack" }, 0.7f, false));
            animations.Add(AnimationType.NOHELMET_IDLE_EAST, new SpritenameAnimation(AnimationType.NOHELMET_IDLE_EAST.ToString(), new string[] { "2IdleSide" }, 0.7f, false));
            animations.Add(AnimationType.NOHELMET_IDLE_SOUTH, new SpritenameAnimation(AnimationType.NOHELMET_IDLE_SOUTH.ToString(), new string[] { "2IdleFront" }, 0.7f, false));
            animations.Add(AnimationType.NOHELMET_IDLE_WEST, new SpritenameAnimation(AnimationType.NOHELMET_IDLE_WEST.ToString(), new string[] { "2IdleSide" }, 0.7f, false, true));

            animations.Add(AnimationType.NOHELMET_WALK_NORTH, new SpritenameAnimation(AnimationType.NOHELMET_WALK_NORTH.ToString(), new string[] { "2WalkBack1", "2WalkBack2" }, 0.7f));
            animations.Add(AnimationType.NOHELMET_WALK_EAST, new SpritenameAnimation(AnimationType.NOHELMET_WALK_EAST.ToString(), new string[] { "2WalkSide1", "2WalkSide2" }, 0.7f));
            animations.Add(AnimationType.NOHELMET_WALK_SOUTH, new SpritenameAnimation(AnimationType.NOHELMET_WALK_SOUTH.ToString(), new string[] { "2WalkFront1", "2WalkFront2" }, 0.7f));
            animations.Add(AnimationType.NOHELMET_WALK_WEST, new SpritenameAnimation(AnimationType.NOHELMET_WALK_WEST.ToString(), new string[] { "2WalkSide1", "2WalkSide2" }, 0.7f, true, true));

            currentAnimationType = AnimationType.HELMET_IDLE_SOUTH;
            currentAnimation = animations[currentAnimationType];
            prevFrameIndex = 0;
        }

        private void ShowSprite(string spriteName)
        {
            if (renderer != null)
            {
                renderer.sprite = SpriteManager.GetSprite("Character", spriteName);                
            }
        }
    }
}