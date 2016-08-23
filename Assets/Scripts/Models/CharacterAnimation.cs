using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CharacterAnimation gets reference to character, and should be able to 
/// figure out which animation to play, by looking at character Facing and IsMoving
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
        if (currentFrame >= animationLength) currentFrame = 0;
        currentFrame++;

        callAnimation();
    }

    private void callAnimation()
    {
        if (character.IsWalking)
        {
            // character walking
            switch (character.Facing)
            {
                case 0: // walk north
                    toggleAnimation(5, 6);
                    renderer.flipX = false;
                    break;
                case 1: // walk east
                    toggleAnimation(3, 4);
                    renderer.flipX = false;
                    break;
                case 2: // walk south
                    toggleAnimation(7, 8);
                    renderer.flipX = false;
                    break;
                case 3: // walk west
                    toggleAnimation(3, 4); // FLIP east sprite
                    renderer.flipX = true;
                    break;
                default:
                    break;
            }
        }
        else
        {
            //character idle
            switch (character.Facing)
            {
                case 0: // walk north
                    showSprite(2);
                    renderer.flipX = false;
                    break;
                case 1: // walk east
                    showSprite(1);
                    renderer.flipX = false;
                    break;
                case 2: // walk south
                    showSprite(0);
                    renderer.flipX = false;
                    break;
                case 3: // walk west
                    showSprite(1); // FLIP east sprite
                    renderer.flipX = true;
                    break;
                default:
                    break;
            }
        }
    }

    private void toggleAnimation(int s1, int s2)
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

    private void showSprite(int s)
    {        
        renderer.sprite = sprites[s];                       
    }

    public void setSprites(Sprite[] s)
    {
        sprites = s;
    }

}

