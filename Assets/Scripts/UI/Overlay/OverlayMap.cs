#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

/// <summary>
/// This game object manages a mesh+texture+renderer+material that is
/// used to superimpose a semi-transparent "overlay" to the map.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OverlayMap : MonoBehaviour {

    public Dictionary<string, OverlayDescriptor> overlays;

    /// <summary>
    /// Starting left corner (x,y) and z-coordinate of mesh and (3d left corner)
    /// </summary>
    public Vector3 leftBottomCorner = new Vector3(-0.5f, -0.5f, 1f);
    /// <summary>
    /// Transparency of overlay
    /// </summary>
    [Range(0,1)]
    public float transparency = 0.8f;
    /// <summary>
    /// Update interval (0 for every Update, inf for never)
    /// </summary>
    public float updateInterval = 5f;
    /// <summary>
    /// Time since last update
    /// </summary>
    float elapsed = 0f;
    
    /// <summary>
    /// Resolution of tile for the overlay
    /// </summary>
    public int xPixelsPerTile = 20;
    public int yPixelsPerTile = 20;

    /// <summary>
    /// Internal storage of size of map
    /// </summary>
    public int xSize = 10;
    public int ySize = 10;

    /// <summary>
    /// Script with user-defined lua valueAt functions
    /// </summary>
    Script script;

    /// <summary>
    /// 
    /// </summary>
    public string currentOverlay;

    /// <summary>
    /// You can set any function, overlay will display value of func at point (x,y)
    /// Depending on how many colors the colorMap has, the displayed values will cycle
    /// </summary>
    public Func<int, int, int> valueAt;
    /// <summary>
    /// Current color map, setting the map causes the colorMapArray to be recreated
    /// </summary>
    private OverlayDescriptor.ColorMap _colorMap;
    public OverlayDescriptor.ColorMap colorMap
    {
        set
        {
            _colorMap = value;
            GenerateColorMap();
        }
        get
        {
            return _colorMap;
        }
    }

    /// <summary>
    /// Name of xml file containing overlay prototypes
    /// </summary>
    public string xmlFileName = "overlay_prototypes.xml";
    /// <summary>
    /// Name of lua script containing overlay prototypes functions
    /// </summary>
    public string LUAFileName = "overlay_functions.lua";

    /// <summary>
    /// Storage for color map as texture, copied from using copyTexture on GPUs
    /// This texture is made of n*x times y pixels, where n is the size of the "colorMap"
    /// x and y is the size of 1 tile of the map (20x20 by default)
    /// Constructed from the colorMap
    /// </summary>
    Texture2D colorMapTexture;

    /// <summary>
    /// Mesh data
    /// </summary>
    Vector3[] newVertices;
    Vector3[] newNormals;
    Vector2[] newUV;

    int[] newTriangles;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    /// <summary>
    /// Array with colors for overlay (colormap)
    /// Each element is a color that will be part of the color palette
    /// </summary>
    Color32[] colorMapArray;

    /// <summary>
    /// True if Init() has been called (i.e. there is a mesh and a color map)
    /// </summary>
    bool initialized = false;

    // The texture applied to the entire overlay map
    Texture2D texture;

    public GameObject parentPanel;
    GameObject dropdownObject;

    /// <summary>
    /// Grabs references, sets a dummy size and evaluation function
    /// </summary>
    void Start()
    {
        // Grab references
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        // Read xml prototypes
        overlays = OverlayDescriptor.ReadPrototypes(xmlFileName);

        // Read LUA
        UserData.RegisterAssembly();
        string scriptFile = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath,
            System.IO.Path.Combine("Overlay", LUAFileName));
        string scriptTxt = System.IO.File.ReadAllText(scriptFile);
        
        script = new Script();
        script.DoString(scriptTxt);

        // Build GUI
        CreateGUI();

        // TODO: remove this dummy set size
        SetSize(100, 100);
        SetOverlay("None");
    }

    /// <summary>
    /// If update is required, redraw texture ("bake") (kinda expensive)
    /// </summary>
    void Update()
    {
        elapsed += Time.deltaTime;
        if (currentOverlay != "None" && elapsed > updateInterval)
        {
            Bake();
            elapsed = 0f;
        }

        // TODO: Prettify
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //World.current.GetTileAt((int) pos.x, (int) pos.y);
        if(valueAt != null)
            textView.GetComponent<UnityEngine.UI.Text>().text =
                string.Format("[DEBUG] Currently over: {0}", valueAt((int)(pos.x + 0.5f), (int)(pos.y + 0.5f)));
    }

    void Destroy()
    {
        dropdownObject.GetComponent<UnityEngine.UI.Dropdown>().onValueChanged.RemoveAllListeners();
        Destroy(dropdownObject);
    }

    /// <summary>
    /// If overlay is toggled on, it should be "baked"
    /// </summary>
    void Awake()
    {
        Bake();
    }

    /// <summary>
    /// Set size of texture and mesh, recreates mesh
    /// </summary>
    /// <param name="x">Num tiles x-dir.</param>
    /// <param name="y">Num tiles y-dir.</param>
    public void SetSize(int x, int y)
    {
        xSize = x;
        ySize = y;
        if (meshRenderer != null)
            Init();
    }

    /// <summary>
    /// Generates the mesh and the texture for the colormap.
    /// </summary>
    void Init()
    {
        GenerateMesh();
        GenerateColorMap();

        // Size in pixels of overlay texture and create texture
        int textureWidth = xSize * xPixelsPerTile;
        int textureHeight = ySize * yPixelsPerTile;
        texture = new Texture2D(textureWidth, textureHeight);
        texture.wrapMode = TextureWrapMode.Clamp;

        // Set material
        Shader shader = Shader.Find("Transparent/Diffuse");
        Material mat = new Material(shader);
        meshRenderer.material = mat;
        if(mat == null || meshRenderer == null || texture == null)
        {
            Debug.LogError("Material or renderer is null. Failing.");
        }
        meshRenderer.material.mainTexture = texture;

        initialized = true;
    }

    /// <summary>
    /// Paint the texture
    /// </summary>
    public void Bake()
    {
        if (initialized && valueAt != null)
            GenerateTexture();
    }
    
    /// <summary>
    /// Create the colormap texture from the color set
    /// </summary>
    void GenerateColorMap()
    {
        // TODO: make the map configurable
        colorMapArray = ColorMap(colorMap, 255);

        // Colormap texture
        int textureWidth = colorMapArray.Length * xPixelsPerTile;
        int textureHeight = yPixelsPerTile;
        colorMapTexture = new Texture2D(textureWidth, textureHeight);
        
        // Loop over each color in the palette and build a noisy texture
        int n = 0;
        foreach (Color32 baseColor in colorMapArray)
        {
            for (int y = 0; y < yPixelsPerTile; y++)
            {
                for (int x = 0; x < xPixelsPerTile; x++)
                {
                    Color colorCopy = baseColor;
                    colorCopy.a = transparency;
                    // Add some noise to "prettify"
                    colorCopy.r += UnityEngine.Random.Range(-0.03f, 0.03f);
                    colorCopy.b += UnityEngine.Random.Range(-0.03f, 0.03f);
                    colorCopy.g += UnityEngine.Random.Range(-0.03f, 0.03f);
                    colorMapTexture.SetPixel(n * xPixelsPerTile + x, y, colorCopy);
                }
            }

            ++n;
        }
        colorMapTexture.Apply();

        colorMapView.GetComponent<UnityEngine.UI.Image>().material.mainTexture = colorMapTexture;
        //colorMapView.GetComponent<UnityEngine.UI.Image>()
    }

    /// <summary>
    /// Build the huge overlay texture
    /// </summary>
    void GenerateTexture()
    {
        //Debug.Log("Regenerating texture!");

        if (colorMapTexture == null)
            Debug.LogError("No color map texture setted!");
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float v = valueAt(x, y);
                Debug.Assert(v >= 0 && v < 256);
                Graphics.CopyTexture(colorMapTexture,
                    0, 0,
                    ((int) v % 256) * xPixelsPerTile, 0,
                    xPixelsPerTile, yPixelsPerTile,
                    texture,
                    0, 0,
                    x * xPixelsPerTile, y * yPixelsPerTile);
            }
        }
        texture.Apply(true);
    }

    /// <summary>
    /// Build mesh
    /// </summary>
    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        if (meshFilter != null)
            meshFilter.mesh = mesh;

        int xSizeP = xSize + 1;
        int ySizeP = ySize + 1;

        newVertices = new Vector3[xSizeP * ySizeP];
        newNormals = new Vector3[xSizeP * ySizeP];
        newUV = new Vector2[xSizeP * ySizeP];
        newTriangles = new int[(xSizeP - 1) * (ySizeP - 1) * 6];
        
        for (int y = 0; y < ySizeP; y++)
        {
            for (int x = 0; x < xSizeP; x++)
            {
                newVertices[y * xSizeP + x] = new Vector3(x, y, 0) + leftBottomCorner;
                newNormals[x * ySizeP + y] = Vector3.up;
                newUV[y * xSizeP + x] = new Vector2((float)x / xSize, (float)y / ySize);
            }
        }

        int offset = 0;
        for (int y = 0; y < ySizeP - 1; y++)
        {
            for (int x = 0; x < xSizeP - 1; x++)
            {
                int index = y * xSizeP + x;
                newTriangles[offset + 0] = index;
                newTriangles[offset + 1] = index + xSizeP;
                newTriangles[offset + 2] = index + xSizeP + 1;

                newTriangles[offset + 3] = index;
                newTriangles[offset + 5] = index + 1;
                newTriangles[offset + 4] = index + xSizeP + 1;

                offset += 6;
            }
        }

        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;

    }

    static List<Color32> _random_colors;
    private GameObject colorMapView;
    private GameObject textView;

    static void GenerateRandomColors(int size)
    {
        if (_random_colors == null)
        {
            _random_colors = new List<Color32>();
        }
        for (int i = _random_colors.Count; i < size; i++)
        {
            _random_colors.Add(UnityEngine.Random.ColorHSV());
        }
    }

    /// <summary>
    /// Returns an array of color, using the preset colormap with the name "name"
    /// Sets the alpha channel to alpha and uses "size" colors
    /// </summary>
    /// <param name="colorMap">Name of colormap</param>
    /// <param name="size">Number of colors to use</param>
    /// <param name="alpha">Alpha channel of color</param>
    /// <returns></returns>
    public static Color32[] ColorMap(OverlayDescriptor.ColorMap colorMap,
        int size = 256, byte alpha = 128)
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
                        c.b = (byte)(256 + 4 * (64 - v));
                    }
                    else if (v < 192)
                    {
                        c.r = (byte)(4 * (v - 128));
                        c.b = 0;
                    }
                    else
                    {
                        c.g = (byte)(256 + 4 * (192 - v));
                        c.b = 0;
                    }

                    return c;
                };
                break;
            case OverlayDescriptor.ColorMap.Random:
                GenerateRandomColors(size);
                map = (int v) =>
                {
                    return _random_colors[v];
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
    /// Set overlay to display perototype with name "name"
    /// </summary>
    /// <param name="name">name of overlay prototype</param>
    public void SetOverlay(string name)
    {
        if(name == "None")
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
                Debug.LogError(string.Format("Couldn't find a function named '{0}' in '{1}'", descr.luaFunctionName));
                return;
            }
            //Debug.Log(string.Format("Setting LUA function for overlay to '{0}'", descr.luaFunctionName));
            valueAt = (x, y) => {
                if (WorldController.Instance == null) return 0;
                Tile tile = WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y, 0));
                return (int) script.Call(handle, new object[] { tile, World.current }).ToScalar().CastToNumber();
            };
            colorMap = descr.colorMap;
            Bake();
        } else
        {
            Debug.LogWarning(string.Format("Overlay with name {0} not found in prototypes", name));
        }
    }

    void CreateGUI()
    {
        //dropdownObject =  new GameObject();
        //dropdownObject.transform.SetParent(parentPanel.transform);
        //UnityEngine.UI.Dropdown dropdown = dropdownObject.AddComponent<UnityEngine.UI.Dropdown>();

        UnityEngine.UI.Dropdown dropdown = parentPanel.GetComponentInChildren<UnityEngine.UI.Dropdown>();
        if(dropdown == null)
        {
            Debug.LogWarning("No parent panel was selected!");
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

        //colorMapView = GameObject.CreatePrimitive(PrimitiveType.Quad);
        colorMapView = new GameObject();
        colorMapView.AddComponent<UnityEngine.UI.Image>();
        colorMapView.transform.SetParent(parentPanel.transform);
        colorMapView.AddComponent<UnityEngine.UI.Text>();
        colorMapView.AddComponent<UnityEngine.UI.LayoutElement>();
        colorMapView.GetComponent<UnityEngine.UI.LayoutElement>().minHeight = 30;
        colorMapView.GetComponent<UnityEngine.UI.LayoutElement>().minWidth = 150;
        Shader shader = Shader.Find("UI/Unlit/Transparent");
        colorMapView.GetComponent<UnityEngine.UI.Image>().material = new Material(shader);

        List <string> options = new List<string> { "None" };
        options.AddRange(overlays.Keys);

        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(
            (int idx) => { SetOverlay(dropdown.captionText.text); }
        );
    }
}
