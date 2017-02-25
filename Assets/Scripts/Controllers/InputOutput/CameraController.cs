#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using UnityEngine;

public class CameraController
{
    [Range(0, 3)]
    public float scrollSpeed = 0.1f;

    // Zoom of the main camera.
    private float zoomTarget = 11f;
    private int currentLayer;
    private Camera[] layerCameras;

    private bool presetBeingApplied;

    private float frameMoveHorizontal = 0;
    private float frameMoveVertical = 0;

    private Vector3 positionTarget;
    private Vector3 prevPositionTarget;

    private CameraData cameraData;

    public CameraController()
    {
        // Main camera handles UI only
        Camera.main.farClipPlane = 9;

        cameraData = World.Current.CameraData;

        KeyboardManager keyboardManager = KeyboardManager.Instance;
        keyboardManager.RegisterInputAction("MoveCameraEast", KeyboardMappedInputType.Key, () => { frameMoveHorizontal++; });
        keyboardManager.RegisterInputAction("MoveCameraWest", KeyboardMappedInputType.Key, () => { frameMoveHorizontal--; });
        keyboardManager.RegisterInputAction("MoveCameraNorth", KeyboardMappedInputType.Key, () => { frameMoveVertical++; });
        keyboardManager.RegisterInputAction("MoveCameraSouth", KeyboardMappedInputType.Key, () => { frameMoveVertical--; });
        keyboardManager.RegisterInputAction("ZoomOut", KeyboardMappedInputType.Key, () => ChangeZoom(0.1f));
        keyboardManager.RegisterInputAction("ZoomIn", KeyboardMappedInputType.Key, () => ChangeZoom(-0.1f));
        keyboardManager.RegisterInputAction("MoveCameraUp", KeyboardMappedInputType.KeyUp, ChangeLayerUp);
        keyboardManager.RegisterInputAction("MoveCameraDown", KeyboardMappedInputType.KeyUp, ChangeLayerDown);

        keyboardManager.RegisterInputAction("ApplyCameraPreset1", KeyboardMappedInputType.KeyUp, () => ApplyPreset(cameraData.presets[0]));
        keyboardManager.RegisterInputAction("ApplyCameraPreset2", KeyboardMappedInputType.KeyUp, () => ApplyPreset(cameraData.presets[1]));
        keyboardManager.RegisterInputAction("ApplyCameraPreset3", KeyboardMappedInputType.KeyUp, () => ApplyPreset(cameraData.presets[2]));
        keyboardManager.RegisterInputAction("ApplyCameraPreset4", KeyboardMappedInputType.KeyUp, () => ApplyPreset(cameraData.presets[3]));
        keyboardManager.RegisterInputAction("ApplyCameraPreset5", KeyboardMappedInputType.KeyUp, () => ApplyPreset(cameraData.presets[4]));
        keyboardManager.RegisterInputAction("AssignCameraPreset1", KeyboardMappedInputType.KeyUp, () => AssignPreset(0));
        keyboardManager.RegisterInputAction("AssignCameraPreset2", KeyboardMappedInputType.KeyUp, () => AssignPreset(1));
        keyboardManager.RegisterInputAction("AssignCameraPreset3", KeyboardMappedInputType.KeyUp, () => AssignPreset(2));
        keyboardManager.RegisterInputAction("AssignCameraPreset4", KeyboardMappedInputType.KeyUp, () => AssignPreset(3));
        keyboardManager.RegisterInputAction("AssignCameraPreset5", KeyboardMappedInputType.KeyUp, () => AssignPreset(4));

        // Set default zoom value on camera
        Camera.main.orthographicSize = zoomTarget;

        positionTarget = Camera.main.transform.position;
        TimeManager.Instance.EveryFrameNotModal += (time) => Update();
    }

    public event Action<Bounds> Moved;

    public int CurrentLayer
    {
        get
        {
            return currentLayer;
        }
    }

    // Update is called once per frame.
    public void Update()
    {
        CreateLayerCameras();

        Vector3 inputAxis = new Vector3(frameMoveHorizontal, frameMoveVertical, 0);
        Camera.main.transform.position += Camera.main.orthographicSize * scrollSpeed * inputAxis;
        frameMoveHorizontal = 0;
        frameMoveVertical = 0;

        Vector3 oldMousePosition;
        oldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        oldMousePosition.z = 0;

        if (Camera.main.orthographicSize != zoomTarget)
        {
            float target = Mathf.Lerp(Camera.main.orthographicSize, zoomTarget, SettingsKeyHolder.ZoomLerp * Time.deltaTime);
            Camera.main.orthographicSize = Mathf.Clamp(target, 3f, 25f);
            SyncCameras();
        }

        if (Vector3.Distance(Camera.main.transform.position, positionTarget) > 0.1f && presetBeingApplied)
        {
            // The ZoomLerp interpolation value is used here so that the moving and zooming of the camera when appliyng a preset take the same amount of time
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, positionTarget, SettingsKeyHolder.ZoomLerp * Time.deltaTime);
        }
        else
        {
            Camera.main.transform.position = positionTarget;
            presetBeingApplied = false;
        }

        if (!presetBeingApplied)
        {
            // Refocus game so the mouse stays in the same spot when zooming.
            Vector3 newMousePosition = Vector3.zero;
            newMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newMousePosition.z = 0;

            Vector3 pushedAmount = oldMousePosition - newMousePosition;
            Camera.main.transform.Translate(pushedAmount);
            positionTarget = Camera.main.transform.position;
        }

        if (prevPositionTarget != positionTarget && Moved != null)
        {
            Moved(GetCameraBounds());
        }

        prevPositionTarget = positionTarget;

        WorldController.Instance.soundController.SetListenerPosition(Camera.main.transform.position.x, Camera.main.transform.position.y, (float)CurrentLayer);
    }

    public void ChangeZoom(float amount)
    {
        zoomTarget = Camera.main.orthographicSize - (SettingsKeyHolder.ZoomSensitivity * (Camera.main.orthographicSize * amount));
    }

    public void ChangeLayer(int newLayer)
    {
        if (layerCameras != null && newLayer >= 0 && newLayer < layerCameras.Length)
        {
            currentLayer = newLayer;
            for (int i = 0; i < layerCameras.Length; i++)
            {
                if (i < newLayer)
                {
                    layerCameras[i].gameObject.SetActive(false);
                }
                else
                {
                    layerCameras[i].gameObject.SetActive(true);
                }
            }

            SyncCameras();
        }
    }

    public void ChangeLayerUp()
    {
        ChangeLayer(currentLayer - 1);
    }

    public void ChangeLayerDown()
    {
        ChangeLayer(currentLayer + 1);
    }

    /// <summary>
    /// Sets up the camera and camera presets if necessary. 
    /// Presets will be null if loading an empty world and its values are then taken from the current camera values;
    /// if loading from a file, preset values are fed to the camera.
    /// </summary>
    public void Initialize()
    {
        if (cameraData.presets == null)
        {
            cameraData.presets = new Preset[5];

            cameraData.position = Camera.main.transform.position;
            cameraData.zoomLevel = zoomTarget;
            cameraData.zLevel = currentLayer;

            for (int i = 0; i < cameraData.presets.Length; i++)
            {
                cameraData.presets[i].position = Camera.main.transform.position;
                cameraData.presets[i].zoomLevel = Camera.main.orthographicSize;
                cameraData.presets[i].zLevel = currentLayer;
            }
        }
        else
        {
            positionTarget = cameraData.position;
            Camera.main.transform.position = positionTarget;

            zoomTarget = cameraData.zoomLevel;
            Camera.main.orthographicSize = zoomTarget;

            ChangeLayer(cameraData.zLevel);
        }
    }

    /// <summary>
    /// Get the bounds of the main camera.
    /// </summary>    
    private Bounds GetCameraBounds()
    {
        float x = Camera.main.transform.position.x;
        float y = Camera.main.transform.position.y;
        float size = Camera.main.orthographicSize * 2;
        float width = size * (float)Screen.width / Screen.height;
        float height = size;

        return new Bounds(new Vector3(x, y, 0), new Vector3(width, height, 0));
    }

    private void ApplyPreset(Preset preset)
    {
        positionTarget = preset.position;
        zoomTarget = preset.zoomLevel;
        presetBeingApplied = true;
    }

    private void AssignPreset(int index)
    {
        cameraData.presets[index].position = Camera.main.transform.position;
        cameraData.presets[index].zoomLevel = Camera.main.orthographicSize;
    }

    private void SyncCameras()
    {
        if (layerCameras != null)
        {
            for (int i = 0; i < layerCameras.Length; i++)
            {
                layerCameras[i].orthographicSize = Camera.main.orthographicSize + (.2f * (i - currentLayer));
            }
        }
    }

    private void CreateLayerCameras()
    {
        if (WorldController.Instance.World == null)
        {
            return;
        }

        // We don't have the right number of cameras for our layers
        if (layerCameras == null || layerCameras.Length != WorldController.Instance.World.Depth)
        {
            int depth = WorldController.Instance.World.Depth;
            layerCameras = new Camera[depth];
            for (int i = 0; i < depth; i++)
            {
                // This creates a new GameObject and adds it to our scene.
                GameObject camera_go = new GameObject();

                camera_go.name = "Layer Camera (" + i + ")";
                camera_go.transform.position = new Vector3(0, 0, 9);
                camera_go.transform.SetParent(Camera.main.transform, false);
                Camera camera = camera_go.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.Depth;
                camera.orthographic = true;
                camera.orthographicSize = Camera.main.orthographicSize;
                camera.nearClipPlane = i + .5f;
                camera.farClipPlane = i + 1.5f;
                camera.depth = -2 - i;

                DepthShading depthShading = camera_go.AddComponent<DepthShading>();
                Material shadingMaterial = new Material(Resources.Load<Material>("Shaders/DepthShading"));
                depthShading.shadingMaterial = shadingMaterial;

                layerCameras[i] = camera;
            }
        }
    }
}
