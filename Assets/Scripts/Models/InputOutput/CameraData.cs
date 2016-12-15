#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using Newtonsoft.Json.Linq;
using UnityEngine;

public struct Preset
{
    public Vector3 position;
    public float zoomLevel;
    public int zLevel;
}

public class CameraData
{
    public Vector3 position;
    public float zoomLevel;
    public int zLevel;
    public Preset[] presets;

    public JToken ToJson()
    {
        JObject cameraJson = new JObject();

        cameraJson.Add("X", Camera.main.transform.position.x);
        cameraJson.Add("Y", Camera.main.transform.position.y);
        cameraJson.Add("Z", Camera.main.transform.position.z);
        cameraJson.Add("ZoomLevel", Camera.main.orthographicSize);
        cameraJson.Add("ZLevel", WorldController.Instance.cameraController.CurrentLayer);

        JArray presetsJson = new JArray();

        foreach (Preset preset in presets)
        {
            JObject presetJson = new JObject();
            presetJson.Add("X", preset.position.x);
            presetJson.Add("Y", preset.position.y);
            presetJson.Add("Z", preset.position.z);
            presetJson.Add("ZoomLevel", preset.zoomLevel);
            presetsJson.Add(presetJson);
        }

        cameraJson.Add("Presets", presetsJson);

        return cameraJson;
    }

    public void FromJson(JToken cameraDataToken)
    {
        if (cameraDataToken == null)
        {
            return;
        }

        int x = (int)cameraDataToken["X"];
        int y = (int)cameraDataToken["Y"];
        int z = (int)cameraDataToken["Z"];
        float zoomLevel = (float)cameraDataToken["ZoomLevel"];
        zLevel = (int)cameraDataToken["ZLevel"];
        Vector3 camPosition = new Vector3(x, y, z);
        Camera.main.transform.position = camPosition;
        Camera.main.orthographicSize = zoomLevel;
    }
}
