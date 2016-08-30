#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[MoonSharpUserData]
public class WorldController : MonoBehaviour
{
    public SoundController soundController;
    public TileSpriteController tileSpriteController;
    public CharacterSpriteController characterSpriteController;
    public JobSpriteController jobSpriteController;
    public InventorySpriteController inventorySpriteController;
    public FurnitureSpriteController furnitureSpriteController;
    public QuestController questController;
    public BuildModeController buildModeController;
    public MouseController mouseController;
    public KeyboardController keyboardController;
    public CameraController cameraController;
    public SpawnInventoryController spawnInventoryController;
    public ModsManager modsManager;
    public float GameTickPerSecond = 5;
    public GameObject inventoryUI;
    public GameObject circleCursorPrefab;

    // If true, a modal dialog box is open so normal inputs should be ignored.
    public bool IsModal;

    private static string loadWorldFromFile = null;

    private float gameTickDelay;
    private float totalDeltaTime;
    private bool isPaused = false;

    // Multiplier of Time.deltaTime.
    private float timeScale = 1f;

    public static WorldController Instance { get; protected set; }

    // The world and tile data.
    public World World { get; protected set; }

    public bool IsPaused
    {
        get
        {
            return isPaused || IsModal;
        }

        set
        {
            isPaused = value;
        }
    }

    public float TimeScale
    {
        get
        {
            return timeScale;
        }
    }
    
    // Use this for initialization.
    public void OnEnable()
    {
        if (Instance != null)
        {
            Debug.ULogErrorChannel("WorldController", "There should never be two world controllers.");
        }

        Instance = this;

        new FurnitureActions();
        new PrototypeManager();

        string dataPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        modsManager = new ModsManager(dataPath);

        if (loadWorldFromFile != null)
        {
            CreateWorldFromSaveFile();
            loadWorldFromFile = null;
        }
        else
        {
            CreateEmptyWorld();
        }

        soundController = new SoundController(World);

        gameTickDelay = 1f / GameTickPerSecond;
    }

    public void Start()
    {
        // Create gameobject so we can have access to a tranform thats position is "Vector3.zero".
        GameObject mat = new GameObject("VisualPath", typeof(VisualPath));
        GameObject go;

        tileSpriteController = new TileSpriteController(World);
        characterSpriteController = new CharacterSpriteController(World);
        furnitureSpriteController = new FurnitureSpriteController(World);
        jobSpriteController = new JobSpriteController(World, furnitureSpriteController);
        inventorySpriteController = new InventorySpriteController(World, inventoryUI);

        buildModeController = new BuildModeController();
        spawnInventoryController = new SpawnInventoryController();
        mouseController = new MouseController(buildModeController, furnitureSpriteController, circleCursorPrefab);
        keyboardController = new KeyboardController(buildModeController, Instance);
        questController = new QuestController();
        cameraController = new CameraController();

        // Hiding Dev Mode spawn inventory controller if devmode is off.
        spawnInventoryController.SetUIVisibility(Settings.GetSettingAsBool("DialogBoxSettings_developerModeToggle", false));

        // Initialising controllers.
        GameObject controllers = GameObject.Find("Controllers");
        Instantiate(Resources.Load("UIController"), controllers.transform);

        GameObject canvas = GameObject.Find("Canvas");
        go = Instantiate(Resources.Load("UI/ContextMenu"), canvas.transform.position, canvas.transform.rotation, canvas.transform) as GameObject;
        go.name = "ContextMenu";
    }

    public void Update()
    {
        // Systems that update every frame.
        mouseController.Update(IsModal);
        keyboardController.Update(IsModal);
        cameraController.Update(IsModal);

        float deltaTime = Time.deltaTime * timeScale;

        // Systems that update every frame when not paused.
        if (IsPaused == false)
        {
            World.UpdateCharacters(deltaTime);
        }

        totalDeltaTime += deltaTime;

        if (totalDeltaTime >= gameTickDelay)
        {
            // Systems that update at fixed frequency. 
            if (IsPaused == false)
            {
                // Systems that update at fixed frequency when not paused.
                World.Tick(totalDeltaTime);
                questController.Update(totalDeltaTime);
            }

            totalDeltaTime = 0f;
        }

        soundController.Update(Time.deltaTime);
    }

    /// <summary>
    /// Set's game speed (it's a multiplier so 1 == normal game speed).
    /// </summary>
    /// <param name="timeScale">Desired time scale.</param>
    public void SetTimeScale(float timeScale)
    {
        this.timeScale = timeScale;
        Debug.ULogChannel("Game speed", "Game speed set to " + timeScale + "x");
    }

    /// <summary>
    /// Gets the tile at the unity-space coordinates.
    /// </summary>
    /// <returns>The tile at world coordinate.</returns>
    /// <param name="coord">Unity World-Space coordinates.</param>
    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x + 0.5f);
        int y = Mathf.FloorToInt(coord.y + 0.5f);

        return World.GetTileAt(x, y);
    }

    public void NewWorld()
    {
        Debug.ULogChannel("WorldController", "NewWorld button was clicked.");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public string FileSaveBasePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "Saves");
    }

    public void LoadWorld(string fileName)
    {
        Debug.ULogChannel("WorldController", "LoadWorld button was clicked.");

        // Reload the scene to reset all data (and purge old references)
        loadWorldFromFile = fileName;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CallTradeShipTest(Furniture landingPad)
    {
        // Currently not using any logic to select a trader
        TraderPrototype prototype = PrototypeManager.Trader.GetPrototype(Random.Range(0, PrototypeManager.Trader.Count - 1));
        Trader trader = prototype.CreateTrader();

        GameObject go = new GameObject(trader.Name);
        go.transform.parent = transform;
        TraderShipController controller = go.AddComponent<TraderShipController>();
        controller.Trader = trader;
        controller.Speed = 5f;
        go.transform.position = new Vector3(-10, 50, 0);
        controller.LandingCoordinates = new Vector3(landingPad.Tile.X + 1, landingPad.Tile.Y + 1, 0);
        controller.LeavingCoordinates = new Vector3(100, 50, 0);
        go.transform.localScale = new Vector3(1, 1, 1);
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteManager.current.GetSprite("Trader", "BasicHaulShip");
        spriteRenderer.sortingLayerName = "TradeShip";
    }

    private void CreateEmptyWorld()
    {
        // get world size from settings
        int width = Settings.GetSettingAsInt("worldWidth", 100);
        int height = Settings.GetSettingAsInt("worldHeight", 100);

        // Create a world with Empty tiles
        World = new World(width, height);

        // Center the Camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }

    private void CreateWorldFromSaveFile()
    {
        Debug.ULogChannel("WorldController", "CreateWorldFromSaveFile");

        // Create a world from our save file data.
        XmlSerializer serializer = new XmlSerializer(typeof(World));

        // This can throw an exception.
        // TODO: Show a error message to the user.
        string saveGameText = File.ReadAllText(loadWorldFromFile);

        TextReader reader = new StringReader(saveGameText);

        // Leaving this for Unitys console because UberLogger mangles multiline messages.
        Debug.Log(reader.ToString());
        World = (World)serializer.Deserialize(reader);
        reader.Close();

        // Center the Camera.
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }
}
