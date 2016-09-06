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
using Scheduler;
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
    public KeyboardManager keyboardManager;
    public CameraController cameraController;
    public SpawnInventoryController spawnInventoryController;
    public TradeController TradeController;
    public TimeManager timeManager;
    public ModsManager modsManager;
    public GameObject inventoryUI;
    public GameObject circleCursorPrefab;

    // If true, a modal dialog box is open so normal inputs should be ignored.
    public bool IsModal;

    private static string loadWorldFromFile = null;

    private float gameTickDelay;
    private bool isPaused = false;

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
            return timeManager.TimeScale;
        }
    }
    
    // Use this for initialization.
    public void OnEnable()
    {
        Debug.IsLogEnabled = true;
        if (Instance != null)
        {
            Debug.ULogErrorChannel("WorldController", "There should never be two world controllers.");
        }

        Instance = this;

        new FurnitureActions();
        new PrototypeManager();

        // FIXME: Do something real here. This is just to show how to register a C# event prototype for the Scheduler.
        PrototypeManager.SchedulerEvent.Add(
            "ping_log",
            new ScheduledEvent(
                "ping_log",
                (evt) => Debug.ULogChannel("Scheduler", "Event {0} fired", evt.Name)));

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

        gameTickDelay = TimeManager.GameTickDelay;
    }

    public void Start()
    {
        // Create gameobject so we can have access to a tranform thats position is "Vector3.zero".
        new GameObject("VisualPath", typeof(VisualPath));
        GameObject go;

        tileSpriteController = new TileSpriteController(World);
        characterSpriteController = new CharacterSpriteController(World);
        furnitureSpriteController = new FurnitureSpriteController(World);
        jobSpriteController = new JobSpriteController(World, furnitureSpriteController);
        inventorySpriteController = new InventorySpriteController(World, inventoryUI);

        buildModeController = new BuildModeController();
        spawnInventoryController = new SpawnInventoryController();
        mouseController = new MouseController(buildModeController, furnitureSpriteController, circleCursorPrefab);
        keyboardManager = KeyboardManager.Instance;
        questController = new QuestController();
        cameraController = new CameraController();
        TradeController = new TradeController();
        timeManager = new TimeManager();

        keyboardManager.RegisterInputAction("Pause", KeyboardMappedInputType.KeyUp, () => { IsPaused = !IsPaused; });

        // Hiding Dev Mode spawn inventory controller if devmode is off.
        spawnInventoryController.SetUIVisibility(Settings.GetSetting("DialogBoxSettings_developerModeToggle", false));

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
        keyboardManager.Update(IsModal);
        cameraController.Update(IsModal);
        timeManager.Update();

        // Systems that update every frame when not paused.
        if (IsPaused == false)
        {
            World.TickEveryFrame(timeManager.DeltaTime);
            Scheduler.Scheduler.Current.Update(timeManager.DeltaTime);
        }

        if (timeManager.TotalDeltaTime >= gameTickDelay)
        {
            // Systems that update at fixed frequency. 
            if (IsPaused == false)
            {
                // Systems that update at fixed frequency when not paused.
                World.TickFixedFrequency(timeManager.TotalDeltaTime);
                questController.Update(timeManager.TotalDeltaTime);
            }

            timeManager.ResetTotalDeltaTime();
        }

        soundController.Update(Time.deltaTime);
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

        return World.GetTileAt(x, y, (int)coord.z);
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

    private void CreateEmptyWorld()
    {
        // get world size from settings
        int width = Settings.GetSetting("worldWidth", 100);
        int height = Settings.GetSetting("worldHeight", 100);

        // FIXME: Need to read this from settings.
        int depth = 5;

        // Create a world with Empty tiles
        World = new World(width, height, depth);

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
