#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System.Collections;
using UnityEngine;

public class KeyboardController
{
    [Range(0, 3)]
    public float scrollSpeed = 0.1f;

    private BuildModeController bmc;
    private WorldController wc;

    // An array of possible time multipliers.
    private float[] possibleTimeScales = new float[6] { 0.1f, 0.5f, 1f, 2f, 4f, 8f };
    
    // Current position in that array.
    private int currentTimeScalePosition = 2;

    // Use this for initialization.
    public KeyboardController(BuildModeController buildModeController, WorldController worldController)
    {
        bmc = buildModeController;
        wc = worldController;
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
        // React to hor./vert. axis (WASD or up/down/...)
        Camera.main.transform.position +=
            Camera.main.orthographicSize * scrollSpeed *
            new Vector3(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"),
                0);

        if (Input.GetKey(KeyCode.PageUp))
        {
            wc.cameraController.ChangeZoom(0.1f);
        }

        if (Input.GetKey(KeyCode.PageDown))
        {
            wc.cameraController.ChangeZoom(-0.1f);
        }
    }

    private void CheckTimeInput()
    {
        // TODO: Move this into centralized keyboard manager where
        // all of the buttons can be rebinded.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            wc.IsPaused = !wc.IsPaused;
            Debug.ULogChannel("KeyboardController", "Game " + (wc.IsPaused ? "paused" : "resumed"));
        }

        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (currentTimeScalePosition == possibleTimeScales.Length - 1)
            {
                // We are on the top of possibleTimeScales so just bail out.
                return;
            }

            currentTimeScalePosition++;
            wc.SetTimeScale(possibleTimeScales[currentTimeScalePosition]);
        }
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (currentTimeScalePosition == 0)
            {
                // We are on the bottom of possibleTimeScales so just bail out.
                return;
            }

            currentTimeScalePosition--;
            wc.SetTimeScale(possibleTimeScales[currentTimeScalePosition]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            wc.SetTimeScale(1f);
            currentTimeScalePosition = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            wc.SetTimeScale(2f);
            currentTimeScalePosition = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            wc.SetTimeScale(4f);
            currentTimeScalePosition = 4;
        }
    }
}
