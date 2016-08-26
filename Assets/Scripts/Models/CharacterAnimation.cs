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

// these enums should make it easy to add 10 if walking and add 100 if helmet is off
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

    // frames before loop. halfway through the next frame is triggered
    private int animationLength = 40;

    // TODO: should be more flexible ....
    private Sprite[] sprites = new Sprite[9];

    // Collection of animations
    private Dictionary<AnimationType, Animation> animations;
    private Animation currentAnimation;
    private AnimationType currentAnimationType;

    public CharacterAnimation(Character c, SpriteRenderer r)
    {
        character = c;
        renderer = r;

        Sprite[] sprites =
            {
                SpriteManager.current.GetSprite("Character", "tp2_idle_north"),
                SpriteManager.current.GetSprite("Character", "tp2_idle_east"),
                SpriteManager.current.GetSprite("Character", "tp2_idle_south"),
                SpriteManager.current.GetSprite("Character", "tp2_walk_north_01"),
                SpriteManager.current.GetSprite("Character", "tp2_walk_north_02"),
                SpriteManager.current.GetSprite("Character", "tp2_walk_east_01"),
                SpriteManager.current.GetSprite("Character", "tp2_walk_east_02"),
                SpriteManager.current.GetSprite("Character", "tp2_walk_south_01"),
                SpriteManager.current.GetSprite("Character", "tp2_walk_south_02"),

                SpriteManager.current.GetSprite("Character", "tp2_nh_idle_north"),
                SpriteManager.current.GetSprite("Character", "tp2_nh_idle_east"),
                SpriteManager.current.GetSprite("Character", "tp2_nh_idle_south"),
                SpriteManager.current.GetSprite("Character", "tp2_nh_walk_north_01"),
                SpriteManager.current.GetSprite("Character", "tp2_nh_walk_north_02"),
                SpriteManager.current.GetSprite("Character", "tp2_nh_walk_east_01"),
                SpriteManager.current.GetSprite("Character", "tp2_nh_walk_east_02"),
                SpriteManager.current.GetSprite("Character", "tp2_nh_walk_south_01"),
                SpriteManager.current.GetSprite("Character", "tp2_nh_walk_south_02")
            };
        SetSprites(sprites);
    }

    public void SetSprites(Sprite[] s)
    {
        sprites = s;

        // make sure that every sprite has correct filtermode
        foreach (Sprite sprite in sprites)
        {
            sprite.texture.filterMode = FilterMode.Point;
        }

        animations = new Dictionary<AnimationType, Animation>();

        animations.Add(AnimationType.HELMET_IDLE_NORTH, new Animation("in", new int[] { 0 }, 0.5f, false, false));
        animations.Add(AnimationType.HELMET_IDLE_EAST, new Animation("ie", new int[] { 1 }, 0.5f, false, false));
        animations.Add(AnimationType.HELMET_IDLE_SOUTH, new Animation("is", new int[] { 2 }, 0.5f, false, false));
        animations.Add(AnimationType.HELMET_IDLE_WEST, new Animation("iw", new int[] { 1 }, 0.5f, false, true));

        animations.Add(AnimationType.HELMET_WALK_NORTH, new Animation("wn", new int[] { 3, 4 }));
        animations.Add(AnimationType.HELMET_WALK_EAST, new Animation("we", new int[] { 5, 6 }));
        animations.Add(AnimationType.HELMET_WALK_SOUTH, new Animation("ws", new int[] { 7, 8 }));
        animations.Add(AnimationType.HELMET_WALK_WEST, new Animation("ww", new int[] { 5, 6 }, 0.5f, true, true));

        animations.Add(AnimationType.NOHELMET_IDLE_NORTH, new Animation("in", new int[] { 9 }, 0.2f, false, false));
        animations.Add(AnimationType.NOHELMET_IDLE_EAST, new Animation("ie", new int[] { 10 }, 0.2f, false, false));
        animations.Add(AnimationType.NOHELMET_IDLE_SOUTH, new Animation("is", new int[] { 11 }, 0.2f, false, false));
        animations.Add(AnimationType.NOHELMET_IDLE_WEST, new Animation("iw", new int[] { 10 }, 0.2f, false, true));

        animations.Add(AnimationType.NOHELMET_WALK_NORTH, new Animation("wn", new int[] { 12, 13 }, 0.2f));
        animations.Add(AnimationType.NOHELMET_WALK_EAST, new Animation("we", new int[] { 14, 15 }, 0.2f));
        animations.Add(AnimationType.NOHELMET_WALK_SOUTH, new Animation("ws", new int[] { 16, 17 }, 0.2f));
        animations.Add(AnimationType.NOHELMET_WALK_WEST, new Animation("ww", new int[] { 14, 15 }, 0.2f, true, true));

        currentAnimationType = AnimationType.HELMET_IDLE_SOUTH;
        currentAnimation = animations[currentAnimationType];
        prevFrameIndex = 0;
    }

    public void Update(float deltaTime)
    {
        /*
        if (currentFrame >= animationLength)
        {
            currentFrame = 0;
        }

        currentFrame++;
        */
        int newAnimation = (int)character.CharFacing;
        if (character.IsWalking)
        {
            newAnimation += 10;
        }
        
        if (character.CurrTile.Room != null)
        {
            if (character.CurrTile.Room.GetGasAmount("O2") > 0.005f)
            {
                newAnimation += 100; // Remove helmet
            }           
        }

        if (newAnimation != (int)currentAnimationType)
        {
            currentAnimationType = (AnimationType)newAnimation;
            currentAnimation = animations[currentAnimationType];
            if (currentAnimation.FlipX == true && renderer.flipX == false)
            {
                renderer.flipX = true;
            }
            else
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
    }    

    private void ShowSprite(int s)
    {
        renderer.sprite = sprites[s];
    }
}