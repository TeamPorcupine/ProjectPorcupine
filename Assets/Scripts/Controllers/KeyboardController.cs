#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController
{
    [Range(0, 3)]
    public float scrollSpeed = 0.1f;

    private WorldController wc;

    private KeyboardMap keyboardMap;

    // Use this for initialization.
    public KeyboardController(WorldController worldController)
    {
        wc = worldController;
        keyboardMap = new KeyboardMap();

        //TODO replace this by loading settings when the discussion about XML vs JSON ends
        keyboardMap.FromList(new List<KeyboadMappedInput>
        {
            new KeyboadMappedInput(InputNames.ZoomOut, KeyCode.PageUp, KeyCode.PageUp),
            new KeyboadMappedInput(InputNames.ZoomIn, KeyCode.PageDown, KeyCode.PageDown),
            new KeyboadMappedInput(InputNames.MoveCameraNorth, KeyCode.W, KeyCode.UpArrow),
            new KeyboadMappedInput(InputNames.MoveCameraSouth, KeyCode.S, KeyCode.DownArrow),
            new KeyboadMappedInput(InputNames.MoveCameraWest, KeyCode.A, KeyCode.LeftArrow),
            new KeyboadMappedInput(InputNames.MoveCameraEast, KeyCode.D, KeyCode.RightArrow),
            new KeyboadMappedInput(InputNames.MoveCameraUp, KeyCode.Home, KeyCode.Home),
            new KeyboadMappedInput(InputNames.MoveCameraDown, KeyCode.End, KeyCode.End),
            new KeyboadMappedInput(InputNames.SetSpeed1, KeyCode.Alpha1, KeyCode.Keypad1),
            new KeyboadMappedInput(InputNames.SetSpeed2, KeyCode.Alpha2, KeyCode.Keypad2),
            new KeyboadMappedInput(InputNames.SetSpeed3, KeyCode.Alpha3, KeyCode.Keypad3),
            new KeyboadMappedInput(InputNames.DecreaseSpeed, KeyCode.Minus, KeyCode.KeypadMinus),
            new KeyboadMappedInput(InputNames.IncreaseSpeed, KeyCode.Plus, KeyCode.KeypadPlus),
            new KeyboadMappedInput(InputNames.Pause, KeyCode.Space, KeyCode.Pause),
        });    
    }

    // Update is called once per frame.
    public void Update(bool isModal)
    {
        if (isModal)
        {
            // A modal dialog box is open. Bail.
            return;
        }

        CheckCameraInput();
        CheckTimeInput();
    }

    private void CheckCameraInput()
    {
        float horizontal = 0;
        float vertical = 0;

        if (keyboardMap.GetKey(InputNames.MoveCameraEast))
        {
            horizontal += 1;
        }
        if (keyboardMap.GetKey(InputNames.MoveCameraWest))
        {
            horizontal -= 1;
        }
        if (keyboardMap.GetKey(InputNames.MoveCameraNorth))
        {
            vertical += 1;
        }
        if (keyboardMap.GetKey(InputNames.MoveCameraSouth))
        {
            vertical -= 1;
        }

        Vector3 inputAxis = new Vector3(horizontal, vertical, 0);
        Camera.main.transform.position += Camera.main.orthographicSize * scrollSpeed * inputAxis;

        if (keyboardMap.GetKey(InputNames.ZoomOut))
        {
            wc.cameraController.ChangeZoom(0.1f);
        }

        if (keyboardMap.GetKey(InputNames.ZoomIn))
        {
            wc.cameraController.ChangeZoom(-0.1f);
        }

        if (keyboardMap.GetKey(InputNames.MoveCameraUp))
        {
            wc.cameraController.ChangeLayerUp();
        }

        if (keyboardMap.GetKey(InputNames.MoveCameraDown))
        {
            wc.cameraController.ChangeLayerDown();
        }
    }

    private void CheckTimeInput()
    {
        // TODO: Move this into centralized keyboard manager where
        // all of the buttons can be rebinded.
        if (keyboardMap.GetKeyDown(InputNames.Pause))
        {
            wc.IsPaused = !wc.IsPaused;
            Debug.ULogChannel("KeyboardController", "Game " + (wc.IsPaused ? "paused" : "resumed"));
        }

        if (keyboardMap.GetKeyDown(InputNames.IncreaseSpeed))
        {
            wc.timeManager.IncreaseTimeScale();
        }
        else if (keyboardMap.GetKeyDown(InputNames.DecreaseSpeed))
        {
            wc.timeManager.DecreaseTimeScale();
        }
        else if (keyboardMap.GetKeyDown(InputNames.SetSpeed1))
        {
            wc.timeManager.SetTimeScalePosition(2);
        }
        else if (keyboardMap.GetKeyDown(InputNames.SetSpeed2))
        {
            wc.timeManager.SetTimeScalePosition(3);
        }
        else if (keyboardMap.GetKeyDown(InputNames.SetSpeed3))
        {
            wc.timeManager.SetTimeScalePosition(4);
        }
    }
}
