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

    // Holds the actual animation sprites from the spritesheet
    private Sprite[] sprites;

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

    public void SetSprites(Sprite[] s)
    {
        sprites = s;

        // make sure that every sprite has correct filtermode
        foreach (Sprite sprite in sprites)
        {
            sprite.texture.filterMode = FilterMode.Point;
        }

        animations = new Dictionary<AnimationType, Animation>();

        animations.Add(AnimationType.HELMET_IDLE_NORTH, new Animation("in", new int[] { 0 }, 0.7f, false, false));
        animations.Add(AnimationType.HELMET_IDLE_EAST, new Animation("ie", new int[] { 1 }, 0.7f, false, false));
        animations.Add(AnimationType.HELMET_IDLE_SOUTH, new Animation("is", new int[] { 2 }, 0.7f, false, false));
        animations.Add(AnimationType.HELMET_IDLE_WEST, new Animation("iw", new int[] { 1 }, 0.7f, false, true));

        animations.Add(AnimationType.HELMET_WALK_NORTH, new Animation("wn", new int[] { 3, 4 }, 0.7f));
        animations.Add(AnimationType.HELMET_WALK_EAST, new Animation("we", new int[] { 5, 6 }, 0.7f));
        animations.Add(AnimationType.HELMET_WALK_SOUTH, new Animation("ws", new int[] { 7, 8 }, 0.7f));
        animations.Add(AnimationType.HELMET_WALK_WEST, new Animation("ww", new int[] { 5, 6 }, 0.7f, true, true));

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
        int newAnimation = (int)character.CharFacing;
        if (character.IsWalking)
        {
            newAnimation += 10;
        }

        // TODO: What is the acceptable amount of O2 gas pressure? A little less than .2?
        // for now, it's set very low, so the change is visible for testing.
        if (character.CurrTile.GetGasPressure("O2") >= 0.005f)
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
    }    

    private void ShowSprite(int s)
    {
        renderer.sprite = sprites[s];
    }
}