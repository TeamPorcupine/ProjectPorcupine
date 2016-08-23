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

/// <summary>
/// CharacterAnimation gets reference to character, and should be able to 
/// figure out which animation to play, by looking at character Facing and IsMoving.
/// </summary>
public class CharacterAnimation
{
    private Character character;
    private SpriteRenderer renderer;

    // currentframe is incremented each update... fix to run on time instead
    private int currentFrame = 0;

    // frames before loop. halfway through the next frame is triggered
    private int animationLength = 40;

    // TODO: should be more flexible ....
    private Sprite[] sprites = new Sprite[9];

    public CharacterAnimation(Character c, SpriteRenderer r)
    {
        character = c;
        renderer = r;
    }

    public void Update(float deltaTime)
    {
        if (currentFrame >= animationLength)
        {
            currentFrame = 0;
        }

        currentFrame++;

        CallAnimation();
    }

    public void SetSprites(Sprite[] s)
    {
        sprites = s;
    }

    private void CallAnimation()
    {
        if (character.IsWalking)
        {
            // character walking
            switch (character.Facing)
            {
                case 0: // walk north
                    ToggleAnimation(5, 6);
                    renderer.flipX = false;
                    break;
                case 1: // walk east
                    ToggleAnimation(3, 4);
                    renderer.flipX = false;
                    break;
                case 2: // walk south
                    ToggleAnimation(7, 8);
                    renderer.flipX = false;
                    break;
                case 3: // walk west
                    ToggleAnimation(3, 4); // FLIP east sprite
                    renderer.flipX = true;
                    break;
                default:
                    break;
            }
        }
        else
        {
            // character idle
            switch (character.Facing)
            {
                case 0: // walk north
                    ShowSprite(2);
                    renderer.flipX = false;
                    break;
                case 1: // walk east
                    ShowSprite(1);
                    renderer.flipX = false;
                    break;
                case 2: // walk south
                    ShowSprite(0);
                    renderer.flipX = false;
                    break;
                case 3: // walk west
                    ShowSprite(1); // FLIP east sprite
                    renderer.flipX = true;
                    break;
                default:
                    break;
            }
        }
    }

    private void ToggleAnimation(int s1, int s2)
    {
        if (currentFrame == 1)
        {
            renderer.sprite = sprites[s1];
        }
        else if (currentFrame == animationLength / 2)
        {
            renderer.sprite = sprites[s2];
        }        
    }

    private void ShowSprite(int s)
    {        
        renderer.sprite = sprites[s];                       
    }
}