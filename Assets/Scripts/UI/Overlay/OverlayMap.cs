#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// This game object manages a mesh+texture+renderer+material that is
/// used to superimpose a semi-transparent "overlay" to the map.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OverlayMap : MonoBehaviour
{
    public Dictionary<string, OverlayDescriptor> overlays;
    
    /// <summary>
    /// Starting left corner (x,y) and z-coordinate of mesh and (3d left corner).
    /// </summary>
    public Vector3 leftBottomCorner = new Vector3(-0.5f, -0.5f, 1f);

    /// <summary>
    /// Transparency of overlay.
    /// </summary>
    [Range(0, 1)]
    public float transparency = 0.8f;
    
    /// <summary>
    /// Update interval (0 for every Update, inf for never).
    /// </summary>
    public float updateInterval = 5f;
    
    /// <summary>
    /// Resolution of tile for the overlay.
    /// </summary>
    public int pixelsPerTileX = 20;
    public int pixelsPerTileY = 20;

    /// <summary>
    /// Internal storage of size of map.
    /// </summary>
    public int sizeX = 10;
    public int sizeY = 10;

    /// <summary>
    /// Current Overlay.
    /// </summary>
    public string currentOverlay;

    /// <summary>
    /// You can set any function, overlay will display value of func at point (x,y)
    /// Depending on how many colors the ColorMapSG has, the displayed values will cycle.
    /// </summary>
    public Func<int, int, int> valueAt;

    /// <summary>
    /// Name of xml file containing overlay prototypes.
    /// </summary>
    public string xmlFileName = "overlay_prototypes.xml";

    /// <summary>
    /// Name of lua script containing overlay prototypes functions.
    /// </summary>
    public string LUAFileName = "overlay_functions.lua";

    public GameObject parentPanel;

    private static List<Color32> randomColors;

    /// <summary>
    /// Time since last update.
    /// </summary>
    private float elapsed = 0f;

    /// <summary>
    /// Script with user-defined lua valueAt functions.
    /// </summary>
    private Script script;

    /// <summary>
    /// Current color map, setting the map causes the colorMapArray to be recreated.
    /// </summary>
    private OverlayDescriptor.ColorMap colorMap;

    /// <summary>
    /// Storage for color map as texture, copied from using copyTexture on GPUs.
    /// This texture is made of n*x times y pixels, where n is the size of the "ColorMapSG"
    /// x and y is the size of 1 tile of the map (20x20 by default).
    /// Constructed from the ColorMapSG.
    /// </summary>
    private Texture2D colorMapTexture;

    /// <summary>
    /// Mesh data.
    /// </summary>
    private Vector3[] newVertices;
    private Vector3[] newNormals;
    private Vector2[] newUV;

    private int[] newTriangles;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    /// <summary>
    /// Array with colors for overlay (colormap).
    /// Each element is a color that will be part of the color palette.
    /// </summary>
    private Color32[] colorMapArray;

    /// <summary>
    /// True if Init() has been called (i.e. there is a mesh and a color map).
    /// </summary>
    private bool initialized = false;

    private GameObject colorMapView;
    private GameObject textView;

    // The texture applied to the entire overlay map.
    private Texture2D texture;

    public OverlayDescriptor.ColorMap ColorMapSG
    {
        get
        {
            return colorMap;
        }

        set
        {
            colorMap = value;
            GenerateColorMap();
        }
    }

    /// <summary>
    /// Returns an array of color, using the preset colormap with the name "name".
    /// Sets the alpha channel to alpha and uses "size" colors.
    /// </summary>
    /// <param name="colorMap">Name of colormap.</param>
    /// <param name="size">Number of colors to use.</param>
    /// <param name="alpha">Alpha channel of color.</param>
    /// <returns></returns>
    public static Color32[] ColorMap(
        OverlayDescriptor.ColorMap colorMap,
        int size = 256,
        byte alpha = 128)
    {
        Color32[] cm = new Color32[size];
        Func<int, Color32> map;

        switch (colorMap)
        {
            default:
            case OverlayDescriptor.ColorMap.Jet:
                map = (int v) =>
                {
                    Color32 c = new Color32(255, 255, 255, alpha);
                    if (v == 64)
                    {
                        c.r = 0;
                        c.g = 255;
                    }
                    else if (v == 128)
                    {
                        c.r = 0;
                        c.b = 255;
                    }
                    else if (v == 192)
                    {
                        c.g = 255;
                        c.b = 0;
                    }
                    else if (v < 64)
                    {
                        c.r = 0;
                        c.g = (byte)(4 * v);
                    }
                    else if (v < 128)
                    {
                        c.r = 0;
                        c.b = (byte)(256 + (4 * (64 - v)));
                    }
                    else if (v < 192)
                    {
                        c.r = (byte)(4 * (v - 128));
                        c.b = 0;
                    }
                    else
                    {
                        c.g = (byte)(256 + (4 * (192 - v)));
                        c.b = 0;
                    }

                    return c;
                };
                break;
            case OverlayDescriptor.ColorMap.Random:
                GenerateRandomColors(size);
                map = (int v) =>
                {
                    return randomColors[v];
                };
                break;
        }

        for (int i = 0; i < size; i++)
        {
            cm[i] = map(i);
        }

        return cm;
    }

    /// <summary>
    /// Set size of texture and mesh, recreates mesh.
    /// </summary>
    /// <param name="x">Num tiles x-dir.</param>
    /// <param name="y">Num tiles y-dir.</param>
    public void SetSize(int x, int y)
    {
        sizeX = x;
        sizeY = y;
        if (meshRenderer != null)
        {
            Init();
        }
    }

    /// <summary>
    /// Paint the texture.
    /// </summary>
    public void Bake()
    {
        if (initialized && valueAt != null)
        {
            GenerateTexture();
        }
    }

    /// <summary>
    /// Set overlay to display perototype with name "name".
    /// </summary>
    /// <param name="name">Name of overlay prototype.</param>
    public void SetOverlay(string name)
    {
        if (name == "None")
        {
            meshRenderer.enabled = false;
            currentOverlay = name;
            return;
        }
        else if (overlays.ContainsKey(name))
        {
            meshRenderer.enabled = true;
            currentOverlay = name;
            OverlayDescriptor descr = overlays[name];
            object handle = script.Globals[descr.luaFunctionName];
            if (handle == null)
            {
                Debug.ULogErrorChannel("OverlayMap", string.Format("Couldn't find a function named '{0}' in '{1}'", descr.luaFunctionName));
                return;
            }

            valueAt = (x, y) =>
            {
                if (WorldController.Instance == null)
                {
                    return 0;
                }

                Tile tile = WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y, 0));
                return (int)script.Call(handle, new object[] { tile, World.Current }).ToScalar().CastToNumber();
            };

            ColorMapSG = descr.colorMap;
            Bake();
        }
        else
        {
            Debug.ULogWarningChannel("OverlayMap", string.Format("Overlay with name {0} not found in prototypes", name));
        }
    }

    private static void GenerateRandomColors(int size)
    {
        if (randomColors == null)
        {
            randomColors = new List<Color32>();
        }

        for (int i = randomColors.Count; i < size; i++)
        {
            randomColors.Add(UnityEngine.Random.ColorHSV());
        }
    }

    /// <summary>
    /// Grabs references, sets a dummy size and evaluation function.
    /// </summary>
    private void Start()
    {
        // Grab references.
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        // Read xml prototypes.
        overlays = OverlayDescriptor.ReadPrototypes(xmlFileName);

        // Read LUA.
        UserData.RegisterAssembly();
        string scriptFile = System.IO.Path.Combine(
            UnityEngine.Application.streamingAssetsPath,
            System.IO.Path.Combine("Overlay", LUAFileName));
        string scriptTxt = System.IO.File.ReadAllText(scriptFile);
        
        script = new Script();
        script.DoString(scriptTxt);

        // Build GUI.
        CreateGUI();

        // TODO: remove this dummy set size.
        SetSize(100, 100);
        SetOverlay("None");
    }

    /// <summary>
    /// If update is required, redraw texture ("bake") (kinda expensive).
    /// </summary>
    private void Update()
    {
        elapsed += Time.deltaTime;
        if (currentOverlay != "None" && elapsed > updateInterval)
        {
            Bake();
            elapsed = 0f;
        }

        // TODO: Prettify.
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (valueAt != null)
        {
            textView.GetComponent<UnityEngine.UI.Text>().text = string.Format("[DEBUG] Currently over: {0}", valueAt((int)(pos.x + 0.5f), (int)(pos.y + 0.5f)));
        }
    }

    /// <summary>
    /// If overlay is toggled on, it should be "baked".
    /// </summary>
    private void Awake()
    {
        parentPanel = GameObject.Find("OverlayPanel");
        Bake();
    }

    /// <summary>
    /// Generates the mesh and the texture for the colormap..
    /// </summary>
    private void Init()
    {
        GenerateMesh();
        GenerateColorMap();

        // Size in pixels of overlay texture and create texture.
        int textureWidth = sizeX * pixelsPerTileX;
        int textureHeight = sizeY * pixelsPerTileY;
        texture = new Texture2D(textureWidth, textureHeight);
        texture.wrapMode = TextureWrapMode.Clamp;

        // Set material.
        Shader shader = Shader.Find("Transparent/Diffuse");
        Material mat = new Material(shader);
        meshRenderer.material = mat;
        if (mat == null || meshRenderer == null || texture == null)
        {
            Debug.ULogErrorChannel("OverlayMap", "Material or renderer is null. Failing.");
        }

        meshRenderer.material.mainTexture = texture;

        initialized = true;
    }

    /// <summary>
    /// Create the colormap texture from the color set.
    /// </summary>
    private void GenerateColorMap()
    {
        // TODO: make the map configurable.
        colorMapArray = ColorMap(ColorMapSG, 255);

        // Colormap texture.
        int textureWidth = colorMapArray.Length * pixelsPerTileX;
        int textureHeight = pixelsPerTileY;
        colorMapTexture = new Texture2D(textureWidth, textureHeight);
        
        // Loop over each color in the palette and build a noisy texture.
        int n = 0;
        foreach (Color32 baseColor in colorMapArray)
        {
            for (int y = 0; y < pixelsPerTileY; y++)
            {
                for (int x = 0; x < pixelsPerTileX; x++)
                {
                    Color colorCopy = baseColor;
                    colorCopy.a = transparency;

                    // Add some noise to "prettify".
                    colorCopy.r += UnityEngine.Random.Range(-0.03f, 0.03f);
                    colorCopy.b += UnityEngine.Random.Range(-0.03f, 0.03f);
                    colorCopy.g += UnityEngine.Random.Range(-0.03f, 0.03f);
                    colorMapTexture.SetPixel((n * pixelsPerTileX) + x, y, colorCopy);
                }
            }

            ++n;
        }

        colorMapTexture.Apply();
        colorMapView.GetComponent<UnityEngine.UI.Image>().material.mainTexture = colorMapTexture;
    }

    /// <summary>
    /// Build the huge overlay texture.
    /// </summary>
    private void GenerateTexture()
    {
        if (colorMapTexture == null)
        {
            Debug.ULogErrorChannel("OverlayMap", "No color map texture setted!");
        }

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                float v = valueAt(x, y);
                Debug.Assert(v >= 0 && v < 256, "v >= 0 && v < 256");
                Graphics.CopyTexture(
                    colorMapTexture,
                    0,
                    0,
                    ((int)v % 256) * pixelsPerTileX,
                    0,
                    pixelsPerTileX,
                    pixelsPerTileY,
                    texture,
                    0,
                    0,
                    x * pixelsPerTileX,
                    y * pixelsPerTileY);
            }
        }

        texture.Apply(true);
    }

    /// <summary>
    /// Build mesh.
    /// </summary>
    private void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        if (meshFilter != null)
        {
            meshFilter.mesh = mesh;
        }

        int sizePixelX = sizeX + 1;
        int sizePixelY = sizeY + 1;

        newVertices = new Vector3[sizePixelX * sizePixelY];
        newNormals = new Vector3[sizePixelX * sizePixelY];
        newUV = new Vector2[sizePixelX * sizePixelY];
        newTriangles = new int[(sizePixelX - 1) * (sizePixelY - 1) * 6];
        
        for (int y = 0; y < sizePixelY; y++)
        {
            for (int x = 0; x < sizePixelX; x++)
            {
                newVertices[(y * sizePixelX) + x] = new Vector3(x, y, 0) + leftBottomCorner;
                newNormals[(x * sizePixelY) + y] = Vector3.up;
                newUV[(y * sizePixelX) + x] = new Vector2((float)x / sizeX, (float)y / sizeY);
            }
        }

        int offset = 0;
        for (int y = 0; y < sizePixelY - 1; y++)
        {
            for (int x = 0; x < sizePixelX - 1; x++)
            {
                int index = (y * sizePixelX) + x;
                newTriangles[offset + 0] = index;
                newTriangles[offset + 1] = index + sizePixelX;
                newTriangles[offset + 2] = index + sizePixelX + 1;

                newTriangles[offset + 3] = index;
                newTriangles[offset + 5] = index + 1;
                newTriangles[offset + 4] = index + sizePixelX + 1;

                offset += 6;
            }
        }

        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;
    }

    private void CreateGUI()
    {
        UnityEngine.UI.Dropdown dropdown = parentPanel.GetComponentInChildren<UnityEngine.UI.Dropdown>();
        if (dropdown == null)
        {
            Debug.ULogWarningChannel("OverlayMap", "No parent panel was selected!");
            return;
        }

        textView = new GameObject();
        textView.AddComponent<UnityEngine.UI.Text>();
        textView.AddComponent<UnityEngine.UI.LayoutElement>();
        textView.GetComponent<UnityEngine.UI.LayoutElement>().minHeight = 30;
        textView.GetComponent<UnityEngine.UI.LayoutElement>().minWidth = 150;
        textView.transform.SetParent(parentPanel.transform);
        textView.GetComponent<UnityEngine.UI.Text>().text = "Currently slected:";
        textView.GetComponent<UnityEngine.UI.Text>().resizeTextForBestFit = true;
        textView.GetComponent<UnityEngine.UI.Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        colorMapView = new GameObject();
        colorMapView.AddComponent<UnityEngine.UI.Image>();
        colorMapView.transform.SetParent(parentPanel.transform);
        colorMapView.AddComponent<UnityEngine.UI.Text>();
        colorMapView.AddComponent<UnityEngine.UI.LayoutElement>();
        colorMapView.GetComponent<UnityEngine.UI.LayoutElement>().minHeight = 30;
        colorMapView.GetComponent<UnityEngine.UI.LayoutElement>().minWidth = 150;
        Shader shader = Shader.Find("UI/Unlit/Transparent");
        colorMapView.GetComponent<UnityEngine.UI.Image>().material = new Material(shader);

        List<string> options = new List<string> { "None" };
        options.AddRange(overlays.Keys);

        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(
            (int idx) => { SetOverlay(dropdown.captionText.text); });
    }
}
