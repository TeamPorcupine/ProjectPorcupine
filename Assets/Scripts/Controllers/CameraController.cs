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

    private float zoomTarget;
    private int currentLayer;
    private Camera[] layerCameras;

    private float frameMoveHorizontal = 0;
    private float frameMoveVertical = 0;

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
    }

    public int CurrentLayer
    {
        get
        {
            return currentLayer;
        }
    }

    // Update is called once per frame.
    public void Update(bool modal)
    {
        CreateLayerCameras();
        if (modal)
        {
            // A modal dialog box is open. Bail.
            return;
        }

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

        // Refocus game so the mouse stays in the same spot when zooming.
        Vector3 newMousePosition = Vector3.zero;
        newMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newMousePosition.z = 0;

        Vector3 pushedAmount = oldMousePosition - newMousePosition;
        Camera.main.transform.Translate(pushedAmount);
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
