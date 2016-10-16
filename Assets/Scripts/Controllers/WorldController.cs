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
    public ShipSpriteController shipSpriteController;
    public FurnitureSpriteController furnitureSpriteController;
    public UtilitySpriteController utilitySpriteController;
    public QuestController questController;
    public BuildModeController buildModeController;
    public MouseController mouseController;
    public KeyboardManager keyboardManager;
    public CameraController cameraController;
    public SpawnInventoryController spawnInventoryController;
    public AutosaveManager autosaveManager;
    public TradeController TradeController;
    public ModsManager modsManager;
    public GameObject inventoryUI;
    public GameObject circleCursorPrefab;

    // If true, a modal dialog box is open, so normal inputs should be ignored.
    public bool IsModal;

    private static string loadWorldFromFile = null;

    public static WorldController Instance { get; protected set; }

    // The world and tile data.
    public World World { get; protected set; }

    public bool IsPaused
    {
        get
        {
            return TimeManager.Instance.IsPaused || IsModal;
        }

        set
        {
            TimeManager.Instance.IsPaused = value;
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

        new FunctionsManager();
        new PrototypeManager();
        new CharacterNameManager();
        new SpriteManager();
        new AudioManager();

        // FIXME: Do something real here. This is just to show how to register a C# event prototype for the Scheduler.
        PrototypeManager.ScheduledEvent.Add(
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
    }

    public void Start()
    {
        // Create GameObject so we can have access to a transform which has a position of "Vector3.zero".
        new GameObject("VisualPath", typeof(VisualPath));
        GameObject go;

        tileSpriteController = new TileSpriteController(World);
        characterSpriteController = new CharacterSpriteController(World);
        furnitureSpriteController = new FurnitureSpriteController(World);
        utilitySpriteController = new UtilitySpriteController(World);
        jobSpriteController = new JobSpriteController(World, furnitureSpriteController, utilitySpriteController);
        inventorySpriteController = new InventorySpriteController(World, inventoryUI);
        shipSpriteController = new ShipSpriteController(World);

        buildModeController = new BuildModeController();
        spawnInventoryController = new SpawnInventoryController();
        mouseController = new MouseController(buildModeController, furnitureSpriteController, utilitySpriteController, circleCursorPrefab);
        keyboardManager = KeyboardManager.Instance;
        questController = new QuestController();
        cameraController = new CameraController();
        TradeController = new TradeController();
        autosaveManager = new AutosaveManager();

        // Register inputs actions
        keyboardManager.RegisterInputAction("Pause", KeyboardMappedInputType.KeyUp, () => { IsPaused = !IsPaused; });
        keyboardManager.RegisterInputAction("DevMode", KeyboardMappedInputType.KeyDown, ChangeDevMode);

        // Hiding Dev Mode spawn inventory controller if devmode is off.
        spawnInventoryController.SetUIVisibility(Settings.GetSetting("DialogBoxSettings_developerModeToggle", false));

        cameraController.Initialize();
        cameraController.Moved += this.World.OnCameraMoved;

        // Initialising controllers.
        GameObject canvas = GameObject.Find("Canvas");
        go = Instantiate(Resources.Load("UI/ContextMenu"), canvas.transform.position, canvas.transform.rotation, canvas.transform) as GameObject;
        go.name = "ContextMenu";
    }

    public void Update()
    {
        TimeManager.Instance.Update(Time.deltaTime);
    }

    /// <summary>
    /// Gets the tile at the Unity-space coordinates.
    /// </summary>
    /// <returns>The tile at world coordinate.</returns>
    /// <param name="coord">Unity World-Space coordinates.</param>
    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x + 0.5f);
        int y = Mathf.FloorToInt(coord.y + 0.5f);

        return World.GetTileAt(x, y, (int)coord.z);
    }

    public string FileSaveBasePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "Saves");
    }

    public void NewWorld()
    {
        Debug.ULogChannel("WorldController", "NewWorld button was clicked.");

        Destroy();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadWorld(string fileName)
    {
        Debug.ULogChannel("WorldController", "LoadWorld button was clicked.");

        // Reload the scene to reset all data (and purge old references)
        loadWorldFromFile = fileName;
        Destroy();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Destroy()
    {
        TimeManager.Instance.Destroy();
        KeyboardManager.Instance.Destroy();
        Scheduler.Scheduler.Current.Destroy();
        GameMenuManager.Instance.Destroy();
    }

    /// <summary>
    /// Change the developper mode.
    /// </summary>
    public void ChangeDevMode()
    {
        bool developerMode = !Settings.GetSetting("DialogBoxSettings_developerModeToggle", false);
        Settings.SetSetting("DialogBoxSettings_developerModeToggle", developerMode);
        spawnInventoryController.SetUIVisibility(developerMode);
        ///FurnitureBuildMenu.instance.RebuildMenuButtons(developerMode);
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

        // Leaving this for Unity's console because UberLogger mangles multiline messages.
        Debug.Log(reader.ToString());
        World = (World)serializer.Deserialize(reader);
        reader.Close();
    }
}
