using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This game object manages a mesh+texture+renderer+material that is
/// used to superimpose a semi-transparent "overlay" to the map.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OverlayMap : MonoBehaviour
{
    /// <summary>
    /// Starting left corner (x,y) and z-coordinate of mesh and (3d left corner)
    /// </summary>
    public Vector3 leftBottomCorner = new Vector3(-0.5f, -0.5f, 1f);
    /// <summary>
    /// Transparency of overlay
    /// </summary>
    [Range(0,1)]
    public float transparency = 0.4f;
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
    /// You can set any function, overlay will display value of func at point (x,y)
    /// Depending on how many colors the colorMap has, the displayed values will cycle
    /// </summary>
    public Func<int, int, int> valueAt;
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
    Color32[] colorMap;

    /// <summary>
    /// True if Init() has been called (i.e. there is a mesh and a color map)
    /// </summary>
    bool initialized = false;

    // The texture applied to the entire overlay map
    Texture2D texture;

    /// <summary>
    /// Grabs references, sets a dummy size and evaluation function
    /// </summary>
    void Start()
    {
        // Grab references
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        // TODO: remove this dummy function
        //valueAt = (x, y) => { return (x + y) % 255; };
        valueAt = (x, y) => {
            if (WorldController.Instance == null) return 0;
            Tile tile = WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y, 0));
            if (tile == null) return 0;
            Room room = WorldController.Instance.GetTileAtWorldCoord(new Vector3(x, y, 0)).room;
            if (room == null) return 0;
            return (int) (room.GetGasAmount("O2") * 1E3f); };
        // TODO: remove this dummy set size
        SetSize(100, 100);
    }

    /// <summary>
    /// If update is required, redraw texture ("bake") (kinda expensive)
    /// </summary>
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed > updateInterval)
        {
            Bake();
            elapsed = 0f;
        }
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
        meshRenderer.sharedMaterials[0] = mat;
        meshRenderer.sharedMaterials[0].mainTexture = texture;

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
        colorMap = ColorMap("jet", 255);

        // Colormap texture
        int textureWidth = colorMap.Length * xPixelsPerTile;
        int textureHeight = yPixelsPerTile;
        colorMapTexture = new Texture2D(textureWidth, textureHeight);
        
        // Loop over each color in the palette and build a noisy texture
        int n = 0;
        foreach (Color32 baseColor in colorMap)
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
                Graphics.CopyTexture(colorMapTexture,
                    0, 0,
                    (valueAt(x, y) % 256) * xPixelsPerTile, 0,
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
    /// <summary>
    /// Returns an array of color, using the preset colormap with the name "name"
    /// Sets the alpha channel to alpha and uses "size" colors
    /// </summary>
    /// <param name="name">Name of colormap</param>
    /// <param name="size">Number of colors to use</param>
    /// <param name="alpha">Alpha channel of color</param>
    /// <returns></returns>
    public static Color32[] ColorMap(string name, int size = 256, byte alpha = 128)
    {
        Color32[] cm = new Color32[size];
        Func<int, Color32> map;

        switch (name)
        {
            default:
            case "jet":
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
        }

        for (int i = 0; i < size; i++)
        {
            cm[i] = map(i);
        }
        return cm;
    }
}
