#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;

public class CameraController
{
    private float zoomTarget;

    // Update is called once per frame.
    public void Update(bool modal)
    {
        if (modal)
        {
            // A modal dialog box is open. Bail.
            return;
        }

        Vector3 oldMousePosition;
        oldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        oldMousePosition.z = 0;

        if (Camera.main.orthographicSize != zoomTarget)
        {
            float target = Mathf.Lerp(Camera.main.orthographicSize, zoomTarget, Settings.GetSettingAsFloat("ZoomLerp", 3) * Time.deltaTime);
            Camera.main.orthographicSize = Mathf.Clamp(target, 3f, 25f);
        }

        // Refocus game so the mouse stays in the same spot when zooming.
        Vector3 newMousePosition = Vector3.zero;
        newMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newMousePosition.z = 0;

        Vector3 pushedAmount = oldMousePosition - newMousePosition;
        Camera.main.transform.Translate(pushedAmount);
    }

    public void ChangeZoom(float amount)
    {
        zoomTarget = Camera.main.orthographicSize - (Settings.GetSettingAsFloat("ZoomSensitivity", 3) * (Camera.main.orthographicSize * amount));
    }
}
