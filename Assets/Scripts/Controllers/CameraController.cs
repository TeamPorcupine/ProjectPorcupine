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
    [Range(0, 3)]
    public float scrollSpeed = 0.1f;

    // Zoom of the main camera.
    private float zoomTarget = 11f;
    private int currentLayer;
    private Camera[] layerCameras;
    private bool presetBeingLoaded;

    private float frameMoveHorizontal = 0;
    private float frameMoveVertical = 0;

    private Vector3 positionTarget;

    private Vector3[] presetCameraPositions = new Vector3[5];
    private float[] presetCameraZoomLevels = new float[5];

    public CameraController()
    {
        // Main camera handles UI only
        Camera.main.farClipPlane = 9;

        KeyboardManager keyboardManager = KeyboardManager.Instance;
        keyboardManager.RegisterInputAction("MoveCameraEast", KeyboardMappedInputType.Key, () => { frameMoveHorizontal++; });
        keyboardManager.RegisterInputAction("MoveCameraWest", KeyboardMappedInputType.Key, () => { frameMoveHorizontal--; });
        keyboardManager.RegisterInputAction("MoveCameraNorth", KeyboardMappedInputType.Key, () => { frameMoveVertical++; });
        keyboardManager.RegisterInputAction("MoveCameraSouth", KeyboardMappedInputType.Key, () => { frameMoveVertical--; });
        keyboardManager.RegisterInputAction("ZoomOut", KeyboardMappedInputType.Key, () => ChangeZoom(0.1f));
        keyboardManager.RegisterInputAction("ZoomIn", KeyboardMappedInputType.Key, () => ChangeZoom(-0.1f));
        keyboardManager.RegisterInputAction("MoveCameraUp", KeyboardMappedInputType.KeyUp, ChangeLayerUp);
        keyboardManager.RegisterInputAction("MoveCameraDown", KeyboardMappedInputType.KeyUp, ChangeLayerDown);

        keyboardManager.RegisterInputAction("GoToPresetCameraPosition1", KeyboardMappedInputType.KeyUp, () => LoadPreset(presetCameraPositions[0], presetCameraZoomLevels[0]));
        keyboardManager.RegisterInputAction("GoToPresetCameraPosition2", KeyboardMappedInputType.KeyUp, () => LoadPreset(presetCameraPositions[1], presetCameraZoomLevels[1]));
        keyboardManager.RegisterInputAction("GoToPresetCameraPosition3", KeyboardMappedInputType.KeyUp, () => LoadPreset(presetCameraPositions[2], presetCameraZoomLevels[2]));
        keyboardManager.RegisterInputAction("GoToPresetCameraPosition4", KeyboardMappedInputType.KeyUp, () => LoadPreset(presetCameraPositions[3], presetCameraZoomLevels[3]));
        keyboardManager.RegisterInputAction("GoToPresetCameraPosition5", KeyboardMappedInputType.KeyUp, () => LoadPreset(presetCameraPositions[4], presetCameraZoomLevels[4]));
        keyboardManager.RegisterInputAction("SavePresetCameraPosition1", KeyboardMappedInputType.KeyUp, () => SavePreset(0));
        keyboardManager.RegisterInputAction("SavePresetCameraPosition2", KeyboardMappedInputType.KeyUp, () => SavePreset(1));
        keyboardManager.RegisterInputAction("SavePresetCameraPosition3", KeyboardMappedInputType.KeyUp, () => SavePreset(2));
        keyboardManager.RegisterInputAction("SavePresetCameraPosition4", KeyboardMappedInputType.KeyUp, () => SavePreset(3));
        keyboardManager.RegisterInputAction("SavePresetCameraPosition5", KeyboardMappedInputType.KeyUp, () => SavePreset(4));

        // Set default zoom value on camera
        Camera.main.orthographicSize = zoomTarget;

        TimeManager.Instance.EveryFrameNotModal += (time) => Update();

        positionTarget = Camera.main.transform.position;
    }

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
            float target = Mathf.Lerp(Camera.main.orthographicSize, zoomTarget, Settings.GetSetting("ZoomLerp", 3) * Time.deltaTime);
            Camera.main.orthographicSize = Mathf.Clamp(target, 3f, 25f);
            SyncCameras();
        }

        if (Vector3.Distance(Camera.main.transform.position, positionTarget) > 0.1f && presetBeingLoaded)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, positionTarget, Settings.GetSetting("ZoomLerp", 3) * Time.deltaTime);
        }
        else
        {
            Camera.main.transform.position = positionTarget;
            presetBeingLoaded = false;
        }

        if (!presetBeingLoaded)
        {
            // Refocus game so the mouse stays in the same spot when zooming.
            Vector3 newMousePosition = Vector3.zero;
            newMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newMousePosition.z = 0;

            Vector3 pushedAmount = oldMousePosition - newMousePosition;
            Camera.main.transform.Translate(pushedAmount);
            positionTarget = Camera.main.transform.position;
        }
    }

    public void ChangeZoom(float amount)
    {
        zoomTarget = Camera.main.orthographicSize - (Settings.GetSetting("ZoomSensitivity", 3) * (Camera.main.orthographicSize * amount));
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

    public void InitializeCameraDataValues()
    {
        if (WorldController.Instance.World.cameraData.presetCameraPositions == null)
        {
            for (int i = 0; i < presetCameraPositions.Length; i++)
            {
                presetCameraPositions[i] = Camera.main.transform.position;
                presetCameraZoomLevels[i] = Camera.main.orthographicSize;
            }

            WorldController.Instance.World.cameraData.position = Camera.main.transform.position;
            WorldController.Instance.World.cameraData.zoomLevel = zoomTarget;
            WorldController.Instance.World.cameraData.presetCameraPositions = presetCameraPositions;
            WorldController.Instance.World.cameraData.presetCameraZoomLevels = presetCameraZoomLevels;
        }
        else
        {
            positionTarget = WorldController.Instance.World.cameraData.position;
            Camera.main.transform.position = positionTarget;

            zoomTarget = WorldController.Instance.World.cameraData.zoomLevel;
            Camera.main.orthographicSize = zoomTarget;

            for (int i = 0; i < WorldController.Instance.World.cameraData.presetCameraPositions.Length; i++)
            {
                presetCameraPositions[i] = WorldController.Instance.World.cameraData.presetCameraPositions[i];
                presetCameraZoomLevels[i] = WorldController.Instance.World.cameraData.presetCameraZoomLevels[i];
            }
        }
    }

    private void LoadPreset(Vector3 presetPosition, float zoomLevel)
    {
        positionTarget = presetPosition;
        zoomTarget = zoomLevel;
        presetBeingLoaded = true;
    }

    private void SavePreset(int index)
    {
        presetCameraPositions[index] = Camera.main.transform.position;
        presetCameraZoomLevels[index] = Camera.main.orthographicSize;
        WorldController.Instance.World.cameraData.presetCameraPositions = presetCameraPositions;
        WorldController.Instance.World.cameraData.presetCameraZoomLevels = presetCameraZoomLevels;
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
